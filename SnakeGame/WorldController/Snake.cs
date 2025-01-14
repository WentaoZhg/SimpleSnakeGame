using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldData;

public class Snake
{
    public int snake;
    public string name;
    public List<Vector2D> body = new List<Vector2D>();
    public Vector2D dir;
    public int score;
    public bool died;
    public bool alive;
    public bool dc;
    public bool join;
    
    public Snake()
    {

    }
}

