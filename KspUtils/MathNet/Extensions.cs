using MathNet;
using MathNet.Spatial.Euclidean;

// ReSharper disable once CheckNamespace
namespace KspUtils.MathNet {
    public static class MyExtensions {

        public static Vector3D ToVec3(this Vector2D vec) {
            return new Vector3D(vec.X, vec.Y, 0);
        }

        public static Vector2D ToVec2(this Vector3D vec) {
            return new Vector2D(vec.X, vec.Y);
        }
        
        public static Vector3D ToVec(this Tuple<double, double, double> tup) {
            return new Vector3D(tup.Item1, tup.Item2, tup.Item3);
        }
        
        public static Vector2D ToVec(this Tuple<double, double> tup) {
            return new Vector2D(tup.Item1, tup.Item2);
        }

        public static Tuple<double, double, double> ToTuple(this Vector3D vec) {
            return new(vec.X, vec.Y, vec.Z);
        }
        
        public static Tuple<double, double> ToTuple(this Vector2D vec) {
            return new(vec.X, vec.Y);
        }

        public static Quaternion ToQuaternion(this Vector3D v) {
            return new Quaternion(0, v.X, v.Y, v.Z);
        }

        public static Vector3D ToVec3(this Quaternion q) {
            return new Vector3D(q.ImagX, q.ImagY, q.ImagZ);
        }

        public static Vector3D Rotate(this Quaternion q, Vector3D v) {
            return (q * v.ToQuaternion() * q.Inversed).ToVec3();
        }
    }
}
