# Music List Sorter

This application for a client.
The Music List Sorter (later got for movies as well) is a WPF-based application that allows you to manage and sort music tracks and albums. It uses an SQLite database and supports importing data from Excel files.
Users can filter, search, and sort music tracks based on various criteria.

## Table of Contents

- [Project Description](#project-description)
- [Installation](#installation)
- [Usage Instructions](#usage-instructions)
- [Code Structure](#code-structure)
- [Contributing](#contributing)
- [License](#license)

## Project Description

This application is a music list manager that allows:
- Adding, editing, and deleting records.
- Importing data from Excel files.
- Searching and filtering records based on criteria such as band name, album title, release date, and disk number.
- Sorting and navigating through pages of records.

## Installation

### Prerequisites

- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework-runtime)
- [Visual Studio 2019 or later](https://visualstudio.microsoft.com/downloads/)
- [SQLite ADO.NET Provider](https://www.nuget.org/packages/System.Data.SQLite/)
- [ExcelDataReader NuGet package](https://www.nuget.org/packages/ExcelDataReader/)

### Steps

1. Clone the repository:
   ```sh
   git clone https://github.com/username/music-list-sorter.git
   ```
2. Open the project in Visual Studio:
  ```sh
  Navigate to the cloned folder and open the `.sln` file.
  ```
3. Install the required NuGet packages:
  ```sh
  - Open the NuGet Package Manager (Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution).
  - Install the following packages:
      System.Data.SQLite
      ExcelDataReader
      ExcelDataReader.DataSet
  ```
4. Update the database file path:
  ```sh
  Modify the dbFilePath variable in the MainWindow.xaml.cs file to point to your SQLite database file.
  ```

5. Run the project:
  ```sh
  Use Visual Studio to run the project (F5).
  ```

## Usage Instructions

### User Interface
- Add New Record: Add a new record.
- Add Multiple Records: Import multiple records from an Excel file.
- Search: Search the database. Searches can be performed in the Band, Title, ReleaseDate, and DiskNumber fields.
- Filter: Filter by band name (Band) starting letter. Only one character is allowed, e.g., 'A' or '1'.
- Previous/Next: Navigate between pages.
- Edit/Delete: Edit or delete records.

### Excel Data Import
- You need a specific `.xls` or `.xlsx` with the following header:
  (here is a sample excel)
  ![image](https://github.com/user-attachments/assets/b741ef3a-2a59-4a02-bcaa-c51a717532d7)
- Click the Add Multiple Records button.
- Select the Excel file that contains the data you want to import.
- The application will read the Excel file and add the data to the database.

## Code Structure

### Main Files
- MainWindow.xaml: The main user interface for displaying and managing data.
- MainWindow.xaml.cs: Contains business logic, including database operations, search, filtering, and Excel import.
- AddNewRecordWindow.xaml and AddNewRecordWindow.xaml.cs: Interface for adding new records.
- AddMultipleRecordsWindow.xaml and AddMultipleRecordsWindow.xaml.cs: Interface for importing data from Excel files.
- EditWindow.xaml and EditWindow.xaml.cs: Interface for editing existing records.

## Contributing

If you would like to contribute to this project, please follow these steps:

- Fork the repository.
- Create a new branch for your changes.
- Submit a pull request to the main repository.

## License
This project is licensed under the MIT License. See the LICENSE file for details.
You can copy and paste this content into a `.txt` file, such as `music-list-sorter-documentation.txt`, and then upload it to your GitHub repository. Let me know if you need any more help!
