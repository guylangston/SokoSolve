using SkiaSharp;
using SkiaUI.Core;

namespace SkiaUI.Gtk;

public class SceneSimpleDialog : ISkiaScene
{
    public SceneSimpleDialog(ISkiaApp app)
    {
        App = app;
        TitlePaint = app.AssetFactory.GetPaint("Title") ?? new SKPaint { Color = SKColors.White };
        TitleFont = app.AssetFactory.GetFont("Title");
        BodyPaint = app.AssetFactory.GetPaint("Body") ?? new SKPaint { Color = SKColors.White };
        BodyFont = app.AssetFactory.GetFont("Body");
    }

    public ISkiaApp App { get; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public string? Footer { get; set; }
    public SKPaint TitlePaint { get; set; }
    public SKFont TitleFont { get; set; }
    public SKPaint BodyPaint { get; set; }
    public SKFont BodyFont { get; set; }

    public void HandleKeyPress(SkiaAppKey key)
    {
    }

    public void HandleMousePress(SkiaAppMouse mouse)
    {
    }

    public void Paint(SKSurface surface)
    {
        surface.Canvas.Clear(SKColors.Black);
        surface.Canvas.DrawText(Title, 100, 100, TitleFont, TitlePaint);
        surface.Canvas.DrawText(Body, 100, 300, BodyFont, BodyPaint);
        if (Footer != null) surface.Canvas.DrawText(Footer, 100, 400, BodyFont, BodyPaint);
    }

    public virtual void Step(TimeSpan step)
    {
    }
}

