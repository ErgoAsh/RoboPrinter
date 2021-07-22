// unset

using DynamicData;
using RoboPrinter.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RoboPrinter.Core.Interfaces
{
	public interface IServoService
	{
		public Servo GetServo(short id);
		public void UpdateServo(Servo servo);
		public void UpdateServos(IEnumerable<Servo> servos);

		public void SendPosition(short id, float position);
		
		public IObservable<IChangeSet<Servo>> GetServoCollectionObservable();
	}
}