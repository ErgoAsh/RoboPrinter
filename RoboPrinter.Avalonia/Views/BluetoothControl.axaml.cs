// unset

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RoboPrinter.Avalonia.Views
{
	public class BluetoothControl : UserControl
	{
		public BluetoothControl()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}