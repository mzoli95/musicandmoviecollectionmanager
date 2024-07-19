using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace MusicListSorter
{
    public partial class EditWindow : Window
    {
        private DataRowView dataRowView;

        public EditWindow(DataRowView rowView)
        {
            InitializeComponent();
            dataRowView = rowView;

            bandTextBox.Text = rowView["Band"].ToString();
            releaseDateTextBox.Text = rowView["releaseDate"].ToString();
            diskNumberTextBox.Text = rowView["diskNumber"].ToString();
            object isAlbumValue = rowView["isAlbum"];
            if (isAlbumValue != DBNull.Value)
            {
                bool isAlbum = Convert.ToBoolean(isAlbumValue);
                isAlbumCheckBox.IsChecked = isAlbum;
            }
            else
            {
                isAlbumCheckBox.IsChecked = null;
            }
            titleTextBox.Text = rowView["Title"].ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string band = bandTextBox.Text.Trim();
            string title = titleTextBox.Text.Trim();
            string releaseDate = releaseDateTextBox.Text.Trim();
            string diskNumber = diskNumberTextBox.Text.Trim();


            using (SQLiteConnection conn = new SQLiteConnection($"Data Source=D://music-list.db;Version=3;"))
            {
                conn.Open();
                string sqlQuery = "UPDATE music SET Band = @band, Title = @title WHERE Id = @id";
                using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@band", band);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@releaseDate", releaseDate);
                    cmd.Parameters.AddWithValue("@diskNumber", diskNumber);
                    cmd.Parameters.AddWithValue("@isAlbum", isAlbumCheckBox.IsChecked.HasValue && isAlbumCheckBox.IsChecked.Value ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", dataRowView["Id"]);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("A változtatások mentve.");
            Close();
        }
    }
}
