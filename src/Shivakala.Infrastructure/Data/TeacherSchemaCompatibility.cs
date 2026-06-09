using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Shivakala.Core.Entities;

namespace Shivakala.Infrastructure.Data;

public static class TeacherSchemaCompatibility
{
    public static async Task<bool> SupportsAboutPageFieldsAsync(ShivakalaDbContext db, CancellationToken ct = default)
    {
        if (!db.Database.IsSqlServer() && !db.Database.IsSqlite())
            return true;

        return await ColumnExistsAsync(db, "Teachers", "ShowOnAboutPage", ct)
            && await ColumnExistsAsync(db, "Teachers", "PublicDesignation", ct)
            && await ColumnExistsAsync(db, "Teachers", "PublicDesignationMarathi", ct)
            && await ColumnExistsAsync(db, "Teachers", "PublicExperience", ct)
            && await ColumnExistsAsync(db, "Teachers", "PublicExperienceMarathi", ct);
    }

    public static async Task<IReadOnlyList<Teacher>> GetTeachersFallbackAsync(ShivakalaDbContext db, CancellationToken ct = default)
    {
        var teachers = new List<Teacher>();
        await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT Id, FullName, Mobile, Email, Qualification, Specialisation, PhotoUrl, Address,
                       EmployeeCode, MonthlySalary, JoiningDate, IsActive, AdminNotes, CreatedDate
                FROM Teachers
                ORDER BY FullName
                """;

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                teachers.Add(MapTeacher(reader));
        });

        return teachers;
    }

    public static async Task<Teacher?> GetTeacherFallbackAsync(ShivakalaDbContext db, int id, CancellationToken ct = default)
    {
        return await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT Id, FullName, Mobile, Email, Qualification, Specialisation, PhotoUrl, Address,
                       EmployeeCode, MonthlySalary, JoiningDate, IsActive, AdminNotes, CreatedDate
                FROM Teachers
                WHERE Id = @Id
                """;
            AddParameter(command, "@Id", id);

            await using var reader = await command.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? MapTeacher(reader) : null;
        });
    }

    public static async Task<Dictionary<int, string>> GetTeacherNamesFallbackAsync(ShivakalaDbContext db, CancellationToken ct = default)
    {
        var result = new Dictionary<int, string>();
        await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, FullName FROM Teachers";

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result[reader.GetInt32(0)] = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
        });

        return result;
    }

    public static async Task<int> InsertTeacherFallbackAsync(ShivakalaDbContext db, Teacher teacher, CancellationToken ct = default)
    {
        teacher.CreatedDate = teacher.CreatedDate == default ? DateTime.UtcNow : teacher.CreatedDate;
        teacher.JoiningDate = teacher.JoiningDate == default ? DateTime.UtcNow : teacher.JoiningDate;

        var id = await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = db.Database.IsSqlServer()
                ? """
                  INSERT INTO Teachers
                      (FullName, Mobile, Email, Qualification, Specialisation, PhotoUrl, Address,
                       EmployeeCode, MonthlySalary, JoiningDate, IsActive, AdminNotes, CreatedDate)
                  VALUES
                      (@FullName, @Mobile, @Email, @Qualification, @Specialisation, @PhotoUrl, @Address,
                       @EmployeeCode, @MonthlySalary, @JoiningDate, @IsActive, @AdminNotes, @CreatedDate);
                  SELECT CAST(SCOPE_IDENTITY() AS int);
                  """
                : """
                  INSERT INTO Teachers
                      (FullName, Mobile, Email, Qualification, Specialisation, PhotoUrl, Address,
                       EmployeeCode, MonthlySalary, JoiningDate, IsActive, AdminNotes, CreatedDate)
                  VALUES
                      (@FullName, @Mobile, @Email, @Qualification, @Specialisation, @PhotoUrl, @Address,
                       @EmployeeCode, @MonthlySalary, @JoiningDate, @IsActive, @AdminNotes, @CreatedDate);
                  SELECT last_insert_rowid();
                  """;

            PopulateTeacherParameters(command, teacher);
            return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        });
        teacher.Id = id;
        return id;
    }

    public static async Task UpdateTeacherFallbackAsync(ShivakalaDbContext db, Teacher teacher, CancellationToken ct = default)
    {
        await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE Teachers
                SET FullName = @FullName,
                    Mobile = @Mobile,
                    Email = @Email,
                    Qualification = @Qualification,
                    Specialisation = @Specialisation,
                    PhotoUrl = @PhotoUrl,
                    Address = @Address,
                    EmployeeCode = @EmployeeCode,
                    MonthlySalary = @MonthlySalary,
                    JoiningDate = @JoiningDate,
                    IsActive = @IsActive,
                    AdminNotes = @AdminNotes
                WHERE Id = @Id
                """;
            PopulateTeacherParameters(command, teacher);
            AddParameter(command, "@Id", teacher.Id);
            await command.ExecuteNonQueryAsync(ct);
        });
    }

    public static async Task DeleteTeacherFallbackAsync(ShivakalaDbContext db, int id, CancellationToken ct = default)
    {
        await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Teachers WHERE Id = @Id";
            AddParameter(command, "@Id", id);
            await command.ExecuteNonQueryAsync(ct);
        });
    }

    private static async Task<bool> ColumnExistsAsync(ShivakalaDbContext db, string tableName, string columnName, CancellationToken ct)
    {
        return await WithOpenConnectionAsync(db, ct, async connection =>
        {
            await using var command = connection.CreateCommand();

            if (db.Database.IsSqlServer())
            {
                command.CommandText = """
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName
                    """;
                AddParameter(command, "@TableName", tableName);
                AddParameter(command, "@ColumnName", columnName);
                return Convert.ToInt32(await command.ExecuteScalarAsync(ct)) > 0;
            }

            command.CommandText = $"PRAGMA table_info({tableName})";
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        });
    }

    private static async Task<T> WithOpenConnectionAsync<T>(
        ShivakalaDbContext db,
        CancellationToken ct,
        Func<DbConnection, Task<T>> operation)
    {
        var connection = db.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

        if (shouldCloseConnection)
            await connection.OpenAsync(ct);

        try
        {
            return await operation(connection);
        }
        finally
        {
            if (shouldCloseConnection)
                await connection.CloseAsync();
        }
    }

    private static async Task WithOpenConnectionAsync(
        ShivakalaDbContext db,
        CancellationToken ct,
        Func<DbConnection, Task> operation)
    {
        await WithOpenConnectionAsync<object?>(db, ct, async connection =>
        {
            await operation(connection);
            return null;
        });
    }

    private static Teacher MapTeacher(DbDataReader reader)
    {
        return new Teacher
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            FullName = reader["FullName"]?.ToString() ?? string.Empty,
            Mobile = reader["Mobile"]?.ToString() ?? string.Empty,
            Email = DbString(reader, "Email"),
            Qualification = DbString(reader, "Qualification"),
            Specialisation = DbString(reader, "Specialisation"),
            PhotoUrl = DbString(reader, "PhotoUrl"),
            Address = DbString(reader, "Address"),
            EmployeeCode = DbString(reader, "EmployeeCode"),
            MonthlySalary = DbDecimal(reader, "MonthlySalary"),
            JoiningDate = DbDateTime(reader, "JoiningDate") ?? DateTime.UtcNow,
            IsActive = DbBool(reader, "IsActive") ?? true,
            AdminNotes = DbString(reader, "AdminNotes"),
            CreatedDate = DbDateTime(reader, "CreatedDate") ?? DateTime.UtcNow,
            ShowOnAboutPage = true
        };
    }

    private static void PopulateTeacherParameters(DbCommand command, Teacher teacher)
    {
        AddParameter(command, "@FullName", teacher.FullName);
        AddParameter(command, "@Mobile", teacher.Mobile);
        AddParameter(command, "@Email", teacher.Email);
        AddParameter(command, "@Qualification", teacher.Qualification);
        AddParameter(command, "@Specialisation", teacher.Specialisation);
        AddParameter(command, "@PhotoUrl", teacher.PhotoUrl);
        AddParameter(command, "@Address", teacher.Address);
        AddParameter(command, "@EmployeeCode", teacher.EmployeeCode);
        AddParameter(command, "@MonthlySalary", teacher.MonthlySalary);
        AddParameter(command, "@JoiningDate", teacher.JoiningDate);
        AddParameter(command, "@IsActive", teacher.IsActive);
        AddParameter(command, "@AdminNotes", teacher.AdminNotes);
        AddParameter(command, "@CreatedDate", teacher.CreatedDate);
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string? DbString(DbDataReader reader, string columnName)
        => reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader[columnName]?.ToString();

    private static decimal? DbDecimal(DbDataReader reader, string columnName)
    {
        if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            return null;

        var value = reader[columnName];
        return value switch
        {
            decimal decimalValue => decimalValue,
            double doubleValue => Convert.ToDecimal(doubleValue),
            float floatValue => Convert.ToDecimal(floatValue),
            _ => Convert.ToDecimal(value)
        };
    }

    private static DateTime? DbDateTime(DbDataReader reader, string columnName)
    {
        if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            return null;

        var value = reader[columnName];
        return value switch
        {
            DateTime dateTimeValue => dateTimeValue,
            string stringValue when DateTime.TryParse(stringValue, out var parsed) => parsed,
            _ => Convert.ToDateTime(value)
        };
    }

    private static bool? DbBool(DbDataReader reader, string columnName)
    {
        if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            return null;

        var value = reader[columnName];
        return value switch
        {
            bool boolValue => boolValue,
            long longValue => longValue != 0,
            int intValue => intValue != 0,
            short shortValue => shortValue != 0,
            string stringValue when bool.TryParse(stringValue, out var parsed) => parsed,
            _ => Convert.ToBoolean(value)
        };
    }
}
