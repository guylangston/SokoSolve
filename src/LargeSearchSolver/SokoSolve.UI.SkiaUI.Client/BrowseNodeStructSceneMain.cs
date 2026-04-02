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
    private SKPaint paintDead;
    SKRect canvasSize;
    int size = 32;
    private uint nodeId;
    public ISkiaApp App { get; }
    public LSolverState SolverState { get; }


    public BrowseNodeStructSceneMain(ISkiaApp app, LSolverState solverState)
    {
        App = app;
        SolverState = solverState;
        nodeId = solverState.RootForward;
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
        this.paintDead = new SKPaint()
        {
            ColorF = new SKColor(0, 255, 0, 0),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = SKColors.Red,
        };
    }

    string lastKey = "";
    public void HandleKeyPress(SkiaAppKey key)
    {
        lastKey = key.Key;
        ref var node = ref SolverState.Heap.GetById(nodeId);
        if (key.Key == "Up") //up
        {
            if (node.ParentId != NodeStruct.NodeId_NULL)
            {
                nodeId = node.ParentId;
            }
        }
        if (key.Key == "Down") //down
        {
            if (node.FirstChildId != NodeStruct.NodeId_NULL)
            {
                nodeId = node.FirstChildId;
            }
        }
        if (key.Key == "Right") // next/right
        {
            if (node.SiblingNextId != NodeStruct.NodeId_NULL)
            {
                nodeId = node.SiblingNextId;
            }
        }
        if (key.Key == "r")
        {
            Console.WriteLine(node.ToDebugString(SolverState.NodeStructContext, true, SolverState));
        }
        if (key.Key == "Left") // prev/left
        {
            if (NodeStructTreeHelper.TryGetPreviousSibling(ref node, SolverState.Heap, out var prev))
            {
                nodeId = prev;
            }
        }
    }

    string mouseTxt = "";
    public void HandleMousePress(SkiaAppMouse mouse)
    {
        var state = SolverState;
        var layout = new LayoutGrid(size,size, state.Request.Puzzle.Width, state.Request.Puzzle.Height);
        var offset = new PixelTransformOffset(new SKPoint(10, 10));
        var center = new PixelTransformCenter(canvasSize, layout.Region);

        var g = offset.Inverse(new SKPoint((float)mouse.X, (float)mouse.Y));
        var gg = center.Inverse(g);
        layout.TryLookup((int)gg.X, (int)gg.Y, out var match);

        var pt = "<NONE>";
        if (match.X > 0 && match.X < state.Request.Puzzle.Width &&
            match.Y > 0 && match.Y < state.Request.Puzzle.Height)
        {
            pt = state.Request.Puzzle[match.X, match.Y].ToString();
        }

        mouseTxt = $"{mouse.X:0}:{mouse.Y:0} -> grid: o{g}:c{gg}:g{match} => {pt}";
    }

    public void Paint(SKSurface surface)
    {
        var canvas = surface.Canvas;
        surface.Canvas.Clear(SKColors.DarkBlue);

        var state = SolverState;
        ref var node = ref state.Heap.GetById(nodeId);
        canvasSize = new SKRect(0,0, surface.Canvas.DeviceClipBounds.Width,  surface.Canvas.DeviceClipBounds.Height);
        var layout = new LayoutGrid(size,size, state.Request.Puzzle.Width, state.Request.Puzzle.Height);
        var offset = new PixelTransformOffset(new SKPoint(10, 10));
        var center = new PixelTransformCenter(canvasSize, layout.Region);
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
            if (node.GetCrateMapAt(state.NodeStructContext, (byte)cell.Data.X, (byte)cell.Data.Y))
            {
                canvas.DrawSvgPic(assets.Crate, dr);
            }
            if (node.GetMoveMapAt(state.NodeStructContext, (byte)cell.Data.X, (byte)cell.Data.Y))
            {
                // todo
            }
            if (state.StaticMaps.DeadMap[cell.Data.X, cell.Data.Y])
            {
                canvas.DrawRect(SKRect.Inflate(dr, -2f, -2f), paintDead);

            }
            if (cell.Data.X == node.PlayerX && cell.Data.Y == node.PlayerY)
            {
                canvas.DrawSvgPic(assets.Player, dr);
            }

        }

        var n1 = $"Depth: {NodeStructTreeHelper.GetDepth(ref node, SolverState.Heap)} ChildrenRev:{NodeStructTreeHelper.GetChildCountRecursive(ref node, SolverState.Heap)}";
        var nodeTxt = $"{NodeStruct.NodeIdToStr(node.NodeId)} ^{NodeStruct.NodeIdToStr(node.ParentId)} v{NodeStruct.NodeIdToStr(node.FirstChildId)} >{NodeStruct.NodeIdToStr(node.SiblingNextId)} {n1}";
        canvas.DrawText(nodeTxt, 20f, canvasSize.Bottom- 60f, dbFont, dbPaint);
        canvas.DrawText($"MOUSE: { surface.Canvas.DeviceClipBounds.Width}x{surface.Canvas.DeviceClipBounds.Height} -> {mouseTxt}", 20f, canvasSize.Bottom- 20f, dbFont, dbPaint);
        canvas.DrawText($"KEY:   '{lastKey}'        NODE: {nodeId}", 20f, canvasSize.Bottom- 40f, dbFont, dbPaint);
    }

    public void Step(TimeSpan step)
    {

    }
}

