using System.Drawing;
using System.Drawing.Imaging;

var sizes = new[] { 16, 32, 48, 256 };
var bitmaps = new List<Bitmap>();

foreach (var size in sizes)
{
    var bmp = new Bitmap(size, size);
    using var g = Graphics.FromImage(bmp);
    g.Clear(Color.Transparent);

    // Draw a simple shield shape
    using var brush = new SolidBrush(Color.FromArgb(137, 180, 250)); // #89B4FA
    var margin = size / 8;
    var rect = new Rectangle(margin, margin, size - margin * 2, size - margin * 2);
    g.FillRectangle(brush, rect);

    // Draw a "3D" text-like pattern
    using var pen = new Pen(Color.FromArgb(30, 30, 46), Math.Max(1, size / 16));
    var cx = size / 2;
    var cy = size / 2;
    var r = size / 4;
    g.DrawLine(pen, cx - r, cy - r, cx + r, cy - r);
    g.DrawLine(pen, cx + r, cy - r, cx + r, cy);
    g.DrawLine(pen, cx + r, cy, cx - r, cy);
    g.DrawLine(pen, cx - r, cy, cx - r, cy + r);
    g.DrawLine(pen, cx - r, cy + r, cx + r, cy + r);

    bitmaps.Add(bmp);
}

// Save as .ico
var iconPath = "DF3D/Resources/Assets/tray-icon.ico";
using var fs = new FileStream(iconPath, FileMode.Create);
using var writer = new BinaryWriter(fs);

// ICO header
writer.Write((short)0);           // Reserved
writer.Write((short)1);           // Type: ICO
writer.Write((short)sizes.Length); // Number of images

var imageDataOffset = 6 + sizes.Length * 16;
var imageDatas = new List<byte[]>();

foreach (var bmp in bitmaps)
{
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    imageDatas.Add(ms.ToArray());
}

// Directory entries
for (int i = 0; i < sizes.Length; i++)
{
    var w = sizes[i] > 255 ? (byte)0 : (byte)sizes[i];
    var h = w;
    writer.Write(w);               // Width
    writer.Write(h);               // Height
    writer.Write((byte)0);         // Color palette
    writer.Write((byte)0);         // Reserved
    writer.Write((short)1);        // Color planes
    writer.Write((short)32);       // Bits per pixel
    writer.Write(imageDatas[i].Length); // Image data size
    writer.Write(imageDataOffset); // Image data offset
    imageDataOffset += imageDatas[i].Length;
}

// Image data
foreach (var data in imageDatas)
{
    writer.Write(data);
}

Console.WriteLine("Icon created: " + iconPath);
