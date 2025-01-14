using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldData;

public class Powerup
{
    public int power;
    public Vector2D loc = new Vector2D();
    public bool died;

    public Powerup()
    {

    }

    // initialize
    public Powerup(int power, World theWorld)
    {
        this.power = power;
        Spawn(theWorld);
    }

    public void Spawn(World theWorld)
    {
        Random randX = new Random();
        Random randY = new Random();
        // spawn at random place and make sure not in walls

        foreach (KeyValuePair<int, Wall> entry in theWorld.Walls)
        {
            loc = new Vector2D(randX.Next(-(theWorld.size / 2) - 100, (theWorld.size / 2) - 100), randY.Next(-(theWorld.size / 2) - 100, (theWorld.size / 2) - 100));
            if (entry.Value.CheckCollisionPowerUps(loc))
            {
                Spawn(theWorld);
            }
        }

        this.died = false;

    }


    // method to check collisions with powerups
    public bool CheckCollision(Vector2D pos)
    {
        if (loc.X - 10 < pos.X && pos.X < loc.X + 10 && loc.Y - 10 < pos.Y && pos.Y < loc.Y + 10 && died == false)
        {
            this.died = true;
            return true;
        }

        return false;
    }
}

