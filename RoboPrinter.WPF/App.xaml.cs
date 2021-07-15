using ReactiveUI;
using RoboPrinter.Core;
using Splat;
using System.Reflection;
using System.Windows;

namespace RoboPrinter.WPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
			Locator.CurrentMutable.RegisterConstant(new BluetoothService(), typeof(IBluetoothService));

			InitializeComponent();
		}
	}
}
