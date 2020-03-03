using System;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
    public partial class SettingsWindow : Window
    {
        private bool changed;
        private bool init;

        public SettingsWindow(Window owner)
        {
            Owner = owner;
            Title = Properties.Resources.TITLE_SETTINGS;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            init = true;
            checkBoxTopmost.IsChecked = Properties.Settings.Default.Topmost;
            textBoxAutoClearClipboard.Text = Convert.ToString(Properties.Settings.Default.AutoClearClipboard);
            textBoxAutoHidePassword.Text = Convert.ToString(Properties.Settings.Default.AutoHidePassword);
            textBoxReenterPassword.Text = Convert.ToString(Properties.Settings.Default.ReenterPassword);
            
            UpdateControls();
            textBoxAutoClearClipboard.Focus();
            init = false;
        }

        private void UpdateControls()
        {
            int val = -1;
            bool ok = Int32.TryParse(textBoxAutoClearClipboard.Text, out val) && val >= 0;
            if (ok)
            {
                ok = Int32.TryParse(textBoxAutoHidePassword.Text, out val) && val >= 0;
            }
            if (ok)
            {
                ok = Int32.TryParse(textBoxReenterPassword.Text, out val) && val >= 0;
            }
           
            buttonOK.IsEnabled = ok && changed;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            int val = -1;
            if (Int32.TryParse(textBoxAutoClearClipboard.Text, out val))
            {
                Properties.Settings.Default.AutoClearClipboard = val;
            }
            if (Int32.TryParse(textBoxAutoHidePassword.Text, out val))
            {
                Properties.Settings.Default.AutoHidePassword = val;
            }
            if (Int32.TryParse(textBoxReenterPassword.Text, out val))
            {
                Properties.Settings.Default.ReenterPassword = val;
            }
            Properties.Settings.Default.Topmost = Topmost;
            DialogResult = true;
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init) return;
            changed = true;
            UpdateControls();
        }

        private void checkBoxTopmost_Checked(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Topmost = true;
            Owner.Topmost = true;
            changed = true;
            UpdateControls();
        }

        private void checkBoxTopmost_Unchecked(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Topmost = false;
            Owner.Topmost = false;
            changed = true;
            UpdateControls();
        }

        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            if (init) return;
            changed = true;
            UpdateControls();
        }
    }
}
