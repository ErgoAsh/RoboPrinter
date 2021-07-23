// unset

using ReactiveUI.Fody.Helpers;
using System;

namespace RoboPrinter.Core.Models
{
	public sealed record Servo
	{
		[Reactive]
		public short Id { get; init; }

		[Reactive]
		public float Position { get; init; }

		[Reactive]
		public float MinPositionConstraint { get; set; }

		[Reactive]
		public float MaxPositionConstraint { get; set; }

		[Reactive]
		public float? FeedbackVoltage { get; set; }

		[Reactive]
		public float? MinPositionVoltage { get; set; } // At boundary position

		[Reactive]
		public float? MaxPositionVoltage { get; set; } // At boundary position

		public float? GetPositionUsingFeedback()
		{
			if (FeedbackVoltage is { } feedbackVoltage &&
			    MaxPositionConstraint is {} maxPositionConstraint &&
			    MinPositionConstraint is {} minPositionConstraint)
			{
				float result = minPositionConstraint + // TODO check validity
				               (feedbackVoltage / (maxPositionConstraint - minPositionConstraint) * minPositionConstraint);

				return Math.Clamp(result, MinPositionConstraint, MaxPositionConstraint);
			}

			return null;
		}
	}
}