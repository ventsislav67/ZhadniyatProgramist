using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using ZhadniyatProgramist.Models;

namespace ZhadniyatProgramist
{
    /// <summary>
    /// Слой за достъп до данните (ADO.NET + SQLite).
    ///
    /// Избрахме SQLite вместо SQL Server LocalDB, защото:
    ///  - не изисква никаква инсталация или настройка;
    ///  - базата е един файл (krachma.db) до .exe файла;
    ///  - проектът се стартира с едно F5 на всяка машина.
    ///
    /// ВАЖНО: Всички заявки, които приемат стойности отвън,
    /// използват ПАРАМЕТРИ (@име) – никога конкатенация на низове.
    /// Това е защитата срещу SQL Injection.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string DbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "krachma.db");

        private static readonly string ConnectionString = "Data Source=" + DbPath;

        /// <summary>Отваря връзка и включва проверката на foreign keys.</summary>
        private static SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            return connection;
        }

        // =====================================================================
        //  Инициализация
        // =====================================================================

        /// <summary>
        /// Създава таблиците, ако не съществуват, и зарежда примерните данни
        /// при първо стартиране. Същата логика е описана и в SQL/create_database.sql.
        /// </summary>
        public static void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Programmers (
                            Id               INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name             TEXT NOT NULL,
                            FavoriteLanguage TEXT NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Tabs (
                            Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProgrammerId INTEGER NOT NULL,
                            ItemName     TEXT NOT NULL,
                            Price        REAL NOT NULL,
                            Date         TEXT NOT NULL,
                            FOREIGN KEY (ProgrammerId) REFERENCES Programmers (Id) ON DELETE CASCADE
                        );";
                    command.ExecuteNonQuery();
                }

                // Примерни данни – само ако базата е празна
                using (var check = connection.CreateCommand())
                {
                    check.CommandText = "SELECT COUNT(*) FROM Programmers;";
                    long count = Convert.ToInt64(check.ExecuteScalar());
                    if (count == 0)
                    {
                        SeedData(connection);
                    }
                }
            }
        }

        /// <summary>Зарежда примерните програмисти и поръчки.</summary>
        private static void SeedData(SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                var programmers = new (string Name, string Language)[]
                {
                    ("Иван Debugger",        "C#"),
                    ("Мария Frontendova",    "JavaScript"),
                    ("Георги Stackoverflow", "Python"),
                    ("Петър NullReference",  "Java"),
                    ("Алекс ConsoleLog",     "TypeScript")
                };

                var programmerIds = new List<long>();

                foreach (var programmer in programmers)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "INSERT INTO Programmers (Name, FavoriteLanguage) VALUES (@name, @language);";
                        command.Parameters.AddWithValue("@name", programmer.Name);
                        command.Parameters.AddWithValue("@language", programmer.Language);
                        command.ExecuteNonQuery();
                    }

                    // Отделна команда за Id-то на току-що вмъкнатия ред –
                    // по-надеждно от batch с няколко statement-а в един CommandText.
                    using (var idCommand = connection.CreateCommand())
                    {
                        idCommand.Transaction = transaction;
                        idCommand.CommandText = "SELECT last_insert_rowid();";
                        programmerIds.Add(Convert.ToInt64(idCommand.ExecuteScalar()));
                    }
                }

                // (индекс на програмист, поръчка, цена, преди колко минути)
                var tabs = new (int Index, string Item, double Price, int MinutesAgo)[]
                {
                    (0, "Крафт бира",          6.50, 240),
                    (0, "Пържени картофи",     5.50, 180),
                    (1, "Бургер „404“",       11.90, 200),
                    (1, "Крафт бира",          6.50,  90),
                    (2, "Шот „Null Pointer“",  7.00, 300),
                    (2, "Крафт бира",          6.50, 250),
                    (2, "Ракия „Legacy Code“",13.00, 210),
                    (2, "Пържени картофи",     5.50, 160),
                    (2, "Бургер „404“",       11.90, 100),
                    (2, "Крафт бира",          6.50,  40),
                    (3, "Пържени картофи",     5.50, 130),
                    (3, "Шот „Null Pointer“",  7.00,  60),
                    (4, "Салата „Clean Code“", 8.90, 150),
                    (4, "Бургер „404“",       11.90,  45)
                };

                foreach (var tab in tabs)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText =
                            "INSERT INTO Tabs (ProgrammerId, ItemName, Price, Date) " +
                            "VALUES (@programmerId, @itemName, @price, @date);";
                        command.Parameters.AddWithValue("@programmerId", programmerIds[tab.Index]);
                        command.Parameters.AddWithValue("@itemName", tab.Item);
                        command.Parameters.AddWithValue("@price", tab.Price);
                        command.Parameters.AddWithValue("@date",
                            DateTime.Now.AddMinutes(-tab.MinutesAgo).ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        // =====================================================================
        //  Четене на данни
        // =====================================================================

        /// <summary>Връща всички програмисти, подредени по име.</summary>
        public static List<Programmer> GetProgrammers()
        {
            var result = new List<Programmer>();

            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT Id, Name, FavoriteLanguage FROM Programmers ORDER BY Name;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Programmer
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            FavoriteLanguage = reader.GetString(2)
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Връща всички поръчки чрез JOIN между Tabs и Programmers,
        /// така че вместо ID-та се показват имена.
        /// По желание филтрира по име / поръчка / език (LIKE с параметър).
        /// Колоните са с български имена – DataGridView ги ползва директно.
        /// </summary>
        public static DataTable GetOrders(string searchText = null)
        {
            var table = new DataTable();
            bool hasSearch = !string.IsNullOrWhiteSpace(searchText);

            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                // Основната JOIN заявка. WHERE клаузата е фиксиран текст –
                // потребителският вход влиза единствено през параметъра @search.
                string sql = @"
                    SELECT p.Name                                   AS ""Програмист"",
                           p.FavoriteLanguage                       AS ""Любим език"",
                           t.ItemName                               AS ""Поръчка"",
                           t.Price                                  AS ""Цена"",
                           strftime('%d.%m.%Y %H:%M', t.Date)       AS ""Дата""
                    FROM Tabs t
                    INNER JOIN Programmers p ON p.Id = t.ProgrammerId";

                if (hasSearch)
                {
                    sql += @"
                    WHERE p.Name LIKE @search
                       OR t.ItemName LIKE @search
                       OR p.FavoriteLanguage LIKE @search";
                }

                sql += @"
                    ORDER BY t.Date DESC;";

                command.CommandText = sql;

                if (hasSearch)
                {
                    command.Parameters.AddWithValue("@search", "%" + searchText.Trim() + "%");
                }

                using (var reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }
            }

            return table;
        }

        /// <summary>
        /// Сумира всички неплатени сметки на даден програмист (SQL SUM).
        /// COALESCE връща 0, ако човекът няма нито една поръчка.
        /// </summary>
        public static double GetTotalDebt(int programmerId)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT COALESCE(SUM(Price), 0) FROM Tabs WHERE ProgrammerId = @ProgrammerId;";
                command.Parameters.AddWithValue("@ProgrammerId", programmerId);
                return Convert.ToDouble(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Обобщена статистика за информационните карти на главния екран.
        /// </summary>
        public static (int ProgrammersCount, int ActiveTabsCount, double TotalDebt,
                       string TopDebtorName, double TopDebtorSum) GetStatistics()
        {
            using (var connection = GetConnection())
            {
                int programmersCount = Convert.ToInt32(
                    ExecuteScalar(connection, "SELECT COUNT(*) FROM Programmers;"));
                int activeTabsCount = Convert.ToInt32(
                    ExecuteScalar(connection, "SELECT COUNT(*) FROM Tabs;"));
                double totalDebt = Convert.ToDouble(
                    ExecuteScalar(connection, "SELECT COALESCE(SUM(Price), 0) FROM Tabs;"));

                string topDebtorName = null;
                double topDebtorSum = 0;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT p.Name, SUM(t.Price) AS Total
                        FROM Tabs t
                        INNER JOIN Programmers p ON p.Id = t.ProgrammerId
                        GROUP BY p.Id, p.Name
                        ORDER BY Total DESC
                        LIMIT 1;";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            topDebtorName = reader.GetString(0);
                            topDebtorSum = reader.GetDouble(1);
                        }
                    }
                }

                return (programmersCount, activeTabsCount, totalDebt, topDebtorName, topDebtorSum);
            }
        }

        /// <summary>Помощен метод за прости скаларни заявки без параметри.</summary>
        private static object ExecuteScalar(SqliteConnection connection, string sql)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                return command.ExecuteScalar();
            }
        }

        // =====================================================================
        //  Промяна на данни
        // =====================================================================

        /// <summary>Добавя нова поръчка в тефтера (parameterized INSERT).</summary>
        public static void AddTab(TabItem tab)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO Tabs (ProgrammerId, ItemName, Price, Date) " +
                    "VALUES (@ProgrammerId, @ItemName, @Price, @Date);";
                command.Parameters.AddWithValue("@ProgrammerId", tab.ProgrammerId);
                command.Parameters.AddWithValue("@ItemName", tab.ItemName);
                command.Parameters.AddWithValue("@Price", tab.Price);
                command.Parameters.AddWithValue("@Date", tab.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>Добавя нов програмист (parameterized INSERT).</summary>
        public static void AddProgrammer(Programmer programmer)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO Programmers (Name, FavoriteLanguage) VALUES (@Name, @FavoriteLanguage);";
                command.Parameters.AddWithValue("@Name", programmer.Name);
                command.Parameters.AddWithValue("@FavoriteLanguage", programmer.FavoriteLanguage);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// „Плаща“ сметката – изтрива всички поръчки на програмиста.
        /// Пример за правилно защитен DELETE с параметър.
        /// </summary>
        public static int PayTab(int programmerId)
        {
            using (var connection = GetConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Tabs WHERE ProgrammerId = @ProgrammerId;";
                command.Parameters.AddWithValue("@ProgrammerId", programmerId);
                return command.ExecuteNonQuery();
            }
        }
    }
}
