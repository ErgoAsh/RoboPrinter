using DynamicData;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using System;
using System.Reactive;
using System.Reactive.Linq;
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
		private const string BluetoothProtocolId = "{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}";

		private readonly SourceCache<BluetoothDevice, string> _devices;
		private DeviceWatcher? _deviceWatcher;
		private DataReader? _reader;
		private RfcommDeviceService? _service;
		private StreamSocket? _socket;
		private DataWriter? _writer;

		public BluetoothService()
		{
			_devices = new SourceCache<BluetoothDevice, string>(device => device.Id);

			string[] requestedProperties = {"System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected"};

			_deviceWatcher = DeviceInformation.CreateWatcher(
				$"(System.Devices.Aep.ProtocolId:=\"{BluetoothProtocolId}\")",
				requestedProperties,
				DeviceInformationKind.AssociationEndpoint);

			// TODO move to UI thread?
			_deviceWatcher.Added += (_, deviceInfo) =>
			{
				if (deviceInfo.Name != "")
				{
					_devices.AddOrUpdate(new BluetoothDevice
					{
						Id = deviceInfo.Id, Name = deviceInfo.Name, IsConnected = false
					});
				}
			};

			_deviceWatcher.Updated += (_, deviceInfoUpdate) =>
			{
				//if(deviceInfoUpdate.Name != "")
				{
					_devices.AddOrUpdate(new BluetoothDevice
					{
						Id = deviceInfoUpdate.Id,
						Name = deviceInfoUpdate.Properties["name"] as string,
						IsConnected = false
					});
				}
			};

			_deviceWatcher.Removed += (_, deviceInfoUpdate) =>
			{
				_devices.Remove(deviceInfoUpdate.Id);
			};

			_deviceWatcher.Stopped += (_, _) =>
			{
				_devices.Clear();
			};

			_deviceWatcher.Start();
		}

		public event EventHandler<PositionEventArgs> PositionSent = null!;
		public event EventHandler<FeedbackEventArgs> FeedbackReceived = null!;

		public async Task Connect(BluetoothDevice device)
		{
			// Initialize the target Bluetooth BR device
			RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(device.Id);

			// Check that the service meets this App's minimum requirement
			if (!SupportsProtection(service))
			{
				return;
			}

			_service = service;
			_socket = new StreamSocket();

			await _socket.ConnectAsync(
				_service.ConnectionHostName,
				_service.ConnectionServiceName,
				SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
			);

			_writer = new DataWriter(_socket.OutputStream);
			_reader = new DataReader(_socket.InputStream);
		}

		public void Disconnect()
		{
			// TODO
		}

		public async void ReadFeedbackRecursive()
		{
			try
			{
				uint size = await _reader.LoadAsync(sizeof(uint));
				if (size < sizeof(uint))
				{
					// "Remote device terminated connection - make sure only one instance
					// of server is running on remote device"
					return;
				}

				uint stringLength = _reader.ReadUInt32();
				uint actualStringLength = await _reader.LoadAsync(stringLength);
				if (actualStringLength != stringLength)
				{
					// The underlying socket was closed before we were able to read the whole data
					return;
				}

				//ReceiveStringLoop(chatReader);
				ReadFeedbackRecursive();
			}
			catch (Exception ex)
			{
				lock (this)
				{
					if (_socket == null)
					{
						// Do not print anything here -  the user closed the socket.
						if ((uint)ex.HResult == 0x80072745)
						{
							;
						}

						// "Disconnect triggered by remote device"
						if ((uint)ex.HResult == 0x800703E3)
						{
							;
						}
						// "The I/O operation has been aborted because of
						// either a thread exit or an application request."
					}
				}
			}
		}

		public IObservable<IChangeSet<BluetoothDevice, string>> GetBluetoothDevicesObservable()
		{
			return _devices.Connect();
		}

		public async void SendPosition(short servoId, float position)
		{
			if (_service == null)
			{
				throw new Exception(
					"[BluetoothService::SendPosition] Connection has not been established yet");
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

		void IBluetoothService.OnPositionSentEvent(PositionEventArgs e)
		{
			PositionSent.Invoke(this, e);
		}

		void IBluetoothService.OnFeedbackReceivedEvent(FeedbackEventArgs e)
		{
			FeedbackReceived.Invoke(this, e);
		}

		public void TestConnection(BluetoothDevice device, Action<int> callback)
		{
			DateTime tBefore = DateTime.Now;

			IObservable<EventPattern<FeedbackEventArgs>> observable = Observable.FromEventPattern<FeedbackEventArgs>(
				handler => FeedbackReceived += handler,
				handler => FeedbackReceived -= handler);

			observable
				.Where(pattern => pattern.EventArgs.IsTest)
				.Take(1) // Subscribe only one event execution
				.Subscribe(
					args =>
					{
						callback.Invoke(DateTime.Now.Subtract(tBefore).Milliseconds);
					},
					error =>
					{
						//throw new Exception()
					});
		}

		void IDisposable.Dispose()
		{
			if (_deviceWatcher?.Status
				is DeviceWatcherStatus.Started
				or DeviceWatcherStatus.EnumerationCompleted)
			{
				_deviceWatcher?.Stop();
			}

			_deviceWatcher = null;

			_writer?.DetachStream();
			_writer?.Dispose();

			_socket?.Dispose();
			_service?.Dispose();
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