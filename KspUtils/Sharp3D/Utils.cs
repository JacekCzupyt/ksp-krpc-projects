using Sharp3D.Math.Core;

namespace KspUtils.Sharp3D;

public static class Util {
    public const double RadToAng = 180 / Math.PI;
    public const double AngToRad = Math.PI / 180;

    public static double ClampAngle(double angle) {
        return (angle + 180) % 360 - 180;
    }

    public static QuaternionD GenerateRotationQuaternion(Vector3D a, Vector3D b, double angle = 0) {
        if (angle != 0)
            throw new NotImplementedException();
        
        a.Normalize();
        b.Normalize();

        if (Math.Abs(Vector3D.DotProduct(a, b) - (-1)) < 1e-10d) {
            throw new ArithmeticException("a and b are opposite vectors");
        }

        var mean = (a + b).Normalized();

        var cross = Vector3D.CrossProduct(a, b).Normalized();

        var vec = mean * Math.Cos(angle) + cross * Math.Sin(angle);
        vec.Normalize();

        return new QuaternionD(0, vec.X, vec.Y, vec.Z);
    }
}
