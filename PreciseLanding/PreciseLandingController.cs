using KRPC.Client.Services.SpaceCenter;
using KRPC.Schema.KRPC;
using Service = KRPC.Client.Services.SpaceCenter.Service;

namespace PreciseLanding;

using KRPC.Client;
using KspUtils;

public class PreciseLandingController {
    public Connection Connection { get; }
    public Service Center { get; }
    public Vessel Vessel { get; }
    public Vessel Target { get; }

    private DockingPort VesselPort { get; }

    private DockingPort TargetPort { get; }

    public PreciseLandingController(Connection? connection = null) {
        Connection = connection ?? new Connection();
        Center = Connection.SpaceCenter();
        Vessel = Center.ActiveVessel;
        Target = Center.TargetVessel ?? Center.Vessels.First(v => v.Name == "Miner 1.0");

        VesselPort = Vessel.Parts.DockingPorts.MinBy(
                p => p.Position(Vessel.ReferenceFrame).Item2
            ) ??
            throw new InvalidOperationException();

        TargetPort = Target.Parts.DockingPorts.First();
    }

    public void ThrustControl(bool land) {
        var maxAcc = Vessel.AvailableThrust / Vessel.Mass;
        
        double DesiredVelocity(double x) {
            return -Math.Sign(x) * Math.Sqrt(Math.Abs(/*1.8*maxAcc**/x));
        }

        var height = VesselPort.Position(TargetPort.ReferenceFrame).Item2;
        var velocity = Vessel.Velocity(TargetPort.ReferenceFrame).Item2;
        var desiredVelocity = Math.Min(DesiredVelocity(height-20), -ErrorFunc(height)* (land ? 1 : 0));

        var gravAcc = Vessel.Orbit.Body.GravitationalParameter /
            Vessel.Position(Vessel.Orbit.Body.ReferenceFrame).ToVec().GetLengthSquared();

        var throttleAtDesiredVelocity = gravAcc * Vessel.Mass / Vessel.AvailableThrust;

        var throttle = throttleAtDesiredVelocity - (velocity - desiredVelocity) * 5 * Vessel.Mass / Vessel.AvailableThrust;

        // Console.WriteLine((velocity, desiredVelocity, throttleAtDesiredVelocity, throttle));

        Vessel.Control.Throttle = (float)throttle;
    }
    
    public double AlignmentControl() {
        var (deltaZ, _, deltaX) = Vessel.Position(TargetPort.ReferenceFrame);
        var (velZ, _, velX) = Vessel.Velocity(TargetPort.ReferenceFrame);

        var targetXVel = -Math.Sign(deltaX) * Math.Sqrt(Math.Abs(deltaX)) / 3;
        var targetZVel = -Math.Sign(deltaZ) * Math.Sqrt(Math.Abs(deltaZ)) / 3;

        var deltaXVel = targetXVel - velX;
        var deltaZVel = targetZVel - velZ;

        var dir = Center.TransformDirection(new(deltaZVel, 0, deltaXVel), TargetPort.ReferenceFrame, Vessel.ReferenceFrame);
        
        Vessel.Control.Up = -(float)ErrorFunc(dir.Item3 * 10);
        Vessel.Control.Forward = -(float)ErrorFunc(dir.Item2 * 10);
        Vessel.Control.Right = (float)ErrorFunc(dir.Item1 * 10);

        // Vessel.AutoPilot.TargetDirection = new(deltaZVel, 15, deltaXVel);
        
        // Vessel.AutoPilot.TargetPitch = (float)(Math.Sqrt(deltaXVel * deltaXVel + deltaZVel * deltaZVel));
        // Vessel.AutoPilot.TargetHeading = (float)(180 * Math.Atan2(deltaZVel, deltaXVel) / Math.PI);

        Console.WriteLine((Vessel.Flight(Target.ReferenceFrame).Pitch, Vessel.Flight(Target.ReferenceFrame).Heading, Vessel.Flight(Target.ReferenceFrame).Roll));
        Console.WriteLine((Vessel.AutoPilot.TargetPitch, Vessel.AutoPilot.TargetHeading));

        var error = deltaX * deltaX + deltaZ * deltaZ + deltaXVel * deltaXVel + deltaZVel * deltaZVel;

        return error;
    }
    
    private void MatchRotation() {
        
        Vessel.AutoPilot.AutoTune = true;
        double stoppingTime = 1;
        Vessel.AutoPilot.StoppingTime = new(stoppingTime, stoppingTime, stoppingTime);
        double decelerationTime = 20;
        Vessel.AutoPilot.DecelerationTime = new(decelerationTime, decelerationTime, decelerationTime);
        double timeToPeak = 1;
        Vessel.AutoPilot.TimeToPeak = new(timeToPeak, timeToPeak, timeToPeak);
        
        Vessel.AutoPilot.Engage();

        var frame = Target.ReferenceFrame;

        Vessel.AutoPilot.ReferenceFrame = frame;

        Vessel.AutoPilot.TargetPitch = Target.Flight(frame).Pitch;
        Vessel.AutoPilot.TargetHeading = Target.Flight(frame).Heading;
        Vessel.AutoPilot.TargetRoll = Target.Flight(frame).Roll;
    }

    public async Task Guide() {
        var rotTask = Task.Run(MatchRotation);

        while (VesselPort.State != DockingPortState.Docked) {
            var offset = AlignmentControl();

            // Console.WriteLine(offset);

            ThrustControl(offset < 1);

            Thread.Sleep(100);
        }

        Vessel.Control.Throttle = 0;
        Vessel.AutoPilot.Disengage();

        Vessel.Control.Up = 0;
        Vessel.Control.Forward = 0;
        Vessel.Control.Right = 0;

        await rotTask;
    }

    private static double ErrorFunc(double x) {
        return (Math.Exp(x) - 1) / (Math.Exp(x) + 1);
    }
}
