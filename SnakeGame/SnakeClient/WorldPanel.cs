using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using WorldData;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Graphics;

namespace SnakeGame;
public class WorldPanel : IDrawable
{

    // A delegate for DrawObjectWithTransform
    // Methods matching this delegate can draw whatever they want onto the canvas  
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    private IImage wall;
    private IImage background;
    private IImage explosion;

    private World theWorld;

    private int viewSize = 900;

    private bool initializedForDrawing = false;

#if MACCATALYST
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#endif

    //Default constructor
    public WorldPanel()
    {
    }
    //Initialize world
    public void SetWorld(World w)
    {
        theWorld = w;
    }

    //initialize drawing
    private void InitializeDrawing()
    {
        wall = loadImage("WallSprite.png");
        background = loadImage("Background.png");
        explosion = loadImage("Explosion.png");
        initializedForDrawing = true;
    }

    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o">The player to draw</param>
    /// <param name="canvas"></param>
    private void PlayerDrawer(object o, ICanvas canvas)
    {
        Snake p = o as Snake;
        int width = 10;
        canvas.FillColor = ColorPicker(p.snake);

        // Ellipses are drawn starting from the top-left corner.
        // So if we want the circle centered on the powerup's location, we have to offset it
        // by half its size to the left (-width/2) and up (-height/2)
        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }

    /// <summary>
    /// A method that returns colors
    /// </summary>
    private Color ColorPicker(int id)
    {
        if (id % 8 == 0)
        {
            return Colors.Pink;
        }
        if (id % 7 == 0)
        {
            return Colors.Orange;
        }
        if (id % 6 == 0)
        {
            return Colors.Brown;
        }
        if (id % 5 == 0)
        {
            return Colors.Purple;
        }
        if (id % 4 == 0)
        {
            return Colors.Yellow;
        }
        if (id % 3 == 0)
        {
            return Colors.Green;
        }
        if (id % 2 == 0)
        {
            return Colors.Blue;
        }
       
        return Colors.Red;
    }


    /// <summary>
    /// A method that can be used as an ObjectDrawer delegate
    /// </summary>
    /// <param name="o">The powerup to draw</param>
    /// <param name="canvas"></param>
    private void PowerupDrawer(object o, ICanvas canvas)
    {
        Powerup p = o as Powerup;
        int width = 10;
        canvas.FillColor = Colors.Orange;

        // Ellipses are drawn starting from the top-left corner.
        // So if we want the circle centered on the powerup's location, we have to offset it
        // by half its size to the left (-width/2) and up (-height/2)
        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }

    // draws walls
    private void WallDrawer(object o, ICanvas canvas)
    {
        Wall p = o as Wall;
        canvas.DrawImage(wall, -25, -25, 50, 50);
    }

    // draws name
    private void NameDrawer(object o, ICanvas canvas)
    {
        string p = o as string;
        canvas.DrawString(p, 0, -25, HorizontalAlignment.Left);
    }

    // draws snake death
    private void DeathDrawer(object o, ICanvas canvas)
    {
        Snake p = o as Snake;
        canvas.DrawImage(explosion, -25, -25, 50, 50);
    }

    /// <summary>
    /// This runs whenever the drawing panel is invalidated and draws the game
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();
        lock (theWorld)
        {
            if (theWorld.Snakes.ContainsKey(theWorld.mySnake))
            {
                /*canvas.DrawImage(wall, 0, 0, wall.Width, wall.Height);*/
                float playerX = (float)theWorld.Snakes[theWorld.mySnake].body.Last().GetX();
                float playerY = (float)theWorld.Snakes[theWorld.mySnake].body.Last().GetY();
                canvas.Translate((-playerX + (viewSize) / 2), -playerY + (viewSize / 2));
            }

            canvas.DrawImage(background, (-theWorld.worldSize / 2), (-theWorld.worldSize / 2), theWorld.worldSize, theWorld.worldSize);

            // draws walls
            foreach (var wall in theWorld.Walls.Values)
            {
                double wallStartX = 0;
                double wallStartY = 0;
                double wallEndX = 0;
                double wallEndY = 0;

                if (wall.p1.X <= wall.p2.X)
                {
                    wallStartX = wall.p1.X;
                    wallEndX = wall.p2.X;
                }
                else
                {
                    wallEndX = wall.p1.X;
                    wallStartX = wall.p2.X;
                }

                if (wall.p1.Y <= wall.p2.Y)
                {
                    wallStartY = wall.p1.Y;
                    wallEndY = wall.p2.Y;
                }
                else
                {
                    wallEndY = wall.p1.Y;
                    wallStartY = wall.p2.Y;
                }

                while (wallStartX < wallEndX || wallStartY < wallEndY)
                {
                    DrawObjectWithTransform(canvas, wall,
                    wallStartX, wallStartY, 0, WallDrawer);
                    if (wallStartX < wallEndX)
                    {
                        wallStartX += 50;
                    }
                    if (wallStartY < wallEndY)
                    {
                        wallStartY += 50;
                    }
                }

                if (wallStartX == wallEndX && wallStartY == wallEndY)
                {
                    DrawObjectWithTransform(canvas, wall,
                    wallStartX, wallStartY, 0, WallDrawer);
                }


            }

            // draws powerups
            foreach (var p in theWorld.Powerups.Values)
            {
                DrawObjectWithTransform(canvas, p,
                p.loc.GetX(), p.loc.GetY(), 0,
                PowerupDrawer);
            }

            //draws each snake and its segments
            foreach (var p in theWorld.Snakes.Values)
            {
                for (int i = 0; i < p.body.Count; i++)
                {
                    if (i > 0)
                    {
                        double currX = p.body[i].GetX();
                        double currY = p.body[i].GetY();
                        double nextX = p.body[i - 1].GetX();
                        double nextY = p.body[i - 1].GetY();

                        if (p.body[i].GetX() > p.body[i - 1].GetX())
                        {
                            nextX = p.body[i].GetX();
                            currX = p.body[i - 1].GetX();
                        }

                        if (p.body[i].GetY() > p.body[i - 1].GetY())
                        {
                            nextY = p.body[i].GetY();
                            currY = p.body[i - 1].GetY();
                        }

                        while (currX < nextX)
                        {
                            currX += 1;
                            DrawObjectWithTransform(canvas, p,
                            currX, currY, 0,
                            PlayerDrawer);
                        }

                        while (currY < nextY)
                        {
                            currY += 1;
                            DrawObjectWithTransform(canvas, p,
                            currX, currY, 0,
                            PlayerDrawer);
                        }

                    }
                    var s = p.body.ElementAt(i);
                    DrawObjectWithTransform(canvas, p,
                    s.GetX(), s.GetY(), 0,
                    PlayerDrawer);
                }

                string playerNameScore = p.name + ": " + p.score;
                DrawObjectWithTransform(canvas, playerNameScore, p.body.Last().GetX(), p.body.Last().GetY(), 0, NameDrawer);
                if (!p.alive)
                {
                    DrawObjectWithTransform(canvas, p, p.body.Last().GetX(), p.body.Last().GetY(), 0, DeathDrawer);
                }

            }
        }

    }
}
