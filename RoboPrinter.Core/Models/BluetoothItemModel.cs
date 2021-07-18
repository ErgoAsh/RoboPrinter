// unset

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace RoboPrinter.Core.Models
{
	public class BluetoothItemModel : ReactiveObject // TODO merge with BluetoothDevice?
	{
		[Reactive]
		public string Id { get; set; }
		
		[Reactive]
		public string Name { get; set; }
		
		[Reactive]
		public bool IsConnected { get; set; }
	}
}