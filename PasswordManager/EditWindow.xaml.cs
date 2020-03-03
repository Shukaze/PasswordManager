using PasswordManager.Repository;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PasswordManager
{
    public partial class EditWindow : Window
    {
        public Password Password { get; private set; }

        private bool pwdChanged;
        private bool changed;

        public EditWindow(Window owner, string title, ImageSource icon, Password password = null)
        {
            Owner = owner;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = Properties.Settings.Default.Topmost;
            Title = title;
            Icon = icon;
            Password = new Password();
            if (password != null)
            {
                Password = password;
                textBoxName.Text = password.Name;
                textBoxLogin.Text = password.Login;
                if (password.SecurePassword.Length > 0)
                {
                    passwordBox.Password = "********";
                    passwordBoxConfirmed.Password = "********";
                }
                
                textBoxDescription.Text = password.Description;
            }
            changed = false;
            pwdChanged = false;
            UpdateControls();
            textBoxName.Focus();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled = changed;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (!passwordBoxConfirmed.SecurePassword.IsEqualTo(passwordBox.SecurePassword))
            {
                MessageBox.Show(Properties.Resources.ERROR_PASSWORD_DOES_NOT_MATCH);
                return;
            }
            Password.Name = textBoxName.Text;
            Password.Login = textBoxLogin.Text;
          
            Password.Description = textBoxDescription.Text;
            if (pwdChanged)
            {
                Password.SecurePassword = passwordBox.SecurePassword;
            }
            DialogResult = true;
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            pwdChanged = true;
            changed = true;
            passwordBoxConfirmed.Password = string.Empty;
            UpdateControls();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            changed = true;
            UpdateControls();
        }

        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RoutedUICommand r = e.Command as RoutedUICommand;
            if (r == null) return;
            switch (r.Name)
            {
                
                case "GeneratePassword":
                    e.CanExecute = true;
                    break;
                default:
                    break;
            }
        }

        private void Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedUICommand r = e.Command as RoutedUICommand;
            if (r == null) return;
            MainWindow mainWindow = Owner as MainWindow;
            if (mainWindow == null) return;
            switch (r.Name)
            {
                
                case "GeneratePassword":
                    mainWindow.GeneratePassword();
                    break;
                default:
                    break;
            }
        }

    }
}
