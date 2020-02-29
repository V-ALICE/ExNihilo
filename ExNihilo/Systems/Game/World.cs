using System;
using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game
{
    public class World : IUI
    {
        protected readonly List<Tuple<AnimatableTexture, Vector2>> Overlays;
        protected readonly ScaleRuleSet WorldRules = TextureLibrary.DefaultScaleRuleSet;
        protected PlayerOverlay PlayerOverlay;
        protected readonly int TimerID;
        protected int TileSize;
        protected float CurrentWorldScale;
        protected Coordinate PlayerCustomHitBox, PlayerCustomHitBoxOffset;
        protected InteractionMap Map;
        protected Texture2D WorldTexture;
        protected Vector2 CurrentWorldPosition;
        protected bool ResetWorldPos;

        public World(int tileSize)
        {
            ResetWorldPos = true;
            TileSize = tileSize;
            CurrentWorldPosition = new Vector2();
            CurrentWorldScale = 1;
            Overlays = new List<Tuple<AnimatableTexture, Vector2>>();
            PlayerCustomHitBox = new Coordinate();
            PlayerOverlay = null;
            TimerID = UniversalTime.NewTimer(false);
            UniversalTime.TurnOnTimer(TimerID);
        }

        public EntityTexture.State GetCurrentState()
        {
            return PlayerOverlay.GetCurrentState();
        }
        public void Halt()
        {
            PlayerOverlay.Halt();
        }
        public void SwapEntity(EntityContainer entity)
        {
            if (entity is null) return;
            if (PlayerOverlay is null) PlayerOverlay = new PlayerOverlay(entity);
            else PlayerOverlay.ForceTexture(entity);
        }
        public virtual void Reset(EntityContainer entity, Coordinate hitBox, Coordinate hitBoxOffset)
        {
            ResetWorldPos = true; //Screen size is required to calculate this which isn't available here
            CurrentWorldPosition = new Vector2();
            UniversalTime.ResetTimer(TimerID);

            SwapEntity(entity);
            PlayerCustomHitBox = hitBox.Copy();
            PlayerCustomHitBoxOffset = hitBoxOffset.Copy();
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            WorldTexture = content.Load<Texture2D>("World/world");
            Map = new InteractionMap("WORLD.info");
        }

        public virtual void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = CurrentWorldScale;
            CurrentWorldScale = WorldRules.GetScale(gameWindow);

            if (PlayerOverlay != null)
            {
                if (ResetWorldPos)
                {
                    ResetWorldPos = false;
                    PlayerOverlay.OnResize(graphics, gameWindow);
                    CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - new Vector2(CurrentWorldScale*400, CurrentWorldScale*200);
                }
                else
                {
                    var adjustedOffset = (CurrentWorldScale / oldScale) * (PlayerOverlay.PlayerCenterScreen - CurrentWorldPosition);
                    PlayerOverlay.OnResize(graphics, gameWindow); //this is specifically warped between these measurements 
                    CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - adjustedOffset;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(WorldTexture, CurrentWorldPosition, null, Color.White, 0, Vector2.Zero, CurrentWorldScale, SpriteEffects.None, 0);
        }

        public virtual void DrawOverlays(SpriteBatch spriteBatch)
        {
            PlayerOverlay?.Draw(spriteBatch);
            foreach (var item in Overlays)
            {
                var pos = CurrentWorldPosition + CurrentWorldScale * item.Item2;
                item.Item1.Draw(spriteBatch, pos, ColorScale.White, CurrentWorldScale);
            }

            if (D.Bug && PlayerOverlay != null)
            {
                LineDrawer.DrawSquare(spriteBatch, PlayerOverlay.PlayerCenterScreen + CurrentWorldScale * PlayerCustomHitBoxOffset, CurrentWorldScale * PlayerCustomHitBox.X, CurrentWorldScale * PlayerCustomHitBox.Y, Color.Red);
            }
        }

        public virtual void ApplyPush(Coordinate push, float mult, bool ignoreWalls=false)
        {
            //check map and move
            PlayerOverlay.Push(push);
            if (push.Origin()) return;

            var tileSize = (int) (CurrentWorldScale * TileSize);
            var moveOffset = 50 * CurrentWorldScale * mult * (float)UniversalTime.GetLastTickTime(TimerID) * push; //offset from current position 
            var hitBox = (Coordinate)(CurrentWorldScale * PlayerCustomHitBox);
            var hitBoxOffset = PlayerOverlay.PlayerCenterScreen + CurrentWorldScale * PlayerCustomHitBoxOffset - CurrentWorldPosition;
            var hitBoxMovedOffset = hitBoxOffset + moveOffset;

            if (!ignoreWalls && Map.CheckIllegalPosition(tileSize, hitBox, hitBoxMovedOffset))
            {
                //being here implies that applying the current push failed (player is hitting something)
                if (push.X == 0 ^ push.Y == 0)
                {
                    //If push is only in one direction and fails that means movement is impossible (player is running into a wall)
                    return;
                }

                if (Map.CheckIllegalPosition(tileSize, hitBox, new Vector2(hitBoxMovedOffset.X, hitBoxOffset.Y)))
                {
                    //being here implies that the current X push is impossible
                    moveOffset.X = 0;
                    if (Map.CheckIllegalPosition(tileSize, hitBox, hitBoxOffset + moveOffset))
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

            CurrentWorldPosition -= moveOffset;
        }

        public Interactive CheckForInteraction()
        {
            var offset = PlayerOverlay.PlayerCenterScreen + TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, PlayerCustomHitBox) - CurrentWorldPosition;
            return Map.GetInteractive((int) (CurrentWorldScale * TileSize), offset, 1);
        }

        public void AddOverlay(AnimatableTexture texture, int x, int y)
        {
            var pos = new Vector2(x * TileSize, y * TileSize);
            Overlays.Add(new Tuple<AnimatableTexture, Vector2>(texture, pos));
        }
        public void AddInteractive(Interactive obj, int x, int y, int width=1, int height=1)
        {
            Map.AddInteractive(obj, x, y, width, height);
        }

        public override string ToString()
        {
            return "World Pos: X:" + CurrentWorldPosition.X.ToString("0.00") + " Y:" + CurrentWorldPosition.Y.ToString("0.00");
        }

    }
}
