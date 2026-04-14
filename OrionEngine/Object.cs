using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrionEngine
{
    public class Object() 
    {
    
    }
    //Vector2 class holdes 2 integers 

    //All objects in the game world must be GameObjects
    public class GameObject
    {
        //global dictionary for all GameObjects
        private static Dictionary<int, GameObject> globalGameObjectList = new Dictionary<int, GameObject>();
        private Transform _transform;
        ///public int globalID { get; }
        public Transform transform { get { return _transform; } }
        public GameObject gameObject { get { return this; } }
        public string name = "GameObject";
        public GameObject(Transform? parent = null)
        {
            //globalID = GlobalIDGenerator.NewID();
            _transform = new Transform(this, parent);
            //globalGameObjectList.Add(globalID, this);
        }
        //public static GameObject GetObjectByGlobalID(int globalID)
        //{
        //    return globalGameObjectList.TryGetValue(globalID, out GameObject? value) ? value : throw new ArgumentNullException("Object does not currently exist");
        //}
    }
    public class Transform
    {
        private Transform? _parent = null;
        private GameObject _gameObject;
        public Transform? parent { get { return _parent; } set { _parent = value; } }
        public GameObject gameObject { get { return _gameObject; } }

        public Vector2 position;
        public int floor = 0;
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
        public static bool operator ==(Vector2 a, Vector2 b) { return (a.x == b.x) && (a.y == b.y); }
        public static bool operator !=(Vector2 a, Vector2 b) { return !(a == b); }
        public static Vector2 operator +(Vector2 a) => a;
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x + -(b.x), a.y + -(b.y));
        public static Vector2 operator *(Vector2 a, int b) => new Vector2(a.x * b, a.y * b);
        public static Vector2 operator *(Vector2 a, Vector2 b) => throw new ArithmeticException("Invalid use of multiplications with Vector2 types.");
        public static Vector2 operator /(Vector2 a, int b) { if (b == 0) throw new ArithmeticException("Cannot divide by 0"); return new Vector2(a.x / b, a.y / b); }
        public static Vector2 operator /(Vector2 a, Vector2 b) => throw new ArithmeticException("Invalid use of division with Vector2 types.");
        public override readonly bool Equals(object? obj)
        {
            if (obj is null or not Vector2)
                return false;

            return (x == ((Vector2)obj).x) && (y == ((Vector2)obj).y);
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public override readonly string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
