using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace RoboPrinter.Core.ViewModels
{
	public class ServoCommandViewModel : ReactiveObject
	{
		private IBluetoothService BluetoothService { get; set; }

		//public SourceCache<float, short> ServoPositions { get; set; }
		[Reactive]
		public float Position { get; set; }

		public ReactiveCommand<float, Unit> UpdatePositionCommand { get; }

		public ServoCommandViewModel(IBluetoothService _bluetoothService = null)
		{
			BluetoothService = _bluetoothService ?? Locator.Current.GetService<IBluetoothService>();

			//ServoPositions = new SourceCache<float, short>();

			UpdatePositionCommand = ReactiveCommand.Create<float>((data) =>
			{
				//ServoPositions[data.Item1] = data.Item2;
				BluetoothService.SendPosition(0, data);
			});
		}
	}
}
