// unset

using DynamicData;
using DynamicData.Kernel;
using RoboPrinter.Core.Models;
using System;
using System.Collections.Generic;

namespace RoboPrinter.Core.Interfaces
{
	public interface IServoService
	{
		public IObservable<IChangeSet<Servo, short>> ServoCollectionChange { get; }
		public Optional<Servo> GetServo(short id);
		public void UpdateServo(Servo servo);
		public void UpdateServos(IEnumerable<Servo> servos);

		public void SendPositions();
	}
}