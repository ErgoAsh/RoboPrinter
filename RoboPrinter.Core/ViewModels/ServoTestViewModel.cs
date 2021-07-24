using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RoboPrinter.Core.ViewModels
{
	public class ServoTestViewModel : ReactiveObject, IActivatableViewModel // TODO add ILockable
	{
		private readonly IServoService _servoService;

		public ServoTestViewModel(IServoService servoService = null)
		{
			_servoService = servoService ?? Locator.Current.GetService<IServoService>();

			Activator = new ViewModelActivator();
			this.WhenActivated(disposable =>
			{
				Items = new ObservableCollectionExtended<Servo>();
				ItemsCache = new ObservableCollectionExtended<Servo>();

				_servoService.ServoCollectionChange
					.Sort(SortExpressionComparer<Servo>.Ascending(item => item.Id))
					.Bind(Items)
					.Subscribe()
					.DisposeWith(disposable);

				Observable
					.Interval(TimeSpan.FromMilliseconds(UpdateRateMilliseconds))
					.ObserveOn(RxApp.MainThreadScheduler)
					.Subscribe(_ =>
					{
						if (IsUpdatingContinuously)
						{
							IEnumerable<Servo> servosToUpdate;
							if (ItemsCache.Count == 0)
							{
								ItemsCache.AddRange(Items);
								servosToUpdate = ItemsCache;
							}
							else
							{
								servosToUpdate = Items.Except(ItemsCache, new ServoComparer());
							}
							ItemsCache = Items;
							
							_servoService.UpdateServos(servosToUpdate);
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
		public bool IsUpdatingContinuously { get; set; } = false;

		[Reactive]
		public int UpdateRateMilliseconds { get; set; } = 5000;
		
		public ObservableCollectionExtended<Servo> Items { get; set; }
		public ObservableCollectionExtended<Servo> ItemsCache { get; set; } // TODO change type?

		public ReactiveCommand<Unit, Unit> UpdatePositionCommand { get; }

		public ViewModelActivator Activator { get; }
	}
}