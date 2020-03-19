using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExNihilo.Entity;
using ExNihilo.Menus;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game
{
    /*
     * The top level container for the main game levels
     * Controls the generation display and interaction of the game world
     * For the outerworld level, see the World class
     */
    public class Level : World, ISavable
    {
        private int _fCount;
        private readonly object _fLock = new object();

        private readonly List<Texture2D> _subLevelTextures;
        private readonly List<InteractionMap> _subLevelMaps;

        private int _seed, _parallax, _curLevel;
        private MapGenerator.Type _genType;

        private TileTextureMap[] _wallTextureMapSet, _floorTextureMapSet, _otherTextureMapSet;
        private string _wallTextureMapFile, _floorTextureMapFile, _otherTextureMapFile;

        public Level() : base(0)
        {
            _subLevelTextures = new List<Texture2D>();
            _subLevelMaps = new List<InteractionMap>();
            //_mobSet = new List<EntityContainer>();
        }

        private async void GenerateLevel(GameContainer g, int slot, int level, int itemSeed, NoteMenu stairs)
        {
            var itemRand = new Random(itemSeed);
            (InteractionMap, Texture2D) DoAll()
            {
                var m = MapGenerator.Get(_seed, level, _genType, out var rand);
                var wall = _wallTextureMapSet[rand.Next(_wallTextureMapSet.Length)];
                var floor = _floorTextureMapSet is null ? wall : _floorTextureMapSet[rand.Next(_floorTextureMapSet.Length)];
                var other = _otherTextureMapSet is null ? wall : _otherTextureMapSet[rand.Next(_otherTextureMapSet.Length)];
                var t = MapStitcher.StitchMap(g.GraphicsDevice, m, level, rand, itemRand, wall, floor, other, stairs);
                return (m, t);
            }
            
            var (map, tex) = await Task.Run(() => DoAll());

            if (slot == -1)
            {
                //Main floor
                Map = map;
                WorldTexture = tex;
            }
            else
            {
                //Sub floors
                _subLevelMaps[slot] = map;
                _subLevelTextures[slot] = tex;
            }

            lock (_fLock)
            {
                if (++_fCount >= 1 + _parallax)
                {
                    //All loading complete
                    SetPlayerAnyTile(MathD.urand);
                    g.Pack();
                    g.RequestSectorChange(GameContainer.SectorID.Void);
                }
            }
        }

        public void DoGenerationQueue(GameContainer g, int level, int itemSeed, NoteMenu stairs)
        {
            if (level == -1) level = _curLevel;
            else if (level == 0) level = _curLevel + 1;
            _fCount = 1 + _parallax;
            var offset = level - _curLevel;
            //if (offset == 0 && _subLevelMaps.Count == _parallax) return;
            g.RequestSectorChange(GameContainer.SectorID.Loading);

            lock (_fLock)
            {
                if (offset > 0 && offset <= _subLevelMaps.Count)
                {
                    //If the requested level was pregenerated use it
                    Map = _subLevelMaps[offset - 1];
                    WorldTexture = _subLevelTextures[offset - 1];
                    for (int i = 0; i < offset; i++)
                    {
                        _subLevelMaps.RemoveAt(0);
                        _subLevelTextures.RemoveAt(0);
                    }
                }
                else
                {
                    //Either no pregenerated levels or this is first start
                    _fCount--;
                    GenerateLevel(g, -1, level, itemSeed, stairs);
                    _subLevelMaps.Clear();
                    _subLevelTextures.Clear();
                }

                //Add new parallax levels as needed
                for (int i = _subLevelMaps.Count; i < _parallax; i++)
                {
                    _fCount--;
                    _subLevelMaps.Add(null);
                    _subLevelTextures.Add(null);
                    GenerateLevel(g, i, level + i + 1, itemSeed, stairs);
                }
            }

            _curLevel = level;
        }

        public void Purge()
        {
            _curLevel = 0;
            _subLevelTextures.Clear();
            _subLevelMaps.Clear();
            Map = null;
            WorldTexture = null;
        }
        public override void Reset(EntityContainer entity)
        {
            base.Reset(entity);
            _subLevelTextures.Clear();
            _subLevelMaps.Clear();
            //_mobSet.Clear();
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
        }

        private void SetPlayerAnyTile(Random rand)
        {
            CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - CurrentWorldScale * TileSize * Map.GetAnyFloor(rand);
            CurrentWorldPosition.Y += 0.5f * CurrentWorldScale * TileSize; //Keeps players out of the wall (for some reason)
        }
        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = CurrentWorldScale;
            CurrentWorldScale = WorldRules.GetScale(gameWindow);

            if (PlayerOverlay != null)
            {
                var adjustedOffset = (CurrentWorldScale / oldScale)*(PlayerOverlay.PlayerCenterScreen - CurrentWorldPosition);
                PlayerOverlay.OnResize(graphics, gameWindow); //this is warped between these measurements purposefully
                CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - adjustedOffset;
            }
        }

        public void DrawDebugInfo(SpriteBatch spriteBatch, Coordinate pos)
        {
            var text = "\n\nSeed:" + _seed + " Floor:" + _curLevel;
            TextDrawer.DrawDumbText(spriteBatch, pos, text, 1, ColorScale.White);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i=_subLevelTextures.Count-1; i>=0; i--)
            {
                //Parallax -> offset from center of main map applied to sub maps with reduction
                var thing = CurrentWorldPosition - PlayerOverlay.PlayerCenterScreen;
                var value = 2.0f / (i + 3); //Reduction multiplier
                var pos = PlayerOverlay.PlayerCenterScreen + new Coordinate(value*thing.X, value*thing.Y);
                var color = new Color(value, value, value);
                var scale = value*CurrentWorldScale;
                spriteBatch.Draw(_subLevelTextures[i], (Vector2)pos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                if (D.Bug) LineDrawer.DrawSquare(spriteBatch, (Vector2)pos, scale*_subLevelTextures[i].Width, scale*_subLevelTextures[i].Height, ColorScale.White);
            }
            base.Draw(spriteBatch);
            if (D.Bug) LineDrawer.DrawSquare(spriteBatch, CurrentWorldPosition, CurrentWorldScale * WorldTexture.Width, CurrentWorldScale * WorldTexture.Height, ColorScale.White);
        }

        // **********Params and Commands*************

        public void ForceRemoveFromBox(int boxNum, int itemId)
        {
            Map.RemoveFromBox(boxNum, itemId);
        }

        public void ChangeParallax(int parallax)
        {
            _parallax = parallax;
            while (_subLevelMaps.Count > _parallax)
            {
                _subLevelMaps.RemoveAt(_subLevelMaps.Count - 1);
                _subLevelTextures.RemoveAt(_subLevelTextures.Count - 1);
            }
        }

        public void ChangeGenerationType(MapGenerator.Type type)
        {
            if (type == _genType) return;
            _genType = type;
            _subLevelMaps.Clear();
            _subLevelTextures.Clear();
        }

        public void ChangeTexturePack(string[] files)
        {
            _wallTextureMapFile = files[0];
            _floorTextureMapFile = files.Length > 1 ? files[1] : "";
            _otherTextureMapFile = files.Length > 2 ? files[2] : "";

            _wallTextureMapSet = TileTextureMap.GetTileTextureMap(GameContainer.Graphics, _wallTextureMapFile);
            TileSize = _wallTextureMapSet[0].TileSize;
            _floorTextureMapSet = _floorTextureMapFile.Length > 0 ? TileTextureMap.GetTileTextureMap(GameContainer.Graphics, _floorTextureMapFile) : null;
            _otherTextureMapSet = _otherTextureMapFile.Length > 0 ? TileTextureMap.GetTileTextureMap(GameContainer.Graphics, _otherTextureMapFile) : null;
        }

        public void ChangeSeed(int seed)
        {
            if (seed == _seed) return;
            _seed = seed;
            _subLevelMaps.Clear();
            _subLevelTextures.Clear();
        }

        public void PrintMap(bool all)
        {
            TextureUtilities.WriteTextureToPNG(WorldTexture, _genType+"-s"+_seed+"-f"+_curLevel+".png", "maps");
            if (all)
            {
                for (int i = 0; i < _subLevelTextures.Count; i++)
                {
                    TextureUtilities.WriteTextureToPNG(_subLevelTextures[i], _genType+"-s" + _seed + "-f" + (_curLevel+i+1) + ".png", "maps");
                }
            }
        }

        public void Pack(PackedGame game)
        {
            game.Floor = _curLevel;
            game.Seed = _seed;
            game.GenType = _genType;
            game.Parallax = _parallax;
            game.TexturePack = new[] {_wallTextureMapFile, _floorTextureMapFile, _otherTextureMapFile};
        }

        public void Unpack(PackedGame game)
        {
            _curLevel = game.Floor;
            _seed = game.Seed;
            _genType = game.GenType;
            _parallax = game.Parallax;
            _wallTextureMapFile = game.TexturePack[0];
            _floorTextureMapFile = game.TexturePack[1];
            _otherTextureMapFile = game.TexturePack[2];

            _wallTextureMapSet = TileTextureMap.GetTileTextureMap(GameContainer.Graphics, _wallTextureMapFile);
            TileSize = _wallTextureMapSet[0].TileSize;
            if (_floorTextureMapFile.Length > 0) _floorTextureMapSet = TileTextureMap.GetTileTextureMap(GameContainer.Graphics, _floorTextureMapFile);
            if (_otherTextureMapFile.Length > 0) _otherTextureMapSet = TileTextureMap.GetTileTextureMap(GameContainer.Graphics, _otherTextureMapFile);
        }
    }
}
