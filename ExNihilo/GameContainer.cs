using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExNihilo.Entity;
using ExNihilo.Input.Commands;
using ExNihilo.Input.Controllers;
using ExNihilo.Sectors;
using ExNihilo.Systems;
using ExNihilo.Systems.Backend;
using ExNihilo.Systems.Game;
using ExNihilo.Systems.Game.Items;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace ExNihilo
{
    public static class D
    {
        public static bool Bug = false;
    }
    public class GameContainer : Game
    {
        public enum SectorID
        {
            MainMenu, Outerworld, Void, Loading
        }

        private Sector ActiveSector => _sectorDirectory[ActiveSectorID];
        private FormWindowState _currentForm;
        private float _mouseScale;
        private int _frameTimeID;
        private int _currentFrameCount, _currentFrameRate;
        private readonly GraphicsDeviceManager _graphics;
        private Coordinate _windowSize;
        private Vector2 _mouseDrawPos;
        private Dictionary<SectorID, Sector> _sectorDirectory;
        private Form _form;
        private AnimatableTexture _mouseTexture;
        private MouseController _mouse;
        private CommandHandler _handler, _superHandler;
        private Point _lastMousePosition;

        public PlayerEntityContainer Player => ((OuterworldSector)_sectorDirectory[SectorID.Outerworld]).Player;
       
        public static ConsoleHandler Console { get; private set; }
        public SectorID PreviousSectorID;
        public static SectorID ActiveSectorID;

        protected bool ShowDebugInfo;
        public static bool FormTouched;
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
        private void OnResize()
        {
            ParticleBackdrop.OnResize(_windowSize);
            Console.OnResize(GraphicsDevice, _windowSize);
            _mouseScale = TextureLibrary.DefaultScaleRuleSet.GetScale(_windowSize);
            foreach (var sector in _sectorDirectory.Values) sector?.OnResize(GraphicsDevice, _windowSize);
        }

        private void ForceWindowUpdate(int width, int height)
        {
            _graphics.PreferredBackBufferWidth = MathHelper.Clamp(width, ScaleRule.MIN_X, ScaleRule.MAX_X);
            _graphics.PreferredBackBufferHeight = MathHelper.Clamp(height, ScaleRule.MIN_Y, ScaleRule.MAX_Y);
            _graphics.ApplyChanges();
            _windowSize = new Coordinate(width, height);

            OnResize();
        }
        private void CheckForWindowUpdate()
        {
            //if (ActiveSectorID == SectorID.Loading) return;
            if (Window.ClientBounds.Width != _windowSize.X || Window.ClientBounds.Height != _windowSize.Y)
            {
                _graphics.PreferredBackBufferWidth = MathHelper.Clamp(Window.ClientBounds.Width, ScaleRule.MIN_X, ScaleRule.MAX_X);
                _graphics.PreferredBackBufferHeight = MathHelper.Clamp(Window.ClientBounds.Height, ScaleRule.MIN_Y, ScaleRule.MAX_Y);
                _graphics.ApplyChanges();
                _windowSize = new Coordinate(Window.ClientBounds.Width, Window.ClientBounds.Height);

                OnResize();
            }
        }
        public void ToggleFullScreen()
        {
            if (ActiveSectorID==SectorID.Loading) return;
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
        private void f_Activate(object sender, EventArgs e)
        {
            IsMouseVisible = false;
            AudioManager.Pause(false);
        }
        private void f_Deactivate(object sender, EventArgs e)
        {
            IsMouseVisible = true;
            AudioManager.Pause(true);
        }

/********************************************************************
------->Game loop
********************************************************************/
        protected override void Initialize()
        {
            ActiveSectorID = SectorID.MainMenu;
            PreviousSectorID = SectorID.MainMenu;
            _mouseScale = 1;
            _lastMousePosition = new Point();
            _mouse = new MouseController();
            Console = new ConsoleHandler();
            _handler = new CommandHandler();
            _superHandler = new CommandHandler();
            _handler.InitializeBase(this);
            _superHandler.InitializeSuper(this);
            SystemClockID = UniversalTime.NewTimer(true);
            _frameTimeID = UniversalTime.NewTimer(true, 1.5);
            TextureLibrary.LoadRuleSets();
            UniversalTime.TurnOnTimer(SystemClockID, _frameTimeID);

            ColorScale.AddToGlobal("Random", new ColorScale(2f, 32, 222));
            ColorScale.LoadColors("COLOR.info");

            //IsMouseVisible = true;
            Window.AllowUserResizing = true;
            _currentForm = FormWindowState.Normal;
            _form = (Form)Control.FromHandle(Window.Handle);
            _form.MaximumSize = new Size(ScaleRule.MAX_X, ScaleRule.MAX_Y);
            _form.MinimumSize = new Size(ScaleRule.MIN_X, ScaleRule.MIN_Y);
            _form.WindowState = FormWindowState.Maximized;

            SaveHandler.LoadParameters();
            PushParameters(SaveHandler.Parameters);
            SaveHandler.LoadAllSaves(SaveHandler.FILE_1, SaveHandler.FILE_2, SaveHandler.FILE_3);
            ParticleBackdrop.AddDefault(GraphicsDevice);

            _sectorDirectory = new Dictionary<SectorID, Sector>
            {
                {SectorID.MainMenu, new TitleSector(this)},
                {SectorID.Outerworld, new OuterworldSector(this) },
                {SectorID.Void, new VoidSector(this) },
                {SectorID.Loading, new LoadingSector(this) }
            };
            foreach (var sector in _sectorDirectory.Values) sector?.Initialize();
            Asura.Ascend(this, _sectorDirectory[SectorID.Void] as VoidSector, _sectorDirectory[SectorID.Outerworld] as OuterworldSector);

            base.Initialize();
            //ForceWindowUpdate(1920, 1080);
            CheckForWindowUpdate();
            ActiveSector?.Enter(_lastMousePosition, _windowSize);

            //AudioManager.Pause(true);
            AudioManager.PlaySong("Title", true);
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

            Activated += f_Activate;
            Deactivated += f_Deactivate;

            Content.RootDirectory = "Content";
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            TextureLibrary.LoadUILibrary(GraphicsDevice, Content, "LOGO.info", "UI.info");
            TextureLibrary.LoadIconLibrary(GraphicsDevice, Content, "ICON.info");
            TextureLibrary.LoadCharacterLibrary(GraphicsDevice, Content, "CHAR.info");
            TextureLibrary.LoadTextureLibrary(GraphicsDevice, Content, "BACK.info");
            ItemLoader.LoadItems(GraphicsDevice, "MAT.info");

            TextDrawer.Initialize(GraphicsDevice, Content.Load<Texture2D>("UI/FONT"));
            foreach (var sector in _sectorDirectory.Values) sector?.LoadContent(GraphicsDevice, Content);
            Console.LoadContent(GraphicsDevice, Content);

            _mouseTexture = Content.Load<Texture2D>("UI/CURSOR");

            AudioManager.Initialize(Content);

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
            _lastMousePosition = tmp.MousePosition;
            if (tmp.PositionChange)
            {
                _mouseDrawPos = tmp.MousePosition.ToVector2();
                ActiveSector?.OnMoveMouse(tmp.MousePosition);
            }

            if (!tmp.StateChange) return;
            if (ConsoleActive) Console.CloseConsole();
            if (tmp.LeftDown) ActiveSector?.OnLeftClick(tmp.MousePosition);
            else if (tmp.LeftUp) ActiveSector?.OnLeftRelease(tmp.MousePosition);
        }

        protected override void Update(GameTime gameTime)
        {
            UniversalTime.Update(gameTime);
            ColorScale.UpdateGlobalScales();
            ParticleBackdrop.Update();
            Console.Update();
            if (IsActive)
            {
                //Don't listen to the keyboard/mouse if the game isn't focused
                UpdateMouse();
                _superHandler.UpdateInput();
                TypingKeyboard.GetText();
                if (!TypingKeyboard.Active) _handler.UpdateInput();
            }
            ActiveSector?.Update();

            base.Update(gameTime);
        }

        protected void DrawDebugInfo(SpriteBatch spriteBatch)
        {
            var text = _currentFrameRate + " FPS (" + GraphicsDevice.Adapter.Description + ")";
            TextDrawer.DrawDumbText(spriteBatch, Vector2.One, text, 1, Color.White);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            ParticleBackdrop.Draw(SpriteBatch);
            ActiveSector?.Draw(SpriteBatch, ShowDebugInfo);

            UpdateFPS(); //FPS numbers are calculated based on drawn frames
            Console.Draw(SpriteBatch); //Console will handle when it should draw
            if (!IsMouseVisible) _mouseTexture.Draw(SpriteBatch, _mouseDrawPos, ColorScale.White, _mouseScale);
            if (ShowDebugInfo) DrawDebugInfo(SpriteBatch);

            SpriteBatch.End();
            base.Draw(gameTime);
        }

/********************************************************************
------->Game functions
********************************************************************/
        public void RequestSectorChange(SectorID newSector)
        {
            if (newSector == SectorID.Loading) //Transitioning to loading
            {
                
            }
            else if (ActiveSectorID == SectorID.Loading) //Transitioning from loading
            {
                CheckForWindowUpdate();
            }

            PreviousSectorID = ActiveSectorID;
            ActiveSector?.Leave(newSector);
            ActiveSectorID = newSector;
            ActiveSector?.Enter(_lastMousePosition, _windowSize);
        }

        public void ToggleShowDebugInfo()
        {
            ShowDebugInfo = !ShowDebugInfo;
        }

        public void OpenConsole(string initMessage="")
        {
            Console.OpenConsole(initMessage);
        }

        public void ExitGame()
        {
            AudioManager.KillCurrentSong();
            foreach (var sector in _sectorDirectory.Values) sector?.OnExit();
            Exit();
        }

        public void BackOut()
        {
            if (ConsoleActive) Console.CloseConsole();
            else ActiveSector?.BackOut();
        }

        public void Pack()
        {
            PackedGame game = new PackedGame(this, SaveHandler.GetLastID());
            foreach (var sector in _sectorDirectory.Values) sector?.Pack(game);
            SaveHandler.Save(SaveHandler.LastLoadedFile, game);
            Console.ForceMessage("<Asura>", "Game has been saved", Color.Purple, Color.White);
        }

        public bool Unpack(PackedGame game)
        {
            if (game is null) return false;

            foreach (var sector in _sectorDirectory.Values) sector?.Unpack(game);
            return true;
        }

        public void PushParameters(GameParameters param)
        {
            AudioManager.MusicVolume = param.MusicVolume;
            AudioManager.EffectVolume = param.EffectVolume;
        }

        public void StartNewGame(int floor=1)
        {
            //This exists solely so that other sectors can tell the void to start a new game
            var a = _sectorDirectory[SectorID.Void] as VoidSector;
            a?.StartNewGame(floor);
        }

        public void GLOBAL_DEBUG_COMMAND(string input)
        {
            var dict = new int[10];
            for (int i = 0; i < 100000; i++)
            {
                var item = ItemLoader.GetEquipment(MathD.urand, 100);
                var desc = item.Name.IndexOf(' ') > 0 ? item.Name.Substring(0, item.Name.IndexOf(' ')) : "None";
                if (desc.StartsWith("Broken")) dict[0]++;
                else if (desc.StartsWith("Damaged")) dict[1]++;
                else if (desc.StartsWith("Shabby")) dict[2]++;
                else if (desc.StartsWith("Basic")) dict[3]++;
                else if (desc.StartsWith("Fine")) dict[5]++;
                else if (desc.StartsWith("Grand")) dict[6]++;
                else if (desc.StartsWith("Legendary")) dict[7]++;
                else if (desc.StartsWith("Mythical")) dict[8]++;
                else if (desc.StartsWith("Absolute")) dict[9]++;
                else dict[4]++;
            }

            Console.ForceMessage("", "Broken:    " + dict[0], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Damaged:   " + dict[1], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Shabby:    " + dict[2], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Basic:     " + dict[3], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Average:   " + dict[4], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Fine:      " + dict[5], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Grand:     " + dict[6], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Legendary: " + dict[7], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Mythical:  " + dict[8], ColorScale.Black, ColorScale.White);
            Console.ForceMessage("", "Absolute:  " + dict[9], ColorScale.Black, ColorScale.White);
        }
    }
}
