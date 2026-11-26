using Npgsql;

namespace Portfolio.Data;

/// <summary>
/// Thrown by ProjectRepository when ONE image line fails inside the transaction.
/// By the time it is thrown, the transaction has already been rolled back.
/// </summary>
public class ImageSaveException : Exception
{
    public int Line { get; }
    public string Url { get; }
    public PostgresException Pg { get; }

    public ImageSaveException(int line, string url, PostgresException pg) : base(pg.Message, pg)
    {
        Line = line; Url = url; Pg = pg;
    }
}
