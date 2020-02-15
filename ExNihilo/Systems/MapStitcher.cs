
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tile = ExNihilo.Systems.TypeMatrix.Type;

namespace ExNihilo.Systems
{
    public static class TileRef
    {
        public enum Type : byte
        {
            None = 2, Floor = 3, Wall = 4, NoneFloor = 5, NoneWall = 6, FloorWall = 7, Any = 9
        }

        public static long ToLong(string list)
        {
            return long.Parse(list, NumberStyles.HexNumber);
        }
        public static long ToLong(Type[] list)
        {
            return ToLong(list.Aggregate("", (current, t) => current + (byte)t));
        }

        /* a will be 2-7/9
         * b will be only 2/3/4
         */
        public static bool Equals(long a, long b)
        {
            var aB = a.ToString("X9").ToCharArray();
            var bB = b.ToString("X9").ToCharArray();
            Array.Reverse(aB);
            Array.Reverse(bB);
            for (int i = 0; i < 9; i++)
            {
                var aBv = aB[i] - '0';
                var bBv = bB[i] - '0';
                if (aBv == (int)Type.Any) continue;
                var sub = aBv - bBv;
                if (aBv != bBv && (sub > (int)Type.Wall || sub < (int)Type.None || sub == bBv)) return false;
            }

            return true;
        }
    }

    /* A TileTextureMap is effectively a texture pack that contains everything needed for a single floor
     * These packs can be an combination of multiple TextureMapFile (.tmf) files
     * A floor map is stitched using only one TileTextureMap
     * A TileTextureMap should have at least one floor, one wall, and one stair tile
     */
    public class TileTextureMap
    {
        public int TileSize;
        private readonly Texture2D _null;
        private readonly Dictionary<long, List<Texture2D>> _mapping;

        private void Entry(GraphicsDevice g, Texture2D texture, string line)
        {
            //desc format -> hexstring x y (ex. 223456778 32 32)
            //bytestring of 0 is stairs
            try
            {
                var set = line.Split(' ');
                var id = TileRef.ToLong(set[0]);
                var rect = new Rectangle(int.Parse(set[1]), int.Parse(set[2]), TileSize, TileSize);
                var tex = TextureUtilities.GetSubTexture(g, texture, rect);
                if (_mapping.TryGetValue(id, out var list))
                {
                    //Another of this type already exists
                    list.Add(tex);
                }
                else
                {
                    //New type
                    _mapping.Add(id, new List<Texture2D>{tex});
                }
            }
            catch (Exception)
            {
                GameContainer.Console.ForceMessage("<error>", "Unexpected line " + line + "in tile map description", Color.DarkRed, Color.White);
            }
        }

        private TileTextureMap(Texture2D n)
        {
            _mapping = new Dictionary<long, List<Texture2D>>();
            _null = n;
            TileSize = -1;
        }
        public static TileTextureMap GetTileTextureMap(GraphicsDevice g, params string[] fileNames)
        {
            var map = new TileTextureMap(new Texture2D(g, 1, 1));

            foreach (var fileName in fileNames)
            {
                try
                {
                    var file = File.OpenRead(fileName);
                    var zip = new ZipArchive(file, ZipArchiveMode.Read);

                    var tex = zip.Entries.FirstOrDefault(f => f.FullName == "tex");
                    var desc = zip.Entries.FirstOrDefault(f => f.FullName == "desc");

                    if (tex != null && desc != null)
                    {
                        var texStream = tex.Open();
                        var texture = Texture2D.FromStream(g, texStream);
                        texStream.Dispose();

                        var descStream = new StreamReader(desc.Open());

                        //Skip comments
                        var line = descStream.ReadLine();
                        while (!descStream.EndOfStream && (line.Length == 0 || line.StartsWith("#")))
                        {
                            line = descStream.ReadLine();
                        }
                        //First real line must be tile size
                        if (!descStream.EndOfStream && int.TryParse(line, out int size))
                        {
                            if (map.TileSize == -1) map.TileSize = size;
                            else if (map.TileSize != size)
                            {
                                zip.Dispose();
                                file.Close();
                                GameContainer.Console.ForceMessage("<error>", "Texture map file \"" + fileName + "\" has a different tile size than a previous file", Color.DarkRed, Color.White);
                                continue;
                            }
                            while (!descStream.EndOfStream)
                            {
                                line = descStream.ReadLine();
                                if (line.Length == 0 || line.StartsWith("#")) continue;
                                map.Entry(g, texture, line);
                            }
                        }

                        descStream.Dispose();
                    }

                    zip?.Dispose();
                    file?.Close();
                }
                catch (FileNotFoundException)
                {
                    GameContainer.Console.ForceMessage("<error>", "Texture map file \"" + fileName + "\" does not exist", Color.DarkRed, Color.White);
                }
            }

            if (map.TileSize <= 0 || map._mapping.Count < 3)
            {
                //if the the texture map description is broken
                var wall = TextureUtilities.CreateSingleColorTexture(g, 16, 16, Color.DarkRed);
                var floor = TextureUtilities.CreateSingleColorTexture(g, 16, 16, Color.ForestGreen);
                var stair = TextureUtilities.CreateSingleColorTexture(g, 16, 16, Color.DeepSkyBlue);
                map.TileSize = 16;
                map._mapping.Add(0, new List<Texture2D> { stair });
                map._mapping.Add(TileRef.ToLong("999939999"), new List<Texture2D>{floor});
                map._mapping.Add(TileRef.ToLong("999949999"), new List<Texture2D>{wall});
                GameContainer.Console?.ForceMessage("<error>", "Texture map is invalid. Loading emergency default", Color.DarkRed, Color.White);
            }
            return map;
        }

        public Texture2D GetAnyOfType(long type, Random rand)
        {
            var g = (from set in _mapping where TileRef.Equals(set.Key, type) select set.Value[rand.Next(set.Value.Count)]).FirstOrDefault();
            return g ?? _null;
        }

        //Convenience function since stair ID is static
        public Texture2D GetAnyStairs(Random rand)
        {
            return GetAnyOfType(0, rand);
        }
    }

    public static class MapStitcher
    {
        private static TileRef.Type[] GetSurroundings(TypeMatrix set, int x, int y)
        {
            var map = new List<TileRef.Type>(9);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    switch (set.Get(x+j, y+i))
                    {
                        case Tile.None:
                            map.Add(TileRef.Type.None);
                            break;
                        case Tile.Wall:
                            map.Add(TileRef.Type.Wall);
                            break;
                        case Tile.Ground:
                        case Tile.Stairs:
                            map.Add(TileRef.Type.Floor);
                            break;
                    }
                }
            }
            return map.ToArray();
        }

        public static Texture2D StitchMap(GraphicsDevice graphics, TypeMatrix set, TileTextureMap map, Random rand)
        {
            var texture = new Texture2D(graphics, map.TileSize * set.X, map.TileSize * set.Y);
            
            for (int i = 0; i < set.Y; i++)
            {
                for (int j = 0; j < set.X; j++)
                {
                    var tile = set.Get(j, i);
                    if (tile == Tile.Stairs) TextureUtilities.SetSubTexture(texture, map.GetAnyStairs(rand), j * map.TileSize, i * map.TileSize);
                    else if (tile == Tile.Wall || tile == Tile.Ground) // TODO: potentially allow this for void as well? some tiles may be in the void
                    {
                        var id = TileRef.ToLong(GetSurroundings(set, j, i));
                        TextureUtilities.SetSubTexture(texture, map.GetAnyOfType(id, rand), j * map.TileSize, i * map.TileSize);
                    }
                }
            }
            return texture;
        }
    }
}
