using System.Net;
using ResumeBuilder.Models;
namespace ResumeBuilder.Services;
public static class TemplateEngine
{
    static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
    public static string Render(ResumeData r)
    {
        var work = string.Join("", r.WorkExperience.Where(x => x.Role.Length > 0 || x.Company.Length > 0).Select(x => $"<article><div><h3>{H(x.Role)}</h3><strong>{H(x.Company)}</strong></div><time>{H(x.Dates)}</time><p>{H(x.Description).Replace("\n", "<br>")}</p></article>"));
        var education = string.Join("", r.Education.Where(x => x.Degree.Length > 0 || x.School.Length > 0).Select(x => $"<article><div><h3>{H(x.Degree)}</h3><strong>{H(x.School)}</strong></div><time>{H(x.Dates)}</time></article>"));
        var skills = string.Join("", r.Skills.Select(x => $"<span>{H(x)}</span>"));
        var photo = string.IsNullOrWhiteSpace(r.PhotoDataUrl) ? "" : "<img class='photo' src='" + r.PhotoDataUrl + "'>";
        var style = r.Template == 1 ? "classic" : r.Template == 2 ? "minimal" : "modern";
        const string document = """
<!doctype html><html><head><meta charset='utf-8'><style>
@page{size:A4;margin:0}*{box-sizing:border-box}body{margin:0;font:10pt Arial,sans-serif;color:#253043;background:#e5e7eb}.page{width:210mm;min-height:297mm;margin:auto;background:#fff;display:grid;grid-template-columns:67mm 1fr}.side{background:#0f766e;color:white;padding:22mm 10mm}.content{padding:20mm 15mm}.photo{width:35mm;height:35mm;border-radius:50%;object-fit:cover;border:2px solid #fff;margin-bottom:8mm}h1{font-size:27pt;margin:0 0 3mm;line-height:1.04}h2{font-size:11pt;letter-spacing:1.5px;text-transform:uppercase;color:#0f766e;border-bottom:1px solid #d5dedc;padding-bottom:3mm;margin:8mm 0 4mm}.side h2{color:#d4f2ed;border-color:#5aa79f}.title{font-size:12pt;color:#d4f2ed}.contact{margin:10mm 0;line-height:1.65;word-break:break-word}.summary{line-height:1.6;margin:0}article{position:relative;margin:0 0 5mm;padding-right:30mm}article h3{font-size:11pt;margin:0 0 1mm}article strong{font-weight:normal;color:#51706c}article time{position:absolute;right:0;top:0;color:#63736f;font-size:9pt;text-align:right}article p{line-height:1.5;margin:2mm 0 0}.skills{display:flex;flex-wrap:wrap;gap:2mm}.skills span{padding:1.5mm 2.5mm;background:@ACCENT@;color:white;border-radius:10mm;font-size:9pt}.classic .page{display:block;padding:18mm 20mm}.classic .side{background:white;color:#253043;padding:0;border-bottom:3px solid #253043}.classic .content{padding:0}.classic h1{font-family:Georgia,serif}.classic .title,.classic .side h2{color:#253043}.classic h2{color:#253043;border-color:#253043}.classic .contact{margin:5mm 0}.minimal .page{display:block;padding:20mm}.minimal .side{background:white;color:#253043;padding:0}.minimal .content{padding:0}.minimal .photo{float:right;border-color:#0f766e}.minimal h1{color:#0f766e}.minimal .title{color:#64748b}.minimal h2{margin-top:7mm}.minimal .side h2{color:#0f766e;border-color:#0f766e}@media print{body{background:white}.page{margin:0}}
</style></head><body class='@CLASS@'><main class='page'><aside class='side'>@PHOTO@<h1>@NAME@</h1><div class='title'>@TITLE@</div><div class='contact'>@EMAIL@<br>@PHONE@<br>@ADDRESS@<br>@LINK@</div><h2>Skills</h2><div class='skills'>@SKILLS@</div></aside><section class='content'><h2>Profile</h2><p class='summary'>@SUMMARY@</p><h2>Experience</h2>@WORK@<h2>Education</h2>@EDUCATION@</section></main></body></html>
""";
        return document.Replace("#0f766e", r.AccentColor).Replace("@ACCENT@", r.AccentColor).Replace("Arial,sans-serif", H(r.FontFamily) + ",sans-serif").Replace("@CLASS@", style).Replace("@PHOTO@", photo).Replace("@NAME@", H(r.FullName)).Replace("@TITLE@", H(r.JobTitle)).Replace("@EMAIL@", H(r.Email)).Replace("@PHONE@", H(r.Phone)).Replace("@ADDRESS@", H(r.Address)).Replace("@LINK@", H(r.Link)).Replace("@SKILLS@", skills).Replace("@SUMMARY@", H(r.Summary).Replace("\n", "<br>")).Replace("@WORK@", work).Replace("@EDUCATION@", education);
    }
}
