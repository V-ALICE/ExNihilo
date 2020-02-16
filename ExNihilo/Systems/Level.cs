
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ExNihilo.Entity;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    /*
     * The top level container for the main game levels
     * Controls the generation display and interaction of the game world
     * For the outerworld level, see the World class
     */
    public class Level : World
    {
        private List<Texture2D> _subLevelTextures;
        private List<InteractionMap> _subLevelMaps;
        private TileTextureMap[] _textureMapSet;

        //private List<EntityContainer> _mobSet;
        private GraphicsDevice _graphics; //for stitching level maps
        private int _curLevel, _seed=1, _parallax=0, _mapSize=128;
        private MapGenerator.Type _genType = MapGenerator.Type.Standard2;
        private string _textureMapFile = "Content/TexturePacks/DawnHackBrickComplete.tmf";
        
        private int _fCount;
        private object _fLock = new object();

        public Level() : base(0)
        {
            _subLevelTextures = new List<Texture2D>();
            _subLevelMaps = new List<InteractionMap>();
            //_mobSet = new List<EntityContainer>();
        }

        private async void GenerateLevel(GameContainer g, int slot, int level)
        {
            Tuple<InteractionMap, Texture2D> DoAll()
            {
                var m = new InteractionMap(new TypeMatrix(MapGenerator.Get(_seed, level, _genType, _mapSize, out var rand)));
                var t = MapStitcher.StitchMap(_graphics, m.Map, rand, _textureMapSet[rand.Next(_textureMapSet.Length)]);
                return Tuple.Create(m, t);
            }
            var levelSet = await Task.Run(() => DoAll());

            if (slot == -1)
            {
                Map = levelSet.Item1;
                WorldTexture = levelSet.Item2;
                SetPlayerAnyTile();
            }
            else
            {
                _subLevelMaps[slot] = levelSet.Item1;
                _subLevelTextures[slot] = levelSet.Item2;
            }

            lock (_fLock)
            {
                if (++_fCount >= 1 + _parallax)
                {
                    g.RequestSectorChange(GameContainer.SectorID.Underworld);
                }
            }
        }

        public void DoGenerationQueue(GameContainer g, int level)
        {
            if (level == -1) level = _curLevel;
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
                    GenerateLevel(g, -1, level);
                    _subLevelMaps.Clear();
                    _subLevelTextures.Clear();
                }

                //Add new parallax levels as needed
                for (int i = _subLevelMaps.Count; i < _parallax; i++)
                {
                    _fCount--;
                    _subLevelMaps.Add(null);
                    _subLevelTextures.Add(null);
                    GenerateLevel(g, i, level + i + 1);
                }
            }

            _curLevel = level;
        }

        public void ChangeMapSize(int size)
        {
            if (size == _mapSize) return;
            _mapSize = size;
            _subLevelMaps.Clear();
            _subLevelTextures.Clear();
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

        public void ChangeSeed(int seed)
        {
            if (seed == _seed) return;
            _seed = seed;
            _subLevelMaps.Clear();
            _subLevelTextures.Clear();
        }
        public int GetSeed()
        {
            return _seed;
        }

        public void Purge()
        {
            _subLevelTextures.Clear();
            _subLevelMaps.Clear();
            Map = null;
            WorldTexture = null;
        }
        public override void Reset(EntityContainer entity, Coordinate hitBox, Coordinate hitBoxOffset)
        {
            base.Reset(entity, hitBox, hitBoxOffset);
            _subLevelTextures.Clear();
            _subLevelMaps.Clear();
            //_mobSet.Clear();
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _graphics = graphics;
            _textureMapSet = TileTextureMap.GetTileTextureMap(graphics, _textureMapFile);
            TileSize = _textureMapSet[0].TileSize;
        }

        private void SetPlayerAnyTile()
        {
            CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - CurrentWorldScale * TileSize * Map.GetAnyFloor(MathD.urand);
            CurrentWorldPosition.Y += 0.5f * CurrentWorldScale * TileSize; //TODO: less magic way to do this
        }
        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = CurrentWorldScale;
            CurrentWorldScale = WorldRules.GetScale(gameWindow);

            if (PlayerOverlay != null)
            {
                var adjustedOffset = (PlayerOverlay.PlayerCenterScreen - CurrentWorldPosition) * (CurrentWorldScale / oldScale);
                PlayerOverlay.OnResize(graphics, gameWindow); //this is warped between these measurements purposefully
                CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - adjustedOffset;
            }
        }

        public void Update()
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i=_subLevelTextures.Count-1; i>=0; i--)
            {
                //Parallax -> offset from center of main map applied to sub maps with reduction
                var pos = PlayerOverlay.PlayerCenterScreen + (CurrentWorldPosition - PlayerOverlay.PlayerCenterScreen) / (i+2);
                var value = 1.0f/(i+2);
                var color = new Color(value, value, value);
                var scale = CurrentWorldScale/(i+2);
                spriteBatch.Draw(_subLevelTextures[i], pos, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                if (D.Bug) LineDrawer.DrawSquare(spriteBatch, pos, scale*_subLevelTextures[i].Width, scale*_subLevelTextures[i].Height, ColorScale.White);
            }
            base.Draw(spriteBatch);
            if (D.Bug) LineDrawer.DrawSquare(spriteBatch, CurrentWorldPosition, CurrentWorldScale * WorldTexture.Width, CurrentWorldScale * WorldTexture.Height, ColorScale.White);
        }

        public override void DrawOverlays(SpriteBatch spriteBatch)
        {
            base.DrawOverlays(spriteBatch);
        }

        public override void ApplyPush(Coordinate push, float mult, bool ignoreWalls=false)
        {
            base.ApplyPush(push, mult, ignoreWalls);
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
    }
}
