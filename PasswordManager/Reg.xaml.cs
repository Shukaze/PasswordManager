using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace PasswordManager
{
    /// <summary>
    /// Логика взаимодействия для Reg.xaml
    /// </summary>
    public partial class Reg : Window
    {
        public Reg()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (login.Text.Length > 0) // проверяем логин
            {
                if (P1.Password.Length > 0) // проверяем пароль
                {
                    if (P2.Password.Length > 0) // проверяем второй пароль
                    {
                        if (P1.Password == P2.Password) // проверка на совпадение паролей
                        {
                            SqlConnection sqlCon = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=LoginDB;Integrated Security=True");
                            {


                
                    try
                    {

                        sqlCon.Open();
                                 string dt_user = string.Format("SELECT * FROM [tblUser] WHERE [Username] = '" + login.Text + "'");
                                    SqlCommand command = new SqlCommand(dt_user,sqlCon);
                                    if (command.ExecuteScalar() != null) // если такая запись существует
                                    {
                                        MessageBox.Show("Пользователь уже зарегистрирован."); // говорим, что авторизовался
                                    }
                                    else
                                    {
                                        string strSQL = "INSERT INTO [tblUser](Username, Password) VALUES ('" + login.Text + "', '" + P1.Password + "')";
                                        SqlCommand cmd = new SqlCommand(strSQL, sqlCon);
                                        if (cmd.ExecuteNonQuery() == 1)
                                            MessageBox.Show("Пользователь зарегистрирован.");
                                   }
                        sqlCon.Close();

                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                            }
                        }
                            else MessageBox.Show("Пароли не совпадают");
                    }
                        else MessageBox.Show("Повторите пароль");
                 }else MessageBox.Show("Укажите пароль");
            }else MessageBox.Show("Укажите логин");
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var win = new LoginScreen(this);
            win.Visibility = Visibility.Visible;
            this.Close();
        }
    }
}
