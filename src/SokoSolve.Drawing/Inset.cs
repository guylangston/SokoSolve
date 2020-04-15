using System.Numerics;
using VectorInt;

namespace SokoSolve.Drawing
{
    public class Inset : Translation<Rect2, Rect2>
    {
        private readonly Vector2 topLeft;
        private readonly Vector2 bottomRight;
        
        public Inset(Vector2 topLeft)
        {
            this.topLeft     = topLeft;
            this.bottomRight = topLeft;
        }

        public Inset(Vector2 topLeft, Vector2 bottomRight)
        {
            this.topLeft     = topLeft;
            this.bottomRight = bottomRight;
        }

        public Vector2 TopLeft => topLeft;

        public Vector2 BottomRight => bottomRight;

        public override Rect2 Translate(Rect2 input)  => Rect2.FromTwoPoints(input.TL + topLeft, input.BR - bottomRight );
        public override Rect2 Inverse(Rect2 output) => Rect2.FromTwoPoints(output.TL - topLeft, output.BR + bottomRight );
    }
}