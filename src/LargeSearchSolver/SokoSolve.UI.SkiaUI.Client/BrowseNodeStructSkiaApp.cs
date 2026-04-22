using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;
using SkiaUI.Core;
using SokoSolve.LargeSearchSolver;
using SokoSolve.Primitives;
using Svg.Skia;

namespace SkiaUI.Gtk;

public class BrowseNodeStructSkiaApp : SkiaAppBase, ISkiaAppMainScene
{
    Stack<ISkiaScene> stack = new();
    State state;
    private SKPaint dbPaint;
    private SKFont dbFont;

    public BrowseNodeStructSkiaApp(Puzzle puzzle, Func<object, object> hostCallback) : base(hostCallback, new MyAssets())
    {
        base.IsLogEventsEnabled = false;
        Active = new SceneSimpleDialog(this)
        {
            Title = "Loading...",
            Body = "Creating solver tree"
        };
        state = new State
        {
            Puzzle = puzzle
        };
        this.dbPaint = AssetFactory.GetPaint("Debug");
        this.dbFont = AssetFactory.GetFont("Debug");
    }

    class State
    {
        public required Puzzle Puzzle { get; init; }
        public LSolverRequest? Request { get; set; }
        public LSolverState? SolverState { get;  set; }
        public DisplayState StateEnum { get; internal set; }
    }

    class MyAssets : ISkiaAppAssetFactory
    {
        readonly SKTypeface typeFace = SKTypeface.FromFamilyName("Jetbrains Mono", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        public string GetAssetPath(string file)
        {
            // TODO: Scan backwards
            return System.IO.Path.Combine("/home/guy/repo/SokoSolve/src/LargeSearchSolver/SokoSolve.UI.SkiaUI.Client/assets/", file);
        }

        public SKFont GetFont(string? name = null)
        {
            return name switch
            {
                null => new SKFont(typeFace, 16),
                "Body" => new SKFont(typeFace, 16),
                "Debug" => new SKFont(typeFace, 12),
                "Title" => new SKFont(typeFace, 32),
                _ => throw new NotSupportedException(name ?? "<null>"),
            };
        }

        public SKPaint GetPaint(string name)
        {
            return new SKPaint { Color = SKColors.White };
        }

        public SKPicture GetSvg(string name)
        {
            var svg = new SKSvg();
            svg.Load(GetAssetPath(name + ".svg"));
            return svg.Picture ?? throw new Exception($"Invalid svg: {name}");
        }
    }

    public ISkiaScene Active { get; private set; }
    public IReadOnlyList<ISkiaScene> SceneStack => stack.ToArray();
    // public override bool HandleAppEvent(object app) => base.HandleAppEvent(app);

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
        surface.Canvas.DrawText($"{state.StateEnum}:{Active.GetType().Name}", 10, 20, dbFont, dbPaint);
    }

    enum DisplayState
    {
        Uninit,
        WaitingSolverState,
        TransitionMain,
        Main
    }

    class MyPeek : ISolverCoodinatorPeek
    {
        public int PeekEvery => 1_000;
        Action<LSolverState, long> peek;

        public MyPeek(Action<LSolverState, long> peek)
        {
            this.peek = peek;
        }

        public void Finished()
        {
        }

        public bool TickUpdate(LSolverState state, int totalNodes, ref NodeStruct current)
        {
            peek(state, totalNodes);
            return true;
        }

    }

    public override void Step(TimeSpan step)
    {
        base.Step(step);
        if (state.StateEnum == DisplayState.Uninit)
        {
            state.StateEnum = DisplayState.WaitingSolverState;
            state.Request = new LSolverRequest(state.Puzzle, new AttemptConstraints()
                    {
                        MaxNodes = 10_000
                    });
            Task.Run(
                ()=>
                {
                    var coord = new SolverCoordinator()
                    {
                        StateFactory = new SolverCoordinatorFactory()
                        {
                            Tags = new HashSet<string>(["FwdOnly"])
                        },
                        Peek = new MyPeek((s, t)=>
                                {
                                    if (Active is SceneSimpleDialog sd)
                                    {
                                        sd.Footer = $"Total Nodes: {t}";
                                    }
                                })

                    };
                    var sstate = coord.Init(state.Request);
                    var result = coord.Solve(sstate);

                    state.SolverState = sstate;  // UI will move on
                    if (Active is SceneSimpleDialog sd)
                    {
                        sd.Body = $"Done in {sstate.Ended - sstate.Started}";
                    }
                    state.StateEnum = DisplayState.TransitionMain; // Tranistion picked on in next Step/some thread
                });
        }
        else if (state.StateEnum == DisplayState.TransitionMain)
        {
            Active = new BrowseNodeStructSceneMain(this, state.SolverState ?? throw new NullReferenceException());
            state.StateEnum = DisplayState.Main;
        }
        else if (state.StateEnum == DisplayState.Main)
        {
            // by scene
        }

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
