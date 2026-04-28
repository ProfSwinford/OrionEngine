using OrionEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrionEngine;

public static class SpriteData
{
    //https://theasciicode.com.ar/
    public static Pixel[,] Character = new Pixel[,]
    {
        {   new Pixel(' '), new Pixel('8'), new Pixel(' ') },
        {   new Pixel('~'), new Pixel('|'), new Pixel('~') },
        {   new Pixel('/'), new Pixel(' '),  new Pixel('\\')},
    };
}

