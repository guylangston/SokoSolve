using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Lite
{
    public interface IGameObject
    {
        VectorInt2 Position { get; set; }
        Block      Block    { get; }
    }
    
    public abstract class GameWithObjects<TObj> : Game where TObj : IGameObject
    {
        public List<TObj> Objects { get; } = new List<TObj>();

        public GameWithObjects(Map start) : base(start)
        {
        }

        protected abstract TObj ToGameObject(Block b, VectorInt2 p);

        public override void Init()
        {
            base.Init();

            foreach (var cell in base.Start.EachBlock())
            {
                Objects.Add(ToGameObject(cell.b, cell.p));
            }
        }

        protected override void MoveCrate(Map newState, VectorInt2 pp, VectorInt2 ppp)
        {
            base.MoveCrate(newState, pp, ppp);
            Get(Block.Crate, pp).Position = ppp;
        }

        public TObj Get(Block blk, VectorInt2 pp) => Objects.First(x => x.Block == blk && x.Position == pp);

        protected override void MovePlayer(Map newState, VectorInt2 p, VectorInt2 pp)
        {
            base.MovePlayer(newState, p, pp);
            Get(Block.Player, p).Position = pp;
        }

        public override bool UndoMove()
        {
            return base.UndoMove();
        }

        public override void Reset()
        {
            base.Reset();
            Init();
            
        }
    }


    public class GameObject : IGameObject
    {
        public GameObject(VectorInt2 position, Block block)
        {
            Position = position;
            Block    = block;
        }

        public VectorInt2 Position { get; set; }
        public Block      Block    { get; }
    }

    public class GameWithObjectSimple : GameWithObjects<GameObject>
    {
        public GameWithObjectSimple(Map start) : base(start)
        {
        }

        protected override GameObject ToGameObject(Block b, VectorInt2 p)
        {
            return new GameObject(p, b);
        }
    }
}