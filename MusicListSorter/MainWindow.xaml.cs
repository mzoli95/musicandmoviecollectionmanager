using System;
using System.Data;
using System.Data.SQLite;
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

        private void LoadMusicData(string searchText = "")
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
                    sqlQuery += $" WHERE Band LIKE @searchText OR Title LIKE @searchText OR ReleaseDate LIKE @searchText OR DiskNumber LIKE @searchText";
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
                    sqlQuery += $" WHERE Band LIKE @searchText OR Title LIKE @searchText OR ReleaseDate LIKE @searchText OR DiskNumber LIKE @searchText";
                }

                sqlQuery += " LIMIT @recordsPerPage OFFSET @offset";

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
            LoadMusicData(searchText);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView rowView)
            {
                EditWindow editWindow = new EditWindow(rowView);
                editWindow.ShowDialog();

                LoadMusicData(searchTextBox.Text.Trim());
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView rowView)
            {
                string band = rowView["Band"].ToString();
                string title = rowView["Title"].ToString();

                MessageBoxResult result = MessageBox.Show($"Biztosan törölni szeretnéd a következő bejegyzést: {band} - {title}?", "Törlés megerősítése", MessageBoxButton.YesNo);
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
                    LoadMusicData(searchTextBox.Text.Trim());
                }
            }
        }

        private void AddNewRecordButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewRecordWindow addNewRecordWindow = new AddNewRecordWindow();
            addNewRecordWindow.ShowDialog();
        }

        private void AddMultipleRecordsButton_Click(object sender, RoutedEventArgs e)
        {
            AddMultipleRecordsWindow addMultipleRecordsWindow = new AddMultipleRecordsWindow();
            addMultipleRecordsWindow.ShowDialog();
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
                LoadMusicData(searchTextBox.Text.Trim());
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * recordsPerPage < totalRecords)
            {
                currentPage++;
                LoadMusicData(searchTextBox.Text.Trim());
            }
        }

        private void UpdatePageInfo()
        {
            pageInfo.Text = $"Page {currentPage} of {Math.Ceiling((double)totalRecords / recordsPerPage)}";
        }
    }
}
