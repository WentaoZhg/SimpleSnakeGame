using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldData
{
    public class World
    {
        public Dictionary<int, Snake> Snakes;
        public Dictionary<int, Wall> Walls;
        public Dictionary<int, Powerup> Powerups;
        public int mySnake;
        public string mySnakeName = "";
        public int worldSize;
        public int respawnRate;
        public int size;

        public World()
        {
            Snakes = new Dictionary<int, Snake>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();

        }
    }
    
}
