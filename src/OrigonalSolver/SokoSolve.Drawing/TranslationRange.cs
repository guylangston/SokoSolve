using System.Numerics;
using VectorInt;

namespace SokoSolve.Drawing
{
    public class TranslationRangeDouble : Translation<double, double>
    {
        private double inputStart;
        private double inputEnd;

        private double outputStart;
        private double outputEnd;

        public TranslationRangeDouble(double inputStart, double inputEnd, double outputStart, double outputEnd)
        {
            this.inputStart  = inputStart;
            this.inputEnd    = inputEnd;
            this.outputStart = outputStart;
            this.outputEnd   = outputEnd;
        }

        public double RangeInput  => inputEnd - inputStart;
        public double RangeOutput => outputEnd - outputStart;

        public override double Translate(double input) => (input - inputStart) / RangeInput * RangeOutput + outputStart;

        public override double Inverse(double output) => (output - outputStart) / RangeOutput * RangeInput + inputStart;
    }

    public class TranslationRangeFloat : Translation<float, float>
    {

        public TranslationRangeFloat(float inputStart, float inputEnd, float outputStart, float outputEnd)
        {
            this.InputStart  = inputStart;
            this.InputEnd    = inputEnd;
            this.OutputStart = outputStart;
            this.OutputEnd   = outputEnd;
        }

        public float InputStart  { get; }
        public float InputEnd    { get; }
        public float OutputStart { get; }
        public float OutputEnd   { get; }

        public float RangeInput  => InputEnd - InputStart;
        public float RangeOutput => OutputEnd - OutputStart;

        public override float Translate(float input) => (input - InputStart) / RangeInput * RangeOutput + OutputStart;

        public override float Inverse(float output) => (output - OutputStart) / RangeOutput * RangeInput + InputStart;
    }

    public class TranslationRangeInt : Translation<int, int>
    {
        private int inputStart;
        private int inputEnd;

        private int outputStart;
        private int outputEnd;

        public TranslationRangeInt(int inputStart, int inputEnd, int outputStart, int outputEnd)
        {
            this.inputStart  = inputStart;
            this.inputEnd    = inputEnd;
            this.outputStart = outputStart;
            this.outputEnd   = outputEnd;
        }

        public int RangeInput  => inputEnd - inputStart;
        public int RangeOutput => outputEnd - outputStart;

        public override int Translate(int input) => (input - inputStart)  * RangeOutput/ RangeInput + outputStart;

        public override int Inverse(int output) => (output - outputStart)  * RangeInput / RangeOutput + inputStart;
    }

    public class TranslationRangeVectorInt : Translation<VectorInt2, VectorInt2>
    {
        private readonly TranslationRangeInt rangeX;
        private readonly TranslationRangeInt rangeY;

        public TranslationRangeVectorInt(TranslationRangeInt rangeX, TranslationRangeInt rangeY)
        {
            this.rangeX = rangeX;
            this.rangeY = rangeY;
        }

        public override VectorInt2 Translate(VectorInt2 input) => new VectorInt2(rangeX.Translate(input.X), rangeY.Translate(input.Y));

        public override VectorInt2 Inverse(VectorInt2 output) => new VectorInt2(rangeX.Inverse(output.X), rangeY.Inverse(output.Y));
    }

    public class TranslationRangeVector2 : Translation<Vector2, Vector2>
    {
        public TranslationRangeVector2(TranslationRangeFloat rangeX, TranslationRangeFloat rangeY)
        {
            this.RangeX = rangeX;
            this.RangeY = rangeY;
        }

        public TranslationRangeFloat RangeX { get; }
        public TranslationRangeFloat RangeY { get; }

        public override Vector2 Translate(Vector2 input) => new Vector2(RangeX.Translate(input.X), RangeY.Translate(input.Y));
        public override Vector2 Inverse(Vector2 output) => new Vector2(RangeX.Inverse(output.X), RangeY.Inverse(output.Y));
    }

}
