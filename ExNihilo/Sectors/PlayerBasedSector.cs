using System.Collections.Generic;
using ExNihilo.Entity;
using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.Systems.Bases;
using ExNihilo.Systems.Game;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public abstract class PlayerBasedSector : Sector, ISuperPlayer, IPlayer
    {
        protected CommandHandler PlayerHandler, SuperPlayerHandler;
        protected Coordinate DebugPosition;
        protected Point LastMousePosition;
        protected readonly World World;
        protected bool DisableCollisions;
        protected float SystemPushSpeed = 1.0f;

        protected bool MenuActive => MenuPoint != null;
        protected Menu MenuPoint;

        protected InventoryMenu InvRef;
        protected NoteMenu StairMenu;

        public List<PlayerOverlay> OtherPlayers => World.GetPlayers();

        protected PlayerBasedSector(GameContainer container, World world) : base(container)
        {
            World = world;
            StairMenu = new NoteMenu(container, "Would you like to descend?", OnDown);
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            World?.OnResize(graphicsDevice, gameWindow);
            DebugPosition = new Coordinate(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            MenuPoint?.OnResize(graphicsDevice, gameWindow);
            if (!(MenuPoint is InventoryMenu)) InvRef.OnResize(graphicsDevice, gameWindow);
            if (!ReferenceEquals(MenuPoint, StairMenu)) StairMenu.OnResize(graphicsDevice, gameWindow);
        }

        public override void Enter(Point point, Coordinate gameWindow)
        {
            OnResize(Container.GraphicsDevice, gameWindow);
        }

        public override void Initialize()
        {
            InvRef = new InventoryMenu(Container, () => { MenuPoint = null;});
            PlayerHandler = new CommandHandler();
            PlayerHandler.InitializePlayer(this);
            SuperPlayerHandler = new CommandHandler();
            SuperPlayerHandler.InitializeSuperPlayer(this);
            DebugPosition = new Coordinate(1, 1 + TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
            LastMousePosition = new Point();
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            World.LoadContent(graphicsDevice, content);
            InvRef.LoadContent(graphicsDevice, content);
            StairMenu.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            if (TypingKeyboard.Active) return;
            SuperPlayerHandler.UpdateInput();
            if (!MenuActive)
            {
                World.ApplyPush(CurrentPush, SystemPushSpeed*CurrentPushMult, DisableCollisions);
                PlayerHandler.UpdateInput();
            }
        }

        protected override void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            var text = World + "\nPush Vec:  " + CurrentPush;
            TextDrawer.DrawDumbText(spriteBatch, DebugPosition, text, 1, ColorScale.White);
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {  
            World.Draw(spriteBatch);
            World.DrawOverlays(spriteBatch);
            MenuPoint?.Draw(spriteBatch);
            if (drawDebugInfo) DrawDebugInfo(spriteBatch);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public override void BackOut()
        {
            if (MenuActive)
            {
                MenuPoint.BackOut();
            }
            else base.BackOut();
        }

        public virtual void CheckNetwork(bool ending)
        {
            StairMenu.UpdateConfirm(NetworkManager.Active && !NetworkManager.Hosting && !ending);
        }

        private void OnDown(bool accepted)
        {
            MenuPoint = null;
            if (accepted)
            {
                Container.PushVoid(VoidSector.Seed, MathD.urand.Next(), 0);
            }
        }

        public override void ToggleTabMenu()
        {
            if (!MenuActive)
            {
                MenuPoint = InvRef;
                InvRef.Enter(LastMousePosition);
                if (MenuActive) World.Halt();
            }
            else if (MenuPoint is InventoryMenu)
            {
                MenuPoint.BackOut();
            }
        }

        public override bool OnMoveMouse(Point point)
        {
            MenuPoint?.OnMoveMouse(point);
            LastMousePosition = point;
            return false;
        }

        public override bool OnLeftClick(Point point)
        {
            return MenuPoint?.OnLeftClick(point) ?? false;
        }

        public override void OnLeftRelease(Point point)
        {
            if (MenuActive)
            {
                MenuPoint.OnLeftRelease(point);
            }
        }

        public override void Touch()
        {
            var obj = World.CheckForInteraction();
            if (obj != null)
            {
                switch (obj)
                {
                    case BoxInteractive box:
                        MenuPoint = box.Access();
                        BoxMenu.Menu.Enter(LastMousePosition, box, box.Index, () => { MenuPoint = null;});
                        if (MenuActive) World.Halt();
                        break;
                    case MenuInteractive menu:
                        MenuPoint = menu.Access();
                        MenuPoint?.Enter(LastMousePosition);
                        if (MenuActive) World.Halt();
                        break;
                    case ActionInteractive action:
                        action.Function?.Invoke();
                        break;
                }
            }
        }

        public virtual void ClearPlayers() { World.ClearPlayers(); }
        public virtual void ClearPlayers(long id) { World.RemovePlayer(id); }
        public virtual void UpdatePlayers(long id, string name, int[] charSet)
        {
            World.AddPlayer(id, name, charSet);
        }

        public StandardUpdate GetStandardUpdate()
        {
            return World.GetStandardUpdate();
        }

        public void ToggleCollisions(bool collisionOn)
        {
            DisableCollisions = !collisionOn;
        }

        public void SetSpeedMultiplier(float mult)
        {
            SystemPushSpeed = mult;
        }
    }
}
