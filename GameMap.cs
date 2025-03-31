using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon3D
{
    public class GameMap
    {
        public readonly string[] field;
        public readonly int MapWidth;
        public readonly int MapHeight;

        public GameMap(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            field = File.ReadAllLines(filePath);
            MapHeight = field.Length;
            MapWidth = field[0].Length;
        }

        public bool IsWall (int x, int y)
        {
            return field[x][y] == '#';
        }
    }
}
