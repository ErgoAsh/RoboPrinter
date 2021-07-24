// unset

using DynamicData;
using DynamicData.Kernel;
using RoboPrinter.Core.Interfaces;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoboPrinter.Core.Models
{
	public class ServoService : IServoService
	{
		private readonly IBluetoothService _bluetoothService;
		private readonly SourceCache<Servo, short> _servos;

		public ServoService(IBluetoothService bluetoothService)
		{
			_bluetoothService = bluetoothService ?? Locator.Current.GetService<IBluetoothService>();

			_servos = new SourceCache<Servo, short>(item => item.Id);
			SetupServos();
		}

		private void SetupServos()
		{
			for (short i = 0; i < 5; i++)
			{
				_servos.AddOrUpdate(new Servo
				{
					Id = i, 
					Position = 90,
					MinPositionConstraint = 60,
					MaxPositionConstraint = 90
				});
			}
		}

		public Optional<Servo> GetServo(short id)
		{
			return _servos.Lookup(id);
		}

		public void UpdateServo(Servo servo)
		{
			_servos.AddOrUpdate(servo);
			SendPosition(servo.Id, servo.Position);
		}

		public void UpdateServos(IEnumerable<Servo> servos)
		{
			_servos.AddOrUpdate(servos);
			foreach (Servo servo in servos)
			{
				SendPosition(servo.Id, servo.Position);
			}
		}

		public void SendPosition(short id, float position)
		{
			if (id is < 0 or > 5)
			{
				return; // TODO throw new exception, read 5 from somewhere
			}

			if (position is < 0 or > 180) // TODO change to support 360 deg servo
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder()
				.Append((char)(id + 65))
				.Append(position)
				.Append('\n');

			_bluetoothService.SendData(stringBuilder.ToString());
		}

		public IObservable<IChangeSet<Servo, short>> ServoCollectionChange => _servos.Connect();
	}
}