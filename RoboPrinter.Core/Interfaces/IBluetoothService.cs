using DynamicData;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using RoboPrinter.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Interfaces
{
	public interface IBluetoothService
	{
		IObservable<string> DataReceived { get; }
		IObservable<string> DataSent { get; }
		IObservable<IChangeSet<BleServiceItem, string>> BluetoothDeviceCollectionChange { get; }
		bool IsTestInProgress { get; set; }
		bool IsScanningInProgress { get; set; }
		bool IsConnectionInProgress { get; set; }
		
		void Connect(BleServiceItem item);
		void Disconnect();
		void StartDeviceDiscovery();
		void StopDeviceDiscovery();
		
		void TestResponseTime(TimeSpan timeout, Action<long> onCompleted, Action<Exception> onError);
		Task SendDataAsync(string data, Action<Exception> onError, CancellationToken token = default);
	}
}