using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using VectorInt;

namespace SokoSolve.Console
{
    public class PuzzleGameLoop : GameLoopProxy
    {
        private ConsoleRendererCHAR_INFO renderer; 
        private Dictionary<char, CHAR_INFO_Attr> theme;
        private Dictionary<char, char> themeChar;
        
        public PuzzleGameLoop(GameLoopBase parent, ConsoleRendererCHAR_INFO renderer) : base(parent)
        {
            this.renderer = renderer;
        }
        

        public InputProvider Input { get; set; }
        public SokobanGameLogic GameLogic { get; set; }
        public Puzzle Current => GameLogic.Current;

        public override void Init()
        {
            this.Input = new InputProvider()
            {
                EnableMouse = true
            };


            GameLogic = new SokobanGameLogic( Puzzle.Builder.DefaultTestPuzzle());
            var def = GameLogic.Current.Definition;
            
            theme = new Dictionary<char, CHAR_INFO_Attr>()
            {
                {def.Void.Underlying,  CHAR_INFO_Attr.BACKGROUND_GRAY },
                {def.Wall.Underlying,  CHAR_INFO_Attr.BACKGROUND_GRAY},
                {def.Floor.Underlying, CHAR_INFO_Attr.FOREGROUND_GRAY },
                {def.Goal.Underlying,  CHAR_INFO_Attr.FOREGROUND_GRAY },
                {def.Crate.Underlying, CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.BACKGROUND_BLUE },
                {def.CrateGoal.Underlying,  CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {def.Player.Underlying,   CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {def.PlayerGoal.Underlying,   CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY },
            };
            themeChar = def.ToDictionary(x => x.Underlying, x => x.Underlying);
            
            // https://www.fileformat.info/info/unicode/block/box_drawing/list.htm
            // http://www.fileformat.info/info/unicode/block/block_elements/images.htm
            themeChar[def.Wall.Underlying] = (char)0xB1;
            themeChar[def.Void.Underlying] = ' ';
            themeChar[def.Floor.Underlying] = ' ';
            themeChar[def.Player.Underlying] = (char)0x02;
            themeChar[def.PlayerGoal.Underlying] = (char)0x02;
            themeChar[def.Crate.Underlying] = (char)0x15;
            themeChar[def.CrateGoal.Underlying] = (char)0x7f;
        }

        

        public override void Step(float elapsedSec)
        {
            HandleInput();
        }
        
        protected virtual void HandleInput()
        {
            if (Input.IsKeyPressed(ConsoleKey.UpArrow))     GameLogic.Move(VectorInt2.Up);
            if (Input.IsKeyPressed(ConsoleKey.DownArrow))   GameLogic.Move(VectorInt2.Down);
            if (Input.IsKeyPressed(ConsoleKey.LeftArrow))   GameLogic.Move(VectorInt2.Left);
            if (Input.IsKeyPressed(ConsoleKey.RightArrow))  GameLogic.Move(VectorInt2.Right);
            if (Input.IsKeyPressed(ConsoleKey.U))           GameLogic.UndoMove();
            if (Input.IsKeyPressed(ConsoleKey.R))           GameLogic.Reset();

            Input.Step();

        }

        public override void Draw()
        {
            var puzzle = new RectInt(0, 0, Current.Width, Current.Height);
            var pos = RectInt.CenterAt(renderer.Geometry.C, puzzle);
            
            renderer.Box(pos.Outset(2,2,2,2), RendererExt.AsciiBox );
            foreach (var tile in Current)
            {
                renderer[pos.TL + tile.Position] = new CHAR_INFO(themeChar[ tile.Value.Underlying], theme[tile.Value.Underlying]);
            }

            var txtStyle= new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY);
            var txt = this.Elapsed.ToString();
            var txtPos = renderer.Geometry.TM - new VectorInt2(txt.Length /2, 0);
            renderer.DrawText(txtPos.X, txtPos.Y, txt, txtStyle );

            if (Input.EnableMouse)
            {
                renderer.DrawText(0, 0, Input.MousePosition.ToString().PadRight(20), txtStyle);

                if (pos.Contains(Input.MousePosition))
                {
                    var pz = Input.MousePosition - pos.TL;
                    var pc = Current[pz];
                    renderer.DrawText(0, 1, $"{pz} -> {pc.Underlying}".PadRight(40), txtStyle);
                }
                else
                {
                    renderer.DrawText(0, 1, $"".PadRight(40), txtStyle);
                }
            }

            // Draw KeyMap
            renderer.DrawText(renderer.Geometry.TR, "Key-Map", new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_RED), false);
            DrawingHelper.DrawByteMatrix(renderer, renderer.Geometry.TR - (16, -1), 
                x=>new CHAR_INFO((char)x, (ushort)Input.KeyDown[x]));

            // FPS
            var fps = $"FPS: {FramesPerSecond:0.0}/sec, {FrameCount} frames";
            renderer.DrawText(renderer.Geometry.BR + (0, -1), fps, new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GRAY), false);
            System.Console.Title = fps;

            renderer.Update();

            
        }

       


        public override void Dispose()
        {
            Input.Dispose();
        }
    }
}