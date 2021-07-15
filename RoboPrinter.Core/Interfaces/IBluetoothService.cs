using System;
using System.Collections.Generic;

namespace RoboPrinter.Core
{
	public class PositionEventArgs
	{
		public short ServoID { get; set; }
		public float Position { get; set; }
	}

	public interface IBluetoothService
	{
		public void Connect(BluetoothDevice device);
		public void Disconnect();
		public void RefreshDeviceList();

		public void SendPosition(short servoID, float position);
		//public void ReceiveFeedbackLoop(data reader)

		public IEnumerable<BluetoothDevice> GetAvilableBluetoothDevices();

		protected void OnPositionSentEvent(PositionEventArgs e);
		protected void OnFeedbackReciviedEvent(PositionEventArgs e);

		public event EventHandler<PositionEventArgs> PositionSent;
		public event EventHandler<PositionEventArgs> FeedbackRecivied;
	}
}
