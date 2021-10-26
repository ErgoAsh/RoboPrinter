using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboPrinter.Core.Models
{
	public enum MessageType
	{
		Information,
		Error,
		Ping
	}

	public class InformationMessage
	{
		public MessageType MessageType { get; set; }
		public string Message { get; set; }	
	}
}
