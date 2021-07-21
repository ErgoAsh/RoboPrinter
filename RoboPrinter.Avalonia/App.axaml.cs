using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RoboPrinter.Avalonia.Services;
using RoboPrinter.Avalonia.Views;
using RoboPrinter.Core.Interfaces;
using Splat;

namespace RoboPrinter.Avalonia
{
	public class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);

			Locator.CurrentMutable.RegisterLazySingleton(() => new BluetoothService(), typeof(IBluetoothService));
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