
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
        private List<Texture2D> _subLevelMaps;
        private List<EntityContainer> _mobSet;
        private GraphicsDevice _graphics; //for stitching level maps
        private int _curLevel;

        public Level(int tileSize) : base(tileSize)
        {
            _subLevelMaps = new List<Texture2D>();
            _mobSet = new List<EntityContainer>();
        }

        public void GenerateLevel(MapGenerator.Type type, int seed, int level)
        {
            Map = new InteractionMap(new TypeMatrix(MapGenerator.Get(seed, level, type)));
            WorldTexture = MapStitcher.StitchMap(_graphics, Map.Map, TileSize);
        }

        public override void Reset(EntityContainer entity, Coordinate hitBox, Coordinate hitBoxOffset)
        {
            base.Reset(entity, hitBox, hitBoxOffset);
        }

        public override void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _graphics = graphics;
        }

        public override void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            base.OnResize(graphics, gameWindow);
        }

        public void Update(GameTime gameTime)
        {
            //TODO: update _mobSet
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void DrawOverlays(SpriteBatch spriteBatch)
        {
            base.DrawOverlays(spriteBatch);
            //TODO: draw _mobSet
        }

        public override void ApplyPush(Coordinate push, float mult)
        {
            base.ApplyPush(push, mult);
        }
    }
}
