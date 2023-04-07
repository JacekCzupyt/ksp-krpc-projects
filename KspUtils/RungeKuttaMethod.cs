using Sharp3D.Math.Core;

namespace KspUtils;

public class RungeKuttaMethod {
    public Acceleration Function { get; }
    public Vector3D Y { get; private set; }
    public Vector3D Yp { get; private set; }
    public double X { get; private set; }

    public double H { get; set; }

    public delegate Vector3D Acceleration(double x, Vector3D y, Vector3D yp);

    public RungeKuttaMethod(Acceleration function, double x0, Vector3D y0, Vector3D yp0, double h) {
        Function = function;
        Y = y0;
        Yp = yp0;
        H = h;
        X = x0;
    }

    public (double, Vector3D, Vector3D) MakeStep() {
        var m1 = H * Yp;
        var k1 = H * Function(X, Y, Yp);

        var m2 = H * (Yp + k1 / 2);
        var k2 = H * Function(X + H / 2, Y + m1 / 2, Yp + k1 / 2);

        var m3 = H * (Yp + k2 / 2);
        var k3 = H * Function(X + H / 2, Y + m2 / 2, Yp + k2 / 2);

        var m4 = H * (Yp + k3);
        var k4 = H * Function(X + H, Y + m3, Yp + k3);

        X += H;
        Y += (m1 + 2 * m2 + 2 * m3 + m4) / 6;
        Yp += (k1 + 2 * k2 + 2 * k3 + k4) / 6;

        return (X, Y, Yp);
    }
}
