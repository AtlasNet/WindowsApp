using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AtlasNetClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Contact SelectedContact
        {
            get { return (Contact)ContactList.SelectedItem; }
            set { ContactList.SelectedItem = value; MainPanel.DataContext = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            SignMessageCheckbox.IsChecked = true;
            ContactList.ItemsSource = App.Instance.Config.Contacts;
            ContactList.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            DisplayAppState(AppState.Idle, "Ready");

            BackgroundWorker.OnProgress = delegate(bool complete, string message)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    DisplayAppState(complete ? AppState.Idle : AppState.Working, complete ? "Done" : message);
                    if (complete)
                        RefreshDialogArea();
                }));
            };

            App.Instance.Config.Messages.CollectionChanged += delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate
                 {
                     RefreshDialogArea(false);
                 }));
            };
        }

        public void RefreshDialogArea(bool scroll = true)
        {
            var messages = new List<Message>();
            if (SelectedContact != null)
            {
                messages.AddRange(App.Instance.Config.Messages.Where((x) => x.ContactKey == SelectedContact.PublicKey));
                MessagesList.ItemsSource = messages;
                MessageTextBox.Focus();
                if (scroll)
                    MessagesListScroll.ScrollToEnd();
            }
        }


        private enum AppState
        {
            Idle, Working
        }

        private void DisplayAppState(AppState state, string info)
        {
            StatusBarText.Content = info;
            if (state == AppState.Idle)
            {
                StatusBarAnimation.Visibility = Visibility.Collapsed;
                StatusBar.Background = new SolidColorBrush(Color.FromRgb(0x25, 0x80, 0xCD));
                StatusBar.Foreground = new SolidColorBrush(Colors.White);
            }
            if (state == AppState.Working)
            {
                StatusBarAnimation.Visibility = Visibility.Visible;
                StatusBar.Background = new SolidColorBrush(Color.FromRgb(0xCD, 0x80, 0x25));
                StatusBar.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        // -------------

        private void ContactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedContact = (Contact)ContactList.SelectedItem;
            RefreshDialogArea();
        }

        private void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new AddContactWindow();
            wnd.Owner = this;
            if (wnd.ShowDialog().Value)
            {
                var contact = new Contact { Name = wnd.Name, PublicKey = wnd.PublicKey };
                App.Instance.Config.Contacts.Add(contact);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new SettingsWindow();
            wnd.Owner = this;
            wnd.ShowDialog();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            while (App.Instance.Config.PublicKey == null)
            {
                MessageBox.Show("Please generate a new encryption key", "AtlasNet", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                SettingsButton_Click(null, null);
            }
            while (string.IsNullOrEmpty(App.Instance.Config.BootstrapNode.Host))
            {
                MessageBox.Show("Please specify a bootstrap node", "AtlasNet", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                SettingsButton_Click(null, null);
            }
        }

        private void DeleteContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Delete contact '{0}'?", SelectedContact.Name), Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var message in new List<Message>(App.Instance.Config.Messages.Where(x => x.ContactKey == SelectedContact.PublicKey)))
                    App.Instance.Config.Messages.Remove(message);
                App.Instance.Config.Contacts.Remove(SelectedContact);
                SelectedContact = null;
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var message = new Message
            {
                Text = MessageTextBox.Text,
                ContactKey = SelectedContact.PublicKey,
                Date = DateTime.UtcNow,
                Read = true,
                Outgoing = true,
                State = Message.States.Sending,
                SignatureVerified = true
            };
            MessageTextBox.Text = "";
            App.Instance.Config.Messages.Add(message);
            RefreshDialogArea();

            BackgroundWorker.SendMessage(message, SignMessageCheckbox.IsChecked.Value);
        }

        private void RetrieveButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker.RetrieveMail();
        }
    }
}
