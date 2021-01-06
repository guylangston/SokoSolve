using System;
using System.Runtime.InteropServices;

namespace ConsoleZ.Win32
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/console/char-info-str
    /// </summary>
    /// <remarks>
    /// CHAR_INFO struct, which was a union in the old days
    /// so we want to use LayoutKind.Explicit to mimic it as closely
    /// as we can</remarks>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
    public struct CHAR_INFO
    {
        public CHAR_INFO(char unicodeChar, ushort attributes) : this()
        {
            UnicodeChar = unicodeChar;
            Attributes = attributes;
        }
        
        public CHAR_INFO(char unicodeChar, CHAR_INFO_Attr attributes) : this()
        {
            UnicodeChar = unicodeChar;
            Attributes = (ushort)attributes;
        }
        
        public CHAR_INFO(ushort unicodeChar, CHAR_INFO_Attr attributes) : this()
        {
            UnicodeNum = unicodeChar;
            Attributes = (ushort)attributes;
        }
        
        public CHAR_INFO(char unicodeChar) : this()
        {
            UnicodeChar = unicodeChar;
            Attributes = (ushort)CHAR_INFO_Attr.FOREGROUND_GRAY;
        }

        [FieldOffset(0)] 
        public ushort UnicodeNum;
        
        [FieldOffset(0)] 
        public char UnicodeChar;

        [FieldOffset(0)] 
        public byte AsciiChar;


        [FieldOffset(2)] 
        public ushort Attributes;


        public CHAR_INFO_Attr AttributesEnum
        {
            get => (CHAR_INFO_Attr) Attributes;
            set => Attributes = (ushort) value;
        }
    }
    
    /// <summary>
    /// NOT Win32 class. Helper. See https://docs.microsoft.com/en-us/windows/console/char-info-str
    /// </summary>
    [Flags]
    public enum CHAR_INFO_Attr
    {

        BLACK      = 0b0000,	

        FOREGROUND_BLUE      = 0b0001,	
        FOREGROUND_GREEN     = 0b0010,	
        FOREGROUND_RED       = 0b0100,	
        FOREGROUND_INTENSITY = 0b1000,
        FOREGROUND_GRAY      = 0b0111,
        
        BACKGROUND_BLUE         =  0b1_0000,	
        BACKGROUND_GREEN        = 0b10_0000,	
        BACKGROUND_RED         = 0b100_0000	,
        BACKGROUND_INTENSITY  = 0b1000_0000,
        BACKGROUND_GRAY        = 0b111_0000	,

        
        COMMON_LVB_LEADING_BYTE = 0x0100,	
        COMMON_LVB_TRAILING_BYTE = 0x0200	,
        COMMON_LVB_GRID_HORIZONTAL = 0x0400	,
        COMMON_LVB_GRID_LVERTICAL = 0x0800,	
        COMMON_LVB_GRID_RVERTICAL = 0x1000	,
        COMMON_LVB_REVERSE_VIDEO = 0x4000,	
        COMMON_LVB_UNDERSCORE = 0x8000,
    }
}