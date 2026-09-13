using Microsoft.Web.WebView2.WinForms;
using ResumeBuilder.Models;
using ResumeBuilder.Services;

namespace ResumeBuilder;

public partial class Form1 : Form
{
    readonly ResumeData resume = new();
    readonly WebView2 preview = new() { Dock = DockStyle.Fill, DefaultBackgroundColor = Color.White };
    readonly System.Windows.Forms.Timer debounce = new() { Interval = 300 };
    readonly FlowLayoutPanel workCards = CardStack(), educationCards = CardStack(), skillPills = PillStack();
    readonly List<Panel> templateCards = [];
    PictureBox? photoBox;
    Image? sourcePhoto;
    bool ready;

    public Form1()
    {
        InitializeComponent(); BuildUi();
        debounce.Tick += async (_, _) => { debounce.Stop(); await RenderAsync(); };
        Shown += async (_, _) => { try { await preview.EnsureCoreWebView2Async(); ready = true; await RenderAsync(); } catch (Exception ex) { MessageBox.Show(ex.Message, "WebView2 initialization failed"); } };
    }

    void BuildUi()
    {
        Text = "Resume Studio"; Size = new Size(1440, 900); MinimumSize = new Size(1100, 700); BackColor = Color.FromArgb(245, 247, 250); Font = new Font("Segoe UI", 9.5f);
        var bar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White };
        bar.Controls.Add(new Label { Text = "Resume Studio", AutoSize = true, Location = new Point(28, 19), Font = new Font("Segoe UI Semibold", 18), ForeColor = Color.FromArgb(15, 23, 42) });
        bar.Controls.Add(new Label { Text = "polished resume, live.", AutoSize = true, Location = new Point(192, 27), ForeColor = Color.SlateGray });
        TopButton(bar, "Export PDF", Color.FromArgb(13, 148, 136), async (_, _) => await ExportPdfAsync(), 28);
        TopButton(bar, "Load draft", Color.White, (_, _) => LoadDraft(), 145); TopButton(bar, "Save draft", Color.White, (_, _) => SaveDraft(), 255);
        var split = new SplitContainer { Dock = DockStyle.Fill };
        split.Panel1.Padding = new Padding(22, 18, 12, 22); split.Panel2.Padding = new Padding(12, 18, 22, 22);
        split.Panel1.Controls.Add(Editor()); split.Panel2.Controls.Add(Preview()); Controls.Add(split); Controls.Add(bar);
        Load += (_, _) => { split.Panel1MinSize = 400; split.Panel2MinSize = 440; split.SplitterDistance = Math.Clamp((int)(split.Width * .42), 400, split.Width - 440); };
    }
    void TopButton(Panel bar, string text, Color color, EventHandler click, int right)
    {
        var b = new Button { Text = text, Size = new Size(102, 38), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = color == Color.White ? Color.FromArgb(15, 23, 42) : Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(bar.Width - right - 102, 16), Font = new Font("Segoe UI Semibold", 9) };
        b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225); b.FlatAppearance.BorderSize = color == Color.White ? 1 : 0; b.Click += click; bar.Resize += (_, _) => b.Left = bar.ClientSize.Width - right - b.Width; bar.Controls.Add(b);
    }
    Control Editor()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 9) };
        tabs.TabPages.Add(Page("Personal", Personal())); tabs.TabPages.Add(Page("Work history", Work())); tabs.TabPages.Add(Page("Education", Education())); tabs.TabPages.Add(Page("Skills", Skills())); tabs.TabPages.Add(Page("Design", Design())); return tabs;
    }
    static TabPage Page(string name, Control body) { var p = new TabPage(name) { BackColor = Color.White, Padding = new Padding(12) }; p.Controls.Add(body); return p; }
    static FlowLayoutPanel ScrollStack() => new() { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(2) };
    static FlowLayoutPanel CardStack() => new() { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = 440, BackColor = Color.White };
    static FlowLayoutPanel PillStack() => new() { FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = 430, BackColor = Color.White };

    Control Personal()
    {
        var s = ScrollStack(); photoBox = new PictureBox { Size = new Size(102, 102), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(226, 232, 240) };
        var upload = Accent("Upload photo"); upload.Click += (_, _) => UploadPhoto(); var crop = new Button { Text = "Crop / position", AutoSize = true, FlatStyle = FlatStyle.Flat, Margin = new Padding(8, 37, 0, 0) }; crop.Click += (_, _) => CropPhoto();
        var row = new FlowLayoutPanel { Width = 440, Height = 118 }; row.Controls.AddRange([photoBox, upload, crop]); s.Controls.Add(row);
        Field(s, "Full name", "e.g. Alex Morgan", x => resume.FullName = x); Field(s, "Professional title", "e.g. Product Designer", x => resume.JobTitle = x); Field(s, "Email", "alex@example.com", x => resume.Email = x); Field(s, "Phone", "+63 555 123 4567", x => resume.Phone = x); Field(s, "Location", "City, Country", x => resume.Address = x); Field(s, "LinkedIn / portfolio", "linkedin.com/in/alex", x => resume.Link = x); Field(s, "Professional summary", "A concise introduction to your strengths.", x => resume.Summary = x, true); return s;
    }
    Control Work() { var s = ScrollStack(); var add = Accent("+ Add experience"); add.Click += (_, _) => { var x = new WorkExperience(); resume.WorkExperience.Add(x); WorkCard(x); Queue(); }; s.Controls.Add(add); s.Controls.Add(workCards); return s; }
    void WorkCard(WorkExperience x) { var c = Card(); Field(c, "Role", "Product Designer", v => x.Role = v); Field(c, "Company", "Company name", v => x.Company = v); Field(c, "Dates", "Jan 2022 - Present", v => x.Dates = v); Field(c, "Highlights", "Describe outcomes, impact, and responsibilities.", v => x.Description = v, true); Remove(c, () => { resume.WorkExperience.Remove(x); workCards.Controls.Remove(c); Queue(); }); workCards.Controls.Add(c); }
    Control Education() { var s = ScrollStack(); var add = Accent("+ Add education"); add.Click += (_, _) => { var x = new Education(); resume.Education.Add(x); EducationCard(x); Queue(); }; s.Controls.Add(add); s.Controls.Add(educationCards); return s; }
    void EducationCard(Education x) { var c = Card(); Field(c, "Qualification", "B.S. Computer Science", v => x.Degree = v); Field(c, "School", "University name", v => x.School = v); Field(c, "Dates", "2017 - 2021", v => x.Dates = v); Remove(c, () => { resume.Education.Remove(x); educationCards.Controls.Remove(c); Queue(); }); educationCards.Controls.Add(c); }
    Control Skills()
    {
        var s = ScrollStack(); s.Controls.Add(new Label { Text = "Skills", AutoSize = true, Font = new Font("Segoe UI Semibold", 12) }); var input = new TextBox { Width = 290, PlaceholderText = "Type a skill and press Enter" };
        input.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { AddSkill(input.Text); input.Clear(); e.SuppressKeyPress = true; } }; var add = Accent("Add"); add.Click += (_, _) => { AddSkill(input.Text); input.Clear(); }; var row = new FlowLayoutPanel { Width = 440, Height = 45 }; row.Controls.AddRange([input, add]); s.Controls.Add(row); s.Controls.Add(skillPills); return s;
    }
    void AddSkill(string value) { value = value.Trim(); if (value.Length == 0 || resume.Skills.Contains(value, StringComparer.OrdinalIgnoreCase)) return; resume.Skills.Add(value); var p = new Button { Text = value + "  x", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 253, 250), ForeColor = Color.Teal, Margin = new Padding(0, 4, 6, 4) }; p.Click += (_, _) => { resume.Skills.Remove(value); skillPills.Controls.Remove(p); Queue(); }; skillPills.Controls.Add(p); Queue(); }

    Control Design()
    {
        var s = ScrollStack(); s.Controls.Add(new Label { Text = "Choose a resume layout", AutoSize = true, Font = new Font("Segoe UI Semibold", 12) }); s.Controls.Add(new Label { Text = "Click a card to preview and select it.", AutoSize = true, ForeColor = Color.SlateGray, Margin = new Padding(0, 2, 0, 8) });
        var gallery = new FlowLayoutPanel { Width = 440, Height = 166, FlowDirection = FlowDirection.LeftToRight, WrapContents = false }; gallery.Controls.Add(TemplateCard(0, "Modern", "Teal sidebar")); gallery.Controls.Add(TemplateCard(1, "Classic", "Traditional header")); gallery.Controls.Add(TemplateCard(2, "Minimal", "Clean single column")); s.Controls.Add(gallery); SetTemplate(resume.Template);
        s.Controls.Add(new Label { Text = "Customize", AutoSize = true, Font = new Font("Segoe UI Semibold", 12), Margin = new Padding(0, 16, 0, 3) });
        var colorRow = new FlowLayoutPanel { Width = 440, Height = 38 }; colorRow.Controls.Add(new Label { Text = "Accent color", AutoSize = true, Width = 105, Margin = new Padding(0, 8, 0, 0) }); var color = new Button { Text = "Choose color", AutoSize = true, BackColor = Color.FromArgb(13, 148, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; color.Click += (_, _) => ChooseColor(color); colorRow.Controls.Add(color); s.Controls.Add(colorRow);
        var fontRow = new FlowLayoutPanel { Width = 440, Height = 42 }; fontRow.Controls.Add(new Label { Text = "Font", AutoSize = true, Width = 105, Margin = new Padding(0, 8, 0, 0) }); var fonts = new ComboBox { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList }; fonts.Items.AddRange(["Arial", "Calibri", "Georgia", "Segoe UI", "Times New Roman", "Verdana"]); fonts.SelectedItem = resume.FontFamily; fonts.SelectedIndexChanged += (_, _) => { resume.FontFamily = fonts.Text; Queue(); }; fontRow.Controls.Add(fonts); s.Controls.Add(fontRow);
        return s;
    }
    Panel TemplateCard(int id, string title, string description)
    {
        var p = new Panel { Size = new Size(132, 140), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 12, 0), Cursor = Cursors.Hand, Tag = id };
        var swatch = new Panel { Location = new Point(10, 10), Size = new Size(110, 66), BackColor = id == 0 ? Color.FromArgb(13, 148, 136) : Color.FromArgb(241, 245, 249) };
        swatch.Paint += (_, e) => { using var pen = new Pen(id == 0 ? Color.White : Color.FromArgb(71, 85, 105), 2); e.Graphics.DrawLine(pen, 17, 15, 17, 52); e.Graphics.DrawLine(pen, 31, 18, 90, 18); e.Graphics.DrawLine(pen, 31, 28, 78, 28); e.Graphics.DrawLine(pen, 31, 42, 87, 42); };
        var name = new Label { Text = title, AutoSize = true, Location = new Point(10, 88), Font = new Font("Segoe UI Semibold", 9) }; var sub = new Label { Text = description, AutoSize = true, Location = new Point(10, 108), ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 7.5f) }; var selected = new Label { Text = "SELECTED", AutoSize = true, Location = new Point(10, 121), ForeColor = Color.FromArgb(13, 148, 136), Font = new Font("Segoe UI Semibold", 7) };
        p.Controls.AddRange([swatch, name, sub, selected]); templateCards.Add(p); void select(object? _, EventArgs __) => SetTemplate(id); p.Click += select; swatch.Click += select; name.Click += select; sub.Click += select; selected.Click += select; return p;
    }
    void SetTemplate(int id) { resume.Template = id; foreach (var card in templateCards) { var selected = (int)card.Tag! == id; card.BackColor = selected ? Color.FromArgb(240, 253, 250) : Color.White; card.Padding = selected ? new Padding(2) : Padding.Empty; card.Controls.OfType<Label>().Last().Visible = selected; } Queue(); }
    void ChooseColor(Button button) { using var d = new ColorDialog { Color = ColorTranslator.FromHtml(resume.AccentColor) }; if (d.ShowDialog() != DialogResult.OK) return; resume.AccentColor = ColorTranslator.ToHtml(d.Color); button.BackColor = d.Color; Queue(); }

    static FlowLayoutPanel Card() => new() { Width = 430, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(14), Margin = new Padding(0, 10, 0, 4) };
    void Field(FlowLayoutPanel s, string label, string hint, Action<string> set, bool multiline = false) { s.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(0, 7, 0, 3) }); var i = new TextBox { Width = 395, PlaceholderText = hint, Multiline = multiline, Height = multiline ? 72 : 30, BorderStyle = BorderStyle.FixedSingle }; i.TextChanged += (_, _) => { set(i.Text); Queue(); }; s.Controls.Add(i); }
    static Button Accent(string text) { var b = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(13, 148, 136), ForeColor = Color.White, Margin = new Padding(0, 8, 0, 6) }; b.FlatAppearance.BorderSize = 0; return b; }
    static void Remove(FlowLayoutPanel c, Action action) { var b = new Button { Text = "Remove entry", AutoSize = true, FlatStyle = FlatStyle.Flat, ForeColor = Color.Firebrick, Margin = new Padding(0, 8, 0, 0) }; b.Click += (_, _) => action(); c.Controls.Add(b); }
    Control Preview() { var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White }; p.Controls.Add(preview); var tag = new Label { Text = "LIVE PREVIEW", AutoSize = true, BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White, Padding = new Padding(8, 4, 8, 4), Location = new Point(12, 12) }; p.Controls.Add(tag); tag.BringToFront(); return p; }
    void Queue() { debounce.Stop(); debounce.Start(); }
    Task RenderAsync()
    {
        if (!ready) return Task.CompletedTask;
        try { preview.NavigateToString(TemplateEngine.Render(resume)); }
        catch (ArgumentException) { MessageBox.Show("This resume preview is too large to render. Use a smaller photo or crop it before continuing.", "Preview size limit", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        return Task.CompletedTask;
    }
    void UploadPhoto() { using var d = new OpenFileDialog { Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp" }; if (d.ShowDialog() != DialogResult.OK) return; sourcePhoto?.Dispose(); sourcePhoto = Image.FromFile(d.FileName); ApplyPhoto(sourcePhoto); }
    void CropPhoto() { if (sourcePhoto is null) { MessageBox.Show("Upload a photo first."); return; } using var editor = new ImageCropForm(sourcePhoto); if (editor.ShowDialog() == DialogResult.OK) ApplyPhoto(editor.CroppedImage!); }
    void ApplyPhoto(Image image)
    {
        using var compact = ResizeForResume(image, 500);
        var copy = new Bitmap(compact); photoBox!.Image?.Dispose(); photoBox.Image = copy;
        using var ms = new MemoryStream(); compact.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        resume.PhotoDataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray()); Queue();
    }
    static Bitmap ResizeForResume(Image image, int maximum)
    {
        var scale = Math.Min(1f, maximum / (float)Math.Max(image.Width, image.Height));
        var width = Math.Max(1, (int)Math.Round(image.Width * scale)); var height = Math.Max(1, (int)Math.Round(image.Height * scale));
        var result = new Bitmap(width, height); using var graphics = Graphics.FromImage(result);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(image, 0, 0, width, height); return result;
    }
    void SaveDraft() { using var d = new SaveFileDialog { Filter = "Resume draft|*.json", FileName = "my-resume.json" }; if (d.ShowDialog() == DialogResult.OK) { SaveLoadService.Save(d.FileName, resume); MessageBox.Show("Draft saved."); } }
    void LoadDraft() { using var d = new OpenFileDialog { Filter = "Resume draft|*.json" }; if (d.ShowDialog() != DialogResult.OK) return; var x = SaveLoadService.Load(d.FileName); if (x is null) return; resume.CopyFrom(x); workCards.Controls.Clear(); educationCards.Controls.Clear(); skillPills.Controls.Clear(); foreach (var w in resume.WorkExperience) WorkCard(w); foreach (var e in resume.Education) EducationCard(e); foreach (var k in resume.Skills.ToList()) { resume.Skills.Remove(k); AddSkill(k); } SetTemplate(resume.Template); Queue(); }
    async Task ExportPdfAsync() { if (!ready) return; using var d = new SaveFileDialog { Filter = "PDF document|*.pdf", FileName = "my-resume.pdf" }; if (d.ShowDialog() != DialogResult.OK) return; try { await preview.CoreWebView2.PrintToPdfAsync(d.FileName, preview.CoreWebView2.Environment.CreatePrintSettings()); MessageBox.Show("Your resume was exported successfully."); } catch (Exception ex) { MessageBox.Show(ex.Message, "PDF export failed"); } }
}
