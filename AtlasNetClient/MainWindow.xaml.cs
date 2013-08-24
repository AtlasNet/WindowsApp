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
            ContactList.ItemsSource = App.Instance.Config.Contacts;
        }

        private void ContactList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedContact = (Contact)ContactList.SelectedItem;
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
    }
}
