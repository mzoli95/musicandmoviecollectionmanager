using ExcelDataReader;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MusicListSorter
{
    public partial class MainWindow : Window
    {
        private string dbFilePath = "D://music-list.db";
        private int currentPage = 1;
        private int recordsPerPage = 100;
        private int totalRecords = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadMusicData();
        }

        private void LoadMusicData(string searchText = "", string column = "All", string filterText = "")
        {
            string connectionString = $"Data Source={dbFilePath};Version=3;";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                if (!TableExists(conn, "music"))
                {
                    CreateMusicTable(conn);
                }

                string sqlQuery = "SELECT COUNT(*) FROM music";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    sqlQuery += BuildWhereClause(searchText, column, filterText);
                }
                else if (!string.IsNullOrWhiteSpace(filterText))
                {
                    sqlQuery += $" WHERE Band LIKE '{filterText}%'";
                }

                using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                {
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");
                    }

                    totalRecords = Convert.ToInt32(cmd.ExecuteScalar());
                }

                int offset = (currentPage - 1) * recordsPerPage;

                sqlQuery = "SELECT Band, Title, ReleaseDate, DiskNumber, isAlbum, Id FROM music";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    sqlQuery += BuildWhereClause(searchText, column, filterText);
                }
                else if (!string.IsNullOrWhiteSpace(filterText))
                {
                    sqlQuery += $" WHERE Band LIKE '{filterText}%'";
                }

                sqlQuery += " ORDER BY Id LIMIT @recordsPerPage OFFSET @offset";

                using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                {
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");
                    }

                    cmd.Parameters.AddWithValue("@recordsPerPage", recordsPerPage);
                    cmd.Parameters.AddWithValue("@offset", offset);

                    DataTable dt = new DataTable();
                    SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);

                    dataGrid1.ItemsSource = dt.DefaultView;
                    dataGrid2.ItemsSource = dt.DefaultView;

                    UpdatePageInfo();
                }
            }
        }

        private string BuildWhereClause(string searchText, string column, string filterText)
        {
            string whereClause = " WHERE ";

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                whereClause += $"Band LIKE '{filterText}%' AND ";
            }

            switch (column)
            {
                case "Band":
                    whereClause += "Band LIKE @searchText";
                    break;
                case "Title":
                    whereClause += "Title LIKE @searchText";
                    break;
                case "ReleaseDate":
                    whereClause += "ReleaseDate LIKE @searchText";
                    break;
                case "DiskNumber":
                    whereClause += "DiskNumber LIKE @searchText";
                    break;
                case "IsAlbum":
                    whereClause += "isAlbum LIKE @searchText";
                    break;
                default:
                    whereClause += "Band LIKE @searchText OR Title LIKE @searchText OR ReleaseDate LIKE @searchText OR DiskNumber LIKE @searchText";
                    break;
            }

            return whereClause;
        }

        private bool TableExists(SQLiteConnection connection, string tableName)
        {
            using (SQLiteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";

                object result = cmd.ExecuteScalar();

                return (result != null && result.ToString().Equals(tableName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void CreateMusicTable(SQLiteConnection connection)
        {
            using (SQLiteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"CREATE TABLE music (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Band TEXT,
                            Title TEXT,
                            ReleaseDate TEXT,
                            DiskNumber TEXT,
                            isAlbum INTEGER
                           )";

                cmd.ExecuteNonQuery();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            string searchText = searchTextBox.Text.Trim();
            string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string filterText = filterTextBox.Text.Trim();
            LoadMusicData(searchText, selectedColumn, filterText);
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            string filterText = filterTextBox.Text.Trim();
            string searchText = searchTextBox.Text.Trim();
            string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            LoadMusicData(searchText, selectedColumn, filterText);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView rowView)
            {
                EditWindow editWindow = new EditWindow(rowView);
                editWindow.ShowDialog();

                string searchText = searchTextBox.Text.Trim();
                string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string filterText = filterTextBox.Text.Trim();
                LoadMusicData(searchText, selectedColumn, filterText);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView rowView)
            {
                string band = rowView["Band"].ToString();
                string title = rowView["Title"].ToString();

                MessageBoxResult result = MessageBox.Show($"Do you want to delete this item: {band} - {title}?", "Delete Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using (SQLiteConnection conn = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
                    {
                        conn.Open();
                        string sqlQuery = "DELETE FROM music WHERE Id = @id";
                        using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", rowView["Id"]);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string searchText = searchTextBox.Text.Trim();
                    string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    string filterText = filterTextBox.Text.Trim();
                    LoadMusicData(searchText, selectedColumn, filterText);
                }
            }
        }

        private void AddNewRecordButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewRecordWindow addNewRecordWindow = new AddNewRecordWindow();
            addNewRecordWindow.ShowDialog();
            string searchText = searchTextBox.Text.Trim();
            string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string filterText = filterTextBox.Text.Trim();
            LoadMusicData(searchText, selectedColumn, filterText);

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

        }
        private void AddMultipleRecordsButton_Click(object sender, RoutedEventArgs e)
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


                    string searchText = searchTextBox.Text.Trim();
                    string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    string filterText = filterTextBox.Text.Trim();
                    LoadMusicData(searchText, selectedColumn, filterText);

                    MessageBox.Show("Items has been added successfully.");


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                string searchText = searchTextBox.Text.Trim();
                string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string filterText = filterTextBox.Text.Trim();
                LoadMusicData(searchText, selectedColumn, filterText);
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * recordsPerPage < totalRecords)
            {
                currentPage++;
                string searchText = searchTextBox.Text.Trim();
                string selectedColumn = (columnComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string filterText = filterTextBox.Text.Trim();
                LoadMusicData(searchText, selectedColumn, filterText);
            }
        }

        private void UpdatePageInfo()
        {
            pageInfo.Text = $"Page {currentPage} of {Math.Ceiling((double)totalRecords / recordsPerPage)}";
        }
    }
}
