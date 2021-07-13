using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RoboPrinter.WPF
{
	public class BluetoothConnection
	{
		private RfcommDeviceService _service;
		private StreamSocket _socket;
		private DataWriter _writer;

		public async void Initialize()
		{
			// Enumerate devices with the object push service
			DeviceInformationCollection services = await DeviceInformation
				.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

			if (services.Count > 0)
			{
				// Initialize the target Bluetooth BR device
				RfcommDeviceService service = await RfcommDeviceService.FromIdAsync(services[0].Id);

				// Check that the service meets this App's minimum requirement
				if (SupportsProtection(service))
				{
					_service = service;
					_socket = new StreamSocket();

					await _socket.ConnectAsync(
						_service.ConnectionHostName,
						_service.ConnectionServiceName, 
						SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication
					);

					_writer = new DataWriter(_socket.OutputStream);

					_writer.WriteString("ABC\n");

					await _writer.StoreAsync();
					await _writer.FlushAsync();
				}
			}
		}

		// This App requires a connection that is encrypted but does not care about
		// whether it's authenticated.
		public static bool SupportsProtection(RfcommDeviceService service)
		{
			switch (service.ProtectionLevel)
			{
				case SocketProtectionLevel.PlainSocket:
					if (service.MaxProtectionLevel is SocketProtectionLevel
							.BluetoothEncryptionWithAuthentication or SocketProtectionLevel
							.BluetoothEncryptionAllowNullAuthentication)
					{
						// The connection can be upgraded when opening the socket so the
						// App may offer UI here to notify the user that Windows may
						// prompt for a PIN exchange.
						return true;
					}
					else
					{
						// The connection cannot be upgraded so an App may offer UI here
						// to explain why a connection won't be made.
						return false;
					}
				case SocketProtectionLevel.BluetoothEncryptionWithAuthentication:
					return true;
				case SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication:
					return true;
				default:
					break;
			}
			return false;
		}
	}
}
