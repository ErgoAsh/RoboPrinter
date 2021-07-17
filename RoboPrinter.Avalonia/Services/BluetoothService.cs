using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RoboPrinter.Avalonia.Services
{
	public class BluetoothService : IBluetoothService, IDisposable
	{
		private DeviceInformationCollection? _deviceCache;

		private RfcommDeviceService? _service;
		private StreamSocket? _socket;
		private DataWriter? _writer;

		public event EventHandler<PositionEventArgs> PositionSent;
		public event EventHandler<PositionEventArgs> FeedbackReceived;

		public BluetoothService()
		{
			RefreshDeviceList();

			//DeviceWatcher aa = new DeviceWatcher();
		}

		public async Task<Task> Connect(BluetoothDevice device)
		{
			// Initialize the target Bluetooth BR device
			RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(device.Id);

			// Check that the service meets this App's minimum requirement
			if (!SupportsProtection(service))
			{
				return Task.CompletedTask;
			}

			_service = service;
			_socket = new StreamSocket();

			return _socket.ConnectAsync(
				_service.ConnectionHostName,
				_service.ConnectionServiceName,
				SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
			).AsTask();
		}

		public void Disconnect()
		{
			((IDisposable)this).Dispose();
		}

		public async void RefreshDeviceList()
		{
			// Enumerate devices with the object push service
			_deviceCache = await DeviceInformation
				.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
		}

		public IEnumerable<BluetoothDevice> GetAvailableBluetoothDevices()
		{
			List<BluetoothDevice> devices = new();
			if (_deviceCache is null)
			{
				return devices;
			}

			devices.AddRange(_deviceCache.Select(device => 
				new BluetoothDevice
				{
					Id = device.Id, Name = device.Name
				}
			));

			return devices;
		}

		public async void SendPosition(short servoId, float position)
		{
			if (_service == null) // TODO remove, return exception
			{
				RefreshDeviceList();
				
				// TODO upgrade
				using IEnumerator<BluetoothDevice> enumerator = GetAvailableBluetoothDevices().GetEnumerator();
				enumerator.MoveNext();
				BluetoothDevice service = enumerator.Current;
				(await Connect(service)).Wait();
					_writer = new DataWriter(_socket?.OutputStream);

			}

			if (servoId is < 0 or > 5)
			{
				return; // TODO throw new exception, read 5 from somewhere
			}

			if (position is < 0 or > 180)
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder()
				.Append((char)(servoId + 65))
				.Append(position)
				.Append('\n');

			if (_writer == null || _socket == null)
			{
				//Connect(_service.)
			}
			else
			{
				_writer.WriteString(stringBuilder.ToString());

				await _writer.StoreAsync();
				await _writer.FlushAsync();
			}
		}

		void IDisposable.Dispose()
		{
			_writer?.Dispose();
			_socket?.Dispose();
			_service?.Dispose();
		}

		void IBluetoothService.OnPositionSentEvent(PositionEventArgs e)
		{
			PositionSent.Invoke(this, e);
		}

		void IBluetoothService.OnFeedbackReceivedEvent(PositionEventArgs e)
		{
			FeedbackReceived.Invoke(this, e);
		}

		// This App requires a connection that is encrypted but does not care about
		// whether it's authenticated.
		private static bool SupportsProtection(RfcommDeviceService service)
		{
			switch (service.ProtectionLevel)
			{
				case SocketProtectionLevel.PlainSocket:
					return service.MaxProtectionLevel is SocketProtectionLevel
						.BluetoothEncryptionWithAuthentication or SocketProtectionLevel
						.BluetoothEncryptionAllowNullAuthentication;
				case SocketProtectionLevel.BluetoothEncryptionWithAuthentication:
					return true;
				case SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication:
					return true;
			}
			return false;
		}
	}
}