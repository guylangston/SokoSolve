using SkiaSharp;
using SkiaUI.Core;
using SokoSolve.LargeSearchSolver;

namespace SkiaUI.Gtk;

public static class SkiaCanvasHelper
{
    extension(SKCanvas canvas)
    {
        public void DrawSvgPic(SKPicture pic, SKRect drawIn)
        {
            canvas.Save();
            canvas.Translate(drawIn.Left, drawIn.Top);
            canvas.Scale(drawIn.Width / pic.CullRect.Width, drawIn.Height / pic.CullRect.Height);
            canvas.DrawPicture(pic);
            canvas.Restore();
        }
    }
}

public class PuzzleAssets<T>
{
    public required T Wall { get; set; }
    public required T Floor { get; set; }
    public required T Crate { get; set; }
    public required T Goal { get; set; }
    public required T Player { get; set; }
}

public class BrowseNodeStructSceneMain : ISkiaScene
{
    PuzzleAssets<SKPicture> assets;
    private SKPaint dbPaint;
    private SKFont dbFont;
    int size = 32;
    public ISkiaApp App { get; }
    public LSolverState SolverState { get; }

    public BrowseNodeStructSceneMain(ISkiaApp app, LSolverState solverState)
    {
        App = app;
        SolverState = solverState;
        assets = new PuzzleAssets<SKPicture>()
        {
            Wall   = app.AssetFactory.GetSvg("wall"),
            Floor  = app.AssetFactory.GetSvg("floor"),
            Crate  = app.AssetFactory.GetSvg("crate"),
            Goal   = app.AssetFactory.GetSvg("goal"),
            Player = app.AssetFactory.GetSvg("player")
        };
        this.dbPaint = app.AssetFactory.GetPaint("Debug");
        this.dbFont = app.AssetFactory.GetFont("Debug");
    }

    public void HandleKeyPress(SkiaAppKey key)
    {
    }

    public void HandleMousePress(SkiaAppMouse mouse)
    {
    }

    public void Paint(SKSurface surface)
    {
        var canvas = surface.Canvas;
        surface.Canvas.Clear(SKColors.DarkBlue);

        var state = SolverState;
        ref var root = ref state.Heap.GetById(state.RootForward);

        var layout = new LayoutGrid(size, size, state.Request.Puzzle.Width, state.Request.Puzzle.Height);
        var offset = new PixelTransformOffset(new SKPoint(10, 10));
        var center = new PixelTransformCenter(
                new SKRect(0,0, surface.Canvas.DeviceClipBounds.Width,  surface.Canvas.DeviceClipBounds.Height),
                layout.Region);
        foreach(var cell in layout.ForEach())
        {
            var dp = offset.Apply(center.Apply(cell.Location.Location));
            var dr = new SKRect(dp.X, dp.Y, dp.X + layout.CellWidth, dp.Y + layout.CellHeight);
            if (state.StaticMaps.WallMap[cell.Data.X, cell.Data.Y])
            {
                canvas.DrawSvgPic(assets.Wall, dr);
            }
            if (state.StaticMaps.FloorMap[cell.Data.X, cell.Data.Y])
            {
                canvas.DrawSvgPic(assets.Floor, dr);
            }
            if (state.StaticMaps.GoalMap[cell.Data.X, cell.Data.Y])
            {
                canvas.DrawSvgPic(assets.Goal, dr);
            }
            if (root.GetCrateMapAt(state.NodeStructContext, (byte)cell.Data.X, (byte)cell.Data.Y))
            {
                canvas.DrawSvgPic(assets.Crate, dr);
            }
            if (root.GetMoveMapAt(state.NodeStructContext, (byte)cell.Data.X, (byte)cell.Data.Y))
            {
                // todo
            }
            if (state.StaticMaps.DeadMap[cell.Data.X, cell.Data.Y])
            {
            }
        }

        canvas.DrawText($"{ surface.Canvas.DeviceClipBounds.Width}x{surface.Canvas.DeviceClipBounds.Height}", 10, 20, dbFont, dbPaint);

    }

    public void Step(TimeSpan step)
    {

    }
}

