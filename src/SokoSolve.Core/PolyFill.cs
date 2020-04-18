using System;

namespace SokoSolve.Core
{
    public static class PolyFill
    {
        #if NET47
        
        public static void Fill<T>(this T[] arr, T val)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = val;
            }
        }
        #else
        
        public static void Fill<T>(this T[] arr, T val)
        {
            Array.Fill(arr, val);
        }
         
#endif
        
        
        
    }
}