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
		public float VoltageInstantaneous { get; set; }
		
		[Reactive]
		public float VoltageMin { get; set; }
		
		[Reactive]
		public float VoltageMax { get; set; }
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