using ReactiveUI;
using RoboPrinter.Core.ViewModels;
using System;

namespace RoboPrinter.WPF.Controls
{
	/// <summary>
	/// Interaction logic for CommandControl.xaml
	/// </summary>
	public partial class CommandControl : ReactiveUserControl<ServoCommandViewModel>
	{
		public CommandControl()
		{
			InitializeComponent();

			ViewModel = new ServoCommandViewModel();

			this.WhenActivated((dispose) =>
			{
				this.BindCommand(ViewModel,
					viewModel => viewModel.UpdatePositionCommand,
					view => view.UpdateButton,
					viewModel => viewModel.Position);

				this.Bind(ViewModel,
					viewModel => viewModel.Position,
					view => view.ServoSlider1.Value);
			});
		}
	}
}
