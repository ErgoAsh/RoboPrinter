﻿// unset

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
		private bool _isConnected;

		public BluetoothViewModel(IBluetoothService bluetoothService = null)
		{
			_bluetoothService = bluetoothService ?? Locator.Current.GetService<IBluetoothService>();

			Activator = new ViewModelActivator();
			this.WhenActivated(disposable =>
			{
				Items = new ObservableCollectionExtended<BleServiceItem>();

				_bluetoothService.StartDeviceDiscovery();
				
				_bluetoothService.WhenBluetoothDevicesChanged
					.AsObservable()
					.ObserveOn(RxApp.MainThreadScheduler)
					.Bind(Items)
					.Subscribe()
					.DisposeWith(disposable);

				_bluetoothService.WhenConnectionStateChanged
					.Subscribe(newState => ConnectionState = newState)
					.DisposeWith(disposable);

				_bluetoothService.WhenInfoMessageChanged
					.Subscribe(message => ConnectionError = message.Message)
					.DisposeWith(disposable);

				//this.WhenAnyValue(vm => vm._bluetoothService.IsConnectionInProgress).
				//	.Subscribe().DisposeWith(disposable);
			});

			ConnectCommand = ReactiveCommand.Create(() =>
			{
				_bluetoothService.Connect(SelectedItem); // TODO Handle error?
			});

			TestConnectionCommand = ReactiveCommand.Create(() =>
			{
				_bluetoothService.TestResponseTime(TimeSpan.FromSeconds(5), 
					time =>
					{
						LastPingMilliseconds = time;
					},
					error =>
					{
						// TODO check and refactor
						ConnectionError = error.Message;
					});
			});
		}

		[Reactive]
		public BleServiceItem SelectedItem { get; set; }

		[Reactive]
		public long? LastPingMilliseconds { get; set; }
		
		[Reactive]
		public string ConnectionError { get; set; }

		[Reactive]
		public ConnectionState ConnectionState { get; set; }

		public ObservableCollectionExtended<BleServiceItem> Items { get; private set; }

		public ReactiveCommand<Unit, Unit> TestConnectionCommand { get; }
		public ReactiveCommand<Unit, Unit> ConnectCommand { get; }

		public ViewModelActivator Activator { get; }
	}
}