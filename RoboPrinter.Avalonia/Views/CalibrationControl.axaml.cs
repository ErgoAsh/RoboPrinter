// unset

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RoboPrinter.Avalonia.Views
{
	public class CalibrationControl : UserControl
	{
		public CalibrationControl()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}