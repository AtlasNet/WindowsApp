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
        }

        public void RefreshDialogArea()
        {
            var messages = new List<Message>();
            messages.AddRange(App.Instance.Config.Messages.Where((x) => x.Contact == SelectedContact));
            MessagesList.ItemsSource = messages;
            MessageTextBox.Focus();
            MessagesListScroll.ScrollToEnd();
        }

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
        }

        private void DeleteContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Delete contact '{0}'?", SelectedContact.Name), Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                App.Instance.Config.Contacts.Remove(SelectedContact);
                SelectedContact = null;
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var message = new Message { Text = MessageTextBox.Text, ContactKey = SelectedContact.PublicKey, Date = DateTime.UtcNow, Read = false, Outgoing = true };
            MessageTextBox.Text = "";
            App.Instance.Config.Messages.Add(message);
            RefreshDialogArea();

            string package = new AtlasClient().PrepareMessage(message, SignMessageCheckbox.IsChecked.Value);
            var c = App.Instance.ConnectionPool[App.Instance.Config.BootstrapNode];
            c.Send(message, package);
        }

        private void RetrieveButton_Click(object sender, RoutedEventArgs e)
        {
            var listings = App.Instance.ConnectionPool[App.Instance.Config.BootstrapNode].GetListings();
            foreach (var listing in listings)
            {
                var client = App.Instance.ConnectionPool[listing.Node];
                var msg = new AtlasClient().DecryptMessage(client.RetrieveMessage(listing.Id));
                App.Instance.Config.Messages.Add(msg);
            }
            RefreshDialogArea();
        }
    }
}
