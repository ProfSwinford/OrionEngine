#include "OrionEngine/Vector2.h"

#include <cmath>
#include <stdexcept>

namespace OrionEngine
{
    const Vector2 Vector2::zero{0, 0};
    const Vector2 Vector2::up{0, -1};
    const Vector2 Vector2::right{1, 0};

    int Vector2::Distance(const Vector2& other) const
    {
        return Distance(*this, other);
    }

    int Vector2::Distance(const Vector2& v1, const Vector2& v2)
    {
        return static_cast<int>(std::sqrt(std::pow(v2.x - v1.x, 2) + std::pow(v2.y - v1.y, 2)));
    }

    bool Vector2::Equals(const Vector2& other) const
    {
        return (x == other.x) && (y == other.y);
    }

    std::string Vector2::ToString() const
    {
        return "(" + std::to_string(x) + ", " + std::to_string(y) + ")";
    }

    bool operator==(const Vector2& a, const Vector2& b)
    {
        return a.Equals(b);
    }

    bool operator!=(const Vector2& a, const Vector2& b)
    {
        return !a.Equals(b);
    }

    Vector2 operator+(const Vector2& a)
    {
        return Vector2(std::abs(a.x), std::abs(a.y));
    }

    Vector2 operator+(const Vector2& a, const Vector2& b)
    {
        return Vector2(a.x + b.x, a.y + b.y);
    }

    Vector2 operator-(const Vector2& a)
    {
        return Vector2(-a.x, -a.y);
    }

    Vector2 operator-(const Vector2& a, const Vector2& b)
    {
        return Vector2(a.x + -(b.x), a.y + -(b.y));
    }

    Vector2 operator*(const Vector2& a, int b)
    {
        return Vector2(a.x * b, a.y * b);
    }

    Vector2 operator*(const Vector2&, const Vector2&)
    {
        throw std::runtime_error("Invalid use of multiplications with Vector2 types.");
    }

    Vector2 operator/(const Vector2& a, int b)
    {
        if (b == 0)
        {
            throw std::runtime_error("Cannot divide by 0");
        }
        return Vector2(a.x / b, a.y / b);
    }

    Vector2 operator/(const Vector2&, const Vector2&)
    {
        throw std::runtime_error("Invalid use of division with Vector2 types.");
    }

    Vector2& operator+=(Vector2& a, const Vector2& b)
    {
        a = a + b;
        return a;
    }

    Vector2& operator-=(Vector2& a, const Vector2& b)
    {
        a = a - b;
        return a;
    }

    Vector2& operator*=(Vector2& a, int b)
    {
        a = a * b;
        return a;
    }

    Vector2& operator/=(Vector2& a, int b)
    {
        a = a / b;
        return a;
    }
}
