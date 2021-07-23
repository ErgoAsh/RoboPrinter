// unset

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using RoboPrinter.Core.ViewModels;
using System.Reactive.Disposables;

namespace RoboPrinter.Avalonia.Views
{
	public class ServoTestControl : ReactiveUserControl<ServoTestViewModel>
	{
		public ServoTestControl()
		{
			InitializeComponent();
		}

		private Button UpdateButton => this.FindControl<Button>("UpdateButton");
		private DataGrid TableGrid => this.FindControl<DataGrid>("TableGrid");

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			ViewModel = new ServoTestViewModel();

			this.WhenActivated(disposable =>
			{
				this.BindCommand(ViewModel,
					viewModel => viewModel.UpdatePositionCommand,
					view => view.UpdateButton).DisposeWith(disposable);
				
				this.OneWayBind(ViewModel,
					viewModel => viewModel.ServoCollection,
					view => view.TableGrid.Items).DisposeWith(disposable);
			});
		}
	}
}