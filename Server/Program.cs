using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			int NotesCount = 0;

			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.InputEncoding = System.Text.Encoding.Unicode;

			IPHostEntry ipHost = Dns.GetHostEntry("localhost");
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);


			Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


			try
			{
				sListener.Bind(ipEndPoint);
				sListener.Listen(10);


				while (true)
				{
					Console.WriteLine("Wait for connection {0}", ipEndPoint);


					Socket handler = sListener.Accept();
					string data = null;

					while (true)
					{
						byte[] bytes = new byte[1024];
						int bytesRec = handler.Receive(bytes);

						if (bytesRec > 0)
						{
							data = Encoding.UTF8.GetString(bytes, 0, bytesRec);
							Console.WriteLine("Received: " + data + "\n");

							if (data.IndexOf("<TheEnd>") > -1)
							{
								Console.WriteLine("Client Disconnected.");
								break;
							}
							else if (data.IndexOf("<GetData>") > -1)
							{
								string getNotes = "Notes for all time: " + NotesCount;
								byte[] msg = Encoding.UTF8.GetBytes(getNotes);
								handler.Send(msg);
								Console.WriteLine("Notes count: " + NotesCount);
								continue;
							}
							else
							{
								NotesCount++;
								string reply = "Sended";
								byte[] msg = Encoding.UTF8.GetBytes(reply);
								handler.Send(msg);
							}
						}

					}

					handler.Shutdown(SocketShutdown.Both);
					handler.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				Console.ReadLine();
			}
		}
	}
}

