using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AtlasNetClient
{
    /// <summary>
    /// Interaction logic for AddContactWindow.xaml
    /// </summary>
    public partial class AddContactWindow : Window
    {
        public string Name { get; set; }
        public string PublicKey { get; set; }

        public AddContactWindow()
        {
            InitializeComponent();
            NameBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameBox.Text.Length == 0)
            {
                MessageBox.Show("Name must not be empty", "Contact", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (Crypto.ReadKey(KeyBox.Text) == null)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show("Key is not valid", "Contact", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            Name = NameBox.Text;
            PublicKey = KeyBox.Text;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
