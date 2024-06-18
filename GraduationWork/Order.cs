using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationWork
{
    internal class Order
    {
        public NpgsqlConnection connection;
        static readonly string direction = "User Id = postgres; Password=1111;Host=localhost;Port=5432; Database=graduationwork_db;";
        public int Price { get; set; }
        public string Place { get; set; }
        public string OrderCustomerName { get; set; } 
        public string OrderDishesList { get; set; }
        public int CustomerId { get; set; }

        public Order(int price, string place, string orderCustomerName, string orderDishesList, int customerId)
        {
            connection = new NpgsqlConnection(direction);
            connection.Open();
            Price = price;
            Place = place;
            OrderCustomerName = orderCustomerName;
            OrderDishesList = orderDishesList;
            CustomerId = customerId;
        }

        public void ExecuteAddQuery()
        {
            string query = $"insert into public.orders(order_id, order_price, order_place ,order_customer_name," +
                $"order_dishes_list, order_customer_id)values(NEXTVAL('s_orders'), '{Price}', '{Place}', '{OrderCustomerName}', '{OrderDishesList}', '{CustomerId}')";
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }
    } 
}
