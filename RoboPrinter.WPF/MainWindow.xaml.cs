using System.Windows;

namespace RoboPrinter.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			var bluetooth = new BluetoothConnection();
			bluetooth.Initialize();
		}
	}
}
