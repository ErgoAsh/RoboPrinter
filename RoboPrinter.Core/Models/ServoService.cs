// unset

using DynamicData;
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
		public ServoService(IBluetoothService bluetoothService)
		{
			_bluetoothService = bluetoothService ?? Locator.Current.GetService<IBluetoothService>();
		}
		
		public Servo GetServo(short id)
		{
			throw new NotImplementedException();
		}

		public void UpdateServo(Servo servo)
		{
			throw new NotImplementedException();
		}

		public void UpdateServos(IEnumerable<Servo> servos)
		{
			throw new NotImplementedException();
		}

		public void SendPosition(short id, float position)
		{
			if (id is < 0 or > 5)
			{
				return; // TODO throw new exception, read 5 from somewhere
			}

			if (position is < 0 or > 180)
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder()
				.Append((char)(id + 65))
				.Append(position)
				.Append('\n');
			
			_bluetoothService.SendData(stringBuilder.ToString());
		}

		public IObservable<IChangeSet<Servo, short>> GetServoCollectionObservable()
		{
			throw new NotImplementedException();
		}

		public IObservable<ServoEventArgs> ServoUpdate { get; }
	}
}