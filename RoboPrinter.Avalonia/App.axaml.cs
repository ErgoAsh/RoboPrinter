using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RoboPrinter.Avalonia.Services;
using RoboPrinter.Avalonia.Views;
using RoboPrinter.Core.Interfaces;
using RoboPrinter.Core.Models;
using Splat;

namespace RoboPrinter.Avalonia
{
	public class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);

			var bluetoothService = new BluetoothService();
			Locator.CurrentMutable.RegisterLazySingleton(() => bluetoothService, typeof(IBluetoothService));
			Locator.CurrentMutable.RegisterLazySingleton(() => new ServoService(bluetoothService), typeof(IServoService));
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new MainWindow();
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}