using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace RoboPrinter.Avalonia.Views
{
	public class GroupBox : UserControl
	{
		public static readonly StyledProperty<string> HeaderProperty =
			AvaloniaProperty.Register<GroupBox, string>(nameof(Header));
		
		public static readonly StyledProperty<IBrush> HeaderBackgroundProperty =
			AvaloniaProperty.Register<GroupBox, IBrush>(nameof(HeaderBackground));
		
		public string Header
		{
			get => GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}
		
		public IBrush HeaderBackground
		{
			get => GetValue(HeaderBackgroundProperty);
			set => SetValue(HeaderBackgroundProperty, value);
		}

		public GroupBox()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}