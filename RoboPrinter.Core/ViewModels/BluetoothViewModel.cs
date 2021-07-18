// unset

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RoboPrinter.Core.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;

namespace RoboPrinter.Core.ViewModels
{
	public class BluetoothViewModel : ReactiveObject
	{
		[Reactive] public BluetoothItemModel SelectedItem { get; set; }
		public ObservableCollection<BluetoothItemModel> Items { get; }

		public ReactiveCommand<Unit, Unit> TestConnectionCommand { get; set; }
		public ReactiveCommand<Unit, Unit> ConnectCommand { get; set; }

		public BluetoothViewModel()
		{
			var temp = new BluetoothItemModel {Id = "aa", Name = "bb", IsConnected = false};
			Items = new ObservableCollection<BluetoothItemModel>(new List<BluetoothItemModel> {temp});
		}
	}
}