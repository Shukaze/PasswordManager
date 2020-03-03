using PasswordManager.Repository;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
    public partial class PropertiesWindow : Window
    {
        private KeyDirectoryCache keyDirCache;
        private PasswordRepository repository;

        public PropertiesWindow(Window owner, string title, KeyDirectoryCache keyDirCache, PasswordRepository repository, string filename)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = Properties.Settings.Default.Topmost;
            this.keyDirCache = keyDirCache;
            this.repository = repository;
            InitializeComponent();
            textBoxName.Text = repository.Name;
            textBoxDescription.Text = repository.Description;
            textBoxPasswordFile.Text = filename;
            textBoxKeyDirectory.Text = keyDirCache.Get(repository.Id);
            textBoxKey.Text = repository.Id;
            textBoxName.Focus();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled = !string.Equals(textBoxName.Text, repository.Name) ||
                !string.Equals(textBoxDescription.Text, repository.Description);
        }
 
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            repository.Name = textBoxName.Text;
            repository.Description = textBoxDescription.Text;
            DialogResult = true;
            Close();
        }

        private void TextBox_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }
    }
}
