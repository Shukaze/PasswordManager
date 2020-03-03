﻿using PasswordManager.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PasswordManager
{
    public partial class MainWindow : Window
    {
        private int autoClearClipboardAfterSec = 30;
        private int autoHidePasswordAfterSec = 30; 
        private int reenterPasswordAfterSec = 300; 

        private PasswordRepository passwordRepository;
        private string passwordFilename;
        private SecureString passwordSecureString;

        private ThumbnailCache thumbnailCache;
        private KeyDirectoryCache keyDirectoryCache;

        private DispatcherTimer timer;
        private DateTime showPasswordSince;
        private DateTime copiedToClipboardSince;
        private DateTime idleSince = DateTime.Now;

        private bool autoHidePassword = false;
        private bool reenterPassword = false;
        private bool copiedToClipboard = false;

        private static BitmapImage imageShow32x32 = new BitmapImage(new Uri("pack://application:,,,/Images/32x32/document-decrypt-3.png"));
        private static BitmapImage imageHide32x32 = new BitmapImage(new Uri("pack://application:,,,/Images/32x32/document-encrypt-3.png"));
        private static BitmapImage imageKey16x16 = new BitmapImage(new Uri("pack://application:,,,/Images/16x16/kgpg_info.png"));
        private static BitmapImage imageShow16x16 = new BitmapImage(new Uri("pack://application:,,,/Images/16x16/document-decrypt-3.png"));
        private static BitmapImage imageHide16x16 = new BitmapImage(new Uri("pack://application:,,,/Images/16x16/document-encrypt-3.png"));
        private static Image menuItemImageHide;
        private static Image menuItemImageShow;
        private static Image menuItemImageShowDisabled;
        private static Image contextMenuItemImageHide;
        private static Image contextMenuItemImageShow;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(OnTimer);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void OnTimer(object obj, EventArgs args)
        {
            try
            {
                UpdateStatus();
                if (autoHidePassword && autoHidePasswordAfterSec > 0)
                {
                    var ts = DateTime.Now - showPasswordSince;
                    if (ts.TotalSeconds > autoHidePasswordAfterSec)
                    {
                        autoHidePassword = false;
                        foreach (PasswordViewItem item in listView.Items)
                        {
                            item.HidePassword = true;
                        }
                        listView.Items.Refresh();
                        UpdateControls();
                    }
                }
                if (copiedToClipboard && autoClearClipboardAfterSec > 0)
                {
                    var tscopied = DateTime.Now - copiedToClipboardSince;
                    if (tscopied.TotalSeconds > autoClearClipboardAfterSec)
                    {
                        copiedToClipboard = false;
                        Clipboard.Clear();
                    }
                }
                if (!reenterPassword && reenterPasswordAfterSec > 0)
                {
                    var tsidle = DateTime.Now - idleSince;
                    if (tsidle.TotalSeconds > reenterPasswordAfterSec)
                    {
                        reenterPassword = true;
                        if (Properties.Settings.Default.AutoLockWindow)
                        {
                            gridMain.Visibility = Visibility.Hidden;
                            gridLock.Visibility = Visibility.Visible;
                            if (Properties.Settings.Default.AutoMinimizeWindow)
                            {
                                WindowState = WindowState.Minimized;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                HandleError(ex);
                Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bool canceled = false;
            if (PromptSaveChanges(ref canceled))
            {
                if (!canceled)
                {
                    try
                    {
                        if (copiedToClipboard)
                        {
                            copiedToClipboard = false;
                            Clipboard.Clear();
                        }
                        if (WindowState == WindowState.Normal)
                        {
                            Properties.Settings.Default.Left = Left;
                            Properties.Settings.Default.Top = Top;
                            Properties.Settings.Default.Width = Width;
                            Properties.Settings.Default.Height = Height;
                        }
                        Properties.Settings.Default.Save();
                        if (thumbnailCache != null)
                        {
                            thumbnailCache.Save();
                        }
                        if (keyDirectoryCache != null)
                        {
                            keyDirectoryCache.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError(ex);
                    }
                    return;
                }
            }
            e.Cancel = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            idleSince = DateTime.Now;
        }

        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            listViewToggleShowPassword.Header = Properties.Resources.CMD_SHOW_PASSWORD;
            listViewToggleShowPassword.Icon = contextMenuItemImageShow;
            if (listView.SelectedItem is PasswordViewItem item && !item.HidePassword)
            {
                listViewToggleShowPassword.Header = Properties.Resources.CMD_HIDE_PASSWORD;
                listViewToggleShowPassword.Icon = contextMenuItemImageHide;
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(listView);
            var lvitem = listView.GetItemAt(mousePosition);
            if (lvitem != null)
            {
                EditItemAsync(lvitem.Content as PasswordViewItem);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (listView == null || passwordRepository == null) return;
                listView.Items.Clear();
                foreach (var password in passwordRepository.Passwords)
                {
                    if (password.Name.StartsWith(textBoxFilter.Text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        listView.Items.Add(new PasswordViewItem(password, imageKey16x16));
                    }
                    if (listView.Items.Count > 0)
                    {
                        listView.SelectedIndex = 0;
                    }
                }
                SortListView();
                InitThumbnailCacheAsync();
                UpdateControls();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RoutedUICommand r = e.Command as RoutedUICommand;
            if (r == null ||
                Properties.Settings.Default.AutoLockWindow && reenterPassword)
            {
                e.CanExecute = false;
                return;
            }
            int selected = 0;
            
            bool hasLogin = false;
            bool hasPassword = false;
            bool hasRepository = passwordRepository != null;
            if (hasRepository && listView.SelectedItems != null)
            {
                selected = listView.SelectedItems.Count;
                if (selected == 1)
                {
                    var item = listView.SelectedItem as PasswordViewItem;
                    if (item.Password != null)
                    {
                        
                        hasLogin = !string.IsNullOrEmpty(item.Login);
                        hasPassword = item.Password.SecurePassword.Length > 0;
                    }
                }
            }
            switch (r.Name)
            {
                case "New":
                case "Exit":
                case "Open":
                case "About":
                case "History":
                case "ShowLoginColumn":
                case "ShowPasswordColumn":
                case "ShowToolbar":
                case "GeneratePassword":
                case "ShowSettings":
                    e.CanExecute = true;
                    break;
                case "Save":
                    e.CanExecute = hasRepository && passwordRepository.Changed;
                    break;
                case "SaveAs":
                case "Close":
                case "Properties":
                case "Add":
                case "ChangeKeyDirectory":
                case "ChangeMasterPassword":
                    e.CanExecute = hasRepository;
                    break;
                case "Edit":
                    e.CanExecute = selected == 1;
                    break;
                case "CopyLogin":
                    e.CanExecute = selected == 1 && hasLogin;
                    break;
                case "CopyPassword":
                    e.CanExecute = selected == 1 && hasPassword;
                    break;
                case "Remove":
                    e.CanExecute = selected >= 1;
                    break;
                case "TogglePassword":
                    e.CanExecute = selected >= 1 && Properties.Settings.Default.ShowPasswordColumn;
                    break;
               
                default:
                    break;
            }
        }

        private void Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedUICommand r = e.Command as RoutedUICommand;
            if (r == null) return;
            switch (r.Name)
            {
                case "Exit":
                    Close();
                    break;
                case "New":
                    CreateRepository();
                    break;
                case "Open":
                    OpenRepository();
                    break;
                case "SaveAs":
                    SaveAs();
                    break;
                case "Close":
                    CloseRepository();
                    break;
                case "Save":
                    Save();
                    break;
                case "ChangeKeyDirectory":
                    ChangeKeyDirectory();
                    break;
                case "ChangeMasterPassword":
                    ChangeMasterPassword();
                    break;
                case "Properties":
                    ShowProperties();
                    break;
                case "Add":
                    AddItemAsync();
                    break;
                case "Edit":
                    EditItem();
                    break;
                case "Remove":
                    RemoveItems();
                    break;
             
                case "TogglePassword":
                    ToggleShowPassword();
                    break;
                case "CopyLogin":
                    CopyLogin();
                    break;
                case "CopyPassword":
                    CopyPassword();
                    break;
                case "About":
                    About();
                    break;
                case "History":
                    LoginScreen();
                    break;
                case "ShowLoginColumn":
                    ShowLoginColumn();
                    break;
                case "ShowPasswordColumn":
                    ShowPasswordColumn();
                    break;
                case "ShowToolbar":
                    ShowToolbar();
                    break;
                case "GeneratePassword":
                    GeneratePassword();
                    break;
                case "ShowSettings":
                    ShowSettings();
                    break;
                default:
                    break;
            }
        }

        private void ButtonVerifyPassword_Click(object sender, RoutedEventArgs e)
        {
            ReenterPassword();
        }

        // actions

        private void Init()
        {
            this.RestorePosition(
                Properties.Settings.Default.Left,
                Properties.Settings.Default.Top,
                Properties.Settings.Default.Width,
                Properties.Settings.Default.Height);
            Topmost = Properties.Settings.Default.Topmost;
            autoClearClipboardAfterSec = Properties.Settings.Default.AutoClearClipboard;
            autoHidePasswordAfterSec = Properties.Settings.Default.AutoHidePassword;
            reenterPasswordAfterSec = Properties.Settings.Default.ReenterPassword;
            menuItemImageShow = new Image{ Source = imageShow16x16, Height=16, Width=16 };
            menuItemImageHide = new Image{ Source = imageHide16x16, Height = 16, Width = 16 };
            menuItemImageShowDisabled = new Image { Source = imageShow16x16, Opacity = 0.5, Height = 16, Width = 16 };
            contextMenuItemImageShow = new Image { Source = imageShow16x16, Height = 16, Width = 16 };
            contextMenuItemImageHide = new Image { Source = imageHide16x16, Height = 16, Width = 16 };
            var cacheDirectory = Properties.Settings.Default.CacheDirectory.ReplaceSpecialFolder();
            PrepareDirectory(cacheDirectory);
            keyDirectoryCache = new KeyDirectoryCache(cacheDirectory);
            keyDirectoryCache.Load();
            PrepareDirectory(keyDirectoryCache.GetLastUsed());
            UpdateLoginColumn();
            UpdatePasswordColumn();
            UpdateToolbar();
            SortListView();
            UpdateControls();
            var filename = Properties.Settings.Default.LastUsedRepositoryFile;
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                OpenRepository(filename, true);
            }
            else
            {
                CreateRepository();
            }
        }

        private void UpdateLoginColumn()
        {
            if (listView.View is GridView gv)
            {
                if (!Properties.Settings.Default.ShowLoginColumn)
                {
                    gv.Columns.Remove(gridViewColumnLogin);
                }
                else if (!gv.Columns.Contains(gridViewColumnLogin))
                {
                    gv.Columns.Insert(1, gridViewColumnLogin);
                }
            }
        }

        private void UpdatePasswordColumn()
        {
            if (listView.View is GridView gv)
            {
                if (!Properties.Settings.Default.ShowPasswordColumn)
                {
                    gv.Columns.Remove(gridViewColumnPassword);
                }
                else if (!gv.Columns.Contains(gridViewColumnPassword))
                {
                    gv.Columns.Add(gridViewColumnPassword);
                }
            }
        }

        private void UpdateToolbar()
        {
            var g = grid.RowDefinitions[1].Height;
            if (g.IsAuto && !Properties.Settings.Default.ShowToolbar)
            {
                grid.RowDefinitions[1].Height = new GridLength(0.0);
                toolbarTray.Visibility = Visibility.Hidden;
            }
            else if (!g.IsAuto && Properties.Settings.Default.ShowToolbar)
            {
                grid.RowDefinitions[1].Height = new GridLength(30, GridUnitType.Auto);
                toolbarTray.Visibility = Visibility.Visible;
            }
        }

        private void SortListView()
        {
            listView.Items.SortDescriptions.Clear();
            listView.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            listView.Items.Refresh();            
        }
  
        private void PrepareDirectory(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void UpdateControls()
        {
            bool showEnabled = false;
            bool hideEnabled = false;
            int selected = listView.SelectedItems.Count;
            if (selected > 0)
            {
                var item = listView.SelectedItem as PasswordViewItem;
                showEnabled = item.HidePassword;
                hideEnabled = !showEnabled;
            }
            if (hideEnabled)
            {
                imageToggleShow.Source = imageHide32x32;
                imageToggleShow.ToolTip = Properties.Resources.TOOLTIP_HIDE_PASSWORD;
               
            }
            else
            {
                imageToggleShow.ToolTip = Properties.Resources.TOOLTIP_SHOW_PASSWORD;
                imageToggleShow.Source = imageShow32x32;
              
            }
            Title = Properties.Resources.TITLE_PASSWORD_MANAGER;
            if (passwordRepository != null)
            {
                Title += $" - {passwordRepository.Name}";
                if (passwordRepository.Changed)
                {
                    Title += " *";
                }
            }
            menuItemShowLoginColumn.IsChecked = Properties.Settings.Default.ShowLoginColumn;
            menuItemShowPasswordColumn.IsChecked = Properties.Settings.Default.ShowPasswordColumn;
            menuItemShowToolbar.IsChecked = Properties.Settings.Default.ShowToolbar;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int selected = listView.SelectedItems.Count;
            int total = listView.Items.Count;
            string status = string.Empty;
            if (selected > 0)
            {
                if (total == 1)
                {
                    status = Properties.Resources.SELECTED_ONE;
                }
                else
                {
                    status = string.Format(Properties.Resources.SELECTED_0_OF_1, selected, total);
                }
            }
            else if (total > 0)
            {
                if (total == 1)
                {
                    status = Properties.Resources.TOTAL_ONE;
                }
                else
                {
                    status = string.Format(Properties.Resources.TOTAL_0, total);
                }
            }
            if (autoHidePassword)
            {
                TimeSpan ts = DateTime.Now - showPasswordSince;
                int sec = Math.Max(0, autoHidePasswordAfterSec - (int)ts.TotalSeconds);
                if (sec > 0)
                {
                    string hidestr;
                    if (sec == 1)
                    {
                        hidestr = Properties.Resources.AUTO_HIDE_IN_ONE;
                    }
                    else
                    {
                        hidestr = string.Format(Properties.Resources.AUTO_HIDE_IN_0, sec);
                    }
                    status += " " + hidestr;
                }
            }
            if (copiedToClipboard)
            {
                TimeSpan ts = DateTime.Now - copiedToClipboardSince;
                int sec = Math.Max(0, autoClearClipboardAfterSec - (int)ts.TotalSeconds);
                if (sec > 0)
                {
                    string hidestr;
                    if (sec == 1)
                    {
                        hidestr = Properties.Resources.AUTO_CLEAR_CLIPBOARD_IN_ONE;
                    }
                    else
                    {
                        hidestr = string.Format(Properties.Resources.AUTO_CLEAR_CLIPBOARD_IN_0, sec);
                    }
                    status += " " + hidestr;
                }
            }
            textBlockStatus.Text = status;
        }

       

        private async void AddItemAsync()
        {
            try
            {
                if (!ReenterPassword())
                {
                    return;
                }
                EditWindow w = new EditWindow(this, Properties.Resources.TITLE_ADD, imageKey16x16);
                if (w.ShowDialog() == true)
                {
                    passwordRepository.Add(w.Password);
                    var item = new PasswordViewItem(w.Password, imageKey16x16);
                    listView.Items.Add(item);
                    listView.SelectedItem = item;
                    listView.ScrollIntoView(item);
                    UpdateControls();
                    if (thumbnailCache != null &&
                        !string.IsNullOrEmpty(w.Password.Url))
                    {
                        var filename = await thumbnailCache.GetImageFileNameAsync(w.Password.Url);
                        if (!string.IsNullOrEmpty(filename))
                        {
                            item.Image = new BitmapImage(new Uri(filename));
                        }
                    }
                    SortListView();
                    listView.ScrollIntoView(item);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void EditItem()
        {
            EditItemAsync(listView.SelectedItem as PasswordViewItem);
        }
  
        private async void EditItemAsync(PasswordViewItem item)
        {
            try
            {
                if (item == null) return;
                if (!ReenterPassword())
                {
                    return;
                }
                var oldurl = item.Password.Url;
                var w = new EditWindow(this, Properties.Resources.TITLE_EDIT, item.Image, item.Password);
                if (w.ShowDialog() == true)
                {
                    passwordRepository.Update(w.Password);
                    item.Update(w.Password);
                    UpdateControls();
                    if (!string.Equals(oldurl, w.Password.Url))
                    {
                        if (thumbnailCache != null &&
                            !string.IsNullOrEmpty(item.Password.Url))
                        {
                            var image = imageKey16x16;
                            var filename = await thumbnailCache.GetImageFileNameAsync(item.Password.Url);
                            if (!string.IsNullOrEmpty(filename))
                            {
                                item.Image = new BitmapImage(new Uri(filename));
                            }
                        }
                    }
                    SortListView();
                    listView.ScrollIntoView(item);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void RemoveItems()
        {
            try
            {
                if (!ReenterPassword())
                {
                    return;
                }
                if (MessageBox.Show(
                        Properties.Resources.QUESTION_DELETE_ITEMS,
                        Title,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    if (!ReenterPassword())
                    {
                        return;
                    }
                    var del = new List<PasswordViewItem>();
                    foreach (PasswordViewItem item in listView.SelectedItems)
                    {
                        del.Add(item);
                    }
                    int idx = listView.SelectedIndex;
                    foreach (var item in del)
                    {
                        listView.Items.Remove(item);
                        passwordRepository.Remove(item.Password);
                    }
                    idx = Math.Min(idx, listView.Items.Count - 1);
                    if (idx >= 0)
                    {
                        listView.SelectedIndex = idx;
                    }
                    UpdateControls();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ToggleShowPassword()
        {
            try
            {
                if (!ReenterPassword())
                {
                    return;
                }
                bool showEnabled = false;
                bool hideEnabled = false;
                int selected = listView.SelectedItems.Count;
                if (selected > 0)
                {
                    var item = listView.SelectedItem as PasswordViewItem;
                    showEnabled = item.HidePassword;
                    hideEnabled = !showEnabled;
                }
                if (showEnabled)
                {
                    foreach (PasswordViewItem item in listView.SelectedItems)
                    {
                        item.HidePassword = false;
                    }
                    listView.Items.Refresh();
                    showPasswordSince = DateTime.Now;
                    autoHidePassword = true;
                }
                else if (hideEnabled)
                {
                    foreach (PasswordViewItem item in listView.SelectedItems)
                    {
                        item.HidePassword = true;
                    }
                    listView.Items.Refresh();
                    bool passwdshown = false;
                    foreach (PasswordViewItem item in listView.Items)
                    {
                        if (!item.HidePassword)
                        {
                            passwdshown = true;
                            break;
                        }
                    }
                    if (!passwdshown)
                    {
                        autoHidePassword = false;
                    }
                }
                UpdateControls();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private bool ReenterPassword()
        {
            try
            {
                if (reenterPassword)
                {
                    foreach (PasswordViewItem item in listView.Items)
                    {
                        item.HidePassword = true;
                    }
                    listView.Items.Refresh();
                    var dlg = new LoginWindow(this, Properties.Resources.VERIFY_PASSWORD, keyDirectoryCache, passwordFilename)
                    {
                        SecurePassword = passwordSecureString
                    };
                    if (dlg.ShowDialog() != true)
                    {
                        return false;
                    }
                    reenterPassword = false;
                    idleSince = DateTime.Now;
                    gridLock.Visibility = Visibility.Hidden;
                    gridMain.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            return true;
        }

        public void CopyToClipboard(string text, bool pwdcheck = true)
        {
            try
            {
                if (pwdcheck && !ReenterPassword())
                {
                    return;
                }
                Clipboard.SetText(text);
                copiedToClipboardSince = DateTime.Now;
                copiedToClipboard = true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void CopyLogin()
        {
            if (listView.SelectedItem is PasswordViewItem item &&
                !string.IsNullOrEmpty(item.Login))
            {
                CopyToClipboard(item.Login);
            }
        }

        private void CopyPassword()
        {
            if (listView.SelectedItem is PasswordViewItem item &&
                item.Password != null && item.Password.SecurePassword.Length > 0)
            {
                CopyToClipboard(item.Password.SecurePassword.GetAsString());
            }
        }

        private void About()
        {
            try
            {
                var dlg = new AboutWindow(this);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        
        private void LoginScreen()
        {
            try
            {
                var dlg = new LoginScreen(this);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }



        private async void InitThumbnailCacheAsync()
        {
            try
            {
                if (thumbnailCache == null)
                {
                    var cacheDirectory = Properties.Settings.Default.CacheDirectory.ReplaceSpecialFolder();
                    thumbnailCache = new ThumbnailCache(cacheDirectory);
                    thumbnailCache.Load();
                }
                foreach (PasswordViewItem item in listView.Items)
                {
                    var filename = await thumbnailCache.GetImageFileNameAsync(item.Password.Url);
                    if (!string.IsNullOrEmpty(filename))
                    {
                        item.Image = new BitmapImage(new Uri(filename));
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private bool PromptSaveChanges()
        {
            bool canceled = false;
            return PromptSaveChanges(ref canceled);
        }

        private bool PromptSaveChanges(ref bool canceled)
        {
            canceled = false;
            bool ret = false;
            try
            {
                if (passwordRepository == null)
                {
                    return true;
                }
                if (passwordRepository.Changed)
                {
                    var r = MessageBox.Show(
                        this,
                        Properties.Resources.QUESTION_SAVE_CHANGES,
                        Title,
                        MessageBoxButton.YesNoCancel, 
                        MessageBoxImage.Question,
                        MessageBoxResult.Cancel);
                    if (r == MessageBoxResult.Cancel)
                    {
                        canceled = true;
                        return false;
                    }
                    if (r == MessageBoxResult.Yes)
                    {
                        if (!ReenterPassword())
                        {
                            return false;
                        }
                        if (!SaveRepository(passwordFilename))
                        {
                            return false;
                        }
                    }
                }
                ret = true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            UpdateControls();
            return ret;
        }

        private bool CloseRepository(bool force = false)
        {
            bool ret = false;
            try
            {
                if (passwordRepository == null)
                {
                    return true;
                }
                if (force || PromptSaveChanges())
                {
                    passwordRepository = null;
                    passwordSecureString.Clear();
                    listView.Items.Clear();
                    reenterPassword = false;
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            UpdateControls();
            return ret;
        }

        private void SaveAs()
        {
            SaveRepository(null);
        }

        private void Save()
        {
            SaveRepository(passwordFilename);
        }

        private bool SaveRepository(string filename)
        {
            bool ret = false;
            try
            {
                if (passwordRepository == null)
                {
                    return false;
                }
                if (!ReenterPassword())
                {
                    return false;
                }
                if (string.IsNullOrEmpty(filename))
                {
                    var dlg = new Microsoft.Win32.SaveFileDialog()
                    {
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Filter = Properties.Resources.FILE_DIALOG_FILTER
                    };
                    var initDir = Properties.Settings.Default.InitialDirectory.ReplaceSpecialFolder();
                    if (Directory.Exists(initDir))
                    {
                        dlg.InitialDirectory = initDir;
                    }
                    if (dlg.ShowDialog() != true)
                    {
                        return false;
                    }
                    if (!ReenterPassword())
                    {
                        return false;
                    }
                    filename = dlg.FileName;
                    Properties.Settings.Default.InitialDirectory = new FileInfo(filename).Directory.FullName;
                }
                var keyDirectory = keyDirectoryCache.Get(passwordRepository.Id);
                passwordRepository.Save(filename, keyDirectory, passwordSecureString);
                passwordFilename = filename;
                Properties.Settings.Default.LastUsedRepositoryFile = filename;
                SortListView();
                ret = true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            UpdateControls();
            return ret;
        }

        private bool CreateRepository()
        {
            bool ret = false;
            try
            {
                if (!PromptSaveChanges())
                {
                    return false;
                }
                PrepareWindow dlg = new PrepareWindow(this, Properties.Resources.TITLE_NEW, keyDirectoryCache);
                if (dlg.ShowDialog() != true)
                {
                    return false;
                }
                if (!CloseRepository(true/*force*/))
                {
                    return false;
                }
                passwordSecureString = dlg.SecurePassword;
                passwordRepository = dlg.PasswordRepository;
                passwordFilename = null;
                ret = true;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            UpdateControls();
            InitThumbnailCacheAsync();
            return ret;
        }

        private void OpenRepository()
        {
            OpenRepository(null, false);
        }

        private bool OpenRepository(string filename, bool silent)
        {
            bool ret = false;
            try
            {
                if (!PromptSaveChanges())
                {
                    return false;
                }
                if (string.IsNullOrEmpty(filename))
                {
                    var opendlg = new Microsoft.Win32.OpenFileDialog()
                    {
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Filter = Properties.Resources.FILE_DIALOG_FILTER
                    };
                    var initDir = Properties.Settings.Default.InitialDirectory.ReplaceSpecialFolder();
                    if (Directory.Exists(initDir))
                    {
                        opendlg.InitialDirectory = initDir;
                    }
                    if (opendlg.ShowDialog() != true)
                    {
                        return false;
                    }
                    filename = opendlg.FileName;
                    Properties.Settings.Default.InitialDirectory = new FileInfo(filename).Directory.FullName;
                }
                LoginWindow dlg = new LoginWindow(
                    this,
                    Properties.Resources.VERIFY_PASSWORD,
                    keyDirectoryCache,
                    filename);
                if (dlg.ShowDialog() == true)
                {
                    if (!CloseRepository(true))
                    {
                        return false;
                    }
                    passwordFilename = filename;
                    passwordRepository = dlg.PasswordRepository;
                    passwordSecureString = dlg.SecurePassword;
                    Properties.Settings.Default.LastUsedRepositoryFile = passwordFilename;
                    foreach (var password in passwordRepository.Passwords)
                    {
                        listView.Items.Add(new PasswordViewItem(password, imageKey16x16));
                    }
                    SortListView();
                    InitThumbnailCacheAsync();
                    textBoxFilter.Focus();
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
                ret = false;
            }
            UpdateControls();
            return ret;
        }

        private void ChangeKeyDirectory()
        {
            try
            {
                if (passwordRepository == null)
                {
                    return;
                }
                if (!ReenterPassword())
                {
                    return;
                }
                var keyDir = keyDirectoryCache.Get(passwordRepository.Id);
                var dlg = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = Properties.Resources.LABEL_SELECT_KEY_DIRECTORY,
                    SelectedPath = keyDir
                };
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (!string.Equals(keyDir, dlg.SelectedPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!ReenterPassword())
                        {
                            return;
                        }
                        passwordRepository.MoveKey(keyDir, dlg.SelectedPath);
                        keyDirectoryCache.Set(passwordRepository.Id, dlg.SelectedPath);                        
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ChangeMasterPassword()
        {
            try
            {
                if (passwordRepository == null)
                {
                    return;
                }
                if (!ReenterPassword())
                {
                    return;
                }
                var dlg = new ChangeMasterPasswordWindow(
                    this,
                    Properties.Resources.TITLE_CHANGE_MASTER_PASSWORD,
                    passwordSecureString);
                if (dlg.ShowDialog() == true)
                {
                    if (!string.IsNullOrEmpty(passwordFilename))
                    {
                        if (!ReenterPassword())
                        {
                            return;
                        }
                        var keyDirectory = keyDirectoryCache.Get(passwordRepository.Id);
                        passwordRepository.ChangeMasterPassword(passwordFilename, keyDirectory, dlg.SecurePassword);                        
                    }
                    passwordSecureString = dlg.SecurePassword;
                    UpdateControls();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowProperties()
        {
            try
            {
                if (passwordRepository == null)
                {
                    return;
                }
                if (!ReenterPassword())
                {
                    return;
                }
                var dlg = new PropertiesWindow(
                    this,
                    Properties.Resources.TITLE_PROPERTIES,
                    keyDirectoryCache,
                    passwordRepository,
                    passwordFilename);
                if (dlg.ShowDialog() == true)
                {
                    UpdateControls();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowLoginColumn()
        {
            try
            {
                if (!ReenterPassword())
                {
                    UpdateControls();
                    return;
                }
                Properties.Settings.Default.ShowLoginColumn = menuItemShowLoginColumn.IsChecked;
                UpdateLoginColumn();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowPasswordColumn()
        {
            try
            {
                if (!ReenterPassword())
                {
                    UpdateControls();
                    return;
                }
                Properties.Settings.Default.ShowPasswordColumn = menuItemShowPasswordColumn.IsChecked;
                UpdatePasswordColumn();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowToolbar()
        {
            try
            {
                Properties.Settings.Default.ShowToolbar = menuItemShowToolbar.IsChecked;
                UpdateToolbar();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        public void GeneratePassword()
        {
            try
            {
                string pwdgen = "%Module%\\MynaPasswordGenerator.exe".ReplaceSpecialFolder();
                if (File.Exists(pwdgen))
                {
                    string args = Topmost ? "topmost" : "";
                    Process.Start(pwdgen, args);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        void ShowSettings()
        {
            try
            {
                if (!ReenterPassword())
                {
                    return;
                }
                var w = new SettingsWindow(this);
                if (w.ShowDialog() == true)
                {
                    autoClearClipboardAfterSec = Properties.Settings.Default.AutoClearClipboard;
                    autoHidePasswordAfterSec = Properties.Settings.Default.AutoHidePassword;
                    reenterPasswordAfterSec = Properties.Settings.Default.ReenterPassword;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void HandleError(Exception ex)
        {
            MessageBox.Show(
                this,
                string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message),
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
