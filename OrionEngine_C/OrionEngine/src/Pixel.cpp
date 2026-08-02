#include "OrionEngine/Pixel.h"

namespace OrionEngine
{
    bool Pixel::Equals(const Pixel& other) const
    {
        return other == *this;
    }

    bool operator==(const Pixel& p1, const Pixel& p2)
    {
        return p1.Character == p2.Character &&
               p1.Foreground == p2.Foreground &&
               p1.Background == p2.Background;
    }

    bool operator!=(const Pixel& p1, const Pixel& p2)
    {
        return p1.Character != p2.Character ||
               p1.Foreground != p2.Foreground ||
               p1.Background != p2.Background;
    }
}
