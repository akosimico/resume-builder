namespace ResumeBuilder;

/// <summary>Simple square crop editor: zoom and position the source photograph.</summary>
public sealed class ImageCropForm : Form
{
    readonly Image source;
    readonly PictureBox canvas = new() { Location = new Point(20, 20), Size = new Size(320, 320), BackColor = Color.FromArgb(30, 41, 59) };
    readonly TrackBar zoom = new() { Minimum = 100, Maximum = 300, Value = 100, TickFrequency = 25, Location = new Point(20, 378), Width = 320 };
    readonly TrackBar horizontal = new() { Minimum = -100, Maximum = 100, Location = new Point(20, 428), Width = 320 };
    readonly TrackBar vertical = new() { Minimum = -100, Maximum = 100, Location = new Point(20, 478), Width = 320 };
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Image? CroppedImage { get; private set; }

    public ImageCropForm(Image image)
    {
        source = image; Text = "Crop and position photo"; ClientSize = new Size(360, 580); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false; StartPosition = FormStartPosition.CenterParent;
        Controls.AddRange([canvas, new Label { Text = "Zoom", Location = new Point(20, 355), AutoSize = true }, zoom, new Label { Text = "Move left / right", Location = new Point(20, 405), AutoSize = true }, horizontal, new Label { Text = "Move up / down", Location = new Point(20, 455), AutoSize = true }, vertical]);
        var save = new Button { Text = "Use crop", DialogResult = DialogResult.OK, Location = new Point(244, 530), Size = new Size(96, 30) }; save.Click += (_, _) => CroppedImage = Crop(); var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(160, 530), Size = new Size(76, 30) }; Controls.AddRange([save, cancel]);
        zoom.ValueChanged += (_, _) => Draw(); horizontal.ValueChanged += (_, _) => Draw(); vertical.ValueChanged += (_, _) => Draw(); Draw();
    }
    void Draw() { var image = Crop(); canvas.Image?.Dispose(); canvas.Image = image; }
    Bitmap Crop()
    {
        var result = new Bitmap(320, 320); using var g = Graphics.FromImage(result); g.Clear(Color.FromArgb(30, 41, 59)); g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        var scale = Math.Max(320f / source.Width, 320f / source.Height) * zoom.Value / 100f; var w = source.Width * scale; var h = source.Height * scale; var x = (320 - w) / 2 + horizontal.Value * 1.2f; var y = (320 - h) / 2 + vertical.Value * 1.2f; g.DrawImage(source, x, y, w, h); return result;
    }
}
