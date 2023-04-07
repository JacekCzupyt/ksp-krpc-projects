using System.Reflection.Metadata.Ecma335;
using Sharp3D.Math.Core;

namespace KspUtils {
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

        public static Vector2D Normalized(this Vector2D vec) {
            return vec / vec.GetLength();
        }
        
        public static Vector3D Normalized(this Vector3D vec) {
            return vec / vec.GetLength();
        }

        public static Vector3D Rotate(this QuaternionD q, Vector3D v) {
            var qi = q.Clone();
            qi.Inverse();

            var qv = new QuaternionD(0, v.X, v.Y, v.Z);

            var res = q * qv * qi;
            return new Vector3D(res.X, res.Y, res.Z);
        }
    }
}
