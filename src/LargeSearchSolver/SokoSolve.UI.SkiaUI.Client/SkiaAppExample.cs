using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;
using SkiaUI.Core;
using Svg.Skia;

namespace SkiaUI.Gtk;

public class SkiaAppExample : ISkiaApp
{
    public TimeSpan Elapsed { get; set; }
    public int FrameCount { get; set; }
    public Func<object, object> HostCallback { get; set; }

    public ISkiaAppAssetFactory? AssetFactory { get; init; }

    private readonly SKFont fontDefault;
    private readonly SKFont fontLabel;
    private readonly SKPaint paintBlack;
    private readonly SKPaint paintWhite;
    private readonly SKPaint paintOverlay;
    private readonly List<AssetItem> assets = new List<AssetItem>();
    int eventCounter=0;

    private class AssetItem
    {
        public string Name { get; set; } = "";
        public SKPicture? Picture { get; set; }
        public SKRect Bounds { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
    }

    public SkiaAppExample()
    {
        paintWhite = new SKPaint()
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill
        };
        paintBlack = new SKPaint()
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill
        };
        paintOverlay = new SKPaint()
        {
            Color = new SKColor(0, 255, 255, 60),
            Style = SKPaintStyle.Fill
        };

        var noto = SKTypeface.FromFamilyName("Jetbrains Mono",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);
        fontDefault = new SKFont(noto, 30);
        fontLabel = new SKFont(noto, 16);

        LoadAssets();
    }

    private void LoadAssets()
    {
        var assetPath = Path.Combine(Environment.CurrentDirectory, "assets");
        Console.WriteLine($"pwd: {Environment.CurrentDirectory}");
        Console.WriteLine($"assets: {assetPath}");

        if (!Directory.Exists(assetPath))
            return;

        var svgFiles = Directory.GetFiles(assetPath, "*.svg");

        var rotations = new float[] { 0, 15, -30, 45, -15 };
        var scales = new float[] { 1.0f, 0.8f, 1.2f, 0.9f, 1.1f };

        for (int i = 0; i < svgFiles.Length; i++)
        {
            SKPicture? picture = null;
            SKRect bounds = SKRect.Empty;

            try
            {
                var svg = new SKSvg();
                picture = svg.Load(svgFiles[i]);
                if (picture != null)
                {
                    bounds = picture.CullRect;
                }
            }
            catch
            {
                // Skip files that can't be loaded
            }

            assets.Add(new AssetItem
            {
                Name = Path.GetFileNameWithoutExtension(svgFiles[i]),
                Picture = picture,
                Bounds = bounds,
                Rotation = rotations[i % rotations.Length],
                Scale = scales[i % scales.Length]
            });
        }
    }

    public bool HandleAppEvent(object app)
    {
        Console.WriteLine($"{eventCounter++}:HandleAppEvent {app}");
        return true;
    }

    public void HandleKeyPress(SkiaAppKey key)
    {
        Console.WriteLine($"{eventCounter++}:HandleKeyPress {key.Key}");
        if (key.Key == "q") SendHost("Quit");
    }

    public void HandleMousePress(SkiaAppMouse mouse)
    {
        Console.WriteLine($"{eventCounter++}:HandleMousePress X:{mouse.X}, Y:{mouse.Y} Btn:{mouse.Button}[{mouse.Type}]");
    }

    public void Paint(SKSurface surface)
    {
        surface.Canvas.Clear(SKColors.White);
        FrameCount++;

        var txt  = $"Frame {FrameCount}. Elapsed: {Elapsed:hh\\:mm\\:ss}";
        surface.Canvas.DrawText(txt, fontDefault.Size, fontDefault.Size, fontDefault, paintBlack);

        int cols = 3;
        int cellWidth = 200;
        int cellHeight = 200;
        int startX = 50;
        int startY = 80;

        for (int i = 0; i < assets.Count; i++)
        {
            var asset = assets[i];
            int col = i % cols;
            int row = i / cols;

            float x = startX + col * (cellWidth + 40);
            float y = startY + row * (cellHeight + 80);

            surface.Canvas.Save();
            surface.Canvas.Translate(x + cellWidth / 2, y + cellHeight / 2);
            surface.Canvas.RotateDegrees(asset.Rotation);
            surface.Canvas.Scale(asset.Scale);

            if (asset.Picture != null && !asset.Bounds.IsEmpty)
            {
                var svgWidth = asset.Bounds.Width;
                var svgHeight = asset.Bounds.Height;

                if (svgWidth > 0 && svgHeight > 0)
                {
                    float scaleX = (cellWidth * 0.7f) / svgWidth;
                    float scaleY = (cellHeight * 0.7f) / svgHeight;
                    float scale = Math.Min(scaleX, scaleY);

                    surface.Canvas.Scale(scale);
                    surface.Canvas.Translate(-svgWidth / 2, -svgHeight / 2);

                    surface.Canvas.DrawPicture(asset.Picture);

                    var rect = new SKRect(0, 0, svgWidth, svgHeight);
                    surface.Canvas.DrawRect(rect, paintOverlay);
                }
            }

            surface.Canvas.Restore();

            var labelX = x + cellWidth / 2;
            var labelY = y + cellHeight + 20;
            var textWidth = fontLabel.MeasureText(asset.Name);
            surface.Canvas.DrawText(asset.Name, labelX - textWidth / 2, labelY, fontLabel, paintBlack);
        }
    }

    public object SendHost(object obj)
    {
        Console.WriteLine($"{eventCounter++}:SendHost {obj}");
        return HostCallback(obj.ToString());
    }

    public void Step(TimeSpan step)
    {
        Elapsed += step;
    }
}

