using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace RoboPrinter.Core.Models
{
	public class BleServiceItem : ReactiveObject
	{
		[Reactive]
		public ulong BluetoothAddress { get; init; }

		[Reactive]
		public string ServerId { get; init; }

		[Reactive]
		public string Name { get; init; }
		
		[Reactive]
		public int SignalStrength { get; set; }

		[Reactive]
		public bool IsConnected { get; set; } = false;

	}
}