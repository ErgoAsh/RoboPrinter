using DynamicData;
using RoboPrinter.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Interfaces
{
	public enum ConnectionState
	{
		NotConnected,
		InProgress,
		Connected
	}

	public interface IBluetoothService
	{
		IObservable<string> WhenDataReceived { get; }
		IObservable<string> WhenDataSent { get; }

		IObservable<InformationMessage> WhenInfoMessageChanged { get; }
		IObservable<ConnectionState> WhenConnectionStateChanged { get; }
		IObservable<IChangeSet<BleServiceItem, string>> WhenBluetoothDevicesChanged { get; }

		void Connect(BleServiceItem item);
		void Disconnect();
		void StartDeviceDiscovery();
		void StopDeviceDiscovery();
		
		void TestResponseTime(TimeSpan timeout, Action<long> onCompleted, Action<Exception> onError);
		Task SendDataAsync(byte[] data, Action<Exception> onError, CancellationToken token = default);
	}
}