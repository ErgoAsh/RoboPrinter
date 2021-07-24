// unset

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;

namespace RoboPrinter.Core.Models
{
	public sealed class Servo : ReactiveObject
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
			// Check if all of these properties are not null
			if (FeedbackVoltage is {} feedbackVoltage &&
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
	
	public class ServoComparer : IEqualityComparer<Servo>   
	{
		bool IEqualityComparer<Servo>.Equals(Servo x, Servo y)
		{
			if (x == null || y == null)
				throw new ArgumentException("Object is null");
			
			return x.Id.Equals(y.Id) && 
			       x.Position.Equals(y.Position) && 
			       x.MinPositionConstraint.Equals(y.MinPositionConstraint) &&
			       x.MaxPositionConstraint.Equals(y.MaxPositionConstraint);        
		}

		int IEqualityComparer<Servo>.GetHashCode(Servo obj)
		{
			if (Object.ReferenceEquals(obj, null))
				return 0;               

			return obj.Id.GetHashCode() + (int) obj.Position;       
		}
	}
}