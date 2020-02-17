using System;
using System.Collections.Generic;
using ExNihilo.Systems.Backend;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Game
{
    public static class ParticleBackdrop
    {
        private class Particle
        {
            private Vector2 _currentPosition, _currentVelocity, _currentAcceleration, _maxVelocity;
            private float _scale, _time;
            private int _len;
            private bool _upDeath, _bottomDeath, _leftDeath, _rightDeath, _curving;

            public bool Dead { get; private set; }

            public void Recycle(Vector2 initPos, Vector2 initVel, Vector2 initAcc, Vector2? maxVelAbs, int tailLen, float scale = 1.0f,
                bool killLeft = true, bool killRight = true, bool killTop = true, bool killBottom = true)
            {
                Dead = false;
                _currentPosition = initPos;
                _currentVelocity = initVel;
                _currentAcceleration = initAcc;
                _maxVelocity = maxVelAbs ?? new Vector2(-1, -1);
                _curving = tailLen <= 0;
                _scale = scale;
                _upDeath = killTop;
                _bottomDeath = killBottom;
                _leftDeath = killLeft;
                _rightDeath = killRight;
                _len = tailLen;
            }

            public void ChangeBorders(bool killLeft = true, bool killRight = true, bool killTop = true, bool killBottom = true)
            {
                _upDeath = killTop;
                _bottomDeath = killBottom;
                _leftDeath = killLeft;
                _rightDeath = killRight;
            }
            public void ApplyAcceleration(Vector2 acceleration, Vector2? maxVel)
            {
                _currentAcceleration = acceleration;
                if (maxVel != null) _maxVelocity = (Vector2) maxVel;
            }

            private bool CheckDead(Vector2 pos, Coordinate window)
            {
                return (_leftDeath && pos.X < 0) || (_rightDeath && pos.X > window.X) || (_upDeath && pos.Y < 0) || (_bottomDeath && pos.Y > window.Y);
            }
            public void Update(float time, Coordinate window)
            {
                if (_maxVelocity.X < 0) _currentVelocity += time * _currentAcceleration;
                else
                {
                    _currentVelocity.X = MathHelper.Clamp(_currentVelocity.X + time * _currentAcceleration.X, -_maxVelocity.X, _maxVelocity.X);
                    _currentVelocity.Y = MathHelper.Clamp(_currentVelocity.Y + time * _currentAcceleration.Y, -_maxVelocity.Y, _maxVelocity.Y);
                }
                _currentPosition += time * _currentVelocity;

                _time += time;
                if (_time > 1) //only check dead once per second
                {
                    _time -= 1;
                    Dead = CheckDead(_currentPosition, window) && CheckDead(_currentPosition - (0.01f * _len * _currentVelocity), window);
                }
            }

            public void Draw(SpriteBatch spriteBatch, AnimatableTexture t, ColorScale c)
            {
                if (_len > 0) LineDrawer.DrawLine(spriteBatch, _currentPosition, _currentPosition - 0.01f * _len * _currentVelocity, c, 0.5f * _scale);
                else t.Draw(spriteBatch, _currentPosition, c, _scale);
            }
        }

        public enum Mode
        {
            SlowDots, Windy, Rainy, Embers
        }

        private static readonly int _timerID = UniversalTime.NewTimer(true, 1);
        private static readonly List<Particle> _particles = new List<Particle>();
        private static readonly List<Particle> _recycleBin = new List<Particle>();
        private static readonly Random _rand = new Random();
        private static readonly ScaleRuleSet _rules = TextureLibrary.ReducedScaleRuleSet;
        private static bool _off, _usingDefaultColor=true;
        private static float _currentScale;
        private static int _max;
        private static Mode _mode;
        private static Vector2 _push;
        private static AnimatableTexture _texture, _default;
        private static ColorScale _color;
        private static Coordinate _window;

        public static void AddDefault(GraphicsDevice graphics)
        {
            _default = TextureUtilities.CreateSingleColorTexture(graphics, 1, 1, Color.White);
        }

        public static void Initialize(Mode mode, ColorScale c = null, AnimatableTexture t = null)
        {
            _off = false;
            float addRate = 0.1f;
            _mode = mode;
            _max = 100;
            switch (mode)
            {
                case Mode.SlowDots:
                    _color = ColorScale.White;
                    break;
                case Mode.Windy:
                    addRate = 0.05f;
                    _color = Color.ForestGreen;
                    _push = new Vector2(MathD.RandomlySigned(_rand, 1, 1), MathD.RandomlySigned(_rand, 1, 1));
                    break;
                case Mode.Rainy:
                    addRate = 0.005f;
                    _color = Color.SkyBlue;
                    _max = 250;
                    _push = new Vector2(MathD.RandomlySigned(_rand, 1, 1), 0);
                    break;
                case Mode.Embers:
                    addRate = 0.05f;
                    _color = ColorScale.GetFromGlobal("Ember");
                    _push = new Vector2(MathD.RandomlySigned(_rand, 1, 1), -1);
                    break;
            }
            UniversalTime.RecycleTimer(_timerID, addRate);
            UniversalTime.TurnOnTimer(_timerID);
            _texture = t ?? _default;
            if (c != null)
            {
                _color = c;
                _usingDefaultColor = false;
            }
            else _usingDefaultColor = true;
            _recycleBin.AddRange(_particles);
            if (_recycleBin.Count > _max) _recycleBin.RemoveRange(0, _recycleBin.Count - _max);
            _particles.Clear();
        }

        public static void ChangeStyle(Mode mode)
        {
            if (_usingDefaultColor) Initialize(mode);
            else Initialize(mode, _color);
        }

        public static void ChangeColor(ColorScale c = null)
        {
            if (c is null)
            {
                switch (_mode)
                {
                    case Mode.SlowDots:
                        _color = ColorScale.White;
                        break;
                    case Mode.Windy:
                        _color = Color.ForestGreen;
                        break;
                    case Mode.Embers:
                        _color = ColorScale.GetFromGlobal("Ember");
                        break;
                    case Mode.Rainy:
                        _color = Color.SkyBlue;
                        break;
                }

                _usingDefaultColor = true;
            }
            else
            {
                _color = c;
                _usingDefaultColor = false;
            }
        }

        public static void Clear()
        {
            _off = true;
            UniversalTime.TurnOffTimer(_timerID);
        }

        public static void Update()
        {
            if (_off || _window is null) return;
            var time = (float) UniversalTime.GetLastTickTime(_timerID);
            while (_particles.Count < _max && UniversalTime.GetAFire(_timerID))
            {
                Particle p;
                if (_recycleBin.Count > 0)
                {
                    p = _recycleBin[0];
                    _recycleBin.RemoveAt(0);
                }
                else
                {
                    p = new Particle();
                }

                if (_mode == Mode.Rainy)
                {
                    var pos = new Vector2(_rand.Next(-_window.X / 2, _window.X + _window.X / 2), _rand.Next(-100, -20)); //above screen
                    var vel = new Vector2(_push.X * _rand.Next(50, 150), _rand.Next(800, 900)); //down and slightly sideways
                    bool left = _push.X < 0;

                    p.Recycle(pos, vel, Vector2.Zero, null, _rand.Next(3, 6), _currentScale, left, !left, false);
                }
                else if (_mode == Mode.SlowDots)
                {
                    var pos = new Vector2(_rand.Next(0, _window.X), _rand.Next(0, _window.Y));
                    var vel = new Vector2(MathD.RandomlySigned(_rand, 5, 10), MathD.RandomlySigned(_rand, 5, 10));
                    p.Recycle(pos, vel, Vector2.Zero, null, 0, _currentScale);
                }
                else if (_mode == Mode.Windy)
                {
                    bool left = _push.X < 0;
                    bool up = _push.Y < 0;
                    var pos = new Vector2
                    {
                        X = left ? _window.X + _rand.Next(20, 100) : _rand.Next(-100, -20),
                        Y = up ? _rand.Next(100, _window.Y + _window.Y/2) : _rand.Next(-_window.Y/2, _window.Y - 100)
                    };
                    var vel = new Vector2
                    {
                        X = _push.X*_rand.Next(400, 600), //bigger push left/right
                        Y = _push.Y*_rand.Next(50, 150)
                    };

                    p.Recycle(pos, vel, Vector2.Zero, null, _rand.Next(2, 4), _currentScale, left, !left, up, !up);
                }
                else if (_mode == Mode.Embers)
                {
                    bool left = _push.X < 0;
                    var pos = new Vector2
                    {
                        X = left ? _rand.Next(100, _window.X + _window.X / 2) : _rand.Next(-_window.X / 2, _window.X - 100),
                        Y = _rand.Next(_window.Y / 2, _window.Y + _window.Y / 2)
                    };
                    var vel = new Vector2(_rand.Next(20, 100) * _push.X, -_rand.Next(20, 100));
                    var acc = new Vector2(_rand.Next(20, 100) * _push.X, -_rand.Next(20, 100));
                    var max = new Vector2(_rand.Next(200, 300), _rand.Next(200, 300));

                    p.Recycle(pos, vel, acc, max, _rand.Next(2, 4), _currentScale, left, !left, true, false);
                }

                _particles.Add(p);
            }

            if (_mode == Mode.Embers && _rand.Next(0, 1000) == 0)
            {
                _push = new Vector2(-_push.X, -1);
                bool left = _push.X < 0;
                foreach (var t in _particles)
                {
                    var acc = new Vector2(_rand.Next(20, 100) * _push.X, -_rand.Next(20, 100));
                    t.ApplyAcceleration(acc, null);
                    t.ChangeBorders(left, !left, true, false);
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update(time, _window);
                if (_particles[i].Dead)
                {
                    _recycleBin.Add(_particles[i]);
                    _particles.RemoveAt(i);
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_off || _texture is null) return;
            foreach (var particle in _particles)
            {
                particle.Draw(spriteBatch, _texture, _color);
            }
        }

        public static void OnResize(Coordinate gameWindow)
        {
            _window = gameWindow.Copy();
            _currentScale = _rules.GetScale(gameWindow);
            UniversalTime.ResetTimer(_timerID);
        }
    }
}
