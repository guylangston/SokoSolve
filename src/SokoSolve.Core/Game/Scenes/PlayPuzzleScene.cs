using System;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using SokoSolve.Core.Lib;
using VectorInt;

namespace SokoSolve.Core.Game.Scenes
{
    public class PlayPuzzleScene : GameScene<SokoSolveMasterGameLoop, SokobanPixel>
    {
        public PlayPuzzleScene(SokoSolveMasterGameLoop parent, LibraryPuzzle libraryPuzzle) : base(parent)
        {
            LibraryPuzzle = libraryPuzzle;
            GameLogic          = new ConsoleAnimatedSokobanGame(LibraryPuzzle, parent.Renderer, this, parent.Style);
        }

        public LibraryPuzzle              LibraryPuzzle { get; }
        public ConsoleAnimatedSokobanGame GameLogic          { get; }


        public SokobanPixel HeaderStyle => Parent.Style.TextTitle.AsPixel();
        public SokobanPixel InfoStyle => Parent.Style.Info.AsPixel();
        public SokobanPixel DefaultStyle => Parent.Style.DefaultPixel;

        public override void Init()
        {
            GameLogic.Init(LibraryPuzzle.Puzzle);
        }

        public override void Step(float elapsedSec)
        {
            if (GameLogic.LastMoveResult == MoveResult.Win)
            {
                // TODO: Win Animation
                Parent.PuzzleComplete();
            }
            
            HandleInput();
            GameLogic.Step(elapsedSec);
        }

        protected virtual void HandleInput()
        {
            if (Input.IsKeyPressed(ConsoleKey.UpArrow)) Move(VectorInt2.Up);
            if (Input.IsKeyPressed(ConsoleKey.DownArrow)) Move(VectorInt2.Down);
            if (Input.IsKeyPressed(ConsoleKey.LeftArrow)) Move(VectorInt2.Left);
            if (Input.IsKeyPressed(ConsoleKey.RightArrow)) Move(VectorInt2.Right);
            if (Input.IsKeyPressed(ConsoleKey.U)) UndoMove();
            if (Input.IsKeyPressed(ConsoleKey.R)) RestartPuzzle();

            if (Input.IsKeyPressed(ConsoleKey.Escape)) Parent.PuzzleGivingUp();
            if (Input.IsKeyPressed(ConsoleKey.Q)) Parent.PuzzleGivingUp();
        }
        
        private void RestartPuzzle() => GameLogic.Reset();
        private void UndoMove() => GameLogic.UndoMove();

        protected virtual void Move(VectorInt2 dir)
        {
            var r = GameLogic.Move(dir);
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
            // Animated game
            GameLogic.Draw();
        }

        public override void Dispose()
        {
            GameLogic.Dispose();
        }
    }
}