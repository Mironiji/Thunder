using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

namespace Thunder.Server.Storage;

public class ServerStorage
{
    private const string CreateTablesSql = """
        CREATE TABLE IF NOT EXISTS Users (
            id INTEGER PRIMARY KEY,
            Login TEXT NOT NULL UNIQUE
        );
        CREATE TABLE IF NOT EXISTS PasswordsHash (
            id INTEGER PRIMARY KEY REFERENCES Users (id) ON DELETE CASCADE,
            PasswordHash TEXT NOT NULL
        );
        """;

    private readonly string _connectionString;
    private readonly PasswordHasher<string> _passwordHasher = new();

    public ServerStorage(IHostEnvironment environment, IConfiguration configuration)
    {
        var connectionString = new SqliteConnectionStringBuilder(configuration.GetConnectionString("Users"))
        {
            ForeignKeys = true
        };

        if (string.IsNullOrWhiteSpace(connectionString.DataSource))
        {
            var databaseFile = environment.IsDevelopment() ? "UsersDB.db" : "Users.Production.db";
            var directory = Path.Combine(environment.ContentRootPath, "Storage");
            Directory.CreateDirectory(directory);
            connectionString.DataSource = Path.Combine(directory, databaseFile);
        }

        _connectionString = connectionString.ToString();
        InitializeDatabase();

        if (environment.IsDevelopment())
        {
            SignUp("root", "root");
        }
    }

    public bool IsFreeLogin(string? login)
    {
        if (string.IsNullOrWhiteSpace(login) || login.Length < 2)
        {
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM Users WHERE Login = @login LIMIT 1";
        command.Parameters.AddWithValue("@login", login);
        return command.ExecuteScalar() is null;
    }

    public bool CheckPassword(string? login, string? password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ph.PasswordHash
            FROM PasswordsHash AS ph
            JOIN Users AS u ON ph.id = u.id
            WHERE u.Login = @login
            """;
        command.Parameters.AddWithValue("@login", login);

        return command.ExecuteScalar() switch
        {
            string hash => _passwordHasher.VerifyHashedPassword(login, hash, password)
                != PasswordVerificationResult.Failed,
            byte[] legacyHash => CryptographicOperations.FixedTimeEquals(
                legacyHash, SHA256.HashData(Encoding.UTF8.GetBytes(password))),
            _ => false
        };
    }

    public bool SignUp(string? login, string? password)
    {
        if (string.IsNullOrWhiteSpace(login) || login.Length < 2
            || string.IsNullOrWhiteSpace(password) || password.Length < 3)
        {
            return false;
        }

        var passwordHash = _passwordHasher.HashPassword(login, password);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using var insertUser = connection.CreateCommand();
        insertUser.Transaction = transaction;
        insertUser.CommandText = """
            INSERT INTO Users (Login) VALUES (@login)
            ON CONFLICT (Login) DO NOTHING
            RETURNING id
            """;
        insertUser.Parameters.AddWithValue("@login", login);
        if (insertUser.ExecuteScalar() is not long userId)
        {
            return false;
        }

        using var insertHash = connection.CreateCommand();
        insertHash.Transaction = transaction;
        insertHash.CommandText = "INSERT INTO PasswordsHash (id, PasswordHash) VALUES (@id, @hash)";
        insertHash.Parameters.AddWithValue("@id", userId);
        insertHash.Parameters.AddWithValue("@hash", passwordHash);
        insertHash.ExecuteNonQuery();

        transaction.Commit();
        return true;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = CreateTablesSql;
        command.ExecuteNonQuery();

        command.CommandText = """
            SELECT EXISTS (
                SELECT 1 FROM pragma_table_info('Users')
                WHERE name = 'Login' COLLATE NOCASE AND type = 'STRING' COLLATE NOCASE
            ) OR EXISTS (
                SELECT 1 FROM pragma_foreign_key_list('PasswordsHash')
                WHERE "table" = 'Users' COLLATE NOCASE AND on_delete = 'SET NULL'
            )
            """;
        if (command.ExecuteScalar() is 1L)
        {
            // Сохраняем идентификаторы пользователей и хеши из прежних таблиц.
            command.CommandText = $"""
                ALTER TABLE Users RENAME TO Users_old;
                ALTER TABLE PasswordsHash RENAME TO PasswordsHash_old;
                {CreateTablesSql}
                INSERT INTO Users (id, Login) SELECT id, Login FROM Users_old;
                INSERT INTO PasswordsHash (id, PasswordHash)
                    SELECT id, PasswordHash FROM PasswordsHash_old;
                DROP TABLE PasswordsHash_old;
                DROP TABLE Users_old;
                """;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
