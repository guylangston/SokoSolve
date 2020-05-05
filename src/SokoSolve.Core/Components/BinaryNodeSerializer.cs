using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using VectorInt;

namespace SokoSolve.Core.Components
{
    public class BinaryNodeSerializer
    {
        public class StagingSolverNode
        {
            public int     SolverNodeId  { get; set; }
            public int     ParentId      { get; set; }
            public int     PlayerBeforeX { get; set; }
            public int     PlayerBeforeY { get; set; }
            public int     PushX         { get; set; }
            public int     PushY         { get; set; }
            public byte    Status        { get; set; }
            public byte[]  Crate         { get; set; }
            public byte[]  Move          { get; set; }
            public IBitmap CrateMap      { get; set; }
            public IBitmap MoveMap       { get; set; }
            public int HashCode { get; set; }

            public override string ToString()
            {
                return $"{nameof(SolverNodeId)}: {SolverNodeId}, {nameof(ParentId)}: {ParentId}, {nameof(PlayerBeforeX)}: {PlayerBeforeX}, {nameof(PlayerBeforeY)}: {PlayerBeforeY}, {nameof(PushX)}: {PushX}, {nameof(PushY)}: {PushY}, {nameof(Status)}: {Status}, {nameof(Crate)}: {Crate}, {nameof(Move)}: {Move}, {nameof(HashCode)}: {HashCode}";
            }
        }

        public void Write(BinaryWriter sw, SolverNode n)
        {
            sw.Write(n.SolverNodeId);
            sw.Write(n.Parent?.SolverNodeId ?? 0);
            sw.Write(n.PlayerBefore.X);
            sw.Write(n.PlayerBefore.Y);
            sw.Write(n.Push.X);
            sw.Write(n.Push.Y);
            sw.Write((byte)n.Status);
            sw.Write(n.GetHashCode());
                
            var c = n.CrateMap is BitmapByteSeq bs ? bs : new BitmapByteSeq(n.CrateMap);
            var cc = c.GetArray();
            sw.Write(cc.Length);
            sw.Write(cc);
                
            var m = n.MoveMap is BitmapByteSeq ms ? ms : new BitmapByteSeq(n.MoveMap);
            var mm = m.GetArray();
            sw.Write(mm.Length);
            sw.Write(mm);
        }

        public StagingSolverNode Read(BinaryReader sr)
        {
            var temp = new StagingSolverNode
            {
                SolverNodeId  = sr.ReadInt32(),
                ParentId      = sr.ReadInt32(),
                PlayerBeforeX = sr.ReadInt32(),
                PlayerBeforeY = sr.ReadInt32(),
                PushX         = sr.ReadInt32(),
                PushY         = sr.ReadInt32(),
                Status        = sr.ReadByte(),
                HashCode      = sr.ReadInt32(),
            };

            var l = sr.ReadInt32();
            temp.Crate = sr.ReadBytes(l);
                
            l         = sr.ReadInt32();
            temp.Move = sr.ReadBytes(l);
            return temp;
        }

        public static readonly byte[] MagicHeaderPreamble = new byte [] {3, 5, 7, 11};
        public static int Version = 1;

        public void WriteHeader(BinaryWriter sw, VectorInt2 dSize, int count)
        {
            sw.Write(MagicHeaderPreamble);
            sw.Write(Version);
            sw.Write(dSize.X);
            sw.Write(dSize.Y);
            sw.Write(count);
        }

        public (VectorInt2 size, int count) ReadHeader(BinaryReader br)
        {
            var preamble = br.ReadBytes(4);
            if (!MagicHeaderPreamble.SequenceEqual(preamble)) throw new InvalidDataException("Bad Preamble");;
            var v = br.ReadInt32();
            if (Version != v) throw new InvalidDataException("Bad Version");

            var x = br.ReadInt32();
            var y = br.ReadInt32();
            var c = br.ReadInt32();
            return  (new VectorInt2(x, y), c);
        }

        public void Write(BinaryWriter bw,  IReadOnlyCollection<SolverNode> allNodes)
        {
            WriteHeader(bw, allNodes.First().MoveMap.Size, allNodes.Count);
            foreach (var node in allNodes)
            {
                Write(bw, node);    
            }
        }
        
        // Better memory usage, does not creat an array of all nodes
        public void WriteTree(BinaryWriter bw, SolverNode root)
        {
            var count = root.CountRecursive();
            WriteHeader(bw, root.MoveMap.Size, count);
            foreach (var node in root.Recurse())
            {
                Write(bw, node);    
            }
        }

        public IEnumerable<StagingSolverNode> ReadAll(BinaryReader br)
        {
            var header = ReadHeader(br);
            for (int i = 0; i < header.count; i++)
            {
                var temp = Read(br);
                temp.CrateMap = new BitmapByteSeq(header.size, temp.Crate);
                temp.MoveMap = new BitmapByteSeq(header.size, temp.Move);

                yield return temp;
            }
        }

        public SolverNode AssembleTree(BinaryReader br)
        {
            var all = ReadAll(br).ToImmutableDictionary(x => x.SolverNodeId);
            var parents = all.Values.GroupBy(x => x.ParentId).ToImmutableDictionary(x=>x.Key, y=>y.ToImmutableArray());
            var root = all.Values.First(x => x.ParentId == 0);
            return Assemble(null, root, all, parents);
        }

        private SolverNode Assemble(
            SolverNode parent, 
            StagingSolverNode flat,
            ImmutableDictionary<int, StagingSolverNode> all,
            ImmutableDictionary<int, ImmutableArray<StagingSolverNode>> parents)
        {
            var n = new SolverNode(parent, 
                new VectorInt2(flat.PlayerBeforeX, flat.PlayerBeforeY),
                new VectorInt2(flat.PushX, flat.PushY),
                flat.CrateMap,
                flat.MoveMap,
                flat.SolverNodeId
                );
            n.Status = (SolverNodeStatus)flat.Status;

            if (parents.TryGetValue(flat.SolverNodeId, out var kids))
            {
                foreach (var kid in kids)
                {
                    n.Add(Assemble(n, kid, all, parents));
                }    
            }

            

            return n;
        }

      
    }
}