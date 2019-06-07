using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ExNihilo.Input.Commands;
using ExNihilo.Input.Controllers;
using ExNihilo.Sectors;
using ExNihilo.Systems;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace ExNihilo
{
    public class GameContainer : Game
    {
        public enum SectorID
        {
            MainMenu, Outerworld, Underworld,
            Unloading, LoadingInit, Loading2Under, Loading2Outer, 
        }

        private SectorID _activeSectorID;
        private Sector ActiveSector => _sectorDirectory[_activeSectorID];
        private FormWindowState _currentForm;
        private float _mouseScale;
        private int _frameTimeID;
        private int _currentFrameCount, _currentFrameRate;
        private readonly GraphicsDeviceManager _graphics;
        private Coordinate _windowSize;
        private Vector2 _mouseDrawPos;
        private Dictionary<SectorID, Sector> _sectorDirectory;
        private Form _form;
        private Texture2D _mouseTexture;
        private MouseController _mouse;
        private CommandHandler _handler;

        public ConsoleHandler Console { get; private set; }

        protected bool ShowDebugInfo, FormTouched;
        protected bool ConsoleActive => Console.Active;
        protected int SystemClockID;
        protected SpriteBatch SpriteBatch;

        public GameContainer()
        {
            _graphics = new GraphicsDeviceManager(this) { SynchronizeWithVerticalRetrace = true };
            IsFixedTimeStep = false;
            _windowSize = new Coordinate(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        }

/********************************************************************
------->Window functions
********************************************************************/
        private void CheckForWindowUpdate()
        {
            if (Window.ClientBounds.Width != _windowSize.X || Window.ClientBounds.Height != _windowSize.Y)
            {
                _graphics.PreferredBackBufferWidth = MathHelper.Clamp(Window.ClientBounds.Width, ScaleRule.MIN_X, ScaleRule.MAX_X);
                _graphics.PreferredBackBufferHeight = MathHelper.Clamp(Window.ClientBounds.Height, ScaleRule.MIN_Y, ScaleRule.MAX_Y);
                _graphics.ApplyChanges();
                _windowSize = new Coordinate(Window.ClientBounds.Width, Window.ClientBounds.Height);

                Console.OnResize(GraphicsDevice, _windowSize);
                _mouseScale = UILibrary.DefaultScaleRuleSet.GetScale(_windowSize);
                foreach (var sector in _sectorDirectory.Values) sector.OnResize(GraphicsDevice, _windowSize);
            }
        }
        private void f_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            //if (MessageBox.Show(@"Are you sure you want to quit?", @"Confirm Quit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            ExitGame();
        }
        private void f_ResizeBegin(object sender, EventArgs e)
        {
            FormTouched = true;
        }
        private void f_Resize(object sender, EventArgs e)
        {
            if (_currentForm != _form.WindowState)
            {
                _currentForm = _form.WindowState;
                CheckForWindowUpdate();
            }
        }
        private void f_ResizeEnd(object sender, EventArgs e)
        {
            CheckForWindowUpdate();
            FormTouched = false;
        }

        public void ToggleFullScreen()
        {
            //if (Loading) return;
            if (Window.IsBorderless)
            {
                Window.IsBorderless = false;
            }
            else
            {
                _form.WindowState = FormWindowState.Minimized;
                Window.IsBorderless = true;
                _form.WindowState = FormWindowState.Maximized;
            }
        }

/********************************************************************
------->Game loop
********************************************************************/
        protected override void Initialize()
        {
            _activeSectorID = SectorID.MainMenu;
            _mouseScale = 1;
            _mouse = new MouseController();
            Console = new ConsoleHandler();
            _handler = new CommandHandler();
            _handler.Initialize(this);
            SystemClockID = UniversalTime.NewTimer(true);
            _frameTimeID = UniversalTime.NewTimer(true, 1.5);
            UniversalTime.TurnOnTimer(SystemClockID, _frameTimeID);

            //IsMouseVisible = true;
            Window.AllowUserResizing = true;
            _currentForm = FormWindowState.Normal;
            _form = (Form)Control.FromHandle(Window.Handle);
            _form.MaximumSize = new Size(ScaleRule.MAX_X, ScaleRule.MAX_Y);
            _form.MinimumSize = new Size(ScaleRule.MIN_X, ScaleRule.MIN_Y);
            _form.WindowState = FormWindowState.Maximized;

            _sectorDirectory = new Dictionary<SectorID, Sector>
            {
                {SectorID.MainMenu, new TitleSector(this)},
                {SectorID.Outerworld, new OuterworldSector(this) },
                {SectorID.Underworld, new UnderworldSector(this) }
            };
            foreach (var sector in _sectorDirectory.Values) sector.Initialize();

            base.Initialize();
            CheckForWindowUpdate();
        }

        protected override void LoadContent()
        {
            if (Control.FromHandle(Window.Handle) is Form f)
            {
                f.FormClosing += f_FormClosing;
                f.MinimizeBox = false;

                f.ResizeBegin += f_ResizeBegin;
                f.ResizeEnd += f_ResizeEnd;
                f.Resize += f_Resize;
            }

            Content.RootDirectory = "Content";
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            UILibrary.LoadLibrary(GraphicsDevice, Content);

            TextDrawer.Initialize(GraphicsDevice, Content.Load<Texture2D>("UI/FONT"));
            foreach (var sector in _sectorDirectory.Values) sector.LoadContent(GraphicsDevice, Content);
            Console.LoadContent(GraphicsDevice, Content);

            _mouseTexture = Content.Load<Texture2D>("UI/Poker");

            base.LoadContent();
        }

        private void UpdateFPS()
        {
            _currentFrameCount++;
            if (UniversalTime.GetNumberOfFires(_frameTimeID, false) > 0) //update display every 1.5 seconds
            {
                _currentFrameRate = (int)(Math.Round(_currentFrameCount / UniversalTime.GetCurrentTime(_frameTimeID)) + double.Epsilon);
                _currentFrameCount = 0;
                UniversalTime.ResetTimer(_frameTimeID);
            }
        }

        private void UpdateMouse()
        {
            var tmp = _mouse.UpdateInput();
            if (tmp.PositionChange)
            {
                _mouseDrawPos = tmp.MousePosition.ToVector2();
                ActiveSector.OnMoveMouse(tmp.MousePosition);
            }

            if (!tmp.StateChange) return;
            if (tmp.LeftDown) ActiveSector.OnLeftClick(tmp.MousePosition);
            else if (tmp.LeftUp) ActiveSector.OnLeftRelease();
        }

        protected override void Update(GameTime gameTime)
        {
            UniversalTime.Update(gameTime);
            Console.Update();

            switch (_activeSectorID)
            {
                case SectorID.MainMenu:
                case SectorID.Outerworld:
                case SectorID.Underworld:
                    if (!ConsoleActive)
                    {
                        _handler.UpdateInput();
                        UpdateMouse();
                    }
                    ActiveSector.Update();
                    break;
                case SectorID.LoadingInit:
                    break;
                case SectorID.Loading2Under:
                    break;
                case SectorID.Loading2Outer:
                    break;
                case SectorID.Unloading:
                    break;
            }
            base.Update(gameTime);
        }

        protected virtual void DrawDebugInfo()
        {
            TextDrawer.DrawDumbText(SpriteBatch, Vector2.One, _currentFrameRate + " FPS", 1, Color.White);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            switch (_activeSectorID)
            {
                case SectorID.MainMenu:
                case SectorID.Outerworld:
                case SectorID.Underworld:
                    ActiveSector.Draw(SpriteBatch, ShowDebugInfo);
                    break;
                case SectorID.LoadingInit:
                    break;
                case SectorID.Loading2Under:
                    break;
                case SectorID.Loading2Outer:
                    break;
                case SectorID.Unloading:
                    break;
            }

            UpdateFPS(); //FPS numbers are calculated based on drawn frames
            Console.Draw(SpriteBatch); //Console will handle when it should draw
            SpriteBatch.Draw(_mouseTexture, _mouseDrawPos, null, Color.White, 0, Vector2.Zero, _mouseScale, SpriteEffects.None, 0);
            if (ShowDebugInfo) DrawDebugInfo();

            SpriteBatch.End();
            base.Draw(gameTime);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public int RequestSectorChange(SectorID newSector)
        {
            if (newSector > SectorID.LoadingInit) //Transitioning to loading
            {
                if (_activeSectorID >= SectorID.LoadingInit)
                {
                    return -1; //wait
                }
            }

            _activeSectorID = newSector;
            return 0; //no issue
        }

        public void ToggleShowDebugInfo()
        {
            ShowDebugInfo = !ShowDebugInfo;
        }

        public void OpenConsole(string initMessage="")
        {
            Console.OpenConsole(initMessage);
        }

        protected virtual void ExitGame()
        {
            AudioManager.KillCurrentSong();
            foreach (var sector in _sectorDirectory.Values) sector.OnExit();
            Exit();
        }

    }
}
