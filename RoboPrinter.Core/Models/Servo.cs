// unset

using ReactiveUI.Fody.Helpers;
using System;

namespace RoboPrinter.Core.Models
{
	public sealed record Servo
	{
		[Reactive]
		public short Id { get; set; }
		
		[Reactive]
		public float Position { get; set; }
		
		[Reactive]
		public float MinPositionConstraint { get; set; }
		
		[Reactive]
		public float MaxPositionConstraint { get; set; }
		
		[Reactive]
		public float FeedbackVoltage { get; set; }
		
		[Reactive]
		public float MinPositionVoltage { get; set; } // At boundary position
		
		[Reactive]
		public float MaxPositionVoltage { get; set; } // At boundary position

		public float GetPositionUsingFeedback()
		{
			float result = MinPositionConstraint + // TODO check validity
			               FeedbackVoltage / (MaxPositionVoltage - MinPositionVoltage) * MinPositionConstraint;
			
			return Math.Clamp(result, MinPositionConstraint, MaxPositionConstraint);
		}
	}
}