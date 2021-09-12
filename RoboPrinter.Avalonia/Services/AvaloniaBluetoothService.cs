// unset

using DynamicData;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using System;
using System.Linq;
using System.Reactive.Subjects;
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

		private readonly Subject<string> _dataReceived;
		private readonly Subject<string> _dataSent;
		private readonly SourceCache<BleServiceItem, string> _devices;

		private bool _isConnected;

		private GattCharacteristic? _positionCharacteristic;
		private GattCharacteristic? _sensorCharacteristic;
		private GattCharacteristic? _latencyTestCharacteristic;

		public AvaloniaBluetoothService()
		{
			_dataSent = new Subject<string>();
			_dataReceived = new Subject<string>();
			_devices = new SourceCache<BleServiceItem, string>(
				device => device.ServerId);

			_bleAdvertisementWatcher = new BluetoothLEAdvertisementWatcher
			{
				ScanningMode = BluetoothLEScanningMode.Active
			};

			_bleAdvertisementWatcher.Received += async (w, btAdv) =>
			{
				BluetoothLEDevice? device =
					await BluetoothLEDevice.FromBluetoothAddressAsync(
						btAdv.BluetoothAddress);

				_devices.AddOrUpdate(new BleServiceItem
				{
					ServerId = device.DeviceId,
					Name = device.Name,
					Rssi = btAdv.RawSignalStrengthInDBm,
					ServiceUuids = device.GattServices.ToString(),
					BluetoothAddress = device.BluetoothAddress
				});
			};
		}

		[Reactive]
		public string ErrorStatus { get; set; } = "";

		[Reactive]
		public bool IsScanningInProgress { get; set; }

		[Reactive]
		public bool IsConnectionInProgress { get; set; }

		public IObservable<string> DataReceived => _dataReceived;
		public IObservable<string> DataSent => _dataSent;

		public IObservable<IChangeSet<BleServiceItem, string>>
			BluetoothDeviceCollectionChange => _devices.Connect();

		[Reactive]
		public bool IsTestInProgress { get; set; }

		public async void Connect(BleServiceItem? item)
		{
			if (item == null || _isConnected)
			{
				return; // TODO change error message
			}

			if (IsScanningInProgress)
			{
				StopDeviceDiscovery();
			}

			IsConnectionInProgress = true;

			BluetoothLEDevice? device =
				await BluetoothLEDevice.FromBluetoothAddressAsync(
					item.BluetoothAddress);

			GattDeviceServicesResult? gattServices =
				await device.GetGattServicesAsync();
			GattDeviceService? mainService = gattServices.Services
				.Single(s => s.Uuid == ServiceUuid);

			if (mainService != null)
			{
				GattCharacteristicsResult? characteristics =
					await mainService.GetCharacteristicsAsync();

				if (characteristics == null)
				{
					IsConnectionInProgress = false;
					return;
				}

				if (characteristics.Status != GattCommunicationStatus.Success)
				{
					ErrorStatus = characteristics.Status + ": " +
					              characteristics.ProtocolError;
				}

				_positionCharacteristic =
					characteristics.Characteristics.Single(
						c => c.Uuid == PositionUuid);

				_sensorCharacteristic =
					characteristics.Characteristics.Single(
						c => c.Uuid == SensorUuid);

				// GattCommunicationStatus status = await _sensorCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
				// 	GattClientCharacteristicConfigurationDescriptorValue.Notify);
				// _sensorCharacteristic.ValueChanged += (s, args) =>
				// {
				// 	var reader =
				// 		DataReader.FromBuffer(args.CharacteristicValue);
				// };
				
				_devices.AddOrUpdate(new BleServiceItem
				{
					ServerId = device.DeviceId,
					Name = device.Name,
					IsConnected = true
				});
			}
			else
			{
				IsConnectionInProgress = false;
				return;
			}

			IsConnectionInProgress = false;
			_isConnected = true;
		}

		public void Disconnect()
		{
			_isConnected = false;
			//_bleClient.Disconnect();
		}

		public void TestResponseTime(TimeSpan timeout, Action<long> onCompleted,
			Action<Exception> onError)
		{
			throw new NotImplementedException();
		}

		public Task SendDataAsync(string data, Action<Exception> onError,
			CancellationToken token = default)
		{
			if (string.IsNullOrEmpty(data))
			{
				return Task.FromException(new InvalidOperationException(
					"No data has been provided"));
			}

			if (_isConnected == false || _positionCharacteristic == null)
			{
				return Task.FromException(new InvalidOperationException(
					"Connection has not been established yet"));
			}

			try
			{
				if (data[^1] != '\n')
				{
					data += '\n';
				}

				DataWriter writer = new();
				writer.WriteString(data);

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

		public void StartDeviceDiscovery()
		{
			ErrorStatus = "";
			IsScanningInProgress = true;
			_bleAdvertisementWatcher.Start();
		}

		public void StopDeviceDiscovery()
		{
			_bleAdvertisementWatcher.Stop();
			IsScanningInProgress = false;
		}

		void IDisposable.Dispose()
		{
			if (_isConnected)
			{
				Disconnect();
			}
		}
	}
}