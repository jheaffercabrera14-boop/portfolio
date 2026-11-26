using Npgsql;
using NpgsqlTypes;

namespace Portfolio.Data;

/// <summary>Small helpers so repositories stay readable. Parameters only: no SQL is ever built by string concatenation of user input.</summary>
internal static class DbExtensions
{
    public static void AddText(this NpgsqlCommand c, string name, string? value) =>
        c.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Text) { Value = (object?)value ?? DBNull.Value });

    public static void AddInt(this NpgsqlCommand c, string name, int? value) =>
        c.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Integer) { Value = (object?)value ?? DBNull.Value });

    public static string? GetStringOrNull(this NpgsqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);
}
