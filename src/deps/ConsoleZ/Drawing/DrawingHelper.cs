using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleZ.Win32;
using VectorInt;
using VectorInt.Collections;

namespace ConsoleZ.Drawing
{
    public static class DrawingHelper
    {
        public static void DrawByteMatrix<TPixel>(this IRenderer<TPixel> render, VectorInt2 pos, Func<byte, TPixel> getPixel)
        {
            for (int x = 0; x < 16; x++)
            {
                for(int y=0; y<16; y++)
                {
                    render[pos.X +x , pos.Y + y] = getPixel((byte)(x + (16*y)));
                }
            }
        }
        
        public static void DrawMap<TPixel, TCell>(this IRenderer<TPixel> renderer, 
            IReadOnlyCartesianMap<TCell> map, 
            VectorInt2 pos,
            Func<TCell, TPixel> toPixel)
        {
            foreach (var (cellPos, cell) in map.ForEach())
            {
                renderer[pos + cellPos] = toPixel(cell);
            }   
        }
        
        public static void DrawMapWithPosition<TPixel, TCell>(this IRenderer<TPixel> renderer, 
            IReadOnlyCartesianMap<TCell>                                 map, 
            VectorInt2                                                   pos,
            Func<VectorInt2, TCell, TPixel>                                          toPixel)
        {
            foreach (var (cellPos, cell) in map.ForEach())
            {
                renderer[pos + cellPos] = toPixel(cellPos, cell);
            }   
        }
        
        public static CHAR_INFO[] AsciiBox = new[]
        {
            
            new CHAR_INFO(0xda, CHAR_INFO_Attr.FOREGROUND_GRAY),        // TL  0
            new CHAR_INFO(0xc4, CHAR_INFO_Attr.FOREGROUND_GRAY),        // TM  1
            new CHAR_INFO(0xbf, CHAR_INFO_Attr.FOREGROUND_GRAY),        // TR  2
            
            
            new CHAR_INFO(0xb3, CHAR_INFO_Attr.FOREGROUND_GRAY),        // ML  3
            new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GRAY),         // C   4
            new CHAR_INFO(0xb3, CHAR_INFO_Attr.FOREGROUND_GRAY),        // MR  5


            new CHAR_INFO(0xc0, CHAR_INFO_Attr.FOREGROUND_GRAY),        // BL  6
            new CHAR_INFO(0xc4, CHAR_INFO_Attr.FOREGROUND_GRAY),        // BM  7
            new CHAR_INFO(0xd9, CHAR_INFO_Attr.FOREGROUND_GRAY),        // BR  8


        };

       

    }
}
