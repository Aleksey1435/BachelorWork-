using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GraduationWork
{
    internal class EstablishmentRecord
    {
        public int EstablishmentId { get; set; }
        public string EstablishmentName { get; set; }
        public string EstablishmentPhoneNumber { get; set; }
        public string EstablishmentAddress { get; set; }
        public string EstablishmentCuisineType { get; set; }
        public string EstablishmentOwner { get; set; }
        public string photo_source { get; set; }
        public int avgPrice { get; set; }

        public Label averageLabel = new Label();



        public EstablishmentRecord(int establishmentId, string establishmentName, string establishmentPhoneNumber, string establishmentAddress, string establishmentCuisineType, string establishmentOwner,string photo_source)
        {
            EstablishmentId = establishmentId;
            EstablishmentName = establishmentName;
            EstablishmentPhoneNumber = establishmentPhoneNumber;
            EstablishmentAddress = establishmentAddress;
            EstablishmentCuisineType = establishmentCuisineType;
            EstablishmentOwner = establishmentOwner;
            this.photo_source = photo_source != String.Empty? photo_source : @"pack://application:,,,/Images/free-icon-cafe-2098374.png";

            UpdatePhotoSource();
        }

        public async void UpdatePhotoSource()
        {
            string query = $"UPDATE establishments SET photo_source = '{photo_source}' WHERE establishments.establishment_id = {EstablishmentId};";
            NpgsqlCommand cmd = new NpgsqlCommand(query, MainWindow.connection);
            await Task.Delay(100);
            cmd.ExecuteNonQuery();
        }
    }
}

