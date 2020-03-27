using System;
using System.Collections.Generic;
using SokoSolve.Core;
using VectorInt;

namespace SokoSolve.Game
{
    public interface IAnimation
    {
        /// <returns>false means done/dispose</returns>
        bool Step(GameElement element);
    }

    public class GameElement
    {
        private List<GameElement> children;

        public GameElement()
        {
            ZIndex = 0;
        }

        public AnimatedSokobanGame      Game          { get; set; }
        public GameElement?             Parent        { get; set; }
        public IEnumerable<GameElement> Children      => children;
        public List<IAnimation>         Animations    { get; set; }
        public bool                     HasAnimations => Animations != null && Animations.Count > 0;
        public CellDefinition<char>     Type          { get; set; }
        public int                      ZIndex        { get; set; }
        public VectorInt2               StartState    { get; set; }
        public VectorInt2               Position      { get; set; }
        public VectorInt2               PositionOld   { get; set; }
        public Action<GameElement>      Paint         { get; set; }
        public int                      Id            { get; set; }

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

        public virtual void Step(float elapsedSec)
        {
            if (Children != null)
                foreach (var child in Children)
                    child.Step(elapsedSec);

            if (Animations != null && Animations.Count > 0)
            {
                var remove = new List<IAnimation>();
                foreach (var animation in Animations)
                    if (!animation.Step(this))
                        remove.Add(animation);
                remove.ForEach(x => Animations.Remove(x));
            }
        }

        public virtual void Draw()
        {
            if (Paint != null) Paint(this);
        }


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

        public override string ToString() => 
            (Position != PositionOld) 
                ? $"[{Type}]@{Position}vs{PositionOld}:{ZIndex} S:{StartState} "
                : $"[{Type}]@{Position}:{ZIndex}";
    }
}