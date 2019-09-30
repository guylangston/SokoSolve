using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Library;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.PuzzleLogic;

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
        public string Name { get; set; }
    }

    public class Statistics
    {
        public Statistics()
        {
            Started = Completed = DateTime.MinValue;
        }

        // Standard
        public int Steps { get; set; }
        public int Pushes { get; set; }
        public int Undos { get; set; }
        public int Restarts { get; set; }
        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }
        public TimeSpan Elapased { get { return (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started; } }
        public double DurationInSec { get { return Elapased.TotalSeconds;}}

    

        public override string ToString()
        {
            return string.Format("Steps: {0}, Pushes: {1}, Undos: {2}, Restarts: {3}", Steps, Pushes, Undos, Restarts);
        }
    }


    // Features:
    // o Keyboard - Push (Up, Down, Left, Right)
    // o Undo/Redo Stack
    // o Bookmarks
    // o Statistics
    // o MouseMove (Drap Drop, PosA to PosB)
    // o PlayerAfter Aids: DeadMap, ValidWalk, ValidPush
    public class SokobanGame : SokobanGameLogic, IDisposable
    {
        private readonly LibraryComponent lib;
        private int cc = 1;


        public SokobanGame()
        {
            lib = new LibraryComponent(null);
            RootElements = new List<GameElement>();
            AllElements = new List<GameElement>();
            
            Bookmarks = new List<Bookmark>();
        
            Profile = lib.LoadProfile(lib.GetPathData("Profiles//guy.profile"));
            Library = lib.LoadLibrary(lib.GetPathData("LegacySSX//"+ Profile.Current.Library + ".ssx"));
            
            Console = new ConsoleElement();
            ToBeRemoved = new List<GameElement>();
            PostStepInstructions = new List<Action<SokobanGame>>();
        }


        public List<GameElement> ToBeRemoved { get; protected set; }
        protected List<GameElement> RootElements { get; set; }
        protected List<GameElement> AllElements { get; set; }

        protected List<Bookmark> Bookmarks { get; set; }

        protected Library.Library Library { get; set; }

        protected Profile Profile { get; set; }

        public ConsoleElement Console { get; protected set; }

        public MouseController MouseController { get; protected set; }

        public PuzzleAnalysis Analysis { get; protected set; }

        public void Dispose()
        {
            lib.SaveProfile(Profile, Profile.FileName);
        }

        public virtual bool HasPendingMoves
        {
            get { return false; }
        }

        public List<Action<SokobanGame>> PostStepInstructions { get; set; }

        public virtual void Draw()
        {
            foreach (var e in AllElements)
            {
                e.Draw();
            }
        }

        public virtual void Step()
        {
            foreach (var e in RootElements)
            {
                e.Step(); // nested steps handles by GameElement.Step()
            }
            if (ToBeRemoved.Any())
            {
                foreach (var e in ToBeRemoved)
                {
                    RemoveElement(e);
                }    
                ToBeRemoved.Clear();
            }
            if (PostStepInstructions != null && PostStepInstructions.Any())
            {
                foreach (var action in PostStepInstructions)
                {
                    action(this);
                }
                PostStepInstructions.Clear();
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

        public virtual void NextPuzzle()
        {
            var p = Library.GetNext(Profile.Current.Puzzle);
            if (p == null)
            {
                Console.WriteLine("No more puzzles. TODO: Next in collection");
                return;
            }
            Profile.Current.Puzzle = p.Name;
            Init(p);
        }

        public virtual void PrevPuzzle()
        {
            var p = Library.GetPrev(Profile.Current.Puzzle);
            if (p == null)
            {
                Console.WriteLine("This is the first puzzle");
                return;
            }
            Profile.Current.Puzzle = p.Name;
            Init(p);
        }


        public virtual void CurrentPuzzle()
        {
            Init(Library[Profile.Current.Puzzle]);
        }

        public virtual void Init()
        {
            CurrentPuzzle();
        }

        public virtual void Init(Puzzle puzzle)
        {
            if (puzzle == null) throw new ArgumentNullException("puzzle");

            Statistics = new Statistics()
            {
                Started = DateTime.Now
            };
            Start =  Current = puzzle;

            Analysis = new PuzzleAnalysis(Start);
            
            PuzzleStack.Clear();
            PuzzleStack.Push(puzzle);

            InitElements();

            var name = "unnamed";
            var lp = puzzle as LibraryPuzzle;
            if (lp != null && lp.Details != null && !string.IsNullOrWhiteSpace(lp.Details.Name))
            {
                name = lp.Details.Name;
            }
            Console.WriteLine("You are taking on the '{0}' puzzle.", name);
        }

        public virtual void InitElements()
        {
            RootElements.Clear();
            AllElements.Clear();
            foreach (var cell in Current)
            {
                Init(cell);
            }
            if (MouseController != null)
            {
                AddAndInitElement(MouseController);
            }
            if (Console != null)
            {
                AddAndInitElement(Console);
            }
        }


        private void Init(Cell cell)
        {
            var parts = Start.Definition.Seperate(cell.State);
            foreach (var part in parts)
            {
                AddAndInitElement(Factory(part, cell));
            }
        }


        
        public virtual void ReplaySolution()
        {
            var lib = Start as LibraryPuzzle;
            if (lib != null && lib.Solution != null)
            {
                Reset();

                Console.WriteLine("Replaying Solution");
                foreach (var move in lib.Solution)
                {
                    Move(move);
                }
                return;
            }

            Console.WriteLine("This puzzle has no solutions :-(");
        }

        public List<GameElement> ElementsAt(VectorInt2 p)
        {
            return RootElements.FindAll(x => x.Position.Equals(p));
        }

        public GameElement ElementAt(VectorInt2 p, char type)
        {
            var all = ElementsAt(p);
            return all.FirstOrDefault(x => x.Type == type);
        }

        public virtual void AddAndInitElement(GameElement e)
        {
            e.Game = this;
            e.ZIndex = cc++;
           
           
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

        protected virtual GameElement Factory(char part, Cell cell)
        {
            return new GameElement()
            {
                Game = this,
                Type = part,
                Position = cell.Position,
                StartState = cell
            };
        }


        public virtual void Undo()
        {
            if (!PuzzleStack.Any())
            {
                return;
            }

            Console.WriteLine("Grrr.");

            Statistics.Undos++;
            MoveStack.Pop();
            Current = PuzzleStack.Pop();

            InitElements();
        }

        public virtual void Reset()
        {
            if (!PuzzleStack.Any())
            {
                return;
            }

            Console.WriteLine("Oops.. Starting again");

            Statistics.Restarts++;
            Current = PuzzleStack.Last();

            PuzzleStack.Clear();
            MoveStack.Clear();

            InitElements();
        }

        public virtual Bookmark CaptureAsBookmark()
        {
            return null;
        }

    }
}