namespace ResumeBuilder.Models;

public sealed class ResumeData
{
    public string FullName { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string Link { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? PhotoDataUrl { get; set; }
    public int Template { get; set; }
    public string AccentColor { get; set; } = "#0D9488";
    public string FontFamily { get; set; } = "Arial";
    public List<WorkExperience> WorkExperience { get; set; } = [];
    public List<Education> Education { get; set; } = [];
    public List<string> Skills { get; set; } = [];
    public void CopyFrom(ResumeData x) { FullName=x.FullName;JobTitle=x.JobTitle;Email=x.Email;Phone=x.Phone;Address=x.Address;Link=x.Link;Summary=x.Summary;PhotoDataUrl=x.PhotoDataUrl;Template=x.Template;AccentColor=x.AccentColor;FontFamily=x.FontFamily;WorkExperience=x.WorkExperience;Education=x.Education;Skills=x.Skills; }
}
public sealed class WorkExperience { public string Role { get; set; }=""; public string Company { get; set; }=""; public string Dates { get; set; }=""; public string Description { get; set; }=""; }
public sealed class Education { public string Degree { get; set; }=""; public string School { get; set; }=""; public string Dates { get; set; }=""; }
