using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace SEC.Docker.Core
{
    public class SqliteHelper
    {
        private string connectionString;
        public SqliteHelper()
        {
            string dataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"config.db");

            connectionString = new SqliteConnectionStringBuilder()
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"config.db")
            }.ToString();

            Connection.Execute(@"CREATE TABLE [EquConfig] (
                                [Id] VARCHAR(100)  PRIMARY KEY NOT NULL,
                                [Equ] VARCHAR(100)  NOT NULL,
                                [ConfigPath] VARCHAR(200)  NOT NULL,
                                [Version] VARCHAR(100)  NOT NULL,
                                [UpdateTime] DATE  NOT NULL
                                );
                                
                                CREATE TABLE [EquState] (
                                [Id] VARCHAR(100)  PRIMARY KEY NULL,
                                [Equ] VARCHAR(100)  NOT NULL,
                                [EquName] NVARCHAR(100)  NOT NULL,
                                [Enable] BOOLEAN  NOT NULL,
                                [UpdateTime] DATE  NOT NULL
                                );"
                                );
        }
        public IDbConnection Connection
        {
            get
            {
                IDbConnection connection = new SqliteConnection(connectionString);
                connection.Open();
                return connection;
            }
        }
    }
}
