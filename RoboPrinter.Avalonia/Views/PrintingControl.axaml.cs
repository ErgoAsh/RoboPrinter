using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RoboPrinter.Avalonia.Views
{
	public class PrintingControl : UserControl
	{
		public PrintingControl()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}