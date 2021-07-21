// unset

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
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

		private Button ConnectButton => this.FindControl<Button>("ConnectButton");
		private Button TestConnectionButton => this.FindControl<Button>("TestConnectionButton");
		private DataGrid ConnectionDataGrid => this.FindControl<DataGrid>("ConnectionDataGrid");

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			ViewModel = new BluetoothViewModel();

			this.WhenActivated(disposable =>
			{
				this.BindCommand(ViewModel,
					viewModel => viewModel.ConnectCommand,
					view => view.ConnectButton).DisposeWith(disposable);

				this.BindCommand(ViewModel,
					viewModel => viewModel.TestConnectionCommand,
					view => view.TestConnectionButton).DisposeWith(disposable);

				this.Bind(ViewModel,
					viewModel => viewModel.SelectedItem,
					view => view.ConnectionDataGrid.SelectedItem).DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.Items,
					view => view.ConnectionDataGrid.Items).DisposeWith(disposable);
			});
		}
	}
}