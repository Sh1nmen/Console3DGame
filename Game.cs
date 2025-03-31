using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Dungeon3D;

class Game
{
    public static int ScreenWidth = 120;
    public static int ScreenHeight = 40;

    public static double ViewDepth = 7.0; // draw distance
    public static double Speed = 5.0;

    public static double StepSize = 0.005;

    static void Main(string[] args)
    {
        Console.SetWindowSize(ScreenWidth, ScreenHeight);
        Console.SetBufferSize(ScreenWidth, ScreenHeight);
        Console.CursorVisible = false;
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        char[] screen = new char[ScreenWidth * ScreenHeight];

        Play(screen);
    }

    public static void Play(char[] screen)
    {
        var map = new GameMap("Maps\\map1.txt");
        var wallSampler = new TextureSampler("Textures\\brickWall.jpg");

        var lastTime = Environment.TickCount;
        while (true)
        {
            var currentTime = Environment.TickCount;
            var deltaTime = (currentTime - lastTime) / 1_000.0;
            lastTime = currentTime;


            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Q:
                        Player.PlayerA -= (Speed * 0.1) * deltaTime;
                        break;
                    case ConsoleKey.E:
                        Player.PlayerA += (Speed * 0.1) * deltaTime;
                        break;
                    case ConsoleKey.W:
                        Player.PlayerX += Math.Sin(Player.PlayerA) * Speed * deltaTime;
                        Player.PlayerY += Math.Cos(Player.PlayerA) * Speed * deltaTime;
                        if (map.field[(int)Player.PlayerX][(int)Player.PlayerY] == '#')
                        {
                            Player.PlayerX -= Math.Sin(Player.PlayerA) * Speed * deltaTime;
                            Player.PlayerY -= Math.Cos(Player.PlayerA) * Speed * deltaTime;
                        }
                        break;
                    case ConsoleKey.S:
                        Player.PlayerX -= Math.Sin(Player.PlayerA) * Speed * deltaTime;
                        Player.PlayerY -= Math.Cos(Player.PlayerA) * Speed * deltaTime;
                        if (map.field[(int)Player.PlayerX][(int)Player.PlayerY] == '#')
                        {
                            Player.PlayerX += Math.Sin(Player.PlayerA) * Speed * deltaTime;
                            Player.PlayerY += Math.Cos(Player.PlayerA) * Speed * deltaTime;
                        }
                        break;
                    case ConsoleKey.A:
                        Player.PlayerX -= Math.Cos(Player.PlayerA) * Speed * deltaTime;
                        Player.PlayerY += Math.Sin(Player.PlayerA) * Speed * deltaTime;
                        if (map.field[(int)Player.PlayerX][(int)Player.PlayerY] == '#')
                        {
                            Player.PlayerX += Math.Cos(Player.PlayerA) * Speed * deltaTime;
                            Player.PlayerY -= Math.Sin(Player.PlayerA) * Speed * deltaTime;
                        }
                        break;
                    case ConsoleKey.D:
                        Player.PlayerX += Math.Cos(Player.PlayerA) * Speed * deltaTime;
                        Player.PlayerY -= Math.Sin(Player.PlayerA) * Speed * deltaTime;
                        if (map.field[(int)Player.PlayerX][(int)Player.PlayerY] == '#')
                        {
                            Player.PlayerX -= Math.Cos(Player.PlayerA) * Speed * deltaTime;
                            Player.PlayerY += Math.Sin(Player.PlayerA) * Speed * deltaTime;
                        }
                        break;
                }
            }

