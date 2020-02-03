using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ExNihilo.Entity;
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
        private PlayerOverlay _playerOverlay;
        private readonly int _timerID, _tileSize;
        private float _currentWorldScale;
        private Coordinate _playerCustomHitBox, _playerCustomHitBoxOffset;
        private InteractionMap _map;
        private Texture2D _world;
        private Vector2 _currentWorldPosition;
        private bool _resetWorldPos;

        public World(int tileSize)
        {
            _resetWorldPos = true;
            _tileSize = tileSize;
            _currentWorldPosition = new Vector2();
            _currentWorldScale = 1;
            _overlays = new List<Tuple<AnimatableTexture, Vector2>>();
            _playerCustomHitBox = new Coordinate();
            _playerOverlay = null;
            _timerID = UniversalTime.NewTimer(false);
            UniversalTime.TurnOnTimer(_timerID);
        }

        public EntityTexture.State GetCurrentState()
        {
            return _playerOverlay.GetCurrentState();
        }
        public void Halt()
        {
            _playerOverlay.Halt();
        }
        public void SwapEntity(EntityContainer entity)
        {
            if (entity is null) return;
            if (_playerOverlay is null) _playerOverlay = new PlayerOverlay(entity);
            else _playerOverlay.ForceTexture(entity);
        }
        public void Reset(EntityContainer entity, Coordinate hitBox, Coordinate hitBoxOffset)
        {
            _resetWorldPos = true; //Screen size is required to calculate this which isn't available here
            _currentWorldPosition = new Vector2();
            UniversalTime.ResetTimer(_timerID);

            SwapEntity(entity);
            _playerCustomHitBox = hitBox.Copy();
            _playerCustomHitBoxOffset = hitBoxOffset.Copy();
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            //TODO: make this universal (what does universal mean?)
            _world = content.Load<Texture2D>("World/world");
            _map = new InteractionMap("WORLD.info");
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = _currentWorldScale;
            _currentWorldScale = _worldRules.GetScale(gameWindow);

            if (_playerOverlay != null)
            {
                if (_resetWorldPos)
                {
                    _resetWorldPos = false;
                    _playerOverlay.OnResize(graphics, gameWindow);
                    _currentWorldPosition = _playerOverlay.PlayerCenterScreen - _currentWorldScale * new Vector2(400, 200);
                }
                else
                {
                    var adjustedOffset = (_playerOverlay.PlayerCenterScreen - _currentWorldPosition) * (_currentWorldScale / oldScale);
                    _playerOverlay.OnResize(graphics, gameWindow); //this is specifically warped between these measurements 
                    _currentWorldPosition = _playerOverlay.PlayerCenterScreen - adjustedOffset;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_world, _currentWorldPosition, null, Color.White, 0, Vector2.Zero, _currentWorldScale, SpriteEffects.None, 0);
        }

        public void DrawOverlays(SpriteBatch spriteBatch)
        {
            _playerOverlay?.Draw(spriteBatch);
            foreach (var item in _overlays)
            {
                var pos = _currentWorldPosition + _currentWorldScale * item.Item2;
                item.Item1.Draw(spriteBatch, pos, ColorScale.White, _currentWorldScale);
            }

            if (GameContainer.GLOBAL_DEBUG && _playerOverlay != null)
            {
                LineDrawer.DrawSquare(spriteBatch, _currentWorldScale * _playerCustomHitBoxOffset + _playerOverlay.PlayerCenterScreen, _currentWorldScale * _playerCustomHitBox.X, _currentWorldScale * _playerCustomHitBox.Y, Color.Red);
            }
        }

        public void ApplyPush(Coordinate push, float mult)
        {
            //check map and move
            _playerOverlay.Push(push);
            if (push.Origin()) return;

            var tileSize = (int) (_currentWorldScale * _tileSize);
            var moveOffset = 50 * _currentWorldScale * mult * (float)UniversalTime.GetLastTickTime(_timerID) * push; //offset from current position 
            var hitBox = _currentWorldScale * _playerCustomHitBox;
            var hitBoxOffset = _currentWorldScale * _playerCustomHitBoxOffset + _playerOverlay.PlayerCenterScreen - _currentWorldPosition;
            var hitBoxMovedOffset = hitBoxOffset + moveOffset;

            if (_map.CheckIllegalPosition(tileSize, hitBox, hitBoxMovedOffset))
            {
                //being here implies that applying the current push failed (player is hitting something)
                if (push.X == 0 ^ push.Y == 0)
                {
                    //If push is only in one direction and fails that means movement is impossible (player is running into a wall)
                    return;
                }

                if (_map.CheckIllegalPosition(tileSize, hitBox, new Vector2(hitBoxMovedOffset.X, hitBoxOffset.Y)))
                {
                    //being here implies that the current X push is impossible
                    moveOffset.X = 0;
                    if (_map.CheckIllegalPosition(tileSize, hitBox, hitBoxOffset + moveOffset))
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
            var offset = _playerOverlay.PlayerCenterScreen - _currentWorldPosition +
                         TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, _playerCustomHitBox);
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

        public override string ToString()
        {
            return "World Pos: X:" + _currentWorldPosition.X.ToString("0.00") + " Y:" + _currentWorldPosition.Y.ToString("0.00");
        }

    }
}
