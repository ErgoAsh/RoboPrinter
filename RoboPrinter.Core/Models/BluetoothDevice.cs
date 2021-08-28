using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace RoboPrinter.Core.Models
{
	public class BluetoothDevice : ReactiveObject
	{
		[Reactive]
		public string Id { get; set; }

		[Reactive]
		public string Name { get; set; }

		[Reactive]
		public bool IsConnected { get; set; }

		public bool IsCorrupted()
		{
			return string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Name);
		}
	}
}