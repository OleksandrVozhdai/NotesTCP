using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media.Animation;

namespace NotesTCP
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 

	public partial class MainWindow : Window
	{
		private static IPHostEntry host = Dns.GetHostEntry("localhost");
		private static IPAddress ipAddress = host.AddressList[0];
		private static IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
		private static Socket sender = new Socket(ipAddress.AddressFamily,
					SocketType.Stream, ProtocolType.Tcp);

		private Dictionary<Button, Notes> notesKeyDict = new Dictionary<Button,Notes>();
		private int buttonCount = 0;

		private List<Notes> noteList = new List<Notes>();

		public MainWindow()
		{
			InitializeComponent();

			MainMenuCanvas.Visibility = Visibility.Visible;
			AddNoteCanvas.Visibility = Visibility.Hidden;
			ReadNotesCanvas.Visibility = Visibility.Hidden;
			SettingsCanvas.Visibility = Visibility.Hidden;
		}

		private void NotesButtonClick(object sender, RoutedEventArgs e)
		{
			MainMenuCanvas.Visibility = Visibility.Visible;
			SettingsCanvas.Visibility = Visibility.Hidden;

			DoubleAnimation scaleNotesButtonAnim = new DoubleAnimation()
			{ 
				From = 1,
				To = 0.8,
				Duration = TimeSpan.FromSeconds(0.1)
			};
		}

		private void PortSettingsButtonClick(object sender, RoutedEventArgs e)
		{
			SettingsCanvas.Visibility = Visibility.Visible;
			MainMenuCanvas.Visibility = Visibility.Hidden;
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				try
				{
					this.DragMove();
				}
				catch (Exception InvalidOperationException)
				{
					Console.WriteLine(InvalidOperationException.Message);
				}
			}
		}

		private void AddNoteButtonClick(object sender, RoutedEventArgs e)
		{
			MainMenuCanvas.Visibility = Visibility.Hidden;
			AddNoteCanvas.Visibility = Visibility.Visible;
		}

		private void AddNoteConfirmClick(object sender, RoutedEventArgs e)
		{
			MainMenuCanvas.Visibility = Visibility.Visible;
			AddNoteCanvas.Visibility = Visibility.Hidden;

			if (buttonCount < 5)
			{
				String buttonText = NoteNameText.Text == "New note " ?
					NoteNameText.Text + Convert.ToString(buttonCount + 1) :
					NoteNameText.Text;

				noteList.Add(new Notes(buttonText, NoteDescriptionText.Text));

				Button newNote = new Button
				{
					Content = noteList[buttonCount].Name,
					Width = 340,
					Height = 55,
					FontSize = 18,

				};

				newNote.Style = (Style)FindResource("MenuButton");

				double topOffset = 160 + buttonCount * 65;
				Canvas.SetLeft(newNote, 10);
				Canvas.SetTop(newNote, topOffset);
				MainMenuCanvas.Children.Add(newNote);

				
				SendMessage(NoteNameText.Text, NoteDescriptionText.Text, "");

				NoteNameText.Text = "New note ";
				NoteDescriptionText.Text = "Note description";

				notesKeyDict[newNote] = noteList[buttonCount];

				newNote.Click += (s, args) => ReadNote(newNote);

				buttonCount++;
			}

			if (buttonCount == 5)
				AddNoteButton.Visibility = Visibility.Hidden;
			else AddNoteButton.Visibility = Visibility.Visible;
		}

		

		private void ReadNote(Button index)
		{ 
			ReadNotesCanvas.Visibility = Visibility.Visible;
			MainMenuCanvas.Visibility = Visibility.Hidden;

			NoteNameText_Read.Text = notesKeyDict[index].Name;
			NoteDescriptionText_Read.Text = notesKeyDict[index].Description;

			DeleteNoteButton.Click += (s, args) => DeleteNode(index);
		}

		private void DeleteNode(Button index)
		{
			ReadNotesCanvas.Visibility = Visibility.Hidden;
			MainMenuCanvas.Visibility = Visibility.Visible;

			if (notesKeyDict.ContainsKey(index))
			{
				Notes tempNote = notesKeyDict[index];

				noteList.Remove(tempNote);
				notesKeyDict.Remove(index);
				MainMenuCanvas.Children.Remove(index);

				buttonCount--;

				int currentIndex = 0;
				foreach (var button in notesKeyDict.Keys.ToList())
				{
					double topOffset = 160 + currentIndex * 65;
					Canvas.SetTop(button, topOffset);
					currentIndex++;
				}
			}

			if (buttonCount == 5)
				AddNoteButton.Visibility = Visibility.Hidden;
			else AddNoteButton.Visibility = Visibility.Visible;
		}

		private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void BackToMenuClick(object sender, RoutedEventArgs e)
		{
			ReadNotesCanvas.Visibility= Visibility.Hidden;
			MainMenuCanvas.Visibility = Visibility.Visible;
			SettingsCanvas.Visibility = Visibility.Hidden;
		}

		public static void SendMessage(string NoteName, string NoteDescription, string Message)
		{
			byte[] bytes = new byte[1024];

			try
			{
				
				if (!sender.Connected)
				{
					sender.Connect(remoteEP);
					Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
				}

				if (Message == "<TheEnd>")
				{
					byte[] endmsg = Encoding.ASCII.GetBytes(Message);
					int bytesSentend = sender.Send(endmsg);
				}
				else if (Message == "Client reconected ")
				{
					byte[] recmsg = Encoding.ASCII.GetBytes(Message);
					int bytesSentrec = sender.Send(recmsg);

				}
				else
				{

					byte[] msg = Encoding.ASCII.GetBytes("Note name: " + NoteName + "\nNote description: " + NoteDescription);

					int bytesSent = sender.Send(msg);

					int bytesRec = sender.Receive(bytes);

					Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));
				}
				
			}
			catch (ArgumentNullException ane)
			{
				Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
			}
			catch (SocketException se)
			{
				Console.WriteLine("SocketException : {0}", se.ToString());
			}
			catch (Exception e)
			{
				Console.WriteLine("Unexpected exception : {0}", e.ToString());
			}
		}

		private string GetMessage(string message)
		{
			try
			{
				byte[] msg = Encoding.UTF8.GetBytes(message);
				sender.Send(msg);

				byte[] bytes = new byte[1024];
				int bytesRec = sender.Receive(bytes);
				string response = Encoding.UTF8.GetString(bytes, 0, bytesRec);

				return response;
			}
			catch (Exception ex)
			{
				return "Error: " + ex.Message;
			}
		}

		public static void StartClient(int CustomPort)
		{
			try
			{
				host = Dns.GetHostEntry("localhost");
				ipAddress = host.AddressList[0];
				remoteEP = new IPEndPoint(ipAddress, CustomPort);
				sender = new Socket(ipAddress.AddressFamily,
					SocketType.Stream, ProtocolType.Tcp);

				sender.Connect(remoteEP);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public void GetTCPAnswerClick(object sender, RoutedEventArgs e)
		{

			NotesCountLabel.Content = "" + GetMessage("<GetData>");
		}

		public void CloseServerClick(object sender, RoutedEventArgs e)
		{
			SendMessage("","", "<TheEnd>");
		}

		public void ReconectButtonClick(object sender, RoutedEventArgs e)
		{
			StartClient(Convert.ToInt32(PortTextEdit.Text));
			SendMessage("","","Client reconected ");
		}

		//Animations

		private void AddButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			DoubleAnimation scaleAnim = new DoubleAnimation()
			{
				From = 1,
				To = 0.8,
				Duration = TimeSpan.FromSeconds(0.1)
			};
		

			AddButtonScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
			AddButtonScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
		}

		private void AddButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			DoubleAnimation scaleAnim = new DoubleAnimation()
			{
				From = 0.8,
				To = 1,
				Duration = TimeSpan.FromSeconds(0.1)
			};

			AddButtonScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
			AddButtonScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
		}

	}

	public class Notes
	{
		private string _name;
		private string _description;
		
		public string Name
		{
			get => _name;
			set {
				try
				{
					if (value.Length == 0)
					{
						throw new Exception("Name must contain at least 1 char");
					}
					else { _name = value; }
				} catch(Exception)
				{
					_name = "New note";
					Console.WriteLine("InvalidNameError");
				}
			}
		}
		public string Description { get => _description; set => _description = value; }

		public Notes(string name, string description) 
		{
			Name = name;
			_description = description;	
		}
	}

	
}
