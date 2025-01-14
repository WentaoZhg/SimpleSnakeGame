using Newtonsoft.Json;
using SnakeGame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorldData;

[DataContract(Namespace ="")]
public class Wall
{
    [JsonProperty(PropertyName = "wall")]
    [DataMember (Name="ID")]
    public int wall;

    [DataMember(Name = "p1")]
    public Vector2D p1 = new Vector2D();

    [DataMember(Name = "p2")]
    public Vector2D p2 = new Vector2D();

    public Wall()
    {

    }

    // checks for collisions of snake and block after spawn
    public bool CheckCollision(Vector2D pos)
    {
        double leftX;
        double rightX;
        double leftY;
        double rightY;

        if (p1.X < p2.X)
        {
            leftX = p1.X - 25;
            rightX = p2.X + 25; 
        } else
        {
            leftX = p2.X - 25;
            rightX = p1.X + 25;
        }
        if (p1.Y < p2.Y)
        {
            leftY = p1.Y - 25;
            rightY = p2.Y + 25;
        }
        else
        {
            leftY = p2.Y - 25;
            rightY = p1.Y + 25;
        }

        if (leftX - 5 < pos.X && pos.X < rightX + 5 && leftY - 5 < pos.Y && pos.Y < rightY + 5)
        {
            return true;
        }

        return false;
    }

    // checks collision on spawning powerups
    public bool CheckCollisionPowerUps(Vector2D pos)
    {
        double leftX;
        double rightX;
        double leftY;
        double rightY;

        if (p1.X < p2.X)
        {
            leftX = p1.X - 25;
            rightX = p2.X + 25;
        }
        else
        {
            leftX = p2.X - 25;
            rightX = p1.X + 25;
        }
        if (p1.Y < p2.Y)
        {
            leftY = p1.Y - 25;
            rightY = p2.Y + 25;
        }
        else
        {
            leftY = p2.Y - 25;
            rightY = p1.Y + 25;
        }

        if (Between(pos.X, leftX - 12, rightX + 12) && Between(pos.Y, leftY - 12, rightY + 12))
        {
            return true;
        }

        return false;
    }

    //checks collisions on snake spawn
    public bool CheckCollisionSnakeSpawn(Vector2D pos1, Vector2D pos2)
    {
        double leftX;
        double rightX;
        double leftY;
        double rightY;

        if (p1.X < p2.X)
        {
            leftX = p1.X - 25;
            rightX = p2.X + 25;
        }
        else
        {
            leftX = p2.X - 25;
            rightX = p1.X + 25;
        }
        if (p1.Y < p2.Y)
        {
            leftY = p1.Y - 25;
            rightY = p2.Y + 25;
        }
        else
        {
            leftY = p2.Y - 25;
            rightY = p1.Y + 25;
        }
        
        if ( ((Between(pos1.X, leftX - 75, rightX + 75) || Between(pos2.X, leftX - 75, rightX + 75)) && Between(pos1.Y, leftY, rightY)) || 
             ((Between(pos1.Y, leftY - 75, rightY + 75) || Between(pos2.Y, leftY - 75, rightY + 75)) && Between(pos1.X, leftX, rightX)) )
        {
            return true;
        }

        return false;
    }

    // helper to check if point is between two other points
    private static bool Between(double num, double lower, double upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= num && num <= upper
            : lower < num && num < upper;
    }

}