using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KspUtils.Sharp3D;
using Sharp3D.Math.Core;

namespace KspTest3;


public class Reentry {
    public Connection Conn { get; }
    public Vessel Vessel { get; }
    public CelestialBody Body { get; }

    private Flight Flight { get; }
    private double GravitationalParameter { get; }
    private Stream<float> Mass { get;  }

    public Reentry(Connection conn, Vessel vessel, CelestialBody body) {
        Conn = conn;
        Vessel = vessel;
        Body = body;

        GravitationalParameter = body.GravitationalParameter;
        Flight = vessel.Flight(body.ReferenceFrame);
        Mass = conn.AddStream(() => vessel.Mass);
    }

    private Vector3D GravityAcceleration(Vector3D position) {
        return -position.Normalized() * GravitationalParameter / position.GetLengthSquared();
    }

    public Vector3D AerodynamicAcceleration(Vector3D position, Vector3D velocity) {
        var center = Conn.SpaceCenter();

        var surfaceVelocity = center.TransformVelocity(
            position.ToTuple(),
            velocity.ToTuple(),
            Body.NonRotatingReferenceFrame,
            Body.ReferenceFrame
        ).ToVec();
        
        var rotatedLocalDir = new Vector3D(0, -1, 0);
        var rotatedLocalVel = rotatedLocalDir * surfaceVelocity.GetLength();
        var rotatedSurfaceVel = center.TransformDirection(rotatedLocalVel.ToTuple(), Vessel.ReferenceFrame, Body.ReferenceFrame);

        var surfacePosition = center.TransformPosition(position.ToTuple(), Body.NonRotatingReferenceFrame, Body.ReferenceFrame);

        var rotatedSurfaceForce = Flight.SimulateAerodynamicForceAt(Body, surfacePosition, rotatedSurfaceVel);
        var rotatedVesselForce = center.TransformDirection(rotatedSurfaceForce, Body.ReferenceFrame, Vessel.ReferenceFrame);
        
        var quaternion = Util.GenerateRotationQuaternion(rotatedLocalDir, surfaceVelocity);
        var surfaceForce = quaternion.Rotate(rotatedVesselForce.ToVec());
        var actualForce = center.TransformDirection(surfaceForce.ToTuple(), Body.ReferenceFrame, Body.NonRotatingReferenceFrame);

        return actualForce.ToVec() / Mass.Get();
    }

    public Vector3D TotalAcceleration(double time, Vector3D pos, Vector3D vel) {
        return GravityAcceleration(pos) + AerodynamicAcceleration(pos, vel);
    }
}
