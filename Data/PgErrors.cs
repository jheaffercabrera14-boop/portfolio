using Npgsql;

namespace Portfolio.Data;

/// <summary>
/// Turns a PostgreSQL refusal (PostgresException.SqlState) into a friendly sentence for the page.
///   23503 = foreign_key_violation   23505 = unique_violation   23514 = check_violation
/// ex.ConstraintName is the name we gave the constraint in schema.sql.
/// </summary>
public static class PgErrors
{
    public static string ToMessage(PostgresException ex)
    {
        switch (ex.SqlState)
        {
            case "23503": // FOREIGN KEY
                // Deleting a parent that still has children: "update or delete on table ..."
                if (ex.MessageText.StartsWith("update or delete", StringComparison.OrdinalIgnoreCase))
                    return "This course still has projects. Delete or move its projects first, then delete the course. (Database error 23503)";
                return "The course you chose no longer exists. Refresh the page and pick another. (Database error 23503)";

            case "23505": // UNIQUE
                return ex.ConstraintName switch
                {
                    "uq_course_code" => "That course code is already used by another course. (Database error 23505)",
                    "uq_project_course_title" => "This course already has a project with that title. (Database error 23505)",
                    "uq_project_image_url" => "The same image URL appears more than once for this project. (Database error 23505)",
                    _ => "That value already exists and must be unique. (Database error 23505)"
                };

            case "23514": // CHECK
                return ex.ConstraintName switch
                {
                    "chk_course_code_format" => "Course code format refused by the database: 2-8 capital letters then 1-4 digits. (Database error 23514)",
                    "chk_course_units" => "Units must be between 1 and 6. (Database error 23514)",
                    "chk_project_year" => "Year must be between 2000 and 2100. (Database error 23514)",
                    "chk_project_video_url" => "The video link is not an accepted YouTube URL. (Database error 23514)",
                    "chk_course_image_url" or "chk_project_project_url" or "chk_image_url_format"
                        => "A URL is not a valid http(s) address. (Database error 23514)",
                    "chk_image_sort_order" => "Image order must be 1 or more. (Database error 23514)",
                    _ => $"A value broke the database rule '{ex.ConstraintName}'. (Database error 23514)"
                };

            case "22001": // string too long
                return "One of the values is longer than its column allows. (Database error 22001)";

            default:
                return "The database refused the request. Please check your values and try again. (Database error " + ex.SqlState + ")";
        }
    }
}
