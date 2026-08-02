#include "OrionEngine/Object.h"

#include <algorithm>
#include <functional>

#include "OrionEngine/EngineModules/Rendering/ConsoleRendererModule.h"

namespace OrionEngine
{
    namespace
    {
        /// <summary>
        /// Stands in for the CLR heap: every GameObject registers here so the engine
        /// can release them all at shutdown without the user tracking lifetimes.
        /// </summary>
        std::vector<GameObject*>& GameObjectRegistry()
        {
            static std::vector<GameObject*> registry;
            return registry;
        }

        bool g_destroyingAllGameObjects = false;
    }

    Object::Object()
        : _value(std::monostate{})
        , _tag(TypeTag::Null)
    {
    }

    Object::Object(int v)
        : _value(v)
        , _tag(TypeTag::Int)
    {
    }

    Object::Object(float v)
        : _value(v)
        , _tag(TypeTag::Float)
    {
    }

    Object::Object(double v)
        : _value(v)
        , _tag(TypeTag::Double)
    {
    }

    Object::Object(bool v)
        : _value(v)
        , _tag(TypeTag::Bool)
    {
    }

    Object::Object(const std::string& v)
        : _value(v)
        , _tag(TypeTag::String)
    {
    }

    Object::Object(const char* v)
        : _value(std::string(v != nullptr ? v : ""))
        , _tag(TypeTag::String)
    {
    }

    Object::Object(const Vector2& v)
        : _value(v)
        , _tag(TypeTag::Vector2)
    {
    }

    Object::Object(GameObject* v)
        : _value(v)
        , _tag(v != nullptr ? TypeTag::GameObject : TypeTag::Null)
    {
    }

    std::string Object::ToString() const
    {
        switch (_tag)
        {
            case TypeTag::Int:
                return std::to_string(std::get<int>(_value));
            case TypeTag::Float:
            {
                // C# float.ToString() trims trailing zeros; mirror that shape.
                std::string text = std::to_string(std::get<float>(_value));
                text.erase(text.find_last_not_of('0') + 1, std::string::npos);
                if (!text.empty() && text.back() == '.')
                {
                    text.pop_back();
                }
                return text;
            }
            case TypeTag::Double:
            {
                std::string text = std::to_string(std::get<double>(_value));
                text.erase(text.find_last_not_of('0') + 1, std::string::npos);
                if (!text.empty() && text.back() == '.')
                {
                    text.pop_back();
                }
                return text;
            }
            case TypeTag::Bool:
                return std::get<bool>(_value) ? "True" : "False";
            case TypeTag::String:
                return std::get<std::string>(_value);
            case TypeTag::Vector2:
                return std::get<Vector2>(_value).ToString();
            case TypeTag::GameObject:
            {
                GameObject* const gameObject = std::get<GameObject*>(_value);
                return gameObject != nullptr ? gameObject->name : "null";
            }
            case TypeTag::Null:
            case TypeTag::Other:
            default:
                return "null";
        }
    }

    bool Object::Equals(const Object& other) const
    {
        return _value == other._value;
    }

    size_t Object::GetHashCode() const
    {
        switch (_tag)
        {
            case TypeTag::Int:
                return std::hash<int>{}(std::get<int>(_value));
            case TypeTag::Float:
                return std::hash<float>{}(std::get<float>(_value));
            case TypeTag::Double:
                return std::hash<double>{}(std::get<double>(_value));
            case TypeTag::Bool:
                return std::hash<bool>{}(std::get<bool>(_value));
            case TypeTag::String:
                return std::hash<std::string>{}(std::get<std::string>(_value));
            case TypeTag::Vector2:
            {
                const Vector2& vector = std::get<Vector2>(_value);
                return std::hash<int>{}(vector.x) ^ (std::hash<int>{}(vector.y) << 1);
            }
            case TypeTag::GameObject:
                return std::hash<const void*>{}(std::get<GameObject*>(_value));
            case TypeTag::Null:
            case TypeTag::Other:
            default:
                return 0;
        }
    }

    bool operator==(const Object& a, const Object& b)
    {
        return a.Equals(b);
    }

    bool operator!=(const Object& a, const Object& b)
    {
        return !a.Equals(b);
    }

    Transform::Transform(GameObject* gameObject, Transform* parent)
        : position(Vector2::zero)
        , scale(Vector2(1, 1))
        , _parent(parent)
        , _gameObject(gameObject)
    {
    }

    Transform::Transform(GameObject* gameObject, const Vector2& position, Transform* parent)
        : position(position)
        , scale(Vector2(1, 1))
        , _parent(parent)
        , _gameObject(gameObject)
    {
    }

    GameObject::GameObject(Transform* parent)
        : _transform(this, parent)
    {
        GameObjectRegistry().push_back(this);
    }

    GameObject::~GameObject()
    {
        if (g_destroyingAllGameObjects)
        {
            return;
        }

        std::vector<GameObject*>& registry = GameObjectRegistry();
        registry.erase(std::remove(registry.begin(), registry.end(), this), registry.end());
    }

    void GameObject::DestroyAllGameObjects()
    {
        g_destroyingAllGameObjects = true;

        // Drop the renderer's references first so nothing dangles mid-teardown.
        EngineModules::Rendering::ConsoleRendererModule::canvas().ClearGameObjectList();

        std::vector<GameObject*> registry;
        registry.swap(GameObjectRegistry());
        for (GameObject* gameObject : registry)
        {
            delete gameObject;
        }

        g_destroyingAllGameObjects = false;
    }

    GameObject2D::GameObject2D(int width, int height, const PixelGrid& image)
        : width(width)
        , height(height)
        , image(image)
    {
        EngineModules::Rendering::ConsoleRendererModule::canvas().AddToGameObjectList(this);
    }

    GameObject2D::GameObject2D(Transform* parent)
        : GameObject(parent)
    {
    }

    void GameObject2D::DrawImage()
    {
        if (image.empty())
        {
            return;
        }

        EngineModules::Rendering::ConsoleCanvas& canvas =
            EngineModules::Rendering::ConsoleRendererModule::canvas();

        for (int y = 0; y < height; y++)
        {
            if (static_cast<size_t>(y) >= image.size())
            {
                break;
            }

            const std::vector<Pixel>& row = image[static_cast<size_t>(y)];
            for (int x = 0; x < width; x++)
            {
                if (static_cast<size_t>(x) >= row.size())
                {
                    break;
                }

                canvas.Set(transform()->position.x + x, transform()->position.y + y, row[static_cast<size_t>(x)]);
            }
        }
    }

    Image2D::Image2D(int width, int height, int /*frames*/, const PixelGrid& pixels)
        : width(width)
        , height(height)
        , pixels(static_cast<size_t>(std::max(width, 0)),
                 std::vector<Pixel>(static_cast<size_t>(std::max(height, 0)), Pixel()))
    {
        if (pixels.empty())
        {
            return;
        }

        for (int x = 0; x < width && static_cast<size_t>(x) < pixels.size(); x++)
        {
            for (int y = 0; y < height && static_cast<size_t>(y) < pixels[static_cast<size_t>(x)].size(); y++)
            {
                this->pixels[static_cast<size_t>(x)][static_cast<size_t>(y)] =
                    pixels[static_cast<size_t>(x)][static_cast<size_t>(y)];
            }
        }
    }
}
