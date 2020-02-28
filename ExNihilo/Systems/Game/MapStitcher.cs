using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ExNihilo.Systems.Backend;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tile = ExNihilo.Systems.Backend.TypeMatrix.Type;

namespace ExNihilo.Systems.Game
{
    public static class TileRef
    {
        public enum Type : byte
        {
            None = 2, Floor = 3, Wall = 4, NoneFloor = 5, NoneWall = 6, FloorWall = 7, Any = 9
        }

        public static string ToString(Type[] list)
        {
            return list.Aggregate("", (current, t) => current + (byte)t);
        }

        //a will be 2-7/9
        //b will be only 2/3/4
        public static bool Equals(string a, string b)
        {
            for (int i = 0; i < 9; i++)
            {
                var aBv = a[i] - '0';
                var bBv = b[i] - '0';
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
        private string _designation;

        public int TileSize;
        public bool CheckVoid;
        private readonly Dictionary<string, List<Texture2D>> _mapping;

        private void Entry(GraphicsDevice g, Texture2D texture, string line, Coordinate offset)
        {
            //desc format -> hexstring x y width height (ex. 223456778 32 32 16 16)
            //note: hexstring of 0 is stairs
            try
            {
                var set = line.Split(' ');
                var id = set[0];
                Texture2D tex;
                if (set.Length == 5)
                {
                    var rect = new Rectangle(int.Parse(set[1]) + offset.X, int.Parse(set[2]) + offset.Y, int.Parse(set[3]), int.Parse(set[4]));
                    tex = TextureUtilities.GetSubTexture(g, texture, rect);
                }
                else if (set.Length == 2 && set[1] == "null")
                {
                    tex = null;
                }
                else throw new IndexOutOfRangeException();

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
            catch (IndexOutOfRangeException)
            {
                GameContainer.Console.ForceMessage("<error>", "Ignoring unexpected line \"" + line + "\" in tile map description", Color.DarkRed, Color.White);
            }
        }

        private static TileTextureMap[] Default(GraphicsDevice g)
        {
            var map = new TileTextureMap();
            var wall = TextureUtilities.CreateSingleColorTexture(g, 16, 16, Color.DarkRed);
            var floor = TextureUtilities.CreateSingleColorTexture(g, 16, 16, Color.DarkGreen);
            var stair = TextureUtilities.CreateSingleColorTexture(g, 16, 16, Color.DeepSkyBlue);
            map.TileSize = 16;
            map._mapping.Add("0", new List<Texture2D> { stair });
            map._mapping.Add("999939999", new List<Texture2D> { floor });
            map._mapping.Add("999949999", new List<Texture2D> { wall });
            return new[] {map};
        }

        private TileTextureMap()
        {
            _mapping = new Dictionary<string, List<Texture2D>>();
            TileSize = -1;
        }
        public static TileTextureMap[] GetTileTextureMap(GraphicsDevice g, string fileName)
        {
            var tileSize = -1;
            var checkVoid = false;
            var designation = "ALL";
            var tmfSet = new List<TileTextureMap>();

            try
            {
                //Open tmf file archive and confirm it has a description file
                var file = File.OpenRead(Environment.CurrentDirectory + "/Content/TexturePacks/" + fileName);
                var zip = new ZipArchive(file, ZipArchiveMode.Read);
                var desc = zip.Entries.FirstOrDefault(f => f.FullName == "desc");
                if (desc is null) throw new IndexOutOfRangeException();

                //Get all valid description lines
                var lines = new List<string>();
                var descStream = new StreamReader(desc.Open());
                while (!descStream.EndOfStream)
                {
                    var line = descStream.ReadLine();
                    if (line is null) break;
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    lines.Add(line);
                }
                descStream.Close();

                //Go through description line by line 
                var offsets = new List<Coordinate>();
                Texture2D curTex = null;
                tmfSet.Add(new TileTextureMap());
                foreach (var line in lines)
                {
                    //Possible line types: TMF, OPEN, OFF, VOID, tile description
                    if (line.StartsWith("TMF "))
                    {
                        //Base tile size
                        var split = line.Split(' ');
                        if (split.Length == 3 && int.TryParse(split[2], out int size))
                        {
                            tileSize = size;
                            designation = split[1];
                        }
                        else throw new IndexOutOfRangeException();
                    }
                    else if (line == "VOID")
                    {
                        //Enable void checks
                        checkVoid = true;
                    }
                    else if (line.StartsWith("OPEN "))
                    {
                        //Open a new texture file
                        offsets.Clear();
                        var tex = zip.Entries.FirstOrDefault(f => f.FullName == line.Substring(5));
                        if (tex is null) throw new IndexOutOfRangeException();
                        var texStream = tex.Open();
                        curTex = Texture2D.FromStream(g, texStream);
                        texStream.Dispose();
                    }
                    else if (line.StartsWith("OFF "))
                    {
                        //Add a new offset
                        var split = line.Split(' '); //Should be OFF, X, Y
                        if (split.Length == 3 && int.TryParse(split[1], out int x) && int.TryParse(split[2], out int y))
                        {
                            offsets.Add(new Coordinate(x, y));
                            if (offsets.Count > tmfSet.Count-1) tmfSet.Add(new TileTextureMap());
                        }
                        else
                        {
                            GameContainer.Console.ForceMessage("<warning>", "Ignoring unexpected line \"" + line + "\" in tile map description", Color.DarkOrange, Color.White);
                        }
                    }
                    else if (curTex != null)
                    {
                        //Tile description (or possibly a junk line)
                        tmfSet[0].Entry(g, curTex, line, new Coordinate());
                        for (int i = 0; i < offsets.Count; i++) tmfSet[i + 1].Entry(g, curTex, line, offsets[i]);
                    }
                    else
                    {
                        //Most likely trying to load tiles without loading a file first
                        GameContainer.Console.ForceMessage("<warning>", "Ignoring unexpected line \"" + line + "\" in tile map description", Color.DarkOrange, Color.White);
                    }
                }

                file.Close(); //Close tmf file
            }
            catch (FileNotFoundException)
            {
                GameContainer.Console.ForceMessage("<error>", "Texture map file \"" + fileName + "\" does not exist or is malformed", Color.DarkRed, Color.White);
                GameContainer.Console.ForceMessage("<error>", "Texture map is invalid. Loading emergency default", Color.DarkRed, Color.White);
                return Default(g);
            }
            catch (IndexOutOfRangeException)
            {
                GameContainer.Console.ForceMessage("<error>", "Texture map file \"" + fileName + "\" is malformed", Color.DarkRed, Color.White);
                GameContainer.Console.ForceMessage("<error>", "Texture map is invalid. Loading emergency default", Color.DarkRed, Color.White);
                return Default(g);
            }
            catch (Exception e)
            {
                GameContainer.Console.ForceMessage("<error>", e.Message, Color.DarkRed, Color.White);
                GameContainer.Console.ForceMessage("<error>", "Texture map is invalid. Loading emergency default", Color.DarkRed, Color.White);
                return Default(g);
            }

            //Remove any sets that are unacceptable and set global params for acceptable sets
            for (int i = tmfSet.Count - 1; i >= 0; i--)
            {
                if (tileSize <= 0 || tmfSet[i]._mapping.Count == 0)
                {
                    tmfSet.RemoveAt(i);
                }
                else
                {
                    tmfSet[i].TileSize = tileSize;
                    tmfSet[i].CheckVoid = checkVoid;
                    tmfSet[i]._designation = designation;
                }
            }

            if (tmfSet.Count != 0) return tmfSet.ToArray();
            GameContainer.Console.ForceMessage("<error>", "Texture map is invalid. Loading emergency default", Color.DarkRed, Color.White);
            return Default(g);
        }

        public Texture2D GetAnyOfType(string type, Random rand)
        {
            foreach (var m in _mapping)
            {
                if (TileRef.Equals(m.Key, type))
                {
                    return m.Value[rand.Next(m.Value.Count)];
                }
            }
            GameContainer.Console.ForceMessage("<warning>", "Texture map contains no definitions for ID " + type, Color.DarkRed, Color.White);
            return null;
        }

        //Convenience function since stair ID is static
        public Texture2D GetAnyStairs(Random rand)
        {
            return GetAnyOfType("0", rand);
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

        public static Texture2D StitchMap(GraphicsDevice graphics, TypeMatrix set, Random rand, TileTextureMap wall, TileTextureMap floor, TileTextureMap other=null)
        {
            var texture = new Texture2D(graphics, wall.TileSize * set.X, wall.TileSize * (set.Y+5));

            for (int i = 0; i < set.Y; i++)
            {
                for (int j = 0; j < set.X; j++)
                {
                    switch (set.Get(j, i))
                    {
                        case Tile.None:
                            if (other != null)
                            {
                                var idn = TileRef.ToString(GetSurroundings(set, j, i));
                                TextureUtilities.SetSubTexture(texture, other.GetAnyOfType(idn, rand), j * other.TileSize, i * other.TileSize);
                            }
                            break;
                        case Tile.Wall:
                            var idw = TileRef.ToString(GetSurroundings(set, j, i));
                            TextureUtilities.SetSubTexture(texture, wall.GetAnyOfType(idw, rand), j * wall.TileSize, i * wall.TileSize);
                            break;
                        case Tile.Ground:
                            var idg = TileRef.ToString(GetSurroundings(set, j, i));
                            TextureUtilities.SetSubTexture(texture, floor.GetAnyOfType(idg, rand), j * floor.TileSize, i * floor.TileSize);
                            break;
                        case Tile.Stairs:
                            TextureUtilities.SetSubTexture(texture, floor.GetAnyStairs(rand), j * floor.TileSize, i * floor.TileSize);
                            break;
                    }
                }
            }
            return texture;
        }
    }
}
