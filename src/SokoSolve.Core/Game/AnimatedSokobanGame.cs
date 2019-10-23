using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Library;
using VectorInt;

namespace SokoSolve.Core.Game
{
    public enum MoveResult
    {
        Invalid,
        Ok,
        Win,
        Dead
    }

    public class Bookmark
    {
        public Puzzle Puzzle { get; set; }
        public string Name   { get; set; }
    }

    // Features:
    // o Bookmarks
    // o MouseMove (Drap Drop, PosA to PosB)
    // o PlayerAfter Aids: DeadMap, ValidWalk, ValidPush
    public class AnimatedSokobanGame : SokobanGameLogic, IDisposable
    {
        private int elementIdCounter = 1;

        public AnimatedSokobanGame(LibraryPuzzle puzzle) : base(puzzle.Puzzle)
        {
            RootElements = new List<GameElement>();
            AllElements = new List<GameElement>();
            Bookmarks = new List<Bookmark>();
            Console = new ConsoleElement();
            ToBeRemoved = new List<GameElement>();
        }

        protected List<GameElement> ToBeRemoved     { get; }
        protected List<GameElement> RootElements    { get; }
        protected List<GameElement> AllElements     { get; }
        protected List<Bookmark>    Bookmarks       { get; }
        protected ConsoleElement    Console         { get; set; }
        public    MouseController   MouseController { get; protected set; }
        public    PuzzleAnalysis    Analysis        { get; protected set; }

        public virtual void Draw()
        {
            foreach (var e in AllElements) e.Draw();
        }

        public virtual void Step(float elapsedSec)
        {
            foreach (var e in RootElements) e.Step(); // nested steps handles by GameElement.Step()
            if (ToBeRemoved.Any())
            {
                foreach (var e in ToBeRemoved) RemoveElement(e);
                ToBeRemoved.Clear();
            }
        }

        protected override void MoveCrate(Puzzle newState, VectorInt2 pp, VectorInt2 ppp)
        {
            base.MoveCrate(newState, pp, ppp);

            // Move 'linked' RootElements
            var eCrate = ElementAt(pp, Current.Definition.Crate);
            eCrate.Move(ppp - pp);
        }

        protected override void MovePlayer(Puzzle newState, VectorInt2 p, VectorInt2 pp)
        {
            base.MovePlayer(newState, p, pp);

            // Move 'linked' RootElements
            var ePlayer = ElementAt(p, Current.Definition.Player);
            ePlayer.Move(pp - p);
        }

        public override void Init(Puzzle puzzle)
        {
            base.Init(puzzle);
            Analysis = new PuzzleAnalysis(Start);
            InitElements();
        }

        public virtual void InitElements()
        {
            RootElements.Clear();
            AllElements.Clear();
            foreach (var cell in Current) Init(cell);
            if (MouseController != null) AddAndInitElement(MouseController);
            if (Console != null) AddAndInitElement(Console);
        }

        private void Init((VectorInt2 pos, CellDefinition<char> Cell) tile)
        {
            var parts = tile.Cell.Decompose();
            foreach (var part in parts) AddAndInitElement(Factory(part, tile.pos));
        }

        public virtual void ReplaySolution()
        {
//            var lib = Start;
//            if (lib != null && lib.Solution != null)
//            {
//                Reset();
//
//                Console.WriteLine("Replaying Solution");
//                foreach (var move in lib.Solution) Move(move);
//                return;
//            }
//
//            Console.WriteLine("This puzzle has no solutions :-(");
            throw new NotImplementedException();
        }

        public List<GameElement> ElementsAt(VectorInt2 p)
        {
            return RootElements.FindAll(x => x.Position.Equals(p));
        }

        public GameElement ElementAt(VectorInt2 p, CellDefinition<char> type)
        {
            var all = ElementsAt(p);
            return all.FirstOrDefault(x => x.Type.Equals(type.Underlying));
        }

        public virtual void AddAndInitElement(GameElement e)
        {
            e.Game = this;
            e.ZIndex = elementIdCounter++;

            if (e.Parent == null) RootElements.Add(e);
            AllElements.Add(e);

            // Finally
            e.Init();
        }

        public void RemoveElement(GameElement e)
        {
            if (e.Parent == null) RootElements.Remove(e);
            AllElements.Remove(e);
        }

        protected virtual GameElement Factory(CellDefinition<char> part, VectorInt2 startState) =>
            new GameElement
            {
                Game = this,
                Type = part,
                Position = startState,
                StartState = startState
            };

        public virtual void Undo()
        {
            if (!PuzzleStack.Any()) return;

            Console.WriteLine("Grrr.");

            Statistics.Undos++;
            MoveStack.Pop();
            Current = PuzzleStack.Pop();

            InitElements();
        }

        public void Dispose()
        {
        }
    }
}