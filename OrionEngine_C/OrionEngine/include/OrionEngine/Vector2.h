// Converted from OrionEngine/Object.cs (Vector2 primary-constructor struct).
#pragma once

#include <string>

namespace OrionEngine
{
    /// <summary>
    /// Vector2 struct holds 2 integers.
    /// </summary>
    struct Vector2
    {
        int x = 0;
        int y = 0;

        constexpr Vector2() = default;
        constexpr Vector2(int x, int y)
            : x(x)
            , y(y)
        {
        }

        // C# exposes these as static fields on the struct. They are defined in
        // Vector2.cpp because a class cannot hold a static member of its own
        // (still incomplete) type inline.
        static const Vector2 zero;
        static const Vector2 up;    // opposite as lower numbers appear higher on the screen
        static const Vector2 right;

        int Distance(const Vector2& other) const;
        static int Distance(const Vector2& v1, const Vector2& v2);

        bool Equals(const Vector2& other) const;
        std::string ToString() const;
    };

    bool operator==(const Vector2& a, const Vector2& b);
    bool operator!=(const Vector2& a, const Vector2& b);

    /// <summary>Unary plus returns the component-wise absolute value (matches the C# operator).</summary>
    Vector2 operator+(const Vector2& a);
    Vector2 operator+(const Vector2& a, const Vector2& b);
    Vector2 operator-(const Vector2& a);
    Vector2 operator-(const Vector2& a, const Vector2& b);
    Vector2 operator*(const Vector2& a, int b);
    /// <summary>Always throws, exactly as the C# operator does.</summary>
    Vector2 operator*(const Vector2& a, const Vector2& b);
    Vector2 operator/(const Vector2& a, int b);
    /// <summary>Always throws, exactly as the C# operator does.</summary>
    Vector2 operator/(const Vector2& a, const Vector2& b);

    // C# gets `position += Vector2.up` for free from the binary operators; C++
    // needs the compound forms spelled out.
    Vector2& operator+=(Vector2& a, const Vector2& b);
    Vector2& operator-=(Vector2& a, const Vector2& b);
    Vector2& operator*=(Vector2& a, int b);
    Vector2& operator/=(Vector2& a, int b);
}
