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
		private CheckBox UpdateCheckBox => this.FindControl<CheckBox>("UpdateCheckBox");
		private TextBox UpdateRateTextBox => this.FindControl<TextBox>("UpdateRateTextBox");
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
					viewModel => viewModel.IsUpdatingContinuously,
					view => view.UpdateButton.IsEnabled,
					value => !value);
				
				this.OneWayBind(ViewModel,
					viewModel => viewModel.Items,
					view => view.TableGrid.Items).DisposeWith(disposable);

				this.Bind(ViewModel, // TODO add validation (ReactiveUI.Validation)
					viewModel => viewModel.UpdateRateMilliseconds,
					view => view.UpdateRateTextBox.Text,
					intValue => intValue.ToString(),
					stringValue => int.TryParse(stringValue, out int value) ? value : 500);
				
				this.Bind(ViewModel,
					viewModel => viewModel.IsUpdatingContinuously,
					view => view.UpdateCheckBox.IsChecked);
			});
		}
	}
}