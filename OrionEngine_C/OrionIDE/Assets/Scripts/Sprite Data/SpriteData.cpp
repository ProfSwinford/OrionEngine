#include "SpriteData.h"

const PixelGrid& SpriteData::Character()
{
    // https://theasciicode.com.ar/
    static const PixelGrid character = {
        {Pixel(' '), Pixel('8'), Pixel(' ')},
        {Pixel('~'), Pixel('|'), Pixel('~')},
        {Pixel('/'), Pixel(' '), Pixel('\\')},
    };

    return character;
}
