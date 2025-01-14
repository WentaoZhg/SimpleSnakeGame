using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WorldData;

[DataContract(Name = "GameSettings", Namespace = "")]
public class GameSettings
{
    [DataMember(Name = "FramesPerShot")]
    public int FramesPerShot;

    [DataMember(Name = "MSPerFrame")]
    public int MSPerFrame;

    [DataMember(Name = "RespawnRate")]
    public int RespawnRate;

    [DataMember(Name = "UniverseSize")]
    public int UniverseSize;

    [DataMember(Name = "Walls")]
    public List<Wall> Walls;

    public GameSettings()
    {
        Walls = new List<Wall>();
    }
}
