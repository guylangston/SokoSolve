using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ConsoleZ
{
    public class VirtualConsole : ConsoleBase
    {
        public VirtualConsole(string handle, int width, int height) : base(handle, width, height)
        {
            
        }

        
        public override void LineChanged(int i, int index, string line, bool updated)
        {
            // Nothing
        }

        public IReadOnlyList<string> GetTextLines() => base.lines;
    }

    public interface IVirtualConsoleRepository
    {
        bool TryGetConsole(string handle, out IConsoleWithProps cons);
        void AddConsole(IConsoleWithProps cons);

        IConsoleWithProps CreateConsole(string handle = null /* auto assign */);
    }

    /// <summary>
    /// Do not use in production.
    /// </summary>
    /// <typeparam name="T">Handle Type</typeparam>
    public sealed class StaticVirtualConsoleRepository : IVirtualConsoleRepository
    {
        private readonly ConcurrentDictionary<string, IConsoleWithProps> consoleList;

        private StaticVirtualConsoleRepository()
        {
            consoleList = new ConcurrentDictionary<string, IConsoleWithProps>();
        }

        private static readonly object locker = new object();
        private static volatile StaticVirtualConsoleRepository instance = null;
        public static StaticVirtualConsoleRepository Singleton
        {
            get
            {
                if (instance != null) return instance;
                lock (locker)
                {
                    if (instance != null) return instance;
                    return instance = new StaticVirtualConsoleRepository();
                }
            }
        }
        

        public bool TryGetConsole(string handle, out IConsoleWithProps cons) => consoleList.TryGetValue(handle, out cons);

        public void AddConsole(IConsoleWithProps cons)
        {
            consoleList[cons.Handle] = cons;
        }

        public  IConsoleWithProps CreateConsole(string handle = null)
        {
            return new VirtualConsole(DateTime.Now.Ticks.ToString(), 300, 100);
        }
    }
}