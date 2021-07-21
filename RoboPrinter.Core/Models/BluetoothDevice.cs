using ReactiveUI.Fody.Helpers;

namespace RoboPrinter.Core.Models
{
	public class BluetoothDevice
	{
		[Reactive]
		public string Id { get; set; }

		[Reactive]
		public string Name { get; set; }

		[Reactive]
		public bool IsConnected { get; set; }
	}
}