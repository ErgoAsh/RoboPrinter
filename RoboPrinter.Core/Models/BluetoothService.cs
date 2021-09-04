// unset

using DynamicData;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Models
{

	public class BluetoothService : IBluetoothService, IDisposable
	{
		private readonly BluetoothClient _bluetoothClient;
		private NetworkStream _stream;
		
		private bool _isConnected;
		
		private readonly Subject<string> _dataReceived;
		private readonly Subject<string> _dataSent;
		private readonly SourceCache<BluetoothDevice, string> _devices;

		public IObservable<string> DataReceived => _dataReceived;
		public IObservable<string> DataSent => _dataSent;
		public IObservable<IChangeSet<BluetoothDevice, string>> BluetoothDeviceCollectionChange => _devices.Connect();

		[Reactive]
		public bool IsTestInProgress { get; set; }

		public BluetoothService()
		{
			_dataSent = new Subject<string>();
			_dataReceived = new Subject<string>();
			_devices = new SourceCache<BluetoothDevice, string>(
				device => device.Id);
			
			_bluetoothClient = new BluetoothClient();
		}

		public void Connect(BluetoothDevice device)
		{
			_bluetoothClient.Connect(BluetoothAddress.Parse(device.Id), 
				InTheHand.Net.Bluetooth.BluetoothService.SerialPort);

			_stream = _bluetoothClient.GetStream();
			
			ReceiveMessageLoop();
		}
		
		public void Disconnect()
		{
			_isConnected = false;
			
			_stream.Dispose();
			_bluetoothClient.Dispose();
		}

		void IDisposable.Dispose()
		{
			if (_isConnected)
			{
				Disconnect();
			}
		}

		public void DiscoverDevices()
		{
			_devices.AddOrUpdate(_bluetoothClient.DiscoverDevices()
				.Select(item => new BluetoothDevice
			{
				Id = item.DeviceAddress.ToString(),
				Name = item.DeviceName,
				State = BluetoothState.Paired //TODO check
			}));

			// TODO use status: "discovered", "paired", "connected"
		}

		public async void TestResponseTime(TimeSpan timeout, Action<long> onCompleted, Action<Exception> onError)
		{
			if (_bluetoothClient == null || _stream == null || IsTestInProgress)
				// TODO exceptions
				return;
			
			IsTestInProgress = true;

			Stopwatch stopwatch = Stopwatch.StartNew();
			
			CancellationTokenSource tokenSource = new();
			tokenSource.CancelAfter(timeout.Milliseconds);
			await SendDataAsync("T", onError, tokenSource.Token);

			_dataReceived
				.Where(parameter => parameter[0] == 'T')
				.Take(1)
				//.TakeUntil(new DateTimeOffset(DateTime.Now, timeout), )
				.Subscribe(
					_ =>
					{
						onCompleted.Invoke(stopwatch.ElapsedMilliseconds);
						IsTestInProgress = false;
					}, error =>
					{
						onError.Invoke(error);
						IsTestInProgress = false;
					}); //TODO check onComplete if isTestInProgress, return error then
		}
		
		private void ReceiveMessageLoop()
		{
			while (_isConnected)
			{
				_bluetoothClient.Client.ReceiveTimeout = 20;

				byte[] dataBuffer = new byte[2048];
				using MemoryStream memoryStream = new();
				do
				{
					try
					{
						int length = _stream.Read(dataBuffer, 0, dataBuffer.Length);
						memoryStream.Write(dataBuffer, 0, length);
					}
					catch //(IOException ex)
					{
						// TODO handle
					}
				} while (_stream.DataAvailable);

				string response = Encoding.ASCII.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
				
				_dataReceived.OnNext(response);
			}
		}

		public ValueTask SendDataAsync(string data, Action<Exception> onError, CancellationToken token = default)
		{
			if (data == null)
				return ValueTask.FromException(new InvalidOperationException(
					"Null data has been provided"));

			if (_bluetoothClient == null || _stream == null)
				return ValueTask.FromException(new InvalidOperationException(
					"Connection has not been established yet"));
			
			try
			{
				if (data[^1] != '\n')
				{
					data += '\n';
				}
				
				byte[] buffer = Encoding.ASCII.GetBytes(data);
				return _stream.WriteAsync(buffer, token);
			}
			catch (Exception e)
			{
				onError.Invoke(e);
				return ValueTask.FromException(e);
			}
		}
	}
}