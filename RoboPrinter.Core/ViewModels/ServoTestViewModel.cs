using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RoboPrinter.Core.ViewModels
{
	public class ServoTestViewModel : ReactiveObject, IActivatableViewModel
	{
		private IServoService _servoService;
		
		public ServoTestViewModel(IServoService servoService = null)
		{
			_servoService = servoService ?? Locator.Current.GetService<IServoService>();

			Activator = new ViewModelActivator();
			this.WhenActivated(disposable =>
			{
				ServoCollection = new ObservableCollectionExtended<Servo>();

				_servoService
					.GetServoCollectionObservable()
					.AsObservable()
					.Sort(SortExpressionComparer<Servo>.Ascending(item => item.Id))
					.Bind(ServoCollection)
					.Subscribe()
					.DisposeWith(disposable);
			});
				
			UpdatePositionCommand = ReactiveCommand.Create(() =>
			{
				foreach (Servo servo in ServoCollection)
				{
					_servoService.SendPosition(servo.Id, servo.Position);
				}
			});
		}

		public ObservableCollectionExtended<Servo> ServoCollection { get; set; }

		public ReactiveCommand<Unit, Unit> UpdatePositionCommand { get; }
		
		public ViewModelActivator Activator { get; }
	}
}