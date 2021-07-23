// unset

using ReactiveUI.Fody.Helpers;

namespace RoboPrinter.Core.Models
{
	public abstract class Feedback
	{
		[Reactive]
		public short ServoId { get; set; }
		
		[Reactive]
		public float Voltage { get; set; }
		
		[Reactive]
		public float? Position { get; set; }
		
		[Reactive]
		public bool IsTest { get; set; }
	}
}