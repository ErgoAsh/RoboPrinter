// unset

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
	public class BluetoothViewModel : ReactiveObject, IActivatableViewModel
	{
		private readonly IBluetoothService _bluetoothService;

		public BluetoothViewModel(IBluetoothService bluetoothService = null)
		{
			_bluetoothService = bluetoothService ?? Locator.Current.GetService<IBluetoothService>();

			Activator = new ViewModelActivator();
			this.WhenActivated(disposable =>
			{
				Items = new ObservableCollectionExtended<BluetoothDevice>();

				_bluetoothService
					.GetBluetoothDevicesObservable()
					.AsObservable()
					.Bind(Items)
					.Subscribe()
					.DisposeWith(disposable);

				this.WhenAnyValue(vm => vm.SelectedItem)
					.Where(item => item != null)
					.Subscribe(item =>
					{
						Console.WriteLine(item.Id);
					})
					.DisposeWith(disposable);
			});
			
			ConnectCommand = ReactiveCommand.Create(() =>
			{
				_bluetoothService.Connect(SelectedItem);
			});
			
			TestConnectionCommand = ReactiveCommand.Create(() =>
			{
				_bluetoothService.TestConnection(SelectedItem, timeInterval =>
				{
					
				});
			});
		}

		[Reactive]
		public BluetoothDevice SelectedItem { get; set; }

		public ObservableCollectionExtended<BluetoothDevice> Items { get; private set; }

		public ReactiveCommand<Unit, Unit> TestConnectionCommand { get; set; }
		public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }

		public ViewModelActivator Activator { get; }
	}
}