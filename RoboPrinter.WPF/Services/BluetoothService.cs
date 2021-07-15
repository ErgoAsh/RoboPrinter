using RoboPrinter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RoboPrinter.WPF
{
	public class BluetoothService : IBluetoothService, IDisposable
	{
		private DeviceInformationCollection _deviceCache;

		private RfcommDeviceService _service;
		private StreamSocket _socket;
		private DataWriter _writer;

		public event EventHandler<PositionEventArgs> PositionSent;
		public event EventHandler<PositionEventArgs> FeedbackRecivied;

		public BluetoothService()
		{
			RefreshDeviceList();

			//DeviceWatcher aa = new DeviceWatcher();
		}

		public async void Connect(BluetoothDevice device)
		{
			// Initialize the target Bluetooth BR device
			RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(device.Id);

			// Check that the service meets this App's minimum requirement
			if (SupportsProtection(service))
			{
				_service = service;
				_socket = new StreamSocket();

				await _socket.ConnectAsync(
					_service.ConnectionHostName,
					_service.ConnectionServiceName,
					SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
				);

				_writer = new DataWriter(_socket.OutputStream);
			}
		}

		public void Disconnect()
		{
			Dispose();
		}

		public async void RefreshDeviceList()
		{
			// Enumerate devices with the object push service
			_deviceCache = await DeviceInformation
				.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
		}

		public IEnumerable<BluetoothDevice> GetAvilableBluetoothDevices()
		{
			List<BluetoothDevice> devices = new List<BluetoothDevice>();
			foreach (DeviceInformation device in _deviceCache)
			{
				devices.Add(new BluetoothDevice
				{
					Id = device.Id,
					Name = device.Name
				});
			}
			return devices;
		}

		public async void SendPosition(short servoID, float position)
		{
			if (_service == null) // TODO remove
			{
				RefreshDeviceList();
				var service = GetAvilableBluetoothDevices().GetEnumerator().Current;
				if (service != null)
					Connect(service);
				else
				{
					var enumerator = GetAvilableBluetoothDevices().GetEnumerator();
					enumerator.MoveNext();
					service = enumerator.Current;
					if (service != null)
						Connect(service);
				}
			}

			if (servoID < 0 || servoID > 5)
			{
				return; // TODO throw new exception, read 5 from somewhere
			}

			if (position < 0 || position > 180)
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder()
				.Append((char)(servoID + 65))
				.Append(position)
				.Append('\n');

			if (_writer == null || _socket == null)
			{
				//Connect(_service.)
			}

			_writer.WriteString(stringBuilder.ToString());

			await _writer.StoreAsync();
			await _writer.FlushAsync();
		}

		public void Dispose()
		{
			_writer.Dispose();
			_socket.Dispose();
			_service.Dispose();
		}

		void IBluetoothService.OnPositionSentEvent(PositionEventArgs e)
		{
			PositionSent.Invoke(this, e);
		}

		void IBluetoothService.OnFeedbackReciviedEvent(PositionEventArgs e)
		{
			FeedbackRecivied.Invoke(this, e);
		}

		// This App requires a connection that is encrypted but does not care about
		// whether it's authenticated.
		public static bool SupportsProtection(RfcommDeviceService service)
		{
			switch (service.ProtectionLevel)
			{
				case SocketProtectionLevel.PlainSocket:
					if (service.MaxProtectionLevel is SocketProtectionLevel
							.BluetoothEncryptionWithAuthentication or SocketProtectionLevel
							.BluetoothEncryptionAllowNullAuthentication)
					{
						// The connection can be upgraded when opening the socket so the
						// App may offer UI here to notify the user that Windows may
						// prompt for a PIN exchange.
						return true;
					}
					else
					{
						// The connection cannot be upgraded so an App may offer UI here
						// to explain why a connection won't be made.
						return false;
					}
				case SocketProtectionLevel.BluetoothEncryptionWithAuthentication:
					return true;
				case SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication:
					return true;
				default:
					break;
			}
			return false;
		}
	}
}
