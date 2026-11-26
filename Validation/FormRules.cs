using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Portfolio.ViewModels;

namespace Portfolio.Validation;

/// <summary>Server-side rules that need Regex.IsMatch. Add errors to ModelState (AddModelError).</summary>
public static class FormRules
{
    public const int MaxImagesPerProject = 20;

    // Regex.IsMatch with a timeout, so a nasty input can never hang the server.
    public static bool Matches(string? value, string pattern)
    {
        if (string.IsNullOrEmpty(value)) return false;
        try { return Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(250)); }
        catch (RegexMatchTimeoutException) { return false; }
    }

    /// <summary>Splits the textarea into lines. Keeps the ORIGINAL line number; blank lines are skipped.</summary>
    public static List<(int Line, string Url)> ParseImageLines(string? text)
    {
        var result = new List<(int Line, string Url)>();
        if (string.IsNullOrWhiteSpace(text)) return result;
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var t = lines[i].Trim();
            if (t.Length > 0) result.Add((i + 1, t));
        }
        return result;
    }

    // Adds an error to a field only if the field has no error yet (avoids two messages for one mistake).
    private static void Add(ModelStateDictionary ms, string key, string message)
    {
        if (ms.GetFieldValidationState(key) != ModelValidationState.Invalid)
            ms.AddModelError(key, message);
    }

    public static void CheckCourse(CourseFormViewModel vm, ModelStateDictionary ms)
    {
        if (!Matches(vm.CourseCode, Patterns.CourseCode))
            Add(ms, nameof(vm.CourseCode),
                "Course code must be 2-8 CAPITAL letters followed by 1-4 digits, no spaces (example: DBMWEB1).");
        if (!string.IsNullOrEmpty(vm.ImageUrl) && !Matches(vm.ImageUrl, Patterns.WebUrl))
            Add(ms, nameof(vm.ImageUrl), "Cover image must be a full http:// or https:// URL.");
    }

    public static void CheckProject(ProjectFormViewModel vm, ModelStateDictionary ms,
                                    IReadOnlyList<(int Line, string Url)> images)
    {
        if (!string.IsNullOrEmpty(vm.ProjectUrl) && !Matches(vm.ProjectUrl, Patterns.WebUrl))
            Add(ms, nameof(vm.ProjectUrl), "Project URL must be a full http:// or https:// URL.");

        if (!string.IsNullOrEmpty(vm.VideoUrl) && !Matches(vm.VideoUrl, Patterns.VideoUrl))
            Add(ms, nameof(vm.VideoUrl),
                "Video must be a YouTube link: youtube.com/watch?v=..., youtu.be/... or youtube.com/shorts/... (11-character id).");

        // Each image URL, line by line.
        var problems = new List<string>();
        foreach (var (line, url) in images)
        {
            if (url.Length > 500) problems.Add($"Line {line}: longer than 500 characters.");
            else if (!Matches(url, Patterns.ImageUrl)) problems.Add($"Line {line}: \"{url}\" is not a valid http(s) URL.");
        }
        if (images.Count > MaxImagesPerProject)
            problems.Add($"At most {MaxImagesPerProject} images per project (you entered {images.Count}).");
        if (problems.Count > 0)
            ms.AddModelError(nameof(vm.ImageUrlsText), string.Join(" ", problems));
    }

    public static void CheckImage(ImageFormViewModel vm, ModelStateDictionary ms)
    {
        if (!Matches(vm.ImageUrl, Patterns.ImageUrl))
            Add(ms, nameof(vm.ImageUrl), "Image must be a full http:// or https:// URL.");
    }
}
