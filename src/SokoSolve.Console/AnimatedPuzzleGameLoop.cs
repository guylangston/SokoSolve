using System;
using System.Linq;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Common;
using SokoSolve.Core.Game;
using SokoSolve.Core.Library;
using VectorInt;

namespace SokoSolve.Console
{
    public class AnimatedPuzzleGameLoop : GameLoopProxy<MasterGameLoop>
    {
        private ConsoleRendererCHAR_INFO renderer; 
        
        public AnimatedPuzzleGameLoop(MasterGameLoop parent, ConsoleRendererCHAR_INFO renderer, LibraryPuzzle libraryPuzzle) : base(parent)
        {
            this.renderer = renderer;
            LibraryPuzzle = libraryPuzzle;
            Game = new ConsoleAnimatedSokobanGame(LibraryPuzzle, renderer);
        }

        public LibraryPuzzle              LibraryPuzzle { get; }
        public ConsoleAnimatedSokobanGame Game          { get; }
        public InputProvider              Input         => Parent.Input;
      
        
        public override void Init()
        {
            Game.Init(LibraryPuzzle.Puzzle);
           
        }

        public override void Step(float elapsedSec)
        {
            HandleInput();
            Game.Step(elapsedSec);
        }

          
        protected virtual void HandleInput()
        {
            if (Input.IsKeyPressed(ConsoleKey.UpArrow))     Move(VectorInt2.Up);
            if (Input.IsKeyPressed(ConsoleKey.DownArrow))   Move(VectorInt2.Down);
            if (Input.IsKeyPressed(ConsoleKey.LeftArrow))   Move(VectorInt2.Left);
            if (Input.IsKeyPressed(ConsoleKey.RightArrow))  Move(VectorInt2.Right);
            if (Input.IsKeyPressed(ConsoleKey.U))           Game.UndoMove();
            if (Input.IsKeyPressed(ConsoleKey.R))           Game.Reset();
            
            if (Input.IsKeyPressed(ConsoleKey.Escape))      Parent.PuzzleGivingUp();
            if (Input.IsKeyPressed(ConsoleKey.Q))           Parent.PuzzleGivingUp();

            if (Input.IsMouseEnabled)
            {
                if (Input.IsMouseClick)
                {
                    // TODO: Mouse Movement + Pushing
                }
            }
        }
        
        protected virtual void Move(VectorInt2 dir)
        {
            var r = Game.Move(dir);
            if (r == MoveResult.Win)
            {
                Parent.PuzzleComplete();
            }
            else if (r == MoveResult.Invalid)
            {
                // TODO: Give feedback: "Oops?"
            }
        }
        
        public override void Draw()
        {
            // Setup
            var txtStyle = new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY);
            renderer.Fill(new CHAR_INFO(' '));

            // Animated game
            Game.Draw();
            
            // Overloys: Mouse
            if (Input.IsMouseEnabled)
            {
                var mousePosition = Input.MousePosition;
                var pz = mousePosition - Game.PuzzleSurface.TL;
                renderer.DrawText(0, 0, mousePosition.ToString().PadRight(20), txtStyle);

                if (Game.PuzzleSurface.Contains(mousePosition))
                {
                    
                    var pc = Game.Current[pz];
                    renderer.DrawText(0, 1, $"{pz} -> {pc.Underlying}".PadRight(40), txtStyle);
                }
                else
                {
                    renderer.DrawText(0, 1, $"".PadRight(40), txtStyle);
                }


                var start = renderer.Geometry.BL;
                foreach (var (item, index) in Game.ElementsAt(pz).WithIndex())
                {
                    renderer.DrawText(start - (0, index), item.ToString(), txtStyle);    
                }

                
            }
            
            // Paint
            renderer.Update();
            System.Console.Title = $"{FramesPerSecond:0.0}fps - {LibraryPuzzle.Name} Rating: {LibraryPuzzle.Rating}";
        }

        public override void Dispose()
        {
            Game.Dispose();
        }
    }

    public class ConsoleAnimatedSokobanGame : AnimatedSokobanGame
    {
        private readonly ConsoleRendererCHAR_INFO renderer;

        public ConsoleAnimatedSokobanGame(LibraryPuzzle puzzle, ConsoleRendererCHAR_INFO renderer) : base(puzzle)
        {
            this.renderer = renderer;
        }
        
        public RectInt PuzzleSurface { get; set; }

        public override void Init(Puzzle puzzle)
        {
            base.Init(puzzle);
            PuzzleSurface = RectInt.CenterAt(renderer.Geometry.C, puzzle.Area);
        }

        protected override GameElement Factory(CellDefinition<char> part, VectorInt2 startState)
        {
            return new GameElement()
            {
                Position = startState,
                PositionOld = startState,
                StartState = startState,
                Game = this,
                Type = part,
                Paint = DefaultPaint,
                ZIndex =  part.MemberOf.All.IndexOf(part)
            };
        }
        
        

        private void DefaultPaint(GameElement el)
        {
            renderer[el.Position + PuzzleSurface.TL] = new CHAR_INFO(el.Type.Underlying);
        }
    }
}