using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Lite
{
    public interface IGameObject
    {
        VectorInt2 Position { get; set; }
        VectorInt2 Size { get; set; }

        VectorInt2 BottomRight => Position + Size;
        Block      Block    { get; }
    }
    
    public abstract class GameWithObjects<TObj> : Game where TObj : class, IGameObject
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

        public IEnumerable<IGameObject> FindAt(VectorInt2 p)
        {
            foreach (var item in Objects)
            {
                if (item.Position.X >= p.X && item.BottomRight.X <= p.X &&
                    item.Position.Y >= p.Y && item.BottomRight.Y <= p.Y)
                    yield return item;
            }
        }
    }


    public class GameObject : IGameObject
    {
        public GameObject(VectorInt2 position, VectorInt2 size, Block block)
        {
            Position = position;
            Block    = block;
            Size = size;
        }

        public VectorInt2 Position { get; set; }
        public VectorInt2 Size { get; set; }
        public Block      Block    { get; }
        
    }

    public class GameWithObjectSimple : GameWithObjects<GameObject>
    {
        public GameWithObjectSimple(Map start, VectorInt2 size) : base(start)
        {
            Size = size;
        }

        public VectorInt2 Size { get; }

        protected override GameObject ToGameObject(Block b, VectorInt2 p)
        {
            return new GameObject(p, Size, b);
        }
    }
}