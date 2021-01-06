// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ.Win32
{
    
    
    
    // https://pinvoke.net/default.aspx/kernel32/ConsoleFunctions.html
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short X;
        public short Y;
    }

    public struct SMALL_RECT
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    public struct CONSOLE_SCREEN_BUFFER_INFO
    {
        public COORD dwSize;
        public COORD dwCursorPosition;
        public short wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SCREEN_BUFFER_INFO_EX
    {
        public uint cbSize;
        public COORD dwSize;
        public COORD dwCursorPosition;
        public short wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;

        public ushort wPopupAttributes;
        public bool bFullscreenSupported;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public COLORREF[] ColorTable;

        public static CONSOLE_SCREEN_BUFFER_INFO_EX Create()
        {
            return new CONSOLE_SCREEN_BUFFER_INFO_EX {cbSize = 96};
        }
    }

    //[StructLayout(LayoutKind.Sequential)]
    //struct COLORREF
    //{
    //    public byte R;
    //    public byte G;
    //    public byte B;
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF
    {
        public uint ColorDWORD;

        public COLORREF(Color color)
        {
            ColorDWORD = color.R + ((uint) color.G << 8) + ((uint) color.B << 16);
        }

        public Color GetColor()
        {
            return Color.FromArgb((int) (0x000000FFU & ColorDWORD),
                (int) (0x0000FF00U & ColorDWORD) >> 8, (int) (0x00FF0000U & ColorDWORD) >> 16);
        }

        public void SetColor(Color color)
        {
            ColorDWORD = color.R + ((uint) color.G << 8) + ((uint) color.B << 16);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_FONT_INFO
    {
        public int nFont;
        public COORD dwFontSize;
    }


    // https://stackoverflow.com/questions/20631634/changing-font-in-a-console-window-in-c-sharp

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    unsafe internal struct CONSOLE_FONT_INFO_EX
    {
        internal uint cbSize;
        internal uint nFont;
        internal COORD dwFontSize;
        internal int FontFamily;
        internal int FontWeight;
        internal fixed char FaceName[LF_FACESIZE];

        public const int LF_FACESIZE = 32;
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_RECORD
    {
        [FieldOffset(0)] public ushort EventType;
        [FieldOffset(4)] public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(4)] public MOUSE_EVENT_RECORD MouseEvent;
        [FieldOffset(4)] public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
        [FieldOffset(4)] public MENU_EVENT_RECORD MenuEvent;
        [FieldOffset(4)] public FOCUS_EVENT_RECORD FocusEvent;

        public const ushort KEY_EVENT = 0x0001,
            MOUSE_EVENT = 0x0002,
            WINDOW_BUFFER_SIZE_EVENT = 0x0004; //m
        
        public const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001,
            FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004,
            FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008,
            FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010,
            RIGHTMOST_BUTTON_PRESSED = 0x0002;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KEY_EVENT_RECORD
    {
        [FieldOffset(0)] [MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;

        [FieldOffset(4)] [MarshalAs(UnmanagedType.U2)]
        public ushort wRepeatCount;

        [FieldOffset(6)] [MarshalAs(UnmanagedType.U2)]
        //public VirtualKeys wVirtualKeyCode;
        public ushort wVirtualKeyCode;

        [FieldOffset(8)] [MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualScanCode;

        [FieldOffset(10)] public char UnicodeChar;

        [FieldOffset(12)] [MarshalAs(UnmanagedType.U4)]
        //public ControlKeyState dwControlKeyState;
        public uint dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSE_EVENT_RECORD
    {
        public COORD dwMousePosition;
        public uint dwButtonState;
        public uint dwControlKeyState;
        public uint dwEventFlags;
    }

    public struct WINDOW_BUFFER_SIZE_RECORD
    {
        public COORD dwSize;

        public WINDOW_BUFFER_SIZE_RECORD(short x, short y)
        {
            dwSize = new COORD();
            dwSize.X = x;
            dwSize.Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENU_EVENT_RECORD
    {
        public uint dwCommandId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FOCUS_EVENT_RECORD
    {
        public uint bSetFocus;
    }


 

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_CURSOR_INFO
    {
        public readonly uint Size;
        public readonly bool Visible;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_HISTORY_INFO
    {
        public readonly ushort cbSize;
        public readonly ushort HistoryBufferSize;
        public readonly ushort NumberOfHistoryBuffers;
        public readonly uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SELECTION_INFO
    {
        public readonly uint Flags;
        public readonly COORD SelectionAnchor;
        public readonly SMALL_RECT Selection;

        // Flags values:
        public const uint CONSOLE_MOUSE_DOWN = 0x0008; // Mouse is down
        public const uint CONSOLE_MOUSE_SELECTION = 0x0004; //Selecting with the mouse
        public const uint CONSOLE_NO_SELECTION = 0x0000; //No selection
        public const uint CONSOLE_SELECTION_IN_PROGRESS = 0x0001; //Selection has begun
        public const uint CONSOLE_SELECTION_NOT_EMPTY = 0x0002; //Selection rectangle is not empty
    }

    // Enumerated type for the control messages sent to the handler routine
    public enum CtrlTypes : uint
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT,
        CTRL_CLOSE_EVENT,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT
    }

    /// <summary>
    ///     --- begin MSDN ---
    ///     http://msdn.microsoft.com/en-us/library/ms682073(VS.85).aspx
    ///     Console Functions
    ///     The following functions are used to access a console.
    ///     --- end MSDN ---
    /// </summary>
    internal class ConsoleInterop
    {
        // http://pinvoke.net/default.aspx/kernel32/AddConsoleAlias.html
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool AddConsoleAlias(
            string Source,
            string Target,
            string ExeName
        );

        // http://pinvoke.net/default.aspx/kernel32/AllocConsole.html
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool AllocConsole();

        // http://pinvoke.net/default.aspx/kernel32/AttachConsole.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(
            uint dwProcessId
        );

        // http://pinvoke.net/default.aspx/kernel32/CreateConsoleScreenBuffer.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateConsoleScreenBuffer(
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwFlags,
            IntPtr lpScreenBufferData
        );

        // http://pinvoke.net/default.aspx/kernel32/FillConsoleOutputAttribute.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FillConsoleOutputAttribute(
            IntPtr hConsoleOutput,
            ushort wAttribute,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfAttrsWritten
        );

        // http://pinvoke.net/default.aspx/kernel32/FillConsoleOutputCharacter.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FillConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            char cCharacter,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfCharsWritten
        );

        // http://pinvoke.net/default.aspx/kernel32/FlushConsoleInputBuffer.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FlushConsoleInputBuffer(
            IntPtr hConsoleInput
        );

        // http://pinvoke.net/default.aspx/kernel32/FreeConsole.html
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool FreeConsole();

        // http://pinvoke.net/default.aspx/kernel32/GenerateConsoleCtrlEvent.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GenerateConsoleCtrlEvent(
            uint dwCtrlEvent,
            uint dwProcessGroupId
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleAlias.html
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool GetConsoleAlias(
            string Source,
            out StringBuilder TargetBuffer,
            uint TargetBufferLength,
            string ExeName
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleAliases.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleAliases(
            StringBuilder[] lpTargetBuffer,
            uint targetBufferLength,
            string lpExeName
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleAliasesLength.html
        [DllImport("kernel32", SetLastError = true)]
        public static extern uint GetConsoleAliasesLength(
            string ExeName
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleAliasExes.html
        [DllImport("kernel32", SetLastError = true)]
        public static extern uint GetConsoleAliasExes(
            out StringBuilder ExeNameBuffer,
            uint ExeNameBufferLength
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleAliasExesLength.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleAliasExesLength();

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleCP.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleCP();

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleCursorInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleCursorInfo(
            IntPtr hConsoleOutput,
            out CONSOLE_CURSOR_INFO lpConsoleCursorInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleDisplayMode.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleDisplayMode(
            out uint ModeFlags
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleFontSize.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern COORD GetConsoleFontSize(
            IntPtr hConsoleOutput,
            int nFont
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleHistoryInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleHistoryInfo(
            out CONSOLE_HISTORY_INFO ConsoleHistoryInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleMode.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out uint lpMode
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleOriginalTitle.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleOriginalTitle(
            out StringBuilder ConsoleTitle,
            uint Size
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleOutputCP.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleOutputCP();

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleProcessList.html
        // TODO: Test - what's an out uint[] during interop? This probably isn't quite right, but provides a starting point:
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleProcessList(
            out uint[] ProcessList,
            uint ProcessCount
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleScreenBufferInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfo(
            IntPtr hConsoleOutput,
            out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleScreenBufferInfoEx.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfoEx(
            IntPtr hConsoleOutput,
            ref CONSOLE_SCREEN_BUFFER_INFO_EX ConsoleScreenBufferInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleSelectionInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleSelectionInfo(
            CONSOLE_SELECTION_INFO ConsoleSelectionInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleTitle.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetConsoleTitle(
            [Out] StringBuilder lpConsoleTitle,
            uint nSize
        );

        // http://pinvoke.net/default.aspx/kernel32/GetConsoleWindow.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        // http://pinvoke.net/default.aspx/kernel32/GetCurrentConsoleFont.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetCurrentConsoleFont(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            out CONSOLE_FONT_INFO lpConsoleCurrentFont
        );

        // http://pinvoke.net/default.aspx/kernel32/GetCurrentConsoleFontEx.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetCurrentConsoleFontEx(
            IntPtr ConsoleOutput,
            bool MaximumWindow,
            out CONSOLE_FONT_INFO_EX ConsoleCurrentFont
        );

        // http://pinvoke.net/default.aspx/kernel32/GetLargestConsoleWindowSize.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern COORD GetLargestConsoleWindowSize(
            IntPtr hConsoleOutput
        );

        // http://pinvoke.net/default.aspx/kernel32/GetNumberOfConsoleInputEvents.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNumberOfConsoleInputEvents(
            IntPtr hConsoleInput,
            out uint lpcNumberOfEvents
        );

        // http://pinvoke.net/default.aspx/kernel32/GetNumberOfConsoleMouseButtons.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetNumberOfConsoleMouseButtons(
            ref uint lpNumberOfMouseButtons
        );

        // http://pinvoke.net/default.aspx/kernel32/GetStdHandle.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(
            int nStdHandle
        );

    // http://pinvoke.net/default.aspx/kernel32/PeekConsoleInput.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool PeekConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
        );

        // http://pinvoke.net/default.aspx/kernel32/ReadConsole.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsole(
            IntPtr hConsoleInput,
            [Out] StringBuilder lpBuffer,
            uint nNumberOfCharsToRead,
            out uint lpNumberOfCharsRead,
            IntPtr lpReserved
        );

        // http://pinvoke.net/default.aspx/kernel32/ReadConsoleInput.html
        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode)]
        public static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
        );

        // http://pinvoke.net/default.aspx/kernel32/ReadConsoleOutput.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            [Out] CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpReadRegion
        );

        // http://pinvoke.net/default.aspx/kernel32/ReadConsoleOutputAttribute.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleOutputAttribute(
            IntPtr hConsoleOutput,
            [Out] ushort[] lpAttribute,
            uint nLength,
            COORD dwReadCoord,
            out uint lpNumberOfAttrsRead
        );

        // http://pinvoke.net/default.aspx/kernel32/ReadConsoleOutputCharacter.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            [Out] StringBuilder lpCharacter,
            uint nLength,
            COORD dwReadCoord,
            out uint lpNumberOfCharsRead
        );

        // http://pinvoke.net/default.aspx/kernel32/ScrollConsoleScreenBuffer.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ScrollConsoleScreenBuffer(
            IntPtr hConsoleOutput,
            [In] ref SMALL_RECT lpScrollRectangle,
            IntPtr lpClipRectangle,
            COORD dwDestinationOrigin,
            [In] ref CHAR_INFO lpFill
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleActiveScreenBuffer.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleActiveScreenBuffer(
            IntPtr hConsoleOutput
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleCP.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCP(
            uint wCodePageID
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleCtrlHandler.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCtrlHandler(
            ConsoleCtrlDelegate HandlerRoutine,
            bool Add
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleCursorInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorInfo(
            IntPtr hConsoleOutput,
            [In] ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleCursorPosition.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorPosition(
            IntPtr hConsoleOutput,
            COORD dwCursorPosition
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleDisplayMode.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleDisplayMode(
            IntPtr ConsoleOutput,
            uint Flags,
            out COORD NewScreenBufferDimensions
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleHistoryInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleHistoryInfo(
            CONSOLE_HISTORY_INFO ConsoleHistoryInfo
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleMode.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            uint dwMode
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleOutputCP.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleOutputCP(
            uint wCodePageID
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleScreenBufferInfoEx.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferInfoEx(
            IntPtr ConsoleOutput,
            CONSOLE_SCREEN_BUFFER_INFO_EX ConsoleScreenBufferInfoEx
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleScreenBufferSize.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(
            IntPtr hConsoleOutput,
            COORD dwSize
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleTextAttribute.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleTextAttribute(
            IntPtr hConsoleOutput,
            ushort wAttributes
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleTitle.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleTitle(
            string lpConsoleTitle
        );

        // http://pinvoke.net/default.aspx/kernel32/SetConsoleWindowInfo.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleWindowInfo(
            IntPtr hConsoleOutput,
            bool bAbsolute,
            [In] ref SMALL_RECT lpConsoleWindow
        );

        // http://pinvoke.net/default.aspx/kernel32/SetCurrentConsoleFontEx.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetCurrentConsoleFontEx(
            IntPtr ConsoleOutput,
            bool MaximumWindow,

            [In] ref CONSOLE_FONT_INFO_EX ConsoleCurrentFontEx
        );

        // http://pinvoke.net/default.aspx/kernel32/SetStdHandle.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetStdHandle(
            uint nStdHandle,
            IntPtr hHandle
        );

        // http://pinvoke.net/default.aspx/kernel32/WriteConsole.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsole(
            IntPtr hConsoleOutput,
            string lpBuffer,
            uint nNumberOfCharsToWrite,
            out uint lpNumberOfCharsWritten,
            IntPtr lpReserved
        );

        // http://pinvoke.net/default.aspx/kernel32/WriteConsoleInput.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleInput(
            IntPtr hConsoleInput,
            INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsWritten
        );

        // http://pinvoke.net/default.aspx/kernel32/WriteConsoleOutput.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpWriteRegion
        );

        // http://pinvoke.net/default.aspx/kernel32/WriteConsoleOutputAttribute.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutputAttribute(
            IntPtr hConsoleOutput,
            ushort[] lpAttribute,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfAttrsWritten
        );

        // http://pinvoke.net/default.aspx/kernel32/WriteConsoleOutputCharacter.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            string lpCharacter,
            uint nLength,
            COORD dwWriteCoord,
            out uint lpNumberOfCharsWritten
        );

        // http://pinvoke.net/default.aspx/kernel32/HandlerRoutine.html
        // Delegate type to be used as the Handler Routine for SCCH
        public delegate bool ConsoleCtrlDelegate(CtrlTypes CtrlType);
    }
}