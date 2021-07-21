using DynamicData;
using RoboPrinter.Core.Models;
using System;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Interfaces
{
	public interface IBluetoothService
	{
		public Task<Task> Connect(BluetoothDevice device);
		public void Disconnect();

		public void TestConnection(Action<int> callback);
		public void SendPosition(short servoId, float position);
		public void ReadFeedbackRecursive();

		public IObservable<IChangeSet<BluetoothDevice, string>> GetBluetoothDevicesObservable();

		void OnPositionSentEvent(PositionEventArgs e);
		void OnFeedbackReceivedEvent(FeedbackEventArgs e);

		public event EventHandler<PositionEventArgs> PositionSent;
		public event EventHandler<FeedbackEventArgs> FeedbackReceived;
	}
}