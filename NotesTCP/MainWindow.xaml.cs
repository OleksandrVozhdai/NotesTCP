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

namespace NotesTCP
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Dictionary<Button, Notes> notesKeyDict = new Dictionary<Button,Notes>();
		private int buttonCount = 0;

		private List<Notes> noteList = new List<Notes>();

		public MainWindow()
		{
			InitializeComponent();

			MainMenuCanvas.Visibility = Visibility.Visible;
			AddNoteCanvas.Visibility = Visibility.Hidden;
			ReadNotesCanvas.Visibility = Visibility.Hidden;
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

				NoteNameText.Text = "New note ";
				NoteDescriptionText.Text = "Note description";

				notesKeyDict[newNote] = noteList[buttonCount];

				newNote.Click += (s, args) => ReadNote(newNote);

				buttonCount++;
			}
		}

		private void ReadNote(Button index)
		{ 
			ReadNotesCanvas.Visibility = Visibility.Visible;
			MainMenuCanvas.Visibility = Visibility.Hidden;

			NoteNameText_Read.Text = notesKeyDict[index].Name;
			NoteDescriptionText_Read.Text = notesKeyDict[index].Description;
		}

		private void BackToMenuClick(object sender, RoutedEventArgs e)
		{
			ReadNotesCanvas.Visibility= Visibility.Hidden;
			MainMenuCanvas.Visibility = Visibility.Visible;
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
				} catch(Exception Exception)
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
