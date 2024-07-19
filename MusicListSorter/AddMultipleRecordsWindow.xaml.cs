using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows;
using ExcelDataReader;

namespace MusicListSorter
{
    public partial class AddMultipleRecordsWindow : Window
    {
        public AddMultipleRecordsWindow()
        {
            InitializeComponent();
        }

        private void ImportFromExcel_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".xlsx";
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
                        {
                            FallbackEncoding = Encoding.GetEncoding(1252), 
                            LeaveOpen = false 
                        }))
                        {
                            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                {
                                    UseHeaderRow = true
                                }
                            });

                            DataTable dataTable = dataSet.Tables[0];

                            SaveToDatabase(dataTable);
                        }
                    }

                    MessageBox.Show("Excel file imported successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void SaveToDatabase(DataTable dataTable)
        {
            string dbFilePath = "D://music-list.db";
            string connectionString = $"Data Source={dbFilePath};Version=3;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;

                        foreach (DataRow row in dataTable.Rows)
                        {
                            command.CommandText = "INSERT INTO Music (Band, Title, ReleaseDate, DiskNumber, isAlbum) " +
                                                  "VALUES (@band, @title, @releaseDate, @diskNumber, @isAlbum)";

                            command.Parameters.AddWithValue("@band", row["Band"]);
                            command.Parameters.AddWithValue("@title", row["Title"]);
                            command.Parameters.AddWithValue("@releaseDate", row["ReleaseDate"].ToString());
                            command.Parameters.AddWithValue("@diskNumber", row["DiskNumber"].ToString());
                            command.Parameters.AddWithValue("@isAlbum", row["isAlbum"]);

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }

                connection.Close();
            }

            MessageBox.Show("Data saved to database.");
        }


        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
