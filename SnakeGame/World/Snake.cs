using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WorldData;

public class Snake
{
    public int snake;
    public string name = "";
    public List<Vector2D> body = new List<Vector2D>();
    public Vector2D dir = new Vector2D();
    public int score;
    public bool died;
    public bool alive;
    public bool dc;
    public bool join;


    [JsonIgnoreAttribute]
    private bool turned;
    [JsonIgnoreAttribute]
    private int turnedTimer;

    [JsonIgnoreAttribute]
    private int deathTimer;

    [JsonIgnoreAttribute]
    private World theWorld = new World();

    [JsonIgnoreAttribute]
    private bool growing;

    [JsonIgnoreAttribute]
    private int growTimer;

    public Snake()
    {

    }

    // initialize snake
    public Snake(int snake, string name, World world)
    {
        this.snake = snake;
        this.name = name;
        this.score = 0;
        this.died = false;
        this.dc = false;
        this.join = true;
        this.turned = false;
        dir = new Vector2D(1, 0) * 3;
        this.deathTimer = 0;
        this.theWorld = world;
        Spawn();
    }

    //spawn snake
    public void Spawn()
    {
        body.Clear();
        this.alive = true;

        Random randX = new Random();
        Random randY = new Random();
        Random randDir = new Random();
        int xLoc = randX.Next(-(theWorld.size / 2) - 100, (theWorld.size / 2) - 100);
        int yLoc = randY.Next(-(theWorld.size / 2) - 100, (theWorld.size / 2) - 100);
        int startDir = randDir.Next(1, 5);

        // chooses random direction
        switch (startDir)
        {
            case 1:
                dir = new Vector2D(0, -1) * 3;
                body.Add(new Vector2D(xLoc, yLoc));
                body.Add(new Vector2D(xLoc, yLoc - 120));
                break;
            case 2:
                dir = new Vector2D(0, 1) * 3;
                body.Add(new Vector2D(xLoc, yLoc));
                body.Add(new Vector2D(xLoc, yLoc + 120));
                break;
            case 3:
                dir = new Vector2D(-1, 0) * 3;
                body.Add(new Vector2D(xLoc, yLoc));
                body.Add(new Vector2D(xLoc - 120, yLoc));
                break;
            case 4:
                dir = new Vector2D(1, 0) * 3;
                body.Add(new Vector2D(xLoc, yLoc));
                body.Add(new Vector2D(xLoc + 120, yLoc));
                break;
            default:
                break;
        }

        // checks to make sure snake doesnt spawn in walls or too close
        foreach (KeyValuePair<int, Wall> entry in theWorld.Walls)
        {
            if (entry.Value.CheckCollisionSnakeSpawn(body.Last(), body.First()))
            {
                Spawn();
            }
        }

        foreach (KeyValuePair<int, Snake> entry in theWorld.Snakes)
        {
            if (entry.Value.CheckCollision(this))
            {
                Spawn();
            }
        }
    }

    // changes the direction the snake moves
    public void ChangeDirection(string direction)
    {
        if (turnedTimer >= 3 || turned == false)
        {
            turned = false;
            turnedTimer = 0;
            Vector2D tempDir = dir;
            switch (direction)
            {
                case "up":
                    tempDir = new Vector2D(0, -1) * 3;
                    break;
                case "down":
                    tempDir = new Vector2D(0, 1) * 3;
                    break;
                case "left":
                    tempDir = new Vector2D(-1, 0) * 3;
                    break;
                case "right":
                    tempDir = new Vector2D(1, 0) * 3;
                    break;
                default:
                    break;
            }

            // oldDir * -1 doesn't work because it counts (0, 1) as different from (-0, 1)
            if (!tempDir.Equals(dir) && (tempDir.X * -1 != dir.X && tempDir.Y != dir.Y))
            {
                dir = tempDir;
                body.Add(new Vector2D(body.Last().X, body.Last().Y));
                turned = true;
            }
        }
    }

