﻿// unset

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.ViewModels;
using System.Reactive.Disposables;

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

		private TextBlock PingTextBlock =>
			this.FindControl<TextBlock>("PingTextBlock");

		private TextBlock ErrorTextBlock =>
			this.FindControl<TextBlock>("ErrorTextBlock");

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
					value => value == ConnectionState.NotConnected)
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
					viewModel => viewModel.LastPingMilliseconds,
					view => view.PingTextBlock.Text,
					value => value.HasValue ? $"Last ping: {value.Value}" : "")
					.DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.ConnectionError,
					view => view.ErrorTextBlock.Text)
					.DisposeWith(disposable);
			});

			// TODO add button: open bluetooth settings
			// var uri = new Uri(@"ms-settings:bluetooth");
			// var success = await Windows.System.Launcher.LaunchUriAsync(uri);
		}
	}
}