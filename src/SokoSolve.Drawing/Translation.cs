using System;

namespace SokoSolve.Drawing
{
    public interface ITranslation
    {
        object Translate(object input);
        object Inverse(object   output);
    }
    
    public interface ITranslation<TInput, TOutput> 
    {
        TOutput Translate(TInput input);
        TInput  Inverse(TOutput  output);
    }
    
    
    public abstract class Translation<TInput, TOutput> : ITranslation<TInput, TOutput>, ITranslation
    {
        public abstract TOutput Translate(TInput input);
        public abstract TInput  Inverse(TOutput  output);
        
        object ITranslation.Translate(object input)  => Translate((TInput) input);
        object ITranslation.Inverse(object   output) => Inverse((TOutput) output);
    }

    public class TranslationFunc<TInput, TOutput> : Translation<TInput, TOutput>
    {
        private Func<TInput, TOutput> inOut;
        private Func<TOutput, TInput> outIn;

        public TranslationFunc(Func<TInput, TOutput> inOut, Func<TOutput, TInput> outIn)
        {
            this.inOut = inOut;
            this.outIn = outIn;
        }

        public override TOutput Translate(TInput input)  => inOut(input);
        public override TInput  Inverse(TOutput  output) => outIn(output);
    }
}