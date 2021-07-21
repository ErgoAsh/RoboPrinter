// unset

namespace RoboPrinter.Core.Models
{
	public abstract class PositionEventArgs
	{
		public short ServoId { get; set; }
		public float Position { get; set; }
	}

	public abstract class FeedbackEventArgs
	{
		public short ServoId { get; set; }
		public float Voltage { get; set; }
		public float? Position { get; set; }
		public bool IsTest { get; set; }
	}
}