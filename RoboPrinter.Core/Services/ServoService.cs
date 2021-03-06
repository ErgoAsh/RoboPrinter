// unset

using DynamicData;
using DynamicData.Kernel;
using RoboPrinter.Core.Interfaces;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboPrinter.Core.Models
{
	public class ServoService : IServoService
	{
		private readonly IBluetoothService _bluetoothService;
		private readonly SourceCache<Servo, short> _servos;

		public ServoService(IBluetoothService bluetoothService)
		{
			_bluetoothService = bluetoothService ??
			                    Locator.Current.GetService<IBluetoothService>();

			_servos = new SourceCache<Servo, short>(item => item.Id);
			SetupServos();
		}

		public Optional<Servo> GetServo(short id)
		{
			return _servos.Lookup(id);
		}

		public void UpdateServos(IEnumerable<Servo> servos)
		{
			_servos.AddOrUpdate(servos);
			SendPositions();
		}

		public async void SendPositions()
		{
			// TODO check servo array length
			var bytes = _servos.Items
				.Select(item => item.InstantaneousPulseWidth)
				.Select(item => BitConverter.GetBytes(item).Reverse())
				.SelectMany(item => item)
				.ToArray();

			await _bluetoothService.SendDataAsync(bytes, error =>
			{
				// TODO print error
			});
		}

		public IObservable<IChangeSet<Servo, short>> ServoCollectionChange =>
			_servos.Connect();

		private void SetupServos()
		{
			for (short i = 0; i < 5; i++)
			{
				_servos.AddOrUpdate(new Servo
				{
					Id = i,
					InstantaneousPulseWidth = 1500,
					MinPulseWidth = 500,
					MaxPulseWidth = 2500,
					ZeroPulseWidth = 1500
				});
			}
		}
	}
}