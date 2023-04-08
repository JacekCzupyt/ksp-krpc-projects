// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using KspTest3;
using KspUtils.Sharp3D;
using KspUtils;
using Sharp3D.Math.Core;

Console.WriteLine("Hello, World!");

var conn = new KRPC.Client.Connection();

var center = conn.SpaceCenter();

var vessel = center.ActiveVessel;

var kerbin = center.Bodies["Kerbin"];

var previousTime = vessel.MET;

var reentry = new Reentry(conn, vessel, kerbin);

var method = new RungeKuttaMethod(
    reentry.TotalAcceleration,
    0,
    vessel.Position(kerbin.NonRotatingReferenceFrame).ToVec(),
    vessel.Velocity(kerbin.NonRotatingReferenceFrame).ToVec(),
    0.1
);

Vector3D pos = method.Y, vel = method.Yp;

Console.WriteLine("Start calculations");
var sw = new Stopwatch();
sw.Start();

while (kerbin.AltitudeAtPosition(pos.ToTuple(), kerbin.NonRotatingReferenceFrame) > 0) {
    // Thread.Sleep(40);
    // var time = vessel.MET;
    // var tPos = vessel.Position(kerbin.NonRotatingReferenceFrame).ToVec();
    // var tVel = vessel.Velocity(kerbin.NonRotatingReferenceFrame).ToVec();
    // var dt = time - previousTime;
    // method.H = dt;

    (_, pos, vel) = method.MakeStep();
    
    // Console.WriteLine(
    //     $"Pos diff: {(tPos - pos).GetLength():0.00}".PadRight(20) +
    //     $"Vel diff: {(tVel - vel).GetLength():0.00}".PadRight(20) + 
    //     $"Force diff: {vessel.Flight(kerbin.NonRotatingReferenceFrame).AerodynamicForce.ToVec().GetLength() / vessel.Flight(kerbin.NonRotatingReferenceFrame).SimulateAerodynamicForceAt(kerbin, tPos.ToTuple(), tVel.ToTuple()).ToVec().GetLength()}"
    // );

    // Console.WriteLine((tPos - pos).GetLength());
    // Console.WriteLine((tVel - vel).GetLength());
    // Console.WriteLine(dt);
    
    // previousTime = time;
}

sw.Stop();

Console.WriteLine(
    $"Pos: {pos} ".PadRight(20) +
    $"Vel: {vel} ".PadRight(20)
);

Console.WriteLine($"Time: {sw.Elapsed}");

Thread.Sleep(5000);

while (true) {
    Thread.Sleep(40);
    
    var tPos = vessel.Position(kerbin.NonRotatingReferenceFrame).ToVec();
    var tVel = vessel.Velocity(kerbin.NonRotatingReferenceFrame).ToVec();

    Console.WriteLine(
        $"Pos diff: {tPos - pos} ".PadRight(30) +
        $"Altitude: {vessel.Flight().SurfaceAltitude}" +
        $"Vel diff: {tVel - vel} ".PadRight(30)
    );
}

