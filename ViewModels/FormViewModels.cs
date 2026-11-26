using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Models;

namespace Portfolio.ViewModels;

// Data annotations = rules for required / length / range (they mirror the column types and CHECKs).
// The REGEX rules live in Validation/Patterns.cs + FormRules.cs and use Regex.IsMatch.

public class CourseFormViewModel
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(12, ErrorMessage = "Course code is at most 12 characters.")]
    [Display(Name = "Course code")]
    public string? CourseCode { get; set; }

    [Required(ErrorMessage = "Course title is required.")]
    [StringLength(120, ErrorMessage = "Title is at most 120 characters.")]
    [Display(Name = "Course title")]
    public string? CourseTitle { get; set; }

    [StringLength(500, ErrorMessage = "Description is at most 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Units is required.")]
    [Range(1, 6, ErrorMessage = "Units must be a whole number from 1 to 6.")]
    public int? Units { get; set; } = 3;

    [StringLength(30, ErrorMessage = "Academic term is at most 30 characters.")]
    [Display(Name = "Academic term")]
    public string? AcademicTerm { get; set; }

    [StringLength(500, ErrorMessage = "Image URL is at most 500 characters.")]
    [Display(Name = "Cover image URL")]
    public string? ImageUrl { get; set; }
}

public class ProjectFormViewModel
{
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Choose a course.")]
    [Display(Name = "Course")]
    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(120, ErrorMessage = "Title is at most 120 characters.")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "Description is at most 1000 characters.")]
    public string? Description { get; set; }

    [StringLength(200, ErrorMessage = "Tech stack is at most 200 characters.")]
    [Display(Name = "Tech stack")]
    public string? TechStack { get; set; }

    [Required(ErrorMessage = "Year is required.")]
    [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100.")]
    [Display(Name = "Year completed")]
    public int? CompletedYear { get; set; }

    [StringLength(300, ErrorMessage = "Project URL is at most 300 characters.")]
    [Display(Name = "Project / demo URL")]
    public string? ProjectUrl { get; set; }

    [StringLength(300, ErrorMessage = "Video URL is at most 300 characters.")]
    [Display(Name = "YouTube video URL")]
    public string? VideoUrl { get; set; }

    /// <summary>Image URLs, ONE PER LINE, exactly as typed in the textarea.</summary>
    [Display(Name = "Image URLs (one per line)")]
    public string? ImageUrlsText { get; set; }

    [ValidateNever] public List<SelectListItem> CourseOptions { get; set; } = new();
    [ValidateNever] public List<ProjectImage> ExistingImages { get; set; } = new();
}

public class ImageFormViewModel
{
    public int ImageId { get; set; }
    public int ProjectId { get; set; }

    [ValidateNever] public string ProjectTitle { get; set; } = "";

    [Required(ErrorMessage = "Image URL is required.")]
    [StringLength(500, ErrorMessage = "Image URL is at most 500 characters.")]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}
