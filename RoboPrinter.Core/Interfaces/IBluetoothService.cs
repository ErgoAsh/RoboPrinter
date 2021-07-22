using DynamicData;
using RoboPrinter.Core.Models;
using System;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Interfaces
{
	public interface IBluetoothService
	{
		public void Disconnect();
		public void Connect(BluetoothDevice device, Action onCompleted, Action<Exception> onError);

		public void TestConnection(
			BluetoothDevice device,
			TimeSpan timeout,
			Action<int> onCompleted,
			Action<Exception> onError);
		
		public void SendData(string data);

		public IObservable<string> DataSent { get; }
		public IObservable<string> DataReceived { get; }
		public IObservable<IChangeSet<BluetoothDevice, string>> BluetoothDeviceChange { get; }
	}
}