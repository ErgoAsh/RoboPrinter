using RoboPrinter.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Interfaces
{
	public interface IBluetoothService
	{
		public Task<Task> Connect(BluetoothDevice device);
		public void Disconnect();
		public void RefreshDeviceList();

		public void SendPosition(short servoId, float position);
		//public void ReceiveFeedbackLoop(data reader)

		public IEnumerable<BluetoothDevice> GetAvailableBluetoothDevices();

		void OnPositionSentEvent(PositionEventArgs e);
		void OnFeedbackReceivedEvent(PositionEventArgs e);

		public event EventHandler<PositionEventArgs> PositionSent;
		public event EventHandler<PositionEventArgs> FeedbackReceived;
	}
}
