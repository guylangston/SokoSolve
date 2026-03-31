using SkiaSharp;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using Svg.Skia;
namespace SokoSolve.UI.SkiaUI.Tests;

public class PuzzleAssets<T>
{
    public required T Wall { get; set; }
    public required T Floor { get; set; }
    public required T Crate { get; set; }
    public required T Goal { get; set; }
    public required T Player { get; set; }
}


public class CanDrawPuzzleWithSvgAssets  : TestAssetHelper
{

    [Fact]
    public void DrawGridPattern()
    {
        int width = 640;
        int height = 600;
        var size = 32;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = false
        };

        for (int x = 0; x <= width; x += size)
        {
            canvas.DrawLine(x, 0, x, height, paint);
        }

        for (int y = 0; y <= height; y += size)
        {
            canvas.DrawLine(0, y, width, y, paint);
        }

        var assets = new PuzzleAssets<SKPicture>()
        {
            Wall = LoadSvg("wall.svg"),
            Floor = LoadSvg("floor.svg"),
            Crate = LoadSvg("crate.svg"),
            Goal = LoadSvg("goal.svg"),
            Player = LoadSvg("player.svg")
        };

        var dead = new SKPaint()
        {
            ColorF = new SKColor(0, 255, 0, 0),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = SKColors.Red,
        };

        var puzzle = PuzzleLibraryStatic.PQ1_P5;
        var st = new StaticAnalysisMaps(puzzle);
        foreach(var tile in puzzle.ForEachTile())
        {
            var px = tile.Position.X * size;
            var py = tile.Position.Y * size;
            if (tile.Cell == puzzle.Definition.Wall)
            {
                DrawSvgPic(canvas, assets.Wall, px, py, size, size);
            }
            if (tile.Cell.IsFloor)
            {
                DrawSvgPic(canvas, assets.Floor, px, py, size, size);
            }
            if (tile.Cell.IsGoal)
            {
                DrawSvgPic(canvas, assets.Goal, px, py, size, size);
            }
            if (tile.Cell.IsCrate)
            {
                DrawSvgPic(canvas, assets.Crate, px, py, size, size);
            }
            if (tile.Cell.IsPlayer)
            {
                DrawSvgPic(canvas, assets.Player, px, py, size, size);
            }
            if (st.DeadMap[tile.Position.X, tile.Position.Y])
            {
                canvas.DrawRect(px, py, size-1, size-1, dead);
            }
        }


        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(GetPathOutputUsingCallerName(".png"));
        data.SaveTo(stream);

        static  void DrawSvgPic(SKCanvas canvas, SKPicture pic, float x, float y, float width, float height)
        {
            canvas.Save();
            canvas.Translate(x, y);
            // canvas.RotateDegrees((float)shape.Rotation);
            canvas.Scale(width / pic.CullRect.Width, height / pic.CullRect.Height);
            canvas.DrawPicture(pic);
            canvas.Restore();
        }
    }
}
