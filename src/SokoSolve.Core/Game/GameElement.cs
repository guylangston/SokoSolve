using System.Collections.Generic;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.PuzzleLogic;

namespace SokoSolve.Core.Game
{

    public interface IAnimation
    {
        /// <returns>false means done/dispose</returns>
        bool Step(GameElement element);
    }

    public class GameElement
    {
        public GameElement()
        {
            ZIndex = int.MinValue;
        }

        private List<GameElement> children = null; 

        public GameElement Parent { get; set; }

        public IEnumerable<GameElement> Children { get { return children; } }


        public List<IAnimation> Animations { get; set; }

        public bool HasAnimations
        {
            get { return Animations != null && Animations.Count > 0; }
        }

        public SokobanGame Game { get; set; }

        public char Type { get; set; }

        public int ZIndex { get; set; }

        public Cell StartState { get; set; }

        public VectorInt2 Position { get; set; }

        public VectorInt2 PositionOld { get; set; }

        public void Move(VectorInt2 dir)
        {
            PositionOld = Position;
            Position = Position + dir;
        }
        
        public void Add(IAnimation animation)
        {
            if (Animations == null) Animations = new List<IAnimation>();
            Animations.Add(animation);
        }

        public void Remove(IAnimation animation)
        {
            if (Animations == null) return;
            Animations.Remove(animation);
        }

        public virtual void Step()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Step();
                }
            }

            if (Animations != null && Animations.Count > 0)
            {
                var remove = new List<IAnimation>();
                foreach (var animation in Animations)
                {
                    if (!animation.Step(this))
                    {
                        remove.Add(animation);
                    }
                }
                remove.ForEach(x=>Animations.Remove(x));
            }
        }
        public virtual void Draw() { }


        public void Add(GameElement element)
        {
            element.Parent = this;
            if (children == null) children = new List<GameElement>();
            children.Add(element);
            Game.AddAndInitElement(element);
        }

        public void Remove(GameElement element)
        {

            if (children == null) return;
            children.Remove(element);
            Game.RemoveElement(element);
            element.Parent = null;
        }

        public virtual void Init()
        {

        }

        public override string ToString()
        {
            return string.Format("Type: {0}, StartState: {1}, Position: {2}, PositionOld: {3}", Type, StartState, Position, PositionOld);
        }
    }
}