using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;
using SkiaUI.Core;
using Svg.Skia;

namespace SkiaUI.Gtk;

public class BrowseNodeStruct : SkiaAppBase, ISkiaAppMainScene
{
    Stack<ISkiaScene> stack = new();

    public BrowseNodeStruct(Func<object, object> hostCallback) : base(hostCallback, new MyAssets())
    {
        Active = new SceneSimpleDialog(this)
        {
            Title = "Loading...",
            Body = "Creating solver tree"
        };
    }

    class MyAssets : ISkiaAppAssetFactory
    {
        readonly SKTypeface typeFace = SKTypeface.FromFamilyName("Jetbrains Mono",
                        SKFontStyleWeight.Normal,
                        SKFontStyleWidth.Normal,
                        SKFontStyleSlant.Upright);

        public SKFont GetFont(string? name = null)
        {
            if (name == null)
            {
                return new SKFont(typeFace, 16);
            }
            if (name == "Body")
            {
                return new SKFont(typeFace, 16);
            }
            if (name == "Title")
            {
                return new SKFont(typeFace, 32);
            }
            throw new NotSupportedException();
        }

        public SKPaint GetPaint(string name)
        {
            return new SKPaint { Color = SKColors.White };
        }
    }

    public ISkiaScene Active { get; private set; }
    public IReadOnlyList<ISkiaScene> SceneStack => stack.ToArray();

    public override bool HandleAppEvent(object app)
    {
        return base.HandleAppEvent(app);
    }

    public override void HandleKeyPress(SkiaAppKey key)
    {
        base.HandleKeyPress(key);
        Active.HandleKeyPress(key);
    }

    public override void HandleMousePress(SkiaAppMouse mouse)
    {
        base.HandleMousePress(mouse);
        Active.HandleMousePress(mouse);
    }

    public override void Paint(SKSurface surface)
    {
        Active.Paint(surface);
    }

    public override void Step(TimeSpan step)
    {
        base.Step(step);
        Active.Step(step);
    }

    public void ScenePop()
    {
    }

    public void ScenePush(ISkiaScene scene)
    {
        stack.Push(scene);
        Active = scene;
    }
}
