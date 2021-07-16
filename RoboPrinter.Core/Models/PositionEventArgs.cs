// unset

namespace RoboPrinter.Core.Models
{
	public abstract class PositionEventArgs
	{
		public short ServoId { get; set; }
		public float Position { get; set; }
	}
}