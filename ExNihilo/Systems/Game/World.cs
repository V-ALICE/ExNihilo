using System;
using System.Collections.Generic;
using System.Linq;
using ExNihilo.Entity;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Backend.Network;
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
        private string[] _shipMap = {
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXOOXXXXXX",
            "XXXXXOOOOXXXXX",
            "XXXOOOOOOOOXXX",
            "XXXOOOOOOOOXXX",
            "XXXOOOOOOOOXXX",
            "XXXOOOOOOOOXXX",
            "XXXXXOOXXXXXXX",
            "XXXXXOOXXXXXXX",
            "XXXOOOOOOOXXXX",
            "XXXOOOOOOOOXXX",
            "XXXOOOOOOOOXXX",
            "XXXXXOOXOOOXXX",
            "XXXXXOOXXXXXXX",
            "XXXXXOOXXXXXXX",
            "XXXOOOOOOOOXXX",
            "XXXOOOOOOOOXXX",
            "XXXOOOXXOOOXXX",
            "XXXXOOXXOOXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX",
            "XXXXXXXXXXXXXX"
        };

        protected readonly List<Tuple<AnimatableTexture, Coordinate>> Overlays;
        protected readonly ScaleRuleSet WorldRules = TextureLibrary.DefaultScaleRuleSet;
        protected readonly int TimerID;
        protected int TileSize;
        protected float CurrentWorldScale;
        protected Coordinate PlayerCustomHitBox, PlayerCustomHitBoxOffset, LastWindowSize;
        protected InteractionMap Map;
        protected Texture2D WorldTexture;
        protected Vector2 CurrentWorldPosition;
        protected bool ResetWorldPos;

        protected PlayerOverlay PlayerOverlay;
        protected readonly List<PlayerOverlay> OtherPlayerOverlays;

        public World(int tileSize)
        {
            ResetWorldPos = true;
            TileSize = tileSize;
            CurrentWorldPosition = new Vector2();
            CurrentWorldScale = 1;
            Overlays = new List<Tuple<AnimatableTexture, Coordinate>>();
            PlayerCustomHitBox = new Coordinate();
            PlayerOverlay = null;
            OtherPlayerOverlays = new List<PlayerOverlay>();
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
            if (PlayerOverlay is null) PlayerOverlay = new PlayerOverlay(entity, true);
            else PlayerOverlay.ForceEntityRef(entity);
        }
        public virtual void Reset(EntityContainer entity)
        {
            ResetWorldPos = true; //Screen size is required to calculate this which isn't available here
            CurrentWorldPosition = new Vector2();
            UniversalTime.ResetTimer(TimerID);

            SwapEntity(entity);
            var size = new Coordinate(entity.Texture.Width, entity.Texture.Height);
            PlayerCustomHitBox = new Coordinate(0.6f * size.X, 0.6f * size.Y / 2);
            PlayerCustomHitBoxOffset = new Coordinate(0.2f * size.X, 0.2f * size.Y + size.Y / 2f);
        }

        public virtual void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            WorldTexture = content.Load<Texture2D>("World/ship_base");
            Map = new InteractionMap(_shipMap);
        }

        public virtual void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            var oldScale = CurrentWorldScale;
            CurrentWorldScale = WorldRules.GetScale(gameWindow);
            LastWindowSize = gameWindow;

            if (PlayerOverlay != null)
            {
                if (ResetWorldPos)
                {
                    ResetWorldPos = false;
                    PlayerOverlay.OnResize(graphics, gameWindow);
                    CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - new Vector2(CurrentWorldScale*104, CurrentWorldScale*232);
                }
                else
                {
                    var adjustedOffset = (CurrentWorldScale / oldScale) * (PlayerOverlay.PlayerCenterScreen - CurrentWorldPosition);
                    PlayerOverlay.OnResize(graphics, gameWindow); //this is specifically warped between these measurements 
                    CurrentWorldPosition = PlayerOverlay.PlayerCenterScreen - adjustedOffset;
                }
            }

            foreach (var player in OtherPlayerOverlays)
            {
                player.OnResize(graphics, gameWindow);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(WorldTexture, MathD.Flatten(CurrentWorldPosition), null, Color.Gray, 0, Vector2.Zero, CurrentWorldScale, SpriteEffects.None, 0);
        }

        private void DrawPlayersInOrder(SpriteBatch spriteBatch)
        {
            var positions = new List<Tuple<int, Vector2>> {Tuple.Create(-1, (Vector2) PlayerOverlay.PlayerCenterScreen)};
            for (int i = 0; i < OtherPlayerOverlays.Count; i++)
            {
                var pos = MathD.Flatten(CurrentWorldPosition) + CurrentWorldScale * OtherPlayerOverlays[i].PlayerCustomWorldPos;
                for (int j = 0; j <= positions.Count; j++)
                {
                    if (j == positions.Count)
                    {
                        positions.Add(Tuple.Create(i, pos));
                        break;
                    }
                    if (pos.Y < positions[j].Item2.Y)
                    {
                        positions.Insert(j, Tuple.Create(i, pos));
                        break;
                    }
                }
            }

            foreach (var item in positions)
            {
                if (item.Item1 == -1) PlayerOverlay.Draw(spriteBatch);
                else OtherPlayerOverlays[item.Item1].Draw(spriteBatch, item.Item2, PlayerOverlay.Scale);
            }
        }

        public virtual void DrawOverlays(SpriteBatch spriteBatch)
        {
            Map.DrawBoxes(spriteBatch, MathD.Flatten(CurrentWorldPosition), TileSize, CurrentWorldScale);
            if (PlayerOverlay is null) return;
            
            DrawPlayersInOrder(spriteBatch);

            foreach (var (tex, pos) in Overlays)
            {
                tex.Draw(spriteBatch, MathD.Flatten(CurrentWorldPosition) + CurrentWorldScale * pos, ColorScale.Grey, CurrentWorldScale);
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
            var offset = PlayerOverlay.PlayerCenterScreen + TextureUtilities.GetOffset(TextureUtilities.PositionType.Center, PlayerOverlay.GetCurrentPixelSize()) - CurrentWorldPosition;
            return Map.GetInteractive((int) (CurrentWorldScale * TileSize), offset, 0.25f, 1.5f, 0.75f, 0.75f);
        }

        public void ClearPlayers()
        {
            OtherPlayerOverlays.Clear();
        }
        public List<PlayerOverlay> GetPlayers()
        {
            return OtherPlayerOverlays;
        }

        public void AddPlayerRef(PlayerOverlay player)
        {
            OtherPlayerOverlays.Add(player);
        }
        public void AddPlayer(long id, string name, string set, int index)
        {
            var tex = TextureLibrary.CharLookup(set, index);
            var instance = OtherPlayerOverlays.FirstOrDefault(p => p.ID == id);
            if (instance is null)
            {
                OtherPlayerOverlays.Add(new PlayerOverlay(new EntityContainer(GameContainer.Graphics, name, tex), false, id));
                OtherPlayerOverlays[OtherPlayerOverlays.Count-1].OnResize(GameContainer.Graphics, LastWindowSize);
            }
            else
            {
                instance.ForceEntityRef(new EntityContainer(GameContainer.Graphics, name, tex));
            }
        }
        public void RemovePlayer(long id)
        {
            var player = OtherPlayerOverlays.FirstOrDefault(p => p.ID == id);
            if (player != null) OtherPlayerOverlays.Remove(player);
        }

        public StandardUpdate GetStandardUpdate()
        {
            var offset = (PlayerOverlay.PlayerCenterScreen - CurrentWorldPosition) / CurrentWorldScale;
            return new StandardUpdate(NetworkManager.MyUniqueID, offset.X, offset.Y, (sbyte) PlayerOverlay.GetCurrentState());
        }

        public void AddOverlay(AnimatableTexture texture, int x, int y)
        {
            var pos = new Coordinate(x * TileSize, y * TileSize);
            Overlays.Add(new Tuple<AnimatableTexture, Coordinate>(texture, pos));
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
