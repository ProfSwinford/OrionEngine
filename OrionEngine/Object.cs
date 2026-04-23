using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrionEngine.EngineModules.Rendering;

namespace OrionEngine
{
    public class Object : System.Object
    {
        public enum TypeTag
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
        }

        private readonly object? _value;
        public TypeTag Tag { get; }

        public Object()
        {
            Tag = TypeTag.Null;
            _value = null;
        }

        public Object(int v) { Tag = TypeTag.Int; _value = v; }
        public Object(float v) { Tag = TypeTag.Float; _value = v; }
        public Object(double v) { Tag = TypeTag.Double; _value = v; }
        public Object(bool v) { Tag = TypeTag.Bool; _value = v; }
        public Object(string v) { Tag = TypeTag.String; _value = v ?? string.Empty; }
        public Object(Vector2 v) { Tag = TypeTag.Vector2; _value = v; }
        public Object(object? v) { Tag = v switch { null => TypeTag.Null, int _ => TypeTag.Int, float _ => TypeTag.Float, double _ => TypeTag.Double, bool _ => TypeTag.Bool, string _ => TypeTag.String, Vector2 _ => TypeTag.Vector2, GameObject _ => TypeTag.GameObject, _ => TypeTag.Other }; _value = v; }
        public Object(GameObject v) { Tag = TypeTag.GameObject; _value = v; }

        public bool IsNull => Tag == TypeTag.Null;

        public T As<T>()
        {
            if (_value is T t) return t;
            // allow unwrapping Nullable defaults for value types
            if (_value == null) throw new InvalidCastException($"Cannot cast null to {typeof(T).Name}.");
            throw new InvalidCastException($"Stored value is {_value.GetType().Name}, cannot cast to {typeof(T).Name}.");
        }

        public bool TryAs<T>(out T? value)
        {
            if (_value is T t)
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }

        public override string ToString()
        {
            return _value?.ToString() ?? "null";
        }

        public override bool Equals(object? obj)
        {
            if (obj is Object o) return Equals(_value, o._value);
            return Equals(_value, obj);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        // implicit convenience conversions INTO Object
        public static implicit operator Object(int v) => new Object(v);
        public static implicit operator Object(float v) => new Object(v);
        public static implicit operator Object(double v) => new Object(v);
        public static implicit operator Object(bool v) => new Object(v);
        public static implicit operator Object(string v) => new Object(v);
        public static implicit operator Object(Vector2 v) => new Object(v);

        // explicit conversions OUT OF Object (will throw if types mismatch)
        public static explicit operator int(Object o) => o.As<int>();
        public static explicit operator float(Object o) => o.As<float>();
        public static explicit operator double(Object o) => o.As<double>();
        public static explicit operator bool(Object o) => o.As<bool>();
        public static explicit operator string(Object o) => o.As<string>();
        public static explicit operator Vector2(Object o) => o.As<Vector2>();
    }

    //All objects in the game world must be GameObjects
    public class GameObject
    {
        //global dictionary for all GameObjects
        //private static Dictionary<int, GameObject> globalGameObjectList = new Dictionary<int, GameObject>();
        private Transform _transform;
        public Transform transform { get { return _transform; } }
        public GameObject gameObject { get { return this; } }
        public string name = "GameObject";
        public GameObject(Transform? parent = null)
        {
            _transform = new Transform(this, parent);
        }

        public List<OrionBehaviour> components = new List<OrionBehaviour>();

    }
    public class GameObject2D : GameObject
    {
        public int width, height;
        Pixel[,]? image;

        public GameObject2D(int width = 1, int height = 1, Pixel[,] image = null)
        {
            this.width = width;
            this.height = height;
            this.image = image;

            ConsoleRendererModule.canvas.AddToGameObjectList(this);
        }

        public void DrawImage()
        {
            for(int y = 0; y < height; y++)
                for(int x = 0; x < width; x++)
                    ConsoleRendererModule.canvas.Set(transform.position.x+x, transform.position.y + y, image[y,x]);
        }

        public GameObject2D(Transform? parent = null) : base(parent) { }
    }
    public struct Image2D 
    {
        public int width;
        public int height;
        public Pixel[,] pixels;
        public Image2D(int width, int height, int frames = 1, Pixel[,] pixels = null)
        {
            this.width = width;
            this.height = height;
            this.pixels = new Pixel[width, height];
            if (pixels != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        this.pixels[x,y] = pixels[x, y];
                    }
                }
            }
        }
    }

    public class Transform
    {
        private Transform? _parent = null;
        private GameObject _gameObject;
        public Transform? parent { get { return _parent; } set { _parent = value; } }
        public GameObject gameObject { get { return _gameObject; } }

        public Vector2 position;
        public Vector2 rotation; 
        public Vector2 scale;

        public Transform(GameObject gameObject, Transform? parent = null)
        {
            this._gameObject = gameObject;
            this.parent = parent;
            this.position = Vector2.zero;
            this.scale = new Vector2(1, 1);
        }

        public Transform(GameObject gameObject, Vector2 position, Transform? parent = null)
        {
            this._gameObject = gameObject;
            this.parent = parent;
            this.position = position;
            this.scale = new Vector2(1, 1);
        }

    }

    //Vector2 class holds 2 integers 
    public struct Vector2(int x, int y)
    {
        public int x = x;
        public int y = y;
        public static Vector2 zero = new(0, 0);
        public static Vector2 up = new(0, -1); //opposite as lower numbers appear higher on the screen
        public static Vector2 right = new(1, 0);

        public readonly int Distance(Vector2 other)
        {
            return Distance(this, other);
        }
        public static int Distance(Vector2 v1, Vector2 v2)
        {
            return (int)Math.Sqrt(Math.Pow(v2.x - v1.x, 2) + Math.Pow(v2.y - v1.y, 2));
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is null or not Vector2)
                return false;

            return (x == ((Vector2)obj).x) && (y == ((Vector2)obj).y);
        }
        public static bool operator ==(Vector2 a, Vector2 b) { return a.Equals(b); }
        public static bool operator !=(Vector2 a, Vector2 b) { return !a.Equals(b); }
        public static Vector2 operator +(Vector2 a) => new Vector2(Math.Abs(a.x),Math.Abs(a.y));
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x + -(b.x), a.y + -(b.y));
        public static Vector2 operator *(Vector2 a, int b) => new Vector2(a.x * b, a.y * b);
        public static Vector2 operator *(Vector2 a, Vector2 b) => throw new ArithmeticException("Invalid use of multiplications with Vector2 types.");
        public static Vector2 operator /(Vector2 a, int b) { if (b == 0) throw new ArithmeticException("Cannot divide by 0"); return new Vector2(a.x / b, a.y / b); }
        public static Vector2 operator /(Vector2 a, Vector2 b) => throw new ArithmeticException("Invalid use of division with Vector2 types.");

        public override readonly string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
