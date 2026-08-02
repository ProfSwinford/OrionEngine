// Converted from OrionEngine/Object.cs.
#pragma once

#include <string>
#include <variant>
#include <vector>

#include "OrionEngine/Console.h"
#include "OrionEngine/Pixel.h"
#include "OrionEngine/Vector2.h"

namespace OrionEngine
{
    class GameObject;
    class OrionBehaviour;
    class Transform;

    /// <summary>
    /// Tagged value type standing in for the C# `OrionEngine.Object`, which
    /// inherited from System.Object and boxed an arbitrary payload. C++ has no
    /// universal base class, so the payload is held in a std::variant instead.
    /// </summary>
    class Object
    {
    public:
        enum class TypeTag
        {
            Null,
            Int,
            Float,
            Double,
            Bool,
            String,
            Vector2,
            GameObject,
            Other
        };

        Object();

        // Implicit, mirroring the C# `implicit operator Object(...)` conversions.
        Object(int v);
        Object(float v);
        Object(double v);
        Object(bool v);
        Object(const std::string& v);
        Object(const char* v);
        Object(const OrionEngine::Vector2& v);
        Object(OrionEngine::GameObject* v);

        TypeTag Tag() const { return _tag; }
        bool IsNull() const { return _tag == TypeTag::Null; }

        /// <summary>Throws std::bad_variant_access equivalent (InvalidCastException) on mismatch.</summary>
        template <typename T>
        T As() const;

        template <typename T>
        bool TryAs(T& value) const;

        std::string ToString() const;
        bool Equals(const Object& other) const;
        size_t GetHashCode() const;

        // Explicit conversions OUT OF Object (will throw if types mismatch).
        explicit operator int() const { return As<int>(); }
        explicit operator float() const { return As<float>(); }
        explicit operator double() const { return As<double>(); }
        explicit operator bool() const { return As<bool>(); }
        explicit operator std::string() const { return As<std::string>(); }
        explicit operator OrionEngine::Vector2() const { return As<OrionEngine::Vector2>(); }

    private:
        using Storage = std::variant<std::monostate, int, float, double, bool, std::string,
                                     OrionEngine::Vector2, OrionEngine::GameObject*>;

        Storage _value;
        TypeTag _tag;
    };

    bool operator==(const Object& a, const Object& b);
    bool operator!=(const Object& a, const Object& b);

    class Transform
    {
    public:
        explicit Transform(GameObject* gameObject, Transform* parent = nullptr);
        Transform(GameObject* gameObject, const Vector2& position, Transform* parent = nullptr);

        Transform* parent() const { return _parent; }
        void parent(Transform* value) { _parent = value; }
        GameObject* gameObject() const { return _gameObject; }

        Vector2 position;
        Vector2 rotation;
        Vector2 scale;

    private:
        Transform* _parent = nullptr;
        GameObject* _gameObject = nullptr;
    };

    /// <summary>
    /// All objects in the game world must be GameObjects.
    /// </summary>
    /// <remarks>
    /// C# GameObjects are reference types collected by the GC. The C++ port keeps
    /// the same usage pattern - allocate with `new` and hold a raw pointer - and
    /// every instance registers itself with a static registry that frees the whole
    /// set in EngineCore::Shutdown. GameObjects must therefore be heap allocated.
    /// </remarks>
    class GameObject
    {
    public:
        explicit GameObject(Transform* parent = nullptr);
        virtual ~GameObject();

        GameObject(const GameObject&) = delete;
        GameObject& operator=(const GameObject&) = delete;

        Transform* transform() { return &_transform; }
        const Transform* transform() const { return &_transform; }
        GameObject* gameObject() { return this; }

        std::string name = "GameObject";
        std::vector<OrionBehaviour*> components;

        /// <summary>Destroys every registered GameObject. Called during engine shutdown.</summary>
        static void DestroyAllGameObjects();

    private:
        Transform _transform;
    };

    class GameObject2D : public GameObject
    {
    public:
        GameObject2D(int width = 1, int height = 1, const PixelGrid& image = PixelGrid{});
        explicit GameObject2D(Transform* parent);

        void DrawImage();

        int width = 0;
        int height = 0;

    private:
        PixelGrid image;
    };

    struct Image2D
    {
        int width = 0;
        int height = 0;
        /// <summary>Indexed as pixels[x][y], matching the C# `Pixel[width, height]` usage.</summary>
        PixelGrid pixels;

        Image2D() = default;
        Image2D(int width, int height, int frames = 1, const PixelGrid& pixels = PixelGrid{});
    };

    // --- Object template definitions ---

    template <typename T>
    T Object::As() const
    {
        if (const T* stored = std::get_if<T>(&_value))
        {
            return *stored;
        }

        if (_tag == TypeTag::Null)
        {
            throw std::runtime_error("Cannot cast null to the requested type.");
        }

        throw std::runtime_error("Stored value cannot be cast to the requested type.");
    }

    template <typename T>
    bool Object::TryAs(T& value) const
    {
        if (const T* stored = std::get_if<T>(&_value))
        {
            value = *stored;
            return true;
        }

        value = T{};
        return false;
    }
}
