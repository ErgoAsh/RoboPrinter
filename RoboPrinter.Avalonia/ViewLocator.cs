using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;
using System;

namespace RoboPrinter.Avalonia
{
	public class ViewLocator : IDataTemplate
	{
		public bool SupportsRecycling => false;

		public IControl Build(object data)
		{
			string name = data.GetType().FullName!.Replace("ViewModel", "View");
			Type? type = Type.GetType(name);

			if (type != null)
			{
				return (Control)Activator.CreateInstance(type)!;
			}

			return new TextBlock {Text = "Not Found: " + name};
		}

		public bool Match(object data)
		{
			return data is ReactiveObject;
		}
	}
}