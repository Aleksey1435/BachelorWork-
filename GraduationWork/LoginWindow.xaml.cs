using Npgsql;
using System;
using System.Collections.Generic;
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
using static GraduationWork.UserRecord;

namespace GraduationWork
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
        static readonly string direction = "User Id = postgres; Password=1111;Host=localhost;Port=5432; Database=graduationwork_db;";
        public NpgsqlConnection connection;
        List<UserRecord> users = new List<UserRecord>();
        UserRecord currentUser = null;
        public LoginWindow()
        {
            InitializeComponent();
            connection = new NpgsqlConnection(direction);
            connection.Open();
            GetAdmins();
            GetCustomers();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            loginPageColumn.Width = new GridLength(0, GridUnitType.Pixel);
            registerPageColumn.Width = new GridLength(400, GridUnitType.Pixel);
        }

        public void GetAdmins()
        {
            string query = $"SELECT * FROM admins";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new UserRecord(
                    (int)reader["admin_id"],
                    reader["admin_first_name"].ToString(),
                    reader["admin_second_name"].ToString(),
                    reader["admin_login"].ToString(),
                    reader["admin_password"].ToString(),
                    reader["admin_phone_number"].ToString(),
                    reader["admin_mail"].ToString(),
                    (int)reader["admin_user_id"],
                    reader["photo_source"].ToString()));
                }
            }
        }

        public void GetCustomers()
        {
            string query = $"SELECT * FROM customers";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(new UserRecord(
                    (int)reader["customer_id"],
                    reader["customer_first_name"].ToString(),
                    reader["customer_second_name"].ToString(),
                    reader["customer_login"].ToString(),
                    reader["customer_password"].ToString(),
                    reader["customer_phone_number"].ToString(),
                    reader["customer_mail"].ToString(),
                    (int)reader["customer_user_id"],
                    reader["photo_source"].ToString()));
                }
            }
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if(LoginFieldsCheck() == true)
            {
                MessageBox.Show("Fill all the fields");
            }
            else
            {
                string login = insertLoginTextBox.Text;
                string password = insertPasswordTextBox.Password;
                bool isDataCorrect = false;
                for (int i = 0; i < users.Count; i++)
                {
                    if (login == users[i].UserLogin)
                    {
                        if (password == users[i].UserPassword)
                        {
                            currentUser = users[i];
                            ClearFields();
                            StartWindow(currentUser);
                            isDataCorrect = true;
                        }
                    }
                }
                if(!isDataCorrect)
                    MessageBox.Show("Login or Password is incorrect");

            }
            
        }

        private void StartWindow(UserRecord currentUser)
        {
            MainWindow startProgram = new MainWindow(currentUser);
            startProgram.Show();
            loginWindow.Close();
        }

        private void CreateAccountBtn_Click(object sender, RoutedEventArgs e)
        {
            if(RegisterFieldsCheck() == true)
            {
                MessageBox.Show("Fill all the fields");
            }
            else
            {
                string firstName = getName.Text.Trim();
                string secondName = getSurname.Text.Trim();
                string login = getLogin.Text.Trim();
                string password = getPassword.Password.Trim();
                string phoneNumber = getPhoneNumber.Text.Trim();
                string mail = getEmail.Text.Trim();
                if (CheckLogin(login) == true)
                {
                    MessageBox.Show("User with the same login already exists");
                }
                else
                {
                    if (CheckEmail(mail) == true)
                    {
                        MessageBox.Show("User with the same email already exists");
                    }
                    else
                    {
                        if (CheckPhoneNumber(phoneNumber) == true)
                        {
                            MessageBox.Show("User with the samr phone number already exists");
                        }
                        else
                        {
                            AddCustomer(firstName, secondName, login, password, phoneNumber, mail);
                            ClearFields();
                        }
                    }
                }
            }
        }

        private bool CheckLogin(string login)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].UserLogin == login)
                    return true;
            }
            return false;
        }

        private bool CheckEmail(string email)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].UserEmail == email)
                    return true;
            }
            return false;
        }

        private bool CheckPhoneNumber(string phoneNumber)
        {
            for(int i = 0; i < users.Count; i++)
            {
                if (users[i].UserPhoneNumber == phoneNumber)
                    return true;
            }
            return false;
        }

        public void AddCustomer(string firstName, string secondName, string login, string password, string phoneNumber, string mail)
        {
            string regTime = DateTime.Now.ToString();
            string query = $"insert into public.users(user_id, user_registration_date)values(NEXTVAL('s_users'), '{regTime}');";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            string query1 = $"insert into public.customers(customer_id, customer_first_name, customer_second_name, customer_login, customer_password, customer_phone_number, customer_mail, customer_user_id)" +
                $"values(NEXTVAL('s_customers'), '{firstName}', '{secondName}', '{login}', '{password}', '{phoneNumber}' ,'{mail}', CURRVAL('s_users'))";
            NpgsqlCommand cmd1 = new NpgsqlCommand(query1, connection);
            cmd1.ExecuteNonQuery();
            ClearFields();  
        }

        public bool LoginFieldsCheck()
        {
            if(insertLoginTextBox.Text == "" ||  insertPasswordTextBox.Password == "")
                return true;
            else
                return false;
        }
        public bool RegisterFieldsCheck()
        {
            if (getName.Text == "" || getSurname.Text == "" || getPhoneNumber.Text == "" || getLogin.Text == "" || getPassword.Password == "" || getEmail.Text == "")
                return true;
            else
                return false;
        }
        public void ClearFields()
        {
            getName.Text = "";
            getSurname.Text = "";
            getLogin.Text = "";
            getPassword.Password = "";
            getPhoneNumber.Text = "";
            getEmail.Text = "";
            insertLoginTextBox.Text = "";
            insertPasswordTextBox.Password = "";

        }

        private void RegBackBtn_Click(object sender, RoutedEventArgs e)
        {
            loginPageColumn.Width = new GridLength(400, GridUnitType.Pixel);
            registerPageColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void Numbers_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumber(e.Text))
            {
                e.Handled = true;
            }
        }

        private bool IsNumber(string text)
        {
            return int.TryParse(text, out _);
        }
    }
}
