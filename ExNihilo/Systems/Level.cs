
using System;
using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Util;
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

        //private List<EntityContainer> _mobSet;
        private GraphicsDevice _graphics; //for stitching level maps
        private int _curLevel, _seed, _parallax;

        public Level(int tileSize) : base(tileSize)
        {
            _subLevelTextures = new List<Texture2D>();
            _subLevelMaps = new List<InteractionMap>();
            //_mobSet = new List<EntityContainer>();
            _curLevel = 1;
        }

        public void GenerateLevel(MapGenerator.Type type, int level=-1)
        {
            _curLevel = level == -1 ? _curLevel+1 : level;
            if (_subLevelMaps.Count > 0)
            {
                //Replace old level if already generated
                Map = _subLevelMaps[0];
                WorldTexture = _subLevelTextures[0];
                _subLevelMaps.RemoveAt(0);
                _subLevelTextures.RemoveAt(0);
            }
            else
            {
                //First time generation or zero parallax
                Map = new InteractionMap(new TypeMatrix(MapGenerator.Get(_seed, _curLevel, type)));
                WorldTexture = MapStitcher.StitchMap(_graphics, Map.Map, TileSize);
            }

            //Add new parallax levels as needed
            while (_subLevelMaps.Count < _parallax)
            {
                var floor = _curLevel + _subLevelMaps.Count + 1;
                var newMap = new InteractionMap(new TypeMatrix(MapGenerator.Get(_seed, floor, type)));
                _subLevelMaps.Add(newMap);
                _subLevelTextures.Add(MapStitcher.StitchMap(_graphics, newMap.Map, TileSize));
            }
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

        public void ChangeSeed(int seed)
        {
            _seed = seed;
            _subLevelMaps.Clear();
            _subLevelTextures.Clear();
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
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = CurrentWorldScale;
            CurrentWorldScale = WorldRules.GetScale(gameWindow);

            if (PlayerOverlay != null)
            {
                if (ResetWorldPos)
                {
                    ResetWorldPos = false;
                    PlayerOverlay.OnResize(graphics, gameWindow);
                    CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - CurrentWorldScale * TileSize * Map.GetAnyFloor(new Random());
                    CurrentWorldPosition.Y += 0.5f * CurrentWorldScale * TileSize; //TODO: less magic way to do this
                }
                else
                {
                    var adjustedOffset = (PlayerOverlay.PlayerCenterScreen - CurrentWorldPosition) * (CurrentWorldScale / oldScale);
                    PlayerOverlay.OnResize(graphics, gameWindow); //this is specifically warped between these measurements 
                    CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - adjustedOffset;
                }
            }
        }

        public void Update()
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //TODO: draw parallax here
            base.Draw(spriteBatch);
        }

        public override void DrawOverlays(SpriteBatch spriteBatch)
        {
            base.DrawOverlays(spriteBatch);
        }

        public override void ApplyPush(Coordinate push, float mult)
        {
            base.ApplyPush(push, mult);
        }
    }
}
