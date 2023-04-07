using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;

namespace PreciseLanding; 

public class AbstractController {
    public Connection Connection { get; }
    public Service Center { get; }
    public Vessel Vessel { get; }

    public AbstractController(Connection? connection = null) {
        Connection = connection ?? new Connection();
        Center = Connection.SpaceCenter();
        Vessel = Center.ActiveVessel;
    }
}
