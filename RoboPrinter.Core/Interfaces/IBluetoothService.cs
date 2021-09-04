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
		IObservable<IChangeSet<BluetoothDevice, string>> BluetoothDeviceCollectionChange { get; }
		bool IsTestInProgress { get; set; }
		
		void Connect(BluetoothDevice device);
		void Disconnect();
		void DiscoverDevices();
		
		void TestResponseTime(TimeSpan timeout, Action<long> onCompleted, Action<Exception> onError);
		ValueTask SendDataAsync(string data, Action<Exception> onError, CancellationToken token = default);
	}
}