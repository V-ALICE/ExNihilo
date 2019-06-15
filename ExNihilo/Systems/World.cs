using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    public class World : IUI
    {
        private readonly List<Tuple<AnimatableTexture, Vector2>> _overlays;
        private readonly ScaleRuleSet _worldRules = TextureLibrary.DefaultScaleRuleSet;
        private readonly PlayerOverlay _playerOverlay;
        private readonly int _timerID, _tileSize;
        private float _currentWorldScale;
        private Coordinate _playerCustomHitBox;
        private InteractionMap _map;
        private Texture2D _world;
        private Vector2 _currentWorldPosition;

        public World(int tileSize)
        {
            _tileSize = tileSize;
            _currentWorldPosition = new Vector2(-200, -350);
            _currentWorldScale = 1;
            _overlays = new List<Tuple<AnimatableTexture, Vector2>>();
            _playerCustomHitBox = new Coordinate();
            _playerOverlay = new PlayerOverlay("null");
            _timerID = UniversalTime.NewTimer(false);
            UniversalTime.TurnOnTimer(_timerID);
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _world = content.Load<Texture2D>("World/world");
            _map = new InteractionMap("WORLD.info");
            _playerOverlay.LoadContent(graphics, content);
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = _currentWorldScale;
            _currentWorldScale = _worldRules.GetScale(gameWindow);
            var adjustedOffset = (_playerOverlay.PlayerCenterScreen - _currentWorldPosition) * (_currentWorldScale / oldScale);
            _playerOverlay.OnResize(graphics, gameWindow);
            _playerCustomHitBox = _playerOverlay.GetCurrentDimensions();
            _currentWorldPosition = _playerOverlay.PlayerCenterScreen - adjustedOffset;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_world, _currentWorldPosition, null, Color.White, 0, Vector2.Zero, _currentWorldScale, SpriteEffects.None, 0);
        }

        public void DrawOverlays(SpriteBatch spriteBatch)
        {
            _playerOverlay.Draw(spriteBatch);
            foreach (var item in _overlays)
            {
                var pos = _currentWorldPosition + _currentWorldScale * item.Item2;
                item.Item1.Draw(spriteBatch, pos, ColorScale.White, _currentWorldScale);
            }
        }

        public void ApplyPush(Coordinate push, float mult)
        {
            //check map and move
            var tileSize = (int) (_currentWorldScale * _tileSize);
            var moveOffset = 50 * _currentWorldScale * mult * (float)UniversalTime.GetLastTickTime(_timerID) * push; //offset from current position 
            var playerOffset = _playerOverlay.PlayerCenterScreen - (_currentWorldPosition - moveOffset); //adjusted position using offset

            if (_map.CheckIllegalPosition(tileSize, _playerCustomHitBox, playerOffset))
            {
                //being here implies that applying the current push failed (player is hitting something)
                if (push.X == 0 ^ push.Y == 0)
                {
                    //If push is only in one direction and fails that means movement is impossible (player is running into a wall)
                    return;
                }

                var playerOffsetX = new Vector2(_playerOverlay.PlayerCenterScreen.X - (_currentWorldPosition.X - moveOffset.X), _playerOverlay.PlayerCenterScreen.Y - _currentWorldPosition.Y);
                if (_map.CheckIllegalPosition(tileSize, _playerCustomHitBox, playerOffsetX))
                {
                    //being here implies that the current X push is impossible
                    moveOffset.X = 0;
                    playerOffset = _playerOverlay.PlayerCenterScreen - (_currentWorldPosition - moveOffset); //using the adjusted X value
                    if (_map.CheckIllegalPosition(tileSize, _playerCustomHitBox, playerOffset))
                    {
                        //being here implies that even stripped of the X push the current Y push is impossible (player is running into a corner)
                        moveOffset.Y = 0;
                    }
                }
                else
                {
                    //being here implies that movement with X+Y is impossible but is possible with only X, therefore Y is booted (player is clipping a corner)
                    moveOffset.Y = 0;
                }
            }

            _currentWorldPosition -= moveOffset;
        }

        public Interactive CheckForInteraction()
        {
            var offset = _playerOverlay.PlayerCenterScreen - _currentWorldPosition;
            return _map.GetInteractive((int) (_currentWorldScale * _tileSize), offset, 1);
        }

        public void AddOverlay(AnimatableTexture texture, int x, int y)
        {
            var pos = new Vector2(x * _tileSize, y * _tileSize);
            _overlays.Add(new Tuple<AnimatableTexture, Vector2>(texture, pos));
        }
        public void AddInteractive(Interactive obj, int x, int y, int width=1, int height=1)
        {
            _map.AddInteractive(obj, x, y, width, height);
        }

        public void ForcePlayerTexture(AnimatableTexture texture)
        {
            _playerOverlay.ForceTexture(texture);
            _playerCustomHitBox = _playerOverlay.GetCurrentDimensions();
        }

        public override string ToString()
        {
            return "World Pos: X:" + _currentWorldPosition.X + " Y:" + _currentWorldPosition.Y;
        }
    }
}
