using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows;
using System;

namespace PasswordManager
{
    public partial class HistoryWindow : Window
    {
        string connectionString;
        SqlDataAdapter adapter;
        DataTable cardsTable;
        string user;

        public HistoryWindow(Window owner, string str)
        {
            user = str;
            InitializeComponent();
           
                
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            string sql = "SELECT * FROM Cards where Username ='" + user +"'";
            cardsTable = new DataTable();
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=LoginDB;Integrated Security=True");
                SqlCommand command = new SqlCommand(sql, connection);
                adapter = new SqlDataAdapter(command);

                // установка команды на добавление для вызова хранимой процедуры
                adapter.InsertCommand = new SqlCommand("sp_InsertCard", connection);
                adapter.InsertCommand.CommandType = CommandType.StoredProcedure;
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@card", SqlDbType.NVarChar, 50, "card"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@pin", SqlDbType.Int, 0, "pin"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@ivc", SqlDbType.Int, 0, "ivc"));
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@username", SqlDbType.NVarChar, 50, "Username"));
                SqlParameter parameter = adapter.InsertCommand.Parameters.Add("@id", SqlDbType.Int, 0, "id");
                parameter.Direction = ParameterDirection.Output;

                connection.Open();
                adapter.Fill(cardsTable);
                cardsGrid.ItemsSource = cardsTable.DefaultView;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        private void UpdateDB()
        {
            SqlCommandBuilder comandbuilder = new SqlCommandBuilder(adapter);
            adapter.Update(cardsTable);
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateDB();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (cardsGrid.SelectedItems != null)
            {
                for (int i = 0; i < cardsGrid.SelectedItems.Count; i++)
                {
                    DataRowView datarowView = cardsGrid.SelectedItems[i] as DataRowView;
                    if (datarowView != null)
                    {
                        DataRow dataRow = (DataRow)datarowView.Row;
                        dataRow.Delete();
                    }
                }
            }
            UpdateDB();
        }
    }
}