            for (int x = 0; x < ScreenWidth; x++)
            {
                var rayAngle = (Player.PlayerA - Player.FOV / 2.0) + ((double)x / (double)ScreenWidth) * Player.FOV;

                var hitWall = false;      

                var eyeX = Math.Sin(rayAngle);
                var eyeY = Math.Cos(rayAngle);
                var sampleX = 0d;

                var distanceToWall = 0d;
                while (!hitWall && distanceToWall < ViewDepth)
                {
                    distanceToWall += StepSize;
                    var rayX = (int)(Player.PlayerX + eyeX * distanceToWall);
                    var rayY = (int)(Player.PlayerY + eyeY * distanceToWall);

                    if (rayX < 0 || rayX >= map.MapWidth || rayY < 0 || rayY >= map.MapHeight)
                    {
                        hitWall = true;           
                        distanceToWall = ViewDepth;
                    }
                    else
                    {
                        if (map.IsWall(rayX, rayY))
                        {
                            hitWall = true;

                            var blockMidX = rayX + 0.5;
                            var blockMidY = rayY + 0.5;

                            var rayPointX = Player.PlayerX + eyeX * distanceToWall;
                            var rayPointY = Player.PlayerY + eyeY * distanceToWall;

                            var vievRayAngle = Math.Atan2((rayPointY - blockMidY), (rayPointX - blockMidX));

                            if (vievRayAngle >= -3.14159f * 0.25f && vievRayAngle < 3.14159f * 0.25f)
                                sampleX = rayPointY - rayY;
                            if (vievRayAngle >= 3.14159f * 0.25f && vievRayAngle < 3.14159f * 0.75f)
                                sampleX = rayPointX - rayX;
                            if (vievRayAngle < -3.14159f * 0.25f && vievRayAngle >= -3.14159f * 0.75f)
                                sampleX = rayPointX - rayX;
                            if (vievRayAngle >= 3.14159f * 0.75f || vievRayAngle < -3.14159f * 0.75f)
                                sampleX = rayPointY - rayY;
                        }
                    }
                }


                var ceilingLine = (int)(ScreenHeight / 2.0 - ScreenHeight / distanceToWall);
                var floorLine = ScreenHeight - ceilingLine;

                var wallShade = ' ';
                char[] starChars = { '*', ' ', ' ',
                                     ' ', ' ', ' ', 
                                     ' ', ' ', ' ',
                                     '✧', ' ', ' ',
                                     '·', ' ', ' ',
                                     ' ', ' ', ' ',};

                for (var y = 0; y < ScreenHeight; y++)
                {
                    if (y <= ceilingLine)
                        screen[y * ScreenWidth + x] = starChars[(y * ScreenWidth + x) % starChars.Count()];
                    else if (y > ceilingLine && y <= floorLine) //wall
                    {
                        if (distanceToWall < ViewDepth)
                        {
                            var fSampleY = (y - ceilingLine) / (floorLine - (double)ceilingLine);
                            var pixelColor = wallSampler.GetPixelColor(sampleX, fSampleY);
                            screen[y * ScreenWidth + x] = CountChar(pixelColor);
                        }
                        else screen[y * ScreenWidth + x] = '░';
                    }
                    else
                    {
                        var b = 1 - ((y - ScreenHeight / 2.0) / (ScreenHeight / 2.0));
                        if (b < 0.25) wallShade = 'x';
                        else if (b < 0.5) wallShade = '*';
                        else if (b < 0.75) wallShade = '.';
                        else if (b < 0.9) wallShade = '-';
                        else wallShade = ' ';
                        screen[y * ScreenWidth + x] = wallShade;
                    }
                }
            }
            PrintMap(map, screen);
            PrintFrame(screen);
        }

    }

    public static char CountChar(Color color)
    {
        //char[] charGradient = { ' ', '.', ':', '!', '/', 'r', '(', 'l', 'Z', '4', 'H', '9', 'W', '8', '$', '#' };
        //char[] charGradient = { '@', '$', '8', 'W', '9', 'H', '4', 'Z', 'l', '(', 'r', '/', '!', ':', '.', ' ' };
        char[] charGradient = { '#', '@', '@', '=', '-', ':', '"', '\'', ',', ' ', ' '};
        //char[] charGradient = {'█', '▓', '▒', '░',' ',' ', ' '};

        var luminance = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
        var charIndex = (int)((luminance / 255) * (charGradient.Length - 1));

        charIndex = Math.Clamp(charIndex, 0, charGradient.Length - 1);

        return charGradient[charIndex];
    }
    public static void PrintFrame(char[] screen)
    {
        Console.SetCursorPosition(0, 0);
        Console.Write(screen);
        System.Threading.Thread.Sleep(16);
    }

    public static void PrintMap(GameMap map, char[] screen)
    {
        for (int nx = 0; nx < map.MapWidth; nx++)
            for (int ny = 0; ny < map.MapWidth; ny++)
            {
                screen[(ny + 1) * ScreenWidth + nx] = map.field[ny][nx];
            }
        screen[((int)Player.PlayerX + 1) * ScreenWidth + (int)Player.PlayerY] = 'P';
    }
}