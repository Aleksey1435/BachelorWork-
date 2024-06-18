using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GraduationWork
{
    public class UserRecord
    {
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserSecondName { get; set; }
        public string UserLogin { get; set; }
        public string UserPassword { get; set; }
        public string UserPhoneNumber { get; set; }
        public string UserEmail { get; set; }
        public int ExternalId { get; set; }

        public string photo_source { get; set; }
        public UserRecord(int userId, string userFirstName, string userSecondName, string userLogin,
            string userPassword, string userPhoneNumber, string userEmail, int externalId,string photo_source)
        {
            UserId = userId;
            UserFirstName = userFirstName;
            UserSecondName = userSecondName;
            UserLogin = userLogin;
            UserPassword = userPassword;
            UserPhoneNumber = userPhoneNumber;
            UserEmail = userEmail;
            ExternalId = externalId;
            this.photo_source = photo_source != String.Empty ? photo_source : @"pack://application:,,,/Images/free-icon-user-456283.png";
        }

        public void UpdatePhotoSource(string tableName, string nameId)
        {
            string query = $"UPDATE {tableName} SET photo_source = '{photo_source}' WHERE {tableName}.{nameId} = {UserId};";
            NpgsqlCommand cmd = new NpgsqlCommand(query, MainWindow.connection);
            cmd.ExecuteNonQuery();
        }

        public void SetProfileData(Label userNameLable, Label userSurnameLable, Label userPhoneLable, Label userMailLable, Label usersId, Label usersFullName, Image user_photo, System.Windows.Media.ImageBrush profilePreview)
        {
            userNameLable.Content = UserFirstName;
            userSurnameLable.Content = UserSecondName;
            userPhoneLable.Content = UserPhoneNumber;
            userMailLable.Content = UserEmail;
            usersId.Content = $"id: {UserId}";
            usersFullName.Content = $"{UserFirstName} {UserSecondName}";
            user_photo.Source = new BitmapImage(new Uri(photo_source));
            profilePreview.ImageSource = new BitmapImage(new Uri(photo_source));
        }
    }
}
