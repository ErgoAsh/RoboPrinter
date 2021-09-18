using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RoboPrinter.Core.ViewModels
{
	public class
		ServoTestViewModel : ReactiveObject,
			IActivatableViewModel // TODO add ILockable
	{
		private readonly IServoService _servoService;

		public ServoTestViewModel(IServoService servoService = null)
		{
			_servoService = servoService ??
			                Locator.Current.GetService<IServoService>();

			UpdateRateMilliseconds = 500; // Default value

			Activator = new ViewModelActivator();
			this.WhenActivated(disposable =>
			{
				Items = new ObservableCollectionExtended<Servo>();
				PositionsCache = new List<KeyValuePair<short, float>>();

				_servoService.ServoCollectionChange
					.Sort(SortExpressionComparer<Servo>.Ascending(item =>
						item.Id))
					.Bind(Items)
					.Subscribe()
					.DisposeWith(disposable);

				//TODO update using: https://stackoverflow.com/a/33470045
				Observable
					.Interval(TimeSpan.FromMilliseconds(UpdateRateMilliseconds))
					.ObserveOn(RxApp.MainThreadScheduler)
					.Subscribe(_ =>
					{
						if (IsUpdatingContinuously)
						{
							_servoService.SendPositions();
						}
					})
					.DisposeWith(disposable);
			});

			UpdatePositionCommand = ReactiveCommand.Create(() =>
			{
				_servoService.UpdateServos(Items);
			});
		}

		[Reactive]
		public bool IsUpdatingContinuously { get; set; }

		[Reactive]
		public int UpdateRateMilliseconds { get; set; }

		public ObservableCollectionExtended<Servo> Items { get; set; }
		public List<KeyValuePair<short, float>> PositionsCache { get; set; }

		public ReactiveCommand<Unit, Unit> UpdatePositionCommand { get; }

		public ViewModelActivator Activator { get; }
	}
}