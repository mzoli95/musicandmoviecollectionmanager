using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace MusicListSorter
{
    public partial class AddNewRecordWindow : Window
    {
        private string dbFilePath = "D://music-list.db";

        public AddNewRecordWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string band = bandTextBox.Text.Trim();
            string releaseDate = releaseDateTextBox.Text.Trim();
            string diskNumber = diskNumberTextBox.Text.Trim();
            string title = titleTextBox.Text.Trim();

            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
            {
                conn.Open();
                string sqlQuery = "INSERT INTO music (Band, ReleaseDate, DiskNumber, isAlbum, Title) " +
                                  "VALUES (@band, @releaseDate, @diskNumber, @isAlbum, @title); " +
                                  "SELECT last_insert_rowid();";
                using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@band", band);
                    cmd.Parameters.AddWithValue("@releaseDate", releaseDate);
                    cmd.Parameters.AddWithValue("@diskNumber", diskNumber);
                    cmd.Parameters.AddWithValue("@isAlbum", isAlbumCheckBox.IsChecked.HasValue && isAlbumCheckBox.IsChecked.Value ? 1 : 0);
                    cmd.Parameters.AddWithValue("@title", title);

                    long newID = (long)cmd.ExecuteScalar();
                    MessageBox.Show($"New item added successfully: {newID}");
                }
            }


            this.Close();
        }
    }
}
