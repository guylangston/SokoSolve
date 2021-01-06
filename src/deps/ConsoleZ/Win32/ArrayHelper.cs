namespace ConsoleZ.Win32
{
    public static class ArrayHelper
    {
        public static  void Fill<T>(T[] array, T val)
        {
            // TODO: Use Array.Fill if avail
            for (var i = 0; i < array.Length; i++) array[i] = val;
        }
    }
}