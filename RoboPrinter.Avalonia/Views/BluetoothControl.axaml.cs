// unset

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.ViewModels;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace RoboPrinter.Avalonia.Views
{
	public class BluetoothControl : ReactiveUserControl<BluetoothViewModel>
	{
		public BluetoothControl()
		{
			InitializeComponent();
		}

		private Button ConnectButton =>
			this.FindControl<Button>("ConnectButton");

		private Button DisconnectButton =>
			this.FindControl<Button>("DisconnectButton");

		private Button TestConnectionButton =>
			this.FindControl<Button>("TestConnectionButton");

		private ToggleSwitch ScanToggleSwitch =>
			this.FindControl<ToggleSwitch>("ScanToggleSwitch");

		private TextBlock PingTextBlock =>
			this.FindControl<TextBlock>("PingTextBlock");

		private TextBlock InfoTextBlock =>
			this.FindControl<TextBlock>("InfoTextBlock");

		private DataGrid ConnectionDataGrid =>
			this.FindControl<DataGrid>("ConnectionDataGrid");

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			ViewModel = new BluetoothViewModel();

			this.WhenActivated(disposable =>
			{
				this.BindCommand(ViewModel,
					viewModel => viewModel.ConnectCommand,
					view => view.ConnectButton) 
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.ConnectionState,
					view => view.ConnectButton.IsVisible,
					value => value != ConnectionState.Connected)
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel, 
					viewModel => viewModel.ConnectionState,
					view => view.DisconnectButton.IsVisible,
					value => value == ConnectionState.Connected)
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.ConnectionState,
					view => view.ConnectButton.IsEnabled,
					value => value != ConnectionState.InProgress)
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.ConnectionState,
					view => view.TestConnectionButton.IsEnabled,
					value => value != ConnectionState.InProgress)
					.DisposeWith(disposable);

				this.BindCommand(ViewModel,
					viewModel => viewModel.TestConnectionCommand,
					view => view.TestConnectionButton)
					.DisposeWith(disposable);

				this.Bind(ViewModel,
					viewModel => viewModel.SelectedItem,
					view => view.ConnectionDataGrid.SelectedItem)
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.Items,
					view => view.ConnectionDataGrid.Items)
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.InfoMessage,
					view => view.InfoTextBlock.Text)
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel, 
					viewModel => viewModel.InfoColorString,
					view => view.InfoTextBlock.Foreground,
					value =>
					{
						if (value == null)
							return Brushes.White;

						return value switch
						{
							"red" => Brushes.Red,
							"gray" => Brushes.LightGray,
							"white" => Brushes.White,
							_ => throw new InvalidOperationException("Invalid color string"),
						};
					}).DisposeWith(disposable);

				Observable.FromEventPattern<RoutedEventArgs>(
					evt => ScanToggleSwitch.Checked += evt,
					evt => ScanToggleSwitch.Checked -= evt)
					.Subscribe(Observer.Create<EventPattern<RoutedEventArgs>>(
						_ => ViewModel?.SetScanningMode(true)
					)).DisposeWith(disposable);

				Observable.FromEventPattern<RoutedEventArgs>(
						evt => ScanToggleSwitch.Unchecked += evt,
						evt => ScanToggleSwitch.Unchecked -= evt)
					.Subscribe(Observer.Create<EventPattern<RoutedEventArgs>>(
						_ => ViewModel?.SetScanningMode(false)
					)).DisposeWith(disposable);
			});

			// TODO add button: open bluetooth settings
			// var uri = new Uri(@"ms-settings:bluetooth");
			// var success = await Windows.System.Launcher.LaunchUriAsync(uri);
		}
	}
}