    // updates snake
    public void Update()
    {
        if (turned == true)
        {
            turnedTimer++;
        }

        if (growing == true)
        {
            growTimer++;
        }

        if (growTimer > 12)
        {
            growing = false;
            growTimer = 0;
        }

        if (this.dc)
        {
            theWorld.Snakes.Remove(this.snake);
        }

        this.join = false;
        this.died = false;

        if (this.alive == true)
        {
            Vector2D tail = body.First();
            Vector2D tailDir = body[1];

            body[body.Count - 1] += dir;

            Vector2D tailVel = tailDir - tail;
            tailVel.Y = (tailVel.Y == 0) ? 0 : tailVel.Y / Math.Abs(tailVel.Y);
            tailVel.X = (tailVel.X == 0) ? 0 : tailVel.X / Math.Abs(tailVel.X);

            if (growing == false)
            {
                body[0] += tailVel * 3;
            }


            if (body.First().Equals(body[1]) || body.First().X * tailVel.X > body[1].X * tailVel.X || body.First().Y * tailVel.Y > body[1].Y * tailVel.Y)
            {
                body.RemoveAt(0);
            }

            //wrap around
            // -x boundary
            if (body.Last().X <= -(theWorld.size / 2))
            {
                body.Add(new Vector2D(-(theWorld.size / 2), body.Last().Y));
                body.Add(new Vector2D((theWorld.size / 2), body.Last().Y));
                body.Add(new Vector2D((theWorld.size / 2), body.Last().Y));
            }
            else

            // +x boundary
            if (body.Last().X >= (theWorld.size / 2))
            {
                body.Add(new Vector2D((theWorld.size / 2), body.Last().Y));
                body.Add(new Vector2D(-(theWorld.size / 2), body.Last().Y));
                body.Add(new Vector2D(-(theWorld.size / 2), body.Last().Y));
            }

            // -y boundary
            if (body.Last().Y <= -(theWorld.size / 2))
            {
                body.Add(new Vector2D(body.Last().X, -(theWorld.size / 2)));
                body.Add(new Vector2D(body.Last().X, (theWorld.size / 2)));
                body.Add(new Vector2D(body.Last().X, (theWorld.size / 2)));
            }
            else

            // +y boundary
            if (body.Last().Y >= (theWorld.size / 2))
            {
                body.Add(new Vector2D(body.Last().X, (theWorld.size / 2)));
                body.Add(new Vector2D(body.Last().X, -(theWorld.size / 2)));
                body.Add(new Vector2D(body.Last().X, -(theWorld.size / 2)));
            }

            if (body.First().X <= -(theWorld.size / 2) || body.First().X >= (theWorld.size / 2) ||
                body.First().Y <= -(theWorld.size / 2) || body.First().Y >= (theWorld.size / 2))
            {
                body.RemoveAt(0);
                body.RemoveAt(0);
            }



            // check collisions
            foreach (KeyValuePair<int, Wall> entry in theWorld.Walls)
            {
                if (entry.Value.CheckCollision(body.Last()))
                {
                    this.died = true;
                    this.alive = false;
                    this.score = 0;
                }
            }

            foreach (KeyValuePair<int, Snake> entry in theWorld.Snakes)
            {
                if (entry.Value.CheckCollision(this))
                {
                    this.died = true;
                    this.alive = false;
                    this.score = 0;
                }
            }

            // powerup collision and grows snake
            foreach (KeyValuePair<int, Powerup> entry in theWorld.Powerups)
            {
                if (entry.Value.CheckCollision(body.Last()))
                {

                    this.score++;
                    this.growing = true;
                }
            }

        }
        else
        {
            deathTimer++;
            if (deathTimer >= theWorld.respawnRate)
            {
                Spawn();
                deathTimer = 0;
            }
        }
    }

    // checks collisions between snakes and self
    public bool CheckCollision(Snake s)
    {
        // self collision
        if (s == this)
        {
            Vector2D headVel = body[body.Count - 1] - body[body.Count - 2];
            headVel.Y = (headVel.Y == 0) ? 0 : headVel.Y / Math.Abs(headVel.Y);
            headVel.X = (headVel.X == 0) ? 0 : headVel.X / Math.Abs(headVel.X);
            if (headVel.X == 0 && headVel.Y == 0)
            {
                return false;
            }

            int oppositeDirectionIndex = body.Count;

            for (int i = body.Count - 2; i >= 0; i--)
            {
                Vector2D vel = body[i + 1] - body[i];
                vel.Y = (vel.Y == 0) ? 0 : vel.Y / Math.Abs(vel.Y);
                vel.X = (vel.X == 0) ? 0 : vel.X / Math.Abs(vel.X);

                if (vel.X * -1 == headVel.X && vel.Y * -1 == headVel.Y)
                {
                    oppositeDirectionIndex = i;
                    break;
                }
            }

            if (oppositeDirectionIndex == body.Count)
            {
                return false;
            }

            for (int i = oppositeDirectionIndex; i >= 0; i--)
            {
                double leftX;
                double rightX;
                double leftY;
                double rightY;

                if (body[i].X < body[i + 1].X)
                {
                    leftX = body[i].X - 5;
                    rightX = body[i + 1].X + 5;
                }
                else
                {
                    leftX = body[i + 1].X - 5;
                    rightX = body[i].X + 5;
                }
                if (body[i].Y < body[i + 1].Y)
                {
                    leftY = body[i].Y + 5;
                    rightY = body[i + 1].Y + 5;
                }
                else
                {
                    leftY = body[i + 1].Y - 5;
                    rightY = body[i].Y + 5;
                }

                if (leftX - 5 < s.body.Last().X && s.body.Last().X < rightX + 5 && leftY - 5 < s.body.Last().Y && s.body.Last().Y < rightY + 5)
                {
                    if (rightX - leftX >= theWorld.size || rightY - leftY >= theWorld.size)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
        else
        {
            // other snake collision
            for (int i = 1; i < body.Count; i++)
            {
                double leftX;
                double rightX;
                double leftY;
                double rightY;

                if (body[i].X < body[i - 1].X)
                {
                    leftX = body[i].X - 5;
                    rightX = body[i - 1].X + 5;
                }
                else
                {
                    leftX = body[i - 1].X - 5;
                    rightX = body[i].X + 5;
                }
                if (body[i].Y < body[i - 1].Y)
                {
                    leftY = body[i].Y - 5;
                    rightY = body[i - 1].Y + 5;
                }
                else
                {
                    leftY = body[i - 1].Y - 5;
                    rightY = body[i].Y + 5;
                }

                if (leftX - 5 < s.body.Last().X && s.body.Last().X < rightX + 5 && leftY - 5 < s.body.Last().Y && s.body.Last().Y < rightY + 5)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

