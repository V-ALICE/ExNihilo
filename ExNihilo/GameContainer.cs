using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using ExNihilo.Input.Commands;
using ExNihilo.Sectors;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        private int _frameTimeID;
        private int _currentFrameCount, _currentFrameRate;
        private readonly GraphicsDeviceManager _graphics;
        private Dictionary<SectorID, Sector> _sectorDirectory;
        private Form _form;
        private CommandHandler _handler;

        protected bool showDebugInfo, formTouched;
        protected int systemClockID;
        protected SpriteBatch spriteBatch;
        protected Thread loadingThread;

        public GameContainer()
        {
            _graphics = new GraphicsDeviceManager(this) { SynchronizeWithVerticalRetrace = true };
        }

/********************************************************************
------->Window functions
********************************************************************/
        private void f_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            //if (MessageBox.Show(@"Are you sure you want to quit?", @"Confirm Quit", MessageBoxButtons.YesNo) == DialogResult.Yes)
            ExitGame();
        }
        private void f_ResizeBegin(object sender, EventArgs e)
        {
            formTouched = true;
        }
        private void f_Resize(object sender, EventArgs e)
        {
            if (_currentForm != _form.WindowState)
            {
                _currentForm = _form.WindowState;
                //TODO: resize UI here
            }
        }
        private void f_ResizeEnd(object sender, EventArgs e)
        {
            //TODO: resize UI here
            formTouched = false;
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
            _activeSectorID = SectorID.Underworld; //Sector.LoadingInit;
            _handler = new CommandHandler(this);
            systemClockID = UniversalTime.NewTimer(true);
            _frameTimeID = UniversalTime.NewTimer(true, 1.5);
            UniversalTime.TurnOnTimer(systemClockID, _frameTimeID);

            Window.AllowUserResizing = true;
            _currentForm = FormWindowState.Normal;
            _form = (Form)Control.FromHandle(Window.Handle);
            _form.WindowState = FormWindowState.Maximized;

            _sectorDirectory = new Dictionary<SectorID, Sector>
            {
                {SectorID.MainMenu, new TitleSector()},
                {SectorID.Outerworld, new OuterworldSector() },
                {SectorID.Underworld, new UnderworldSector() }
            };
            foreach (var sector in _sectorDirectory.Values) sector.Initialize();

            base.Initialize();
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            TextDrawer.Initialize(GraphicsDevice, Content.Load<Texture2D>("UI/FONT"));
            foreach (var sector in _sectorDirectory.Values) sector.LoadContent();

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

        protected override void Update(GameTime gameTime)
        {
            UniversalTime.Update(gameTime);
            _handler.UpdateInput();

            switch (_activeSectorID)
            {
                case SectorID.MainMenu:
                case SectorID.Outerworld:
                case SectorID.Underworld:
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
            TextDrawer.DrawDumbText(spriteBatch, Vector2.One, _currentFrameRate + " FPS", 1, Color.White);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            UpdateFPS();

            switch (_activeSectorID)
            {
                case SectorID.MainMenu:
                case SectorID.Outerworld:
                case SectorID.Underworld:
                    ActiveSector.Draw(showDebugInfo);
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

            if (showDebugInfo) DrawDebugInfo();
            spriteBatch.End();
            base.Draw(gameTime);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public int RequestSectorChange(SectorID newSector)
        {
            if (newSector > SectorID.LoadingInit) //Transitioning to loading
            {
                if (_activeSectorID >= SectorID.LoadingInit || loadingThread.IsAlive)
                {
                    return -1; //wait
                }
            }

            _activeSectorID = newSector;
            return 0; //no issue
        }

        public void ToggleShowDebugInfo()
        {
            showDebugInfo = !showDebugInfo;
        }

        protected virtual void ExitGame()
        {
            loadingThread?.Abort();
            loadingThread?.Join();
            AudioManager.KillCurrentSong();
            foreach (var sector in _sectorDirectory.Values) sector.ExitGame();
            Exit();
        }
    }
}
