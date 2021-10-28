// unset

using DynamicData;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace RoboPrinter.Avalonia.Services
{
	public class AvaloniaBluetoothService : IBluetoothService, IDisposable
	{
		public static readonly Guid ServiceUuid =
			Guid.Parse("e31ba033-e901-4566-afb2-8c545c554c8b");

		public static readonly Guid SensorUuid =
			Guid.Parse("d8593ea1-0153-4397-9f2e-13703b529215");

		public static readonly Guid PositionUuid =
			Guid.Parse("944eedc2-3725-425e-9817-0ab0bfda64fe");

		public static readonly Guid TimeResponseUuid =
			Guid.Parse("9539242d-efbb-414e-a10a-2a06e7445c67");

		private readonly BluetoothLEAdvertisementWatcher
			_bleAdvertisementWatcher;

		private readonly Subject<string> _whenDataReceived;
		private readonly Subject<string> _whenDataSent;
		private readonly Subject<InformationMessage> _whenInfoMessageIsChanged;
		private readonly Subject<ConnectionState> _whenConnectionChanged;
		private readonly SourceCache<BleServiceItem, string> _devices;

		private ConnectionState _connectionState;
		private string _appDeviceAddress;

		private GattCharacteristic? _positionCharacteristic;
		private GattCharacteristic? _sensorCharacteristic;
		private GattCharacteristic? _latencyTestCharacteristic;

		public AvaloniaBluetoothService()
		{
			_connectionState = ConnectionState.NotConnected;

			_whenDataSent = new Subject<string>();
			_whenDataReceived = new Subject<string>();
			_whenInfoMessageIsChanged = new Subject<InformationMessage>();
			_whenConnectionChanged = new Subject<ConnectionState>();
			_devices = new SourceCache<BleServiceItem, string>(
				device => device.ServerId);

			_bleAdvertisementWatcher = new BluetoothLEAdvertisementWatcher
			{
				ScanningMode = BluetoothLEScanningMode.Active
			};
			_bleAdvertisementWatcher.Received += OnBleAdvertisementWatcherReceived;
		}

		private async void OnBleAdvertisementWatcherReceived(BluetoothLEAdvertisementWatcher w, BluetoothLEAdvertisementReceivedEventArgs btAdv)
		{
			BluetoothLEDevice? device = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
			if (device == null) 
				return;

			if (string.IsNullOrWhiteSpace(_appDeviceAddress))
			{
				if (device.DeviceId.Contains("BluetoothLE#BluetoothLE"))
				{
					_appDeviceAddress = device.DeviceId.Substring(23, 17);
				}
			}

			if (_devices.Items.Select(item => item.BluetoothAddress)
			    .Contains(device.BluetoothAddress))
				return;

			BleServiceItem newDevice = new()
			{
				ServerId = device.DeviceId.Split("-")[^1],
				Name = device.Name,
				SignalStrength = btAdv.RawSignalStrengthInDBm,
				BluetoothAddress = device.BluetoothAddress
			};

			_devices.AddOrUpdate(newDevice);
		}

		public IObservable<string> WhenDataReceived => _whenDataReceived;
		public IObservable<string> WhenDataSent => _whenDataSent;
		public IObservable<InformationMessage> WhenInfoMessageChanged => _whenInfoMessageIsChanged;
		public IObservable<ConnectionState> WhenConnectionStateChanged => _whenConnectionChanged;

		public IObservable<IChangeSet<BleServiceItem, string>>
			WhenBluetoothDevicesChanged => _devices.Connect();

		[Reactive]
		public bool IsTestInProgress { get; set; }

		public async void Connect(BleServiceItem? item)
		{
			if (item == null || _connectionState == ConnectionState.Connected)
			{
				_whenInfoMessageIsChanged.OnNext(new InformationMessage
				{
					Message = "Client is already connected", 
					MessageType = MessageType.Error
				});
				return;
			}

			_connectionState = ConnectionState.InProgress;
			_whenConnectionChanged.OnNext(ConnectionState.InProgress);
			StopDeviceDiscovery();

			BluetoothLEDevice? device =
				await BluetoothLEDevice.FromBluetoothAddressAsync(
					item.BluetoothAddress);

			GattDeviceServicesResult? gattServices =
				await device.GetGattServicesAsync();

			try
			{
				GattDeviceService? mainService = gattServices.Services
					.Single(s => s.Uuid == ServiceUuid);

				if (mainService == null)
				{
					_connectionState = ConnectionState.NotConnected;
					_whenConnectionChanged.OnNext(ConnectionState.NotConnected);
					return;
				}

				GattCharacteristicsResult? characteristics =
					await mainService.GetCharacteristicsAsync();

				if (characteristics == null)
				{
					_connectionState = ConnectionState.NotConnected;
					_whenConnectionChanged.OnNext(ConnectionState.NotConnected);
					return;
				}

				if (characteristics.Status != GattCommunicationStatus.Success)
				{
					_whenInfoMessageIsChanged.OnNext(new InformationMessage
					{
						MessageType = MessageType.Error,
						Message = characteristics.Status + ": " + characteristics.ProtocolError
					});
				}

				_positionCharacteristic =
					characteristics.Characteristics.Single(
						c => c.Uuid == PositionUuid);

				_sensorCharacteristic =
					characteristics.Characteristics.Single(
						c => c.Uuid == SensorUuid);

				// TODO sensors
				// GattCommunicationStatus status = await _sensorCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
				// 	GattClientCharacteristicConfigurationDescriptorValue.Notify);
				// _sensorCharacteristic.ValueChanged += (s, args) =>
				// {
				// 	var reader =
				// 		DataReader.FromBuffer(args.CharacteristicValue);
				// };

				_devices.AddOrUpdate(new BleServiceItem
				{
					ServerId = device.BluetoothDeviceId.Id, //TODO add RSSI
					Name = device.Name,
					IsConnected = true
				});

				_connectionState = ConnectionState.Connected;
				_whenConnectionChanged.OnNext(ConnectionState.Connected);
			}
			catch (InvalidOperationException e)
			{
				_connectionState = ConnectionState.NotConnected;
				_whenConnectionChanged.OnNext(ConnectionState.NotConnected);
				_whenInfoMessageIsChanged.OnNext(new InformationMessage
				{
					Message = "Selected BLE device does not contain required services",
					MessageType = MessageType.Error
				});
			}
		}

		public void Disconnect()
		{
			// TODO If it's already connecting...

			_connectionState = ConnectionState.NotConnected;
			_whenConnectionChanged.OnNext(ConnectionState.NotConnected);
			//_bleClient.Disconnect();
		}

		public void TestResponseTime(TimeSpan timeout, Action<long> onCompleted,
			Action<Exception> onError)
		{
			throw new NotImplementedException();
		}

		public Task SendDataAsync(byte[] data, Action<Exception> onError,
			CancellationToken token = default)
		{
			// if (data == null || data.em)
			// {
			// 	return Task.FromException(new ArgumentNullException(data,
			// 		"No data has been provided"));
			// }
			
			if (data.Length != 20)
			{
				//return Task.FromException(new ArgumentOutOfRangeException(data,
				//	"Data length is invalid"));
			}

			if (_connectionState != ConnectionState.Connected || _positionCharacteristic == null)
			{
				return Task.FromException(new InvalidOperationException(
					"Connection has not been established yet"));
			}

			try
			{
				DataWriter writer = new();
				writer.WriteBytes(data);

				return _positionCharacteristic.WriteValueAsync(
					writer.DetachBuffer(),
					GattWriteOption.WriteWithResponse).AsTask(token);
			}
			catch (Exception e)
			{
				onError.Invoke(e);
				return Task.FromException(e);
			}
		}

		public string GetAppDeviceAddress()
		{
			return _appDeviceAddress;
		}

		public void StartDeviceDiscovery()
		{
			//ErrorStatus = "";
			_bleAdvertisementWatcher.Start();
		}

		public void StopDeviceDiscovery()
		{
			_bleAdvertisementWatcher.Stop();
		}

		void IDisposable.Dispose()
		{
			if (_connectionState == ConnectionState.Connected)
			{
				Disconnect();
			}
		}
	}
}