using System;

namespace SokoSolve.Core.Primitives
{
    public class RandomFloat
    {
        private readonly Random random = new Random();

        public float Next(float from, float too)
        {
            return random.Next((int) from, (int) too) + (float) random.NextDouble();
        }
    }

}
