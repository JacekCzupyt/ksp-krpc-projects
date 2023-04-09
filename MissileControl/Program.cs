using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;
using MissileControl;

var conn = new KRPC.Client.Connection();

var center = conn.SpaceCenter();

var missile = center.ActiveVessel;
var target = center.TargetVessel;

var missileController = new MissileController(missile, target, conn, throttle: 0.75);
missileController.Fire();




