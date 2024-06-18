using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationWork
{
    class DishRecord
    {
        public int DishId { get; set; }
        public string DishName { get; set; }
        public int DishPrice { get; set; }
        public int DishPortion { get; set; }
        public string DishRecipe { get; set; }
        public int DishEstablishmentId { get; set; }
        public string photo_source { get; set; }

        public DishRecord(int dishId, string dishName, int dishPrice, int dishPortion, string dishRecipe, int dishEstablishmentId, string photo_source)
        {
            DishId = dishId;
            DishName = dishName;
            DishPrice = dishPrice;
            DishPortion = dishPortion;
            DishRecipe = dishRecipe;
            DishEstablishmentId = dishEstablishmentId;

            this.photo_source = photo_source != String.Empty ? photo_source : @"pack://application:,,,/Images/free-icon-serving-dish-1046874.png";

            UpdatePhotoSource();
        }

        public async void UpdatePhotoSource()
        {
            string query = $"UPDATE dishes SET photo_source = '{photo_source}' WHERE dishes.dish_id = {DishId};";
            NpgsqlCommand cmd = new NpgsqlCommand(query, MainWindow.connection);
            await Task.Delay(100);
            cmd.ExecuteNonQuery();
        }
    }
}

