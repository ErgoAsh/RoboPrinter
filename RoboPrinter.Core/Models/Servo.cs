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

		/**
		 * <summary>
		 * Z-axis offset length. Denavit-Hartenberg parameter denoted as <c>d</c>
		 * </summary>
		 */
		[Reactive]
		public float ZOffset { get; set; }

		/**
		 * <summary>
		 * X-axis offset length. Denavit-Hartenberg parameter denoted as <c>r</c>
		 * </summary>
		 */
		[Reactive]
		public float XOffset { get; set; }

		/**
		 * <summary>
		 * Rotation angle around z-axis. Denavit-Hartenberg parameter denoted as <c>&theta;</c>
		 * </summary>
		 */
		[Reactive]
		public float ZAngle { get; set; }
		
		/**
		 * <summary>
		 * Rotation angle around x-axis. Denavit-Hartenberg parameter denoted as <c>&alpha;</c>
		 * </summary>
		 */
		[Reactive]
		public float XAngle { get; set; }

		/**
		 * <summary>
		 * Rotation angle around x-axis. Denavit-Hartenberg parameter denoted as <c>&alpha;</c>.
		 * It is added to the <see cref="XAngle"/> before any movement calculation is done.
		 * </summary>
		 */
		[Reactive]
		public float Position { get; set; }
		
		[Reactive]
		public float MinPositionConstraint { get; set; }

		[Reactive]
		public float MaxPositionConstraint { get; set; }

		[Reactive]
		public float MinPulseWidth { get; set; }

		[Reactive]
		public float MaxPulseWidth { get; set; }

		[Reactive]
		public float MinVoltage { get; set; }
		
		[Reactive]
		public float MaxVoltage { get; set; }

		[Reactive]
		public float InstantaneousVoltage { get; set; }

		[Reactive]
		public float InstantaneousPulseWidth { get; set; }

		public void IncrementPosition()
		{
			Position = Math.Clamp(Position + 1, MinPositionConstraint, MaxPositionConstraint);
		}

		public void DecrementPosition()
		{
			Position = Math.Clamp(Position - 1, MinPositionConstraint, MaxPositionConstraint);
		}

		public float GetPulseWidth()
		{
			return Map(Position, 0, 180, 100, 2000);
		}

		private static float EPSILON = 1e-12F;
		private static float Map(float value,
			float startSource, float endSource,
			float startTarget, float endTarget)
		{
			if (Math.Abs(endSource - startSource) < EPSILON)
			{
				throw new ArithmeticException("/ 0");
			}

			float offset = startTarget;
			float ratio = (endTarget - startTarget) / (endSource - startSource);
			return ratio * (value - startSource) + offset;
		}
	}

	public class ServoComparer : IEqualityComparer<Servo>
	{
		bool IEqualityComparer<Servo>.Equals(Servo x, Servo y)
		{
			if (x == null || y == null)
			{
				throw new ArgumentException("Object is null");
			}

			return x.Id.Equals(y.Id) &&
			       x.Position.Equals(y.Position) &&
			       x.MinPositionConstraint.Equals(y.MinPositionConstraint) &&
			       x.MaxPositionConstraint.Equals(y.MaxPositionConstraint);
		}

		int IEqualityComparer<Servo>.GetHashCode(Servo obj)
		{
			if (ReferenceEquals(obj, null))
			{
				return 0;
			}

			return obj.Id.GetHashCode() + (int)obj.Position;
		}
	}
}