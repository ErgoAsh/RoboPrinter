using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using Splat;
using System.Reactive;

namespace RoboPrinter.Core.ViewModels
{
	public class ServoTestViewModel : ReactiveObject
	{
		private IBluetoothService BluetoothService { get; set; }

		//public SourceCache<float, short> ServoPositions { get; set; }
		[Reactive] public float Position { get; set; }

		public ReactiveCommand<Unit, Unit> UpdatePositionCommand { get; }

		public ServoTestViewModel(IBluetoothService bluetoothService = null)
		{
			BluetoothService = bluetoothService ?? Locator.Current.GetService<IBluetoothService>();

			//ServoPositions = new SourceCache<float, short>();

			UpdatePositionCommand = ReactiveCommand.Create(() =>
			{
				//ServoPositions[data.Item1] = data.Item2;
				BluetoothService.SendPosition(0, Position);
			});
		}
	}
}
