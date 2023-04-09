using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using KspUtils;
using KspUtils.MathNet;
using MathNet.Numerics;
using MathNet.Spatial.Euclidean;

namespace MissileControl;

public class MissileController {
    private const double KerbinSurfaceGravity = 9.81;
    private readonly string[] FuelTypes = { "LiquidFuel", "Oxidizer" };
    private const double FuelMassMultiplier = 5;

    public enum FireMode {
        IfInRange,
        WhenInRange,
        Force
    }

    public Vessel Missile { get; }
    public Vessel Target { get; }
    public double Throttle { get; }
    public ReferenceFrame Ref { get; }

    private Stream<Tuple<double, double, double>> targetPosStream, missilePosStream;


    public MissileController(Vessel missile, Vessel target, Connection conn, double throttle = 1) {
        Missile = missile;
        Target = target;
        Throttle = throttle;

        Ref = missile.Orbit.Body.NonRotatingReferenceFrame;

        targetPosStream = conn.AddStream(() => Target.Position(Ref));
        missilePosStream = conn.AddStream(() => Missile.Position(Ref));
        // var positionStream = conn.AddStream(() => Target.Position(referenceFrameStream.Get()));
    }

    public async void Fire(FireMode fireMode = FireMode.IfInRange) {
        if (fireMode != FireMode.IfInRange)
            throw new NotImplementedException();

        var timeToTarget = TargetMissile();
        if (!(timeToTarget < ComputeThrustTime()))
            return;

        Missile.AutoPilot.ReferenceFrame = Missile.Orbit.Body.NonRotatingReferenceFrame;
        var tuner = new VesselAutoPilotTuner(Missile.AutoPilot, 0.2, 250, 0.2, interval: 10);
        Missile.AutoPilot.Engage();
        tuner.Tune();
        
        
        bool active = false;

        while (Missile is not null) {
            Console.WriteLine((Missile.AutoPilot.Error, Missile.AngularVelocity(Ref).ToVec().Length));
            if (!active && Missile.AutoPilot.Error < 3 && Missile.AngularVelocity(Ref).ToVec().Length < 20) {
                active = true;
                foreach(var engine in Missile.Parts.Engines) {
                    engine.Active = true;
                }

                tuner.StoppingTimeMultiplier = 250 + 400 * Throttle;
                Missile.Control.Throttle = (float)Throttle;
            }
            TargetMissile();
            await Task.Delay(40);
        }
    }

    private Vector3D? lastTickTargetVelocity = null;
    private double? lastTickTime = null;

    double TargetMissile() {
        var refFrame = Missile.Orbit.Body.NonRotatingReferenceFrame;
        var relativeTargetPosition = targetPosStream.Get().ToVec() - missilePosStream.Get().ToVec();
        var relativeTargetVelocity = Target.Velocity(refFrame).ToVec() - Missile.Velocity(refFrame).ToVec();

        var time = Missile.MET;

        var targetAcceleration = (
            (relativeTargetVelocity - lastTickTargetVelocity) / (time - lastTickTime) 
            + Missile.Thrust * Missile.Direction(refFrame).ToVec() / Missile.Mass
            ) ?? new Vector3D();

        var maxMissileAcceleration = Throttle * Missile.MaxVacuumThrust / Missile.Mass;

        var (desiredAcceleration, timeToTarget) = ComputeIntercept(
            relativeTargetPosition,
            relativeTargetVelocity,
            targetAcceleration,
            maxMissileAcceleration
        );

        if (!desiredAcceleration.HasValue)
            return timeToTarget;
        
        // Console.Write($"{timeToTarget:00.000}, {relativeTargetPosition.Length:0000.00}, {relativeTargetVelocity.Length:000.00}, {targetAcceleration.Length:000.000}\n");
        Missile.AutoPilot.TargetDirection = desiredAcceleration.Value.ToTuple();

        lastTickTargetVelocity = relativeTargetVelocity;
        lastTickTime = time;
        
        return timeToTarget;
    }

    double ComputeThrustTime() {
        var engines = Missile.Parts.Engines;
        var fuelConsumption = engines
            .Select(e => e.MaxVacuumThrust / (e.VacuumSpecificImpulse * KerbinSurfaceGravity))
            .Sum();

        var fuelAmount = Missile.Parts.All
            .Select(e => FuelTypes.Select(fuel => e.Resources.Amount(fuel)).Sum()).Sum();

        return fuelAmount * FuelMassMultiplier / (fuelConsumption * Throttle);
    }

    public static (Vector3D?, double) ComputeIntercept(
        Vector3D targetPosition,
        Vector3D targetVelocity,
        Vector3D targetAcceleration,
        double maxMissileAcceleration
    ) {
        var equation = new Polynomial(-maxMissileAcceleration * maxMissileAcceleration);
        for (int i = 0; i < 3; i++) {
            var axisPolynomial = new Polynomial(
                targetAcceleration.ToVector()[i],
                2 * targetVelocity.ToVector()[i],
                2 * targetPosition.ToVector()[i]
            );
            equation += axisPolynomial * axisPolynomial;
        }
        var roots = equation.Roots().Where(e => e.IsReal()).Select(e => e.Real).ToArray();

        if (!roots.Any())
            return (null, Double.PositiveInfinity);

        var targetTime = 1 / roots.Max();

        var missileAcceleration = 2 * targetPosition / (targetTime * targetTime) + 2 * targetVelocity / targetTime + targetAcceleration;

        return (missileAcceleration, targetTime);
    }
}
