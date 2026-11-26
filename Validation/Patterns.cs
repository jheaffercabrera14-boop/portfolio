namespace Portfolio.Validation;

/// <summary>
/// The three regular expressions required by the exam (plus one reused).
/// The SAME text is used (1) on the server with Regex.IsMatch and (2) in the browser:
/// HTML pattern="" on inputs and new RegExp(...) in wwwroot/js/site.js for the textarea.
/// The database CHECK constraints use an equivalent PostgreSQL regex.
/// </summary>
public static class Patterns
{
    // 1) EACH IMAGE URL (checked line by line): http or https, a host with a dot, optional port/path.
    public const string ImageUrl =
        @"^https?://[A-Za-z0-9\-]+(\.[A-Za-z0-9\-]+)+(:[0-9]+)?(/\S*)?$";

    // 2) VIDEO LINK: youtube.com/watch?v=ID, youtu.be/ID or youtube.com/shorts/ID (ID = 11 chars).
    public const string VideoUrl =
        @"^https?://(www\.|m\.)?(youtube\.com/watch\?(\S*&)?v=|youtube\.com/shorts/|youtu\.be/)[A-Za-z0-9_\-]{11}(\S*)?$";

    // 3) COURSE CODE: 2-8 capital letters then 1-4 digits (DBMWEB1). Also forced by chk_course_code_format.
    public const string CourseCode = @"^[A-Z]{2,8}[0-9]{1,4}$";

    // Project / demo URL and course cover image use the same shape as an image URL.
    public const string WebUrl = ImageUrl;
}
