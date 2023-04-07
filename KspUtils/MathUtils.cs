namespace KspUtils; 

public static class MathUtils {
    public static double ErrorFunc(double x, double lambda = 1) {
        return (Math.Exp(x*lambda) - 1) / (Math.Exp(x*lambda) + 1);
    }

    public static double SmoothInverseRange(double x, double a = -1, double b = 1, double lambda = 5) {
        return 1 - ErrorFunc(x - a, lambda) * (1 - ErrorFunc(x - b, lambda));
    }
}
