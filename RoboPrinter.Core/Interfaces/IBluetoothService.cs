using RoboPrinter.Core.Models;
using System;
using System.Collections.Generic;

namespace RoboPrinter.Core.Interfaces
{
	public interface IBluetoothService
	{
		public void Connect(BluetoothDevice device);
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
