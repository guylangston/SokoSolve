namespace SokoSolve.Core.Debugger
{
    public interface IDebugEvent
    {
    }

    public interface IDebugEventPublisher
    {
        void Raise(object source, IDebugEvent dEvent, object context = null);

        void RaiseFormat(object source, IDebugEvent dEvent, string stringFormat, params object[] args);
    }
}