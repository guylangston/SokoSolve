using System;
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

        public AnimatedPuzzleGameLoop(MasterGameLoop parent, ConsoleRendererCHAR_INFO renderer,
            LibraryPuzzle                            libraryPuzzle) : base(parent)
        {
            this.renderer = renderer;
            LibraryPuzzle = libraryPuzzle;
            GameLogic          = new ConsoleAnimatedSokobanGame(LibraryPuzzle, renderer, this);
        }

        public LibraryPuzzle              LibraryPuzzle { get; }
        public ConsoleAnimatedSokobanGame GameLogic          { get; }
        public InputProvider              Input         => Parent.Input;
        
        public CHAR_INFO HeaderStyle = new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY);
        public CHAR_INFO InfoStyle = new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_BLUE | CHAR_INFO_Attr.FOREGROUND_INTENSITY);
        public CHAR_INFO DefaultStyle = new CHAR_INFO(' ');

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
        
        private void RestartPuzzle()
        {
            GameLogic.Reset();
        }

        private void UndoMove()
        {
            GameLogic.UndoMove();
        }


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
            // Setup
           
            renderer.Fill(new CHAR_INFO(' '));

            // Animated game
            GameLogic.Draw();

           

            // Paint
            renderer.Update();
            System.Console.Title = $"{FramesPerSecond:0.0}fps - {LibraryPuzzle.Name} Rating: {LibraryPuzzle.Rating}";
        }

        public override void Dispose()
        {
            GameLogic.Dispose();
        }
    }
}