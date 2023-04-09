using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using KRPC.Client.Services.SpaceCenter;

namespace KspUtils;

public class VesselAutoPilotTuner {
    public AutoPilot AutoPilot { get; set; }
    public double DecelerationTimeMultiplier { get; set; }
    public double StoppingTimeMultiplier { get; set; }
    public double TimeToPeak { get; set; }

    public Task? TuneTask { get; private set; } = null;
    public CancellationTokenSource? CancellationTokenSource { get; private set; } = null;
    public int Interval { get; set; }

    public VesselAutoPilotTuner(
        AutoPilot autoPilot,
        double decelerationTimeMultiplier = 0.2,
        double stoppingTimeMultiplier = 250,
        double timeToPeak = 0.2,
        double attenuationAngle = 0.5,
        int interval = 50
    ) {
        AutoPilot = autoPilot;
        Interval = interval;
        DecelerationTimeMultiplier = decelerationTimeMultiplier;
        StoppingTimeMultiplier = stoppingTimeMultiplier;
        TimeToPeak = timeToPeak;
        AutoPilot.AttenuationAngle = new(attenuationAngle, attenuationAngle, attenuationAngle);
    }

    public Task Tune() {
        AutoPilot.AutoTune = false;
        CancellationTokenSource = new CancellationTokenSource();
        TuneTask = _tune(CancellationTokenSource.Token);
        return TuneTask;
    }

    private async Task _tune(CancellationToken cancellationToken) {
        while (true) {
            var err = AutoPilot.Error;
            var stoppingTime = err / StoppingTimeMultiplier;
            AutoPilot.StoppingTime = new(stoppingTime, stoppingTime, stoppingTime);
            var decelerationTime = DecelerationTimeMultiplier / err;
            AutoPilot.DecelerationTime = new(decelerationTime, decelerationTime, decelerationTime);
            await Task.Delay(Interval, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
