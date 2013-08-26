using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            KeyLengthBox.ItemsSource = new int[] { 2048, 4096, 8192 };
            Refresh();
        }

        private void Refresh()
        {
            KeyBox.Text = App.Instance.Config.PublicKey;
            BootstrapNodeHostBox.Text = App.Instance.Config.BootstrapNode.Host;
            BootstrapNodePortBox.Text = App.Instance.Config.BootstrapNode.Port.ToString();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (BootstrapNodeHostBox.Text.Length == 0)
            {
                MessageBox.Show("Node address must not be empty", "AtlasNet", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            App.Instance.Config.BootstrapNode.Host = BootstrapNodeHostBox.Text;
            try
            { App.Instance.Config.BootstrapNode.Port = int.Parse(BootstrapNodePortBox.Text); }
            catch
            {
                MessageBox.Show("Node port must be numeric", "AtlasNet", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void RegenerateKeyButton_Click(object sender, RoutedEventArgs e)
        {
            KeyGeneratorProgress.Visibility = Visibility.Visible;
            int strength = (int)KeyLengthBox.SelectedItem;
            new Thread(delegate()
            {
                var keys = Crypto.GenerateKey(strength);
                App.Instance.Config.PublicKey = Crypto.SaveKey(keys.Public);
                App.Instance.Config.PrivateKey = Crypto.SaveKey(keys.Private);
                Dispatcher.Invoke(new Action(delegate()
                {
                    Refresh();
                    KeyGeneratorProgress.Visibility = Visibility.Collapsed;
                }));
            }).Start();
        }

        private void KeyBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            KeyBox.SelectAll();
        }
    }
}
