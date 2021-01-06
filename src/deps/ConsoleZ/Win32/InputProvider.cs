using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VectorInt;

namespace ConsoleZ.Win32
{
    public interface IInputProvider : IDisposable
    {
        bool       IsMouseEnabled { get; set; }
        VectorInt2 MousePosition  { get; set; }
        bool       IsMouseClick   { get; }

        bool IsKeyDown(ConsoleKey key);
        bool IsKeyPressed();
        bool IsKeyPressed(ConsoleKey key);
        void Step(float              elapsed);
    }

    public class InputProvider : InputProviderBase
    {
        private Task background;
        private bool isMouseEnabled;
        
        public InputProvider()
        {
            CancellationToken = new CancellationToken();
            background = Task.Run(ProcessMessagesLoop, CancellationToken);
        }
        
        public CancellationToken CancellationToken { get; }

        // TODO: Mouse Interaction https://stackoverflow.com/questions/1944481/console-app-mouse-click-x-y-coordinate-detection-comparison
        public override bool IsMouseEnabled
        {
            get => isMouseEnabled;
            set
            {
                if (value)
                {
                    ConsoleInteropHelper.EnableMouseSupport();
                }
                else
                {
                    MousePosition = new VectorInt2(-1);
                }
                isMouseEnabled = value;
            }
        }

        private void ProcessMessagesLoop()
        {
            var inputHandle =  ConsoleInteropHelper.Get_STD_INPUT_HANDLE;
            var inputRecords = Enumerable.Range(0, 10).Select(x => new INPUT_RECORD()).ToArray();

            uint numRead = 0;
            while (!CancellationToken.IsCancellationRequested)
            {
                ConsoleInterop.ReadConsoleInput(inputHandle, inputRecords, (uint)inputRecords.Length, out numRead);
                if (numRead > 0)
                {
                    foreach (var rec in inputRecords)
                    {
                        if (rec.EventType == INPUT_RECORD.MOUSE_EVENT && isMouseEnabled)
                        {
                            MousePosition = new VectorInt2(rec.MouseEvent.dwMousePosition.X, rec.MouseEvent.dwMousePosition.Y);
                            if ((rec.MouseEvent.dwButtonState & 1) > 0)
                            {
                                CaptureMouseDown(0);
                            }
                        }
                        else if (rec.EventType == INPUT_RECORD.KEY_EVENT)
                        {
                            if (rec.KeyEvent.bKeyDown)
                            {
                                CaptureKeyDown((ConsoleKey)rec.KeyEvent.wVirtualKeyCode);
                            }
                        }
                    } 
                }
            }
        }

        public override void Dispose()
        {
            background.Dispose();
        }
        
    }
}