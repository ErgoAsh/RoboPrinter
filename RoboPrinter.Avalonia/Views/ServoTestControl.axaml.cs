// unset

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.ReactiveUI;
using DynamicData;
using ReactiveUI;
using RoboPrinter.Core.ViewModels;
using System.Collections.ObjectModel;
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

				this.Bind(ViewModel,
					viewModel => viewModel.Position,
					view => view.TempSliderButton.Value).DisposeWith(disposable);

				this.OneWayBind(ViewModel,
					viewModel => viewModel.Position,
					view => view.TempSliderTextBlock.Text).DisposeWith(disposable);
			});
		}
	}
}