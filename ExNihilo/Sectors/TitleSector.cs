using ExNihilo.Input.Commands;
using ExNihilo.Menus;
using ExNihilo.Systems;
using ExNihilo.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Sectors
{
    public class TitleSector : Sector
    {
        private TitleMenu _title;

        public TitleSector(GameContainer container) : base(container)
        {
        }

/********************************************************************
------->Game loop
********************************************************************/
        public override void OnResize(GraphicsDevice graphicsDevice, Coordinate gameWindow)
        {
            _title.OnResize(graphicsDevice, gameWindow);
        }

        public override void Enter()
        {
            _title.Enter();
        }

        public override void Initialize()
        {
            Handler = new CommandHandler();
            Handler.Initialize(this);
            _title = new TitleMenu(Container);
        }

        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _title.LoadContent(graphicsDevice, content);
        }

        public override void Update()
        {
            if (!TypingKeyboard.Active) Handler.UpdateInput();
        }

        protected override void DrawDebugInfo()
        {
        }

        public override void Draw(SpriteBatch spriteBatch, bool drawDebugInfo)
        {
            if (drawDebugInfo) DrawDebugInfo();
            _title.Draw(spriteBatch);
        }

/********************************************************************
------->Game functions
********************************************************************/
        protected override void Exit()
        {
        }

        public override void BackOut()
        {
            if (_title.BackOut()) RequestSectorChange(Container.PreviousSectorID);
        }
        public override void ReceiveCommand(Menu.MenuCommand command)
        {
            _title.ReceiveCommand(command);
        }

        public override void OnMoveMouse(Point point)
        {
            _title.OnMoveMouse(point);
        }

        public override bool OnLeftClick(Point point)
        {
            return _title.OnLeftClick(point);
        }

        public override void OnLeftRelease(Point point)
        {
            _title.OnLeftRelease(point);
        }

        public override void Pack(PackedGame game)
        {
        }

        public override void Unpack(PackedGame game)
        {
        }
    }
}
