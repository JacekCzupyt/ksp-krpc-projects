using MathNet.Spatial.Euclidean;

namespace KspUtils.MathNet;

public static class Util {
    public const double RadToAng = 180 / Math.PI;
    public const double AngToRad = Math.PI / 180;

    public static double ClampAngle(double angle) {
        return (angle + 180) % 360 - 180;
    }

    public static Quaternion GenerateRotationQuaternion(Vector3D a, Vector3D b, double angle = 0) {
        if (angle != 0)
            throw new NotImplementedException();
        
        var au = a.Normalize();
        var bu = b.Normalize();

        if (Math.Abs(au * bu - (-1)) < 1e-10d) {
            throw new ArithmeticException("a and b are opposite vectors");
        }

        var mean = (au + bu).Normalize();

        var cross = au.CrossProduct(bu);

        var vec = (Math.Cos(angle) * mean + Math.Sin(angle) * cross).Normalize();

        return new Quaternion(0, vec.X, vec.Y, vec.Z);
    }
}
