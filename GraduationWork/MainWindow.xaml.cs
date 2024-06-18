using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
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
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Win32;
using Npgsql;

namespace GraduationWork
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NpgsqlConnection connection;
        public static readonly string direction = "User Id = postgres; Password=1111;Host=localhost;Port=5432; Database=graduationwork_db;";
        int currentEstablishment = 0;
        int establishmentCounter = 0;
        int ordersCounter = 0;
        int dishCounter = 0;
        UserRecord currentUserRecord = null;
        List<EstablishmentRecord> establishmentRecords = new List<EstablishmentRecord>();
        List<DishRecord> dishRecords = new List<DishRecord>();
        List<Order> orders = new List<Order>();
        List<int> dishesIndexes = new List<int>();
        EstablishmentRecord curr_est;
        DishRecord current_dish;
        
        public MainWindow(UserRecord record)
        {
            InitializeComponent();
            connection = new NpgsqlConnection(direction);
            connection.Open();
            currentUserRecord = record;
            currentUserRecord.UpdatePhotoSource(record.UserLogin == "admin" ? "admins" : "customers", record.UserLogin == "admin" ? "admin_id" : "customer_id");
            SetProfileData();
            SetWindow();
            GetEstablishments();
            GetDishes();
            GenerateEstablishments();

        }

        private void SetWindow()
        {
            if (currentUserRecord.UserLogin != "admin")
            {
                addEstablishmentColumn.Width = new GridLength(0, GridUnitType.Pixel);
                cartColumn.Width = new GridLength(250, GridUnitType.Pixel);
                currentOrders.Height = new GridLength(0, GridUnitType.Pixel);
                myOrders.Height = new GridLength(65, GridUnitType.Pixel);
                choose_photo.Height = 0;
                choose_photo_dish.Height = 0;
            }
            //= currentUserRecord.UserLogin != "admin" ? 0 : choose_photo.Height;
        }
        public void SetProfileData()
        {
            userNameLable.Content = currentUserRecord.UserFirstName;
            userSurnameLable.Content = currentUserRecord.UserSecondName;
            userPhoneLable.Content = currentUserRecord.UserPhoneNumber;
            userMailLable.Content = currentUserRecord.UserEmail;
            usersId.Content = $"id: {currentUserRecord.UserId}";
            usersFullName.Content = $"{currentUserRecord.UserFirstName} {currentUserRecord.UserSecondName}";
            user_photo.Source = new BitmapImage(new Uri(currentUserRecord.photo_source));
            profilePreview.ImageSource = new BitmapImage(new Uri(currentUserRecord.photo_source));
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.Close();
        }
        public void RemoveData(int index, string table, string idType)
        {
            string query = "";
            query = $"DELETE FROM {table} WHERE {idType} = {index}";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        private void RemoveOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var parentGrid = button.Parent as Grid;
            int dishIndex = Convert.ToInt32(button.Tag);
            //int cartItemIndex = Grid.GetRow(button);

            ////cartGrid.RowDefinitions[cartItemIndex].Height = new GridLength(0, GridUnitType.Pixel);
            //var elementsInRow = parentGrid.Children.Cast<UIElement>()
            //                       .Where(element => Grid.GetRow(element) == cartItemIndex)
            //                       .ToList();

            //// Удалить найденные элементы из Grid
            //foreach (var element in elementsInRow)
            //{
            //    parentGrid.Children.Remove(element);
            //}
            //cartGrid.RowDefinitions.RemoveAt(cartItemIndex);
            dishesIndexes.Remove(dishIndex);
            CartRefresh();
            //ordersCounter -= 1;
        }

        private void CartRefresh()
        {
            ordersCounter = 0;
            cartGrid.Children.Clear();
            cartGrid.RowDefinitions.Clear();
            for (int i = 0; i < dishRecords.Count; i++)
            {
                for(int j = 0; j < dishesIndexes.Count; j++)
                {
                    if (dishRecords[i].DishId == dishesIndexes[j])
                    {
                        GenerateOrder(dishesIndexes[j], dishRecords[i].DishName);
                    }
                }
            }
        }
        //private void ClearGridRow(int rowIndex)
        //{
        //    // Найти элементы, которые находятся в заданной строке
        //    var elementsInRow = MyGrid.Children.Cast<UIElement>()
        //                           .Where(element => Grid.GetRow(element) == rowIndex)
        //                           .ToList();

        //    // Удалить найденные элементы из Grid
        //    foreach (var element in elementsInRow)
        //    {
        //        MyGrid.Children.Remove(element);
        //    }
        //}

        public void SequenceRefresh(string sequence, string table, string idType)
        {
            string updateQuery = $"ALTER SEQUENCE {sequence} RESTART; UPDATE {table} SET {idType} = DEFAULT;";
            NpgsqlCommand cmd = new NpgsqlCommand(updateQuery, connection);
            cmd.ExecuteNonQuery();
        }

        private void FoodPlacesBtn_Click(object sender, RoutedEventArgs e)
        {
            CartClear();
            CloseConfirmWindow();
            SetLength();
            searchColumn.Width = new GridLength(620, GridUnitType.Pixel);
            establishmetColumn.Width = new GridLength(620, GridUnitType.Pixel);
            addDishColumn.Width = new GridLength(0, GridUnitType.Pixel);
            if (currentUserRecord.UserLogin == "admin")
                addEstablishmentColumn.Width = new GridLength(250, GridUnitType.Pixel);
        }

        private void SetLength()
        {
            establishmentInfo.Width = new GridLength(0, GridUnitType.Pixel);
            dishInfo.Width = new GridLength(0, GridUnitType.Pixel);
            profileInfo.Width = new GridLength(0, GridUnitType.Pixel);
            statistics.Width = new GridLength(0, GridUnitType.Pixel);
            searchColumn.Width = new GridLength(0, GridUnitType.Pixel);
            establishmetColumn.Width = new GridLength(0, GridUnitType.Pixel);
            dishColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void SetLengthManagingColumn()
        {
            addEstablishmentColumn.Width = new GridLength(0, GridUnitType.Pixel);
            addDishColumn.Width = new GridLength(0, GridUnitType.Pixel);
            cartColumn.Width = new GridLength(0, GridUnitType.Pixel);
            messageColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {
            CartClear();
            CloseConfirmWindow();
            SetLength();
            profileInfo.Width = new GridLength(620, GridUnitType.Pixel);
        }

        private void OrdersPageBtn_Click(object sender, RoutedEventArgs e)
        {
            CartClear();
            orders.Clear();
            CloseConfirmWindow();
            data.ItemsSource = null;
            data.Items.Clear();
            SetLength();
            statistics.Width = new GridLength(620, GridUnitType.Pixel);
            GetOrders();
            if(currentUserRecord.UserLogin == "admin")
            {
                foreach (Order order in orders)
                {
                    data.Items.Add(order);
                }
            }
            else
            {
                foreach (Order order in orders)
                {
                    if(order.CustomerId == currentUserRecord.UserId)
                    {
                        data.Items.Add(order);
                    }
                }
            }
        }

        public void GetOrders()
        {
            string query = $"SELECT * FROM orders";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    orders.Add(new Order(
                    (int)reader["order_price"],
                    reader["order_place"].ToString(),
                    reader["order_customer_name"].ToString(),
                    reader["order_dishes_list"].ToString(),
                    (int)reader["order_customer_id"]));
                }
            }
        }

        public void ClearFields()
        {
            establishmentName.Text = "";
            establishmentAddress.Text = "";
            establishmentPhoneNumber.Text = "";
            establishmentCuisineType.Text = "";
            establishmentOwnerName.Text = "";
            dishName.Text = "";
            dishPrice.Text = "";
            dishPortion.Text = "";
            dishRecipe.Text = "";
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string template = searchBox.Text.Trim();
            string query = "";
            GridLength length = new GridLength(0, GridUnitType.Pixel);
            if(establishmetColumn.Width != length)
            {
                establishmentGrid.Children.Clear();
                establishmentRecords.Clear();
                query = $"SELECT * FROM establishments WHERE establishment_name LIKE'%{template}%' OR establishment_cuisine_type LIKE'%{template}%' OR establishment_address LIKE'%{template}%' OR establishment_owner LIKE'%{template}%'";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        establishmentRecords.Add(new EstablishmentRecord(
                        (int)reader["establishment_id"],
                        reader["establishment_name"].ToString(),
                        reader["establishment_phone_number"].ToString(),
                        reader["establishment_address"].ToString(),
                        reader["establishment_cuisine_type"].ToString(),
                        reader["establishment_owner"].ToString(),
                        reader["photo_source"].ToString()));
                    }
                }
                GenerateEstablishments();
                //establishmentGridScroll.Height = establishmentGrid.ActualHeight;
            }
            else
            {
                dishGrid.Children.Clear();
                dishRecords.Clear();
                query = $"SELECT * FROM dishes WHERE dish_name LIKE'%{template}%' OR dish_recipe LIKE'%{template}%'";
                NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dishRecords.Add(new DishRecord(
                        (int)reader["dish_id"],
                        reader["dish_name"].ToString(),
                        (int)reader["dish_price"],
                        (int)reader["dish_portion"],
                        reader["dish_recipe"].ToString(),
                        (int)reader["dish_establishment_id"],
                        reader["photo_source"].ToString()));
                    }
                }
                GenerateDishes(currentEstablishment);
                //dishGridScroll.Height = dishGrid.ActualHeight;
            }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            GridLength length = new GridLength(0, GridUnitType.Pixel);
            if (establishmetColumn.Width != length)
            {
                establishmentCounter = 0;
                establishmentRecords.Clear();
                establishmentGrid.Children.Clear();
                establishmentGrid.RowDefinitions.Clear();
                GetEstablishments();
                GenerateEstablishments();
                establishmentGridScroll.Height = establishmentGrid.ActualHeight;
            }
            else
            {
                dishRecords.Clear();
                dishGrid.Children.Clear();
                GetDishes();
                GenerateDishes(currentEstablishment);
                dishGridScroll.Height = dishGrid.ActualHeight;
            }
        }

        private void LogOutBtn_Click(object sender, RoutedEventArgs e)
        {
            CartClear();
            CloseConfirmWindow();
            LoginWindow reLogin = new LoginWindow();
            reLogin.Show();
            mainWindow.Close();
        }



        private void EstablishmentInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            SetLength();
            establishmentInfo.Width = new GridLength(620, GridUnitType.Pixel);
            Button button = sender as Button;
            int establishmentIndex = (int)button.Tag;
            curr_est = establishmentRecords.Find(x => x.EstablishmentId == (int)((Button)sender).Tag);
            SetEstablishmentData(establishmentIndex);
        }
        private void EstablishmentMenuBtn_Click(object sender, RoutedEventArgs e)
        {

            SetLength();
            searchColumn.Width = new GridLength(620, GridUnitType.Pixel);
            if (currentUserRecord.UserLogin == "admin")
                addDishColumn.Width = new GridLength(250, GridUnitType.Pixel);
            dishColumn.Width = new GridLength(620, GridUnitType.Pixel);
            addEstablishmentColumn.Width = new GridLength(0, GridUnitType.Pixel);
            Button button = sender as Button;
            var parentGrid = button.Parent as Grid;
            int establishmentIndex = (int)button.Tag;
            currentEstablishment = establishmentIndex;
            GenerateDishes(establishmentIndex);

        }
        private void EstablishmentRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            string query = "";
            Button button = sender as Button;
            int establishmentIndex = (int)button.Tag;
            query = $"DELETE FROM establishments WHERE establishment_id = {establishmentIndex}";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            establishmentRecords.Clear();
            establishmentGrid.Children.Clear();
            GetEstablishments();
            dishRecords.Clear();
            GetDishes();
            GenerateEstablishments();
        }
        private void GenerateEstablishment(string establishmentName, int establishmentId)
        {
            Brush br = new SolidColorBrush(Color.FromRgb(173, 216, 230));
            Rectangle rectangle = new Rectangle();
           
            Label name = new Label();
            name.Height = 40;
            //name.Width = 120;
            name.Content = $@"""{establishmentName}""";
            name.FontSize = 13;
            name.Margin = new Thickness(0, 0, 300, 0);
            name.VerticalContentAlignment = VerticalAlignment.Center;
            name.HorizontalContentAlignment = HorizontalAlignment.Center;

            EstablishmentRecord record = establishmentRecords.Find(x => establishmentId == x.EstablishmentId);
            record.avgPrice = GetAvgPrice(record);
            
            record.averageLabel.Content = $@"Avg. Price: {record.avgPrice} UAH";
            record.averageLabel.FontSize = 11;
            record.averageLabel.Margin = new Thickness(0, 30, 300, 0);
            record.averageLabel.VerticalContentAlignment = VerticalAlignment.Center;
            record.averageLabel.HorizontalContentAlignment = HorizontalAlignment.Center;


            rectangle.Height = 100;
            rectangle.Width = 580;
            rectangle.RadiusX = 20;
            rectangle.RadiusY = 20;
            rectangle.Stroke = br;
            rectangle.Margin = new Thickness(0, 10, 0, 0);
            rectangle.StrokeThickness = 2;
            Button btn2 = new Button();
            btn2.Click += EstablishmentInfoBtn_Click;
            btn2.Width = 80;
            btn2.Height = 25;
            btn2.Margin = new Thickness(450, 0, 0, 0);
            btn2.Style = (Style)Application.Current.FindResource("AvgButton1");
            btn2.Tag = establishmentId;
            btn2.Content = "Info";
            Button btn3 = new Button();
            btn3.Click += EstablishmentMenuBtn_Click;
            btn3.Width = 80;
            btn3.Height = 25;
            btn3.Margin = new Thickness(250, 0, 0, 0);
            btn3.Style = (Style)Application.Current.FindResource("AvgButton1");
            btn3.Content = "Show menu";
            btn3.Tag = establishmentId;
            Image img1 = new Image();
            img1.Height = 50;
            img1.Width = 50;
            img1.Source = new BitmapImage(new Uri("pack://application:,,,/Images/free-icon-cafe-2098374.png"));
            img1.Margin = new Thickness(0, 0, 500, 0);
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(120);
            establishmentGrid.RowDefinitions.Add(rowDefinition);
            establishmentGrid.Children.Add(rectangle);
            establishmentGrid.Children.Add(btn2);
            establishmentGrid.Children.Add(btn3);
            establishmentGrid.Children.Add(img1);
            establishmentGrid.Children.Add(name);
            establishmentGrid.Children.Add(record.averageLabel);
            Grid.SetRow(rectangle, establishmentCounter);
            Grid.SetRow(btn2, establishmentCounter);
            Grid.SetRow(btn3, establishmentCounter);
            Grid.SetRow(img1, establishmentCounter);
            Grid.SetRow(name, establishmentCounter);
            Grid.SetRow(record.averageLabel, establishmentCounter);
            if (currentUserRecord.UserLogin == "admin")
            {
                Button btn1 = new Button();
                btn1.Click += EstablishmentRemoveBtn_Click;
                btn1.Tag = establishmentId;
                btn1.Width = 80;
                btn1.Height = 25;
                btn1.Margin = new Thickness(50, 0, 0, 0);
                btn1.Content = "Remove";
                btn1.Style = (Style)Application.Current.FindResource("AvgButton1");
                establishmentGrid.Children.Add(btn1);
                Grid.SetRow(btn1, establishmentCounter);
            }
            establishmentCounter += 1;
        }

        private int GetAvgPrice(EstablishmentRecord establishment)
        {
            int sum = 0;
            List<DishRecord> menu = new List<DishRecord>();
            menu = dishRecords.FindAll(x => x.DishEstablishmentId == establishment.EstablishmentId);
            menu.ForEach(x => sum += x.DishPrice);
            return menu.Count != 0 ? sum / menu.Count : 0;
        }

        private void GenerateEstablishments()
        {
            establishmentGrid.Children.Clear();
            establishmentGrid.RowDefinitions.Clear();
            for (int i = 0; i < establishmentRecords.Count; i++)
            {
                GenerateEstablishment(establishmentRecords[i].EstablishmentName, establishmentRecords[i].EstablishmentId);
            }
            establishmentCounter = 0;
        }
        private void GenerateEstablishmentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckEstablishmentFields() == true)
            {
                MessageBox.Show("Fill all the fields");
            }
            else
            {
                string name = $"{establishmentName.Text.Trim()}";
                string address = establishmentAddress.Text.Trim();
                string phoneNumber = establishmentPhoneNumber.Text.Trim();
                string cuisineType = establishmentCuisineType.Text.Trim();
                string owner = establishmentOwnerName.Text.Trim();
                AddEstablishment(name, address, phoneNumber, cuisineType, owner);
                ClearFields();
            }
        }
        public void GetEstablishments()
        {
            establishmentRecords.Clear();
            string query = $"SELECT * FROM establishments";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    establishmentRecords.Add(new EstablishmentRecord(
                    (int)reader["establishment_id"],
                    reader["establishment_name"].ToString(),
                    reader["establishment_phone_number"].ToString(),
                    reader["establishment_address"].ToString(),
                    reader["establishment_cuisine_type"].ToString(),
                    reader["establishment_owner"].ToString(),
                    reader["photo_source"].ToString()));
                }
            }
        }
        public void AddEstablishment(string establishmentName, string establishmentAddress, string establishmentPhoneNumber, string establishmentCuisineType, string establishmentOwnerName)
        {
            string query = $"insert into public.establishments(establishment_id, establishment_name, establishment_phone_number ,establishment_address," +
                $"establishment_cuisine_type, establishment_owner)values(NEXTVAL('s_establishments'), '{establishmentName}', '{establishmentPhoneNumber}', '{establishmentAddress}', '{establishmentCuisineType}', '{establishmentOwnerName}')";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            establishmentRecords.Clear();
            establishmentGrid.Children.Clear();
            GetEstablishments();
            GenerateEstablishments();
        }
        private void SetEstablishmentData(int establishmentIndex)
        {
            for(int i = 0; i < establishmentRecords.Count; i++)
            {
                if (establishmentRecords[i].EstablishmentId == establishmentIndex)
                {
                    establishmentNameLable.Content = establishmentRecords[i].EstablishmentName;
                    establishmentPhoneLable.Content = establishmentRecords[i].EstablishmentPhoneNumber;
                    establishmentAddressLable.Content = establishmentRecords[i].EstablishmentAddress;
                    establishmentOwnerLable.Content = establishmentRecords[i].EstablishmentOwner;
                    try
                    {
                        estab_img.Source = new BitmapImage(new Uri(establishmentRecords[i].photo_source));
                    }
                    catch (Exception)
                    {
                        establishmentRecords[i].photo_source = @"pack://application:,,,/Images/free-icon-cafe-2098374.png";
                    }
                }
            }
        }

        private void DishInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            SetLength();
            dishInfo.Width = new GridLength(620, GridUnitType.Pixel);
            Button button = sender as Button;
            var parentGrid = button.Parent as Grid;
            int dishIndex = (int)button.Tag;
            current_dish = dishRecords.Find(x => x.DishId == (int)((Button)sender).Tag);
            SetDishData(dishIndex);
        }
        private void DishRemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            string query = "";
            SequenceRefresh("s_dishes", "dishes", "dish_id");
            Button button = sender as Button;
            var parentGrid = button.Parent as Grid;
            int dishIndex = (int)button.Tag;
            //query = $"DELETE FROM dishes WHERE dish_id = {dishIndex} AND dish_establishment_id = {currentEstablishment}";
            query = $"DELETE FROM dishes WHERE dish_id = {dishIndex}";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            dishGrid.Children.Clear();
            dishRecords.Clear();
            GetDishes();
            GenerateDishes(currentEstablishment);
            SequenceRefresh("s_dishes", "dishes", "dish_id");

        }
        private void GenerateDish(string dishName, int dishPrice, int dishPortion, int dishId)
        {
            Brush br = new SolidColorBrush(Color.FromRgb(200, 162, 255));
            Rectangle rectangle = new Rectangle();
            Label name = new Label();
            name.Height = 30;
            name.Width = 100;
            name.Content = dishName;
            name.Margin = new Thickness(0, 0, 300, 50);
            Label price = new Label();
            price.Height = 30;
            price.Width = 100;
            price.Content = $"Price: {dishPrice} UAH";
            price.Margin = new Thickness(0, 0, 300, 0);
            Label portion = new Label();
            portion.Height = 30;
            portion.Width = 100;
            portion.Content = $"Portion: {dishPortion} g;";
            portion.Margin = new Thickness(0, 50, 300, 0);
            rectangle.Height = 100;
            rectangle.Width = 580;
            rectangle.RadiusX = 20;
            rectangle.RadiusY = 20;
            rectangle.Stroke = br;
            rectangle.Margin = new Thickness(0, 10, 0, 0);
            rectangle.StrokeThickness = 2;
            Button btn2 = new Button();
            btn2.Click += DishInfoBtn_Click;
            btn2.Width = 80;
            btn2.Height = 25;
            btn2.Margin = new Thickness(450, 0, 0, 0);
            btn2.Content = "Info";
            btn2.Style = (Style)Application.Current.FindResource("AvgButton1");
            btn2.Tag = (int)dishId;
            Image img1 = new Image();
            img1.Height = 50;
            img1.Width = 50;
            img1.Source = new BitmapImage(new Uri("pack://application:,,,/Images/free-icon-serving-dish-1046874.png"));
            img1.Margin = new Thickness(0, 0, 500, 0);
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(120);
            dishGrid.RowDefinitions.Add(rowDefinition);
            dishGrid.Children.Add(rectangle);
            dishGrid.Children.Add(btn2);
            dishGrid.Children.Add(img1);
            dishGrid.Children.Add(name);
            dishGrid.Children.Add(price);
            dishGrid.Children.Add(portion);
            Grid.SetRow(rectangle, dishCounter);
            Grid.SetRow(btn2, dishCounter);
            Grid.SetRow(img1, dishCounter);
            Grid.SetRow(name, dishCounter);
            Grid.SetRow(price, dishCounter);
            Grid.SetRow(portion, dishCounter);
            if (currentUserRecord.UserLogin == "admin")
            {
                Button btn1 = new Button();
                btn1.Click += DishRemoveBtn_Click;
                btn1.Width = 80;
                btn1.Height = 25;
                btn1.Margin = new Thickness(250, 0, 0, 0);
                btn1.Content = "Remove";
                btn1.Style = (Style)Application.Current.FindResource("AvgButton1");
                btn1.Tag = (int)dishId;
                dishGrid.Children.Add(btn1);
                Grid.SetRow(btn1, dishCounter);
            }
            else
            {
                Button btn3 = new Button();
                btn3.Click += AddToCartBtn_Click;
                btn3.Width = 80;
                btn3.Height = 25;
                btn3.Margin = new Thickness(250, 0, 0, 0);
                btn3.Content = "Add to cart";
                btn3.Style = (Style)Application.Current.FindResource("AvgButton1");
                dishGrid.Children.Add(btn3);
                btn3.Tag = (int)dishId;
                Grid.SetRow(btn3, dishCounter);
            }
            dishCounter += 1;
        }
        private void GenerateDishes(int establishmentId)
        {
            dishGrid.Children.Clear();
            dishGrid.RowDefinitions.Clear();
            for (int i = 0; i < dishRecords.Count; i++)
            {
                GenerateDish(dishRecords[i].DishName, dishRecords[i].DishPrice, dishRecords[i].DishPortion, dishRecords[i].DishId);
                if (dishRecords[i].DishEstablishmentId != establishmentId)
                {
                    dishGrid.RowDefinitions[i].Height = new GridLength(0, GridUnitType.Pixel);
                }
                else
                {
                    dishGrid.RowDefinitions[i].Height = new GridLength(120, GridUnitType.Pixel);
                }
            }
            dishCounter = 0;
        }
        private void GenerateDishBtn_Click(object sender, RoutedEventArgs e)
        {
            if(CheckDishFields() == true)
            {
                MessageBox.Show("Fill all the fields");
            }
            else
            {
                string name = dishName.Text.Trim();
                int price = Convert.ToInt32(dishPrice.Text.Trim());
                int portion = Convert.ToInt32(dishPortion.Text.Trim());
                string recipe = dishRecipe.Text;
                AddDish(name, price, portion, recipe);
                EstablishmentRecord record = establishmentRecords.Find(x => currentEstablishment == x.EstablishmentId);
                record.avgPrice = GetAvgPrice(record);
                record.averageLabel.Content = $@"Avg. Price: {record.avgPrice} UAH";
                ClearFields();
            }
        }
        public void GetDishes()
        {
            string query = $"SELECT * FROM dishes";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    dishRecords.Add(new DishRecord(
                    (int)reader["dish_id"],
                    reader["dish_name"].ToString(),
                    (int)reader["dish_price"],
                    (int)reader["dish_portion"],
                    reader["dish_recipe"].ToString(),
                    (int)reader["dish_establishment_id"],
                     reader["photo_source"].ToString()));
                }
            }
        }
        public void AddDish(string name, int price, int portion, string recipe)
        {
            string query = $"insert into public.dishes(dish_id, dish_name, dish_price, dish_portion, dish_recipe, dish_establishment_id)values(NEXTVAL('s_dishes'), '{name}', {price}, {portion}, '{recipe}', {currentEstablishment})";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
            dishRecords.Clear();
            dishGrid.Children.Clear();
            GetDishes();
            GenerateDishes(currentEstablishment);
        }
        private void SetDishData(int dishIndex)
        {
            for (int i = 0; i < dishRecords.Count; i++)
            {
                if (currentEstablishment == dishRecords[i].DishEstablishmentId && dishIndex == dishRecords[i].DishId)
                {
                    dishNameLable.Content = dishRecords[i].DishName;
                    dishPriceLable.Content = dishRecords[i].DishPrice;
                    dishPortionLable.Content = dishRecords[i].DishPortion;
                    dishRecipeLable.Text = dishRecords[i].DishRecipe;
                    dish_picture.Source = new BitmapImage(new Uri(current_dish.photo_source));
                }
            }
        }


        private void AddToCartBtn_Click(object sender, RoutedEventArgs e)
        {
            string dishName = "";
            Button button = sender as Button;
            var parentGrid = button.Parent as Grid;
            int pickedDish = (int)button.Tag;
            dishesIndexes.Add(pickedDish);
            for(int i = 0; i < dishRecords.Count; i++)
            {
                if (dishRecords[i].DishId == pickedDish)
                {
                    dishName = dishRecords[i].DishName;
                    break;
                }
            }
            cartColumn.Width = new GridLength(250, GridUnitType.Pixel);
            GenerateOrder(pickedDish, dishName);
        }
        private void GenerateOrder(int pickedDish, string dishName)
        {
            Brush br = new SolidColorBrush(Color.FromRgb(173, 216, 230));
            Rectangle rectangle = new Rectangle();
            Label orderLable = new Label();
            orderLable.Height = 30;
            orderLable.Width = 150;
            orderLable.Content = $"{dishName}     x1";
            orderLable.Margin = new Thickness(0, 0, 0, 0);
            rectangle.Height = 70;
            rectangle.Width = 230;
            rectangle.RadiusX = 20;
            rectangle.RadiusY = 20;
            rectangle.Stroke = br;
            rectangle.Margin = new Thickness(0, 10, 0, 0);
            rectangle.StrokeThickness = 2;
            Button btn2 = new Button();
            btn2.Click += RemoveOrderBtn_Click;
            btn2.Width = 25;
            btn2.Height = 25;
            btn2.Tag = pickedDish;
            btn2.Margin = new Thickness(150, 0, 0, 0);
            btn2.Content = "×";
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(100);
            cartGrid.RowDefinitions.Add(rowDefinition);
            cartGrid.Children.Add(rectangle);
            cartGrid.Children.Add(btn2);
            cartGrid.Children.Add(orderLable);
            Grid.SetRow(rectangle, ordersCounter);
            Grid.SetRow(btn2, ordersCounter);
            Grid.SetRow(orderLable, ordersCounter);
            ordersCounter += 1;

        }
        private void MakeOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ordersCounter == 0)
            {
                MessageBox.Show("Your cart is empty");
            }
            else
            {
                ordersCounter = 0;
                string str = "";
                int price = 0;
                cartGrid.Children.Clear();
                List<DishRecord> orderedDishes = new List<DishRecord>();
                for (int i = 0; i < dishesIndexes.Count; i++)
                {
                    string query = $"SELECT * FROM dishes WHERE dish_id = {dishesIndexes[i]}";
                    NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderedDishes.Add(new DishRecord(
                            (int)reader["dish_id"],
                            reader["dish_name"].ToString(),
                            (int)reader["dish_price"],
                            (int)reader["dish_portion"],
                            reader["dish_recipe"].ToString(),
                            (int)reader["dish_establishment_id"],
                             reader["photo_source"].ToString()));
                        }
                    }
                }

                for (int i = 0; i < orderedDishes.Count; i++)
                {
                    str += $"{orderedDishes[i].DishName} - {orderedDishes[i].DishPrice}\n";
                    price += orderedDishes[i].DishPrice;
                }

                string establishmentName = "";
                string query1 = $"SELECT establishment_name FROM establishments WHERE establishment_id = {currentEstablishment}";
                NpgsqlCommand cmd1 = new NpgsqlCommand(query1, connection);
                using (var reader = cmd1.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        establishmentName = reader.GetString(0);
                    }
                }
                Order newOrder = new Order(price, establishmentName, $"{currentUserRecord.UserFirstName} {currentUserRecord.UserSecondName}", str, currentUserRecord.UserId);
                newOrder.ExecuteAddQuery();
                messageColumn.Width = new GridLength(250, GridUnitType.Pixel);
                cartColumn.Width = new GridLength(0, GridUnitType.Pixel);
                nameLabel.Content = $"{currentUserRecord.UserFirstName} {currentUserRecord.UserSecondName}";
                orderPriceLabel.Content = $"{price} UAH";
                CartClear();
            }

        }

        private void CartClear()
        {
            cartGrid.Children.Clear();
            cartGrid.RowDefinitions.Clear();
            dishesIndexes.Clear();
            ordersCounter = 0;
        }

        private void CloseConfirmWindow()
        {
            messageColumn.Width = new GridLength(0, GridUnitType.Pixel);
            cartColumn.Width = new GridLength(250, GridUnitType.Pixel);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            CloseConfirmWindow();
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

        private bool CheckDishFields()
        {
            if (dishName.Text == "" || dishPrice.Text == "" || dishPortion.Text == "" || dishRecipe.Text == "")
                return true;
            else
                return false;
        }

        private bool CheckEstablishmentFields()
        {
            if (establishmentName.Text == "" || establishmentAddress.Text == "" || establishmentPhoneNumber.Text == "" || establishmentCuisineType.Text == "" || establishmentOwnerName.Text == "")
                return true;
            else
                return false;
        }

        private void choose_photo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            try
            {
                estab_img.Source = new BitmapImage(new Uri(dialog.FileName));
                curr_est.photo_source = dialog.FileName;
                curr_est.UpdatePhotoSource();
            }
            catch (Exception)
            {
                MessageBox.Show("File does not exist!","Warning!",MessageBoxButton.OK,MessageBoxImage.Warning);
            }
        }

        private void choose_photo_profile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            try
            {
                user_photo.Source = new BitmapImage(new Uri(dialog.FileName));
                currentUserRecord.photo_source = dialog.FileName;
                currentUserRecord.UpdatePhotoSource(currentUserRecord.UserLogin == "admin" ? "admins" : "customers", currentUserRecord.UserLogin == "admin" ? "admin_id" : "customer_id");
                currentUserRecord.SetProfileData(userNameLable,userSurnameLable,userPhoneLable,userMailLable,usersId,usersFullName,user_photo,profilePreview);
            }
            catch (Exception)
            {
                MessageBox.Show("File does not exist!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void choose_photo_dish_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            try
            {
                dish_picture.Source = new BitmapImage(new Uri(dialog.FileName));
                current_dish.photo_source = dialog.FileName;
                current_dish.UpdatePhotoSource();
            }
            catch (Exception)
            {
                MessageBox.Show("File does not exist!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
