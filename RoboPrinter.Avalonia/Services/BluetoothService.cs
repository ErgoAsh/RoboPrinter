using DynamicData;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RoboPrinter.Avalonia.Services
{
	public class BluetoothService : IBluetoothService, IDisposable
	{
		private const string BluetoothProtocolId = "{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}";

		private readonly Subject<string> _dataReceived;
		private readonly Subject<string> _dataSent;
		private readonly SourceCache<BluetoothDevice, string> _devices;

		private DeviceWatcher? _deviceWatcher;
		private DataReader? _reader;
		private RfcommDeviceService? _service;
		private StreamSocket? _socket;
		private DataWriter? _writer;

		public BluetoothService()
		{
			_dataSent = new Subject<string>();
			_dataReceived = new Subject<string>();
			_devices = new SourceCache<BluetoothDevice, string>(device => device.Id);

			InitializeWatcher();
		}

		[Reactive]
		public bool IsTestInProgress { get; set; }

		public IObservable<string> DataReceived => _dataReceived;
		public IObservable<IChangeSet<BluetoothDevice, string>> BluetoothDeviceCollectionChange => _devices.Connect();
		public IObservable<string> DataSent => _dataSent;

		public async void Connect(BluetoothDevice device, Action onCompleted, Action<Exception> onError)
		{
			// Initialize the target Bluetooth BR device
			RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(
				device.Id + "#RFCOMM:00000000:{" + RfcommServiceId.SerialPort.Uuid + "}");

			// Check that the service meets this App's minimum requirement
			if (!SupportsProtection(service))
			{
				return;
			}

			_service = service;
			_socket = new StreamSocket();

			try
			{
				await _socket.ConnectAsync(
					_service.ConnectionHostName,
					_service.ConnectionServiceName,
					SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
				);

				_writer = new DataWriter(_socket.OutputStream);
				_reader = new DataReader(_socket.InputStream);

				device.IsConnected = true;
				_devices.AddOrUpdate(device);

				onCompleted.Invoke();
			}
			catch (Exception ex) when ((uint)ex.HResult == 0x80070490) // ERROR_ELEMENT_NOT_FOUND
			{
				onError.Invoke(ex);
				// TODO
			}
			catch (Exception ex) when ((uint)ex.HResult == 0x80072740) // WSAEADDRINUSE
			{
				onError.Invoke(ex);
				// TODO Only one usage of each socket address (protocol/network address/port) is normally permitted
			}
		}

		public void Disconnect()
		{
			// TODO
		}

		public async void SendData(string data)
		{
			if (_service == null || _writer == null || _socket == null)
			{
				return;
				//throw new Exception(
				//	"[BluetoothService::SendPosition] Connection has not been established yet");
			}

			// If not ends with \n, add one

			_writer.WriteString(data);

			await _writer.StoreAsync();
			await _writer.FlushAsync();
		}

		public void TestConnection(
			BluetoothDevice device,
			TimeSpan timeout,
			Action<int> onCompleted,
			Action<Exception> onError)
		{
			// TODO use timeoutSeconds
			IsTestInProgress = true;

			DateTime tBefore = DateTime.Now;

			Connect(device, () =>
			{
				SendData("T");

				DataReceived
					.Where(parameter => parameter[0] == 'T')
					.Take(1)
					//.TakeUntil(new DateTimeOffset(DateTime.Now, timeout), )
					.Subscribe(
						_ =>
						{
							onCompleted.Invoke(DateTime.Now.Subtract(tBefore).Milliseconds);
							IsTestInProgress = false;
						}, error =>
						{
							onError.Invoke(error);
							IsTestInProgress = false;
						}); //TODO check onComplete if isTestInProgress, return error then
			}, error =>
			{
				onError.Invoke(error);
				IsTestInProgress = false;
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

		private void InitializeWatcher()
		{
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
				if (deviceInfoUpdate.Properties.ContainsKey("Name"))
				{
					_devices.AddOrUpdate(new BluetoothDevice
					{
						Id = deviceInfoUpdate.Id,
						Name = deviceInfoUpdate.Properties["Name"] as string,
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

				_dataReceived.OnNext(size.ToString());

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