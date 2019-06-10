using System;
using System.Collections.Generic;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    public static class ParticleBackdrop
    {
        private class Particle
        {
            private readonly List<Vector2> _tail;
            private Vector2 _currentPosition, _currentVelocity, _currentAcceleration;
            private float _scale;
            private bool _upDeath, _bottomDeath, _leftDeath, _rightDeath, _curving;

            public bool Dead { get; private set; }

            public Particle(Vector2 initPos, Vector2 initVel, int tailLen, Vector2? initAcc=null, float scale = 1.0f, 
                bool killLeft = true, bool killRight = true, bool killTop = true, bool killBottom = true)
            {
                _currentPosition = Utilities.Copy(initPos);
                _currentVelocity = Utilities.Copy(initVel);
                _curving = initAcc != null;
                if (_curving) _currentAcceleration = (Vector2) initAcc;
                _scale = scale;
                _upDeath = killTop;
                _bottomDeath = killBottom;
                _leftDeath = killLeft;
                _rightDeath = killRight;
                _tail = new List<Vector2>(tailLen);
            }

            public void Recycle(Vector2 initPos, Vector2 initVel, int tailLen, Vector2? initAcc=null, float scale = 1.0f,
                bool killLeft = true, bool killRight = true, bool killTop = true, bool killBottom = true)
            {
                _currentPosition = Utilities.Copy(initPos);
                _currentVelocity = Utilities.Copy(initVel);
                _curving = initAcc != null;
                if (_curving) _currentAcceleration = (Vector2)initAcc;
                _scale = scale;
                _upDeath = killTop;
                _bottomDeath = killBottom;
                _leftDeath = killLeft;
                _rightDeath = killRight;
                _tail.Clear();
                _tail.Capacity = tailLen;
            }

            public void ApplyAcceleration(Vector2 acceleration)
            {
                _currentAcceleration = Utilities.Copy(acceleration);
            }

            public void Update(float time, Coordinate window)
            {
                if (_tail.Count < _tail.Capacity) _tail.Add(Utilities.Copy(_currentPosition));
                else if (_tail.Count > 0) _tail.RemoveAt(0);
                _currentVelocity += time * _currentAcceleration;
                _currentPosition += time * _currentVelocity;

                var chunk = _tail.Count > 0 ? _tail[0] : _currentPosition;
                Dead = (_leftDeath && chunk.X < 0) || (_rightDeath && chunk.X > window.X) || (_upDeath && chunk.Y < 0) || (_bottomDeath && chunk.Y > window.Y);
            }

            public void Draw(SpriteBatch spriteBatch, AnimatableTexture t, ColorScale c, bool fade=false)
            {
                if (_curving) //around `_tail.Count`+1 times more expensive
                {
                    t.Draw(spriteBatch, _currentPosition, c, _scale);
                    for (int i = 0; i < _tail.Count; i++)
                    {
                        if (fade)
                        {
                            var f = c.Get();
                            f.A = (byte) (f.A * (1 - i / _tail.Count));
                            t.Draw(spriteBatch, _tail[i], f, _scale);
                        }
                        else t.Draw(spriteBatch, _tail[i], c, _scale);
                    }
                }
                else if (_tail.Count > 0) LineDrawer.DrawLine(spriteBatch, _currentPosition, _tail[0], c, 0.25f*_scale);
            }
        }

        public enum Mode
        {
            Random, Windy, Rainy, Stars, RandomTurning
        }

        private static readonly int _timerID = UniversalTime.NewTimer(true, 1);
        private static readonly List<Particle> _particles = new List<Particle>();
        private static readonly List<Particle> _recycleBin = new List<Particle>();
        private static readonly Random rand = new Random();
        private static readonly ScaleRuleSet _rules = UILibrary.ReducedScaleRuleSet;
        private static float _currentScale;
        private static int _max;
        private static Mode _mode;
        private static Vector2 _push;
        private static AnimatableTexture _texture;
        private static ColorScale _color;
        private static Coordinate _window;

        public static void AddDefault(GraphicsDevice graphics)
        {
            _texture = TextureUtilities.CreateSingleColorTexture(graphics, 1, 1, Color.White);
        }

        public static void Initialize(Mode mode, ColorScale c = null, AnimatableTexture t = null, float addRate = 1.0f)
        {
            UniversalTime.RecycleTimer(_timerID, addRate);
            UniversalTime.TurnOnTimer(_timerID);
            _mode = mode;
            switch (mode)
            {
                case Mode.Random:
                case Mode.Stars:
                    _color = ColorScale.White;
                    _max = 100;
                    break;
                case Mode.Windy:
                    _color = Color.ForestGreen;
                    _max = 100;
                    break;
                case Mode.Rainy:
                    _color = Color.SkyBlue;
                    _max = 250;
                    _push = new Vector2(MathD.RandomlySigned(rand, 1, 1), 0);
                    break;
            }
            if (t != null) _texture = t;
            if (c != null) _color = c;
            while (_particles.Count > _max) _particles.RemoveAt(0);
        }

        public static void Clear()
        {
            _particles.Clear();
            UniversalTime.TurnOffTimer(_timerID);
        }

        public static void Update()
        {
            var time = (float) UniversalTime.GetLastTickTime(_timerID);
            if (UniversalTime.GetAFire(_timerID) && _particles.Count < _max)
            {
                if (_mode == Mode.Rainy)
                {
                    var pos = new Vector2(rand.Next(-_window.X / 2, _window.X + _window.X / 2), -20); //above screen
                    var vel = new Vector2(_push.X * rand.Next(50, 150), rand.Next(600, 750)); //down and slightly sideways
                    bool left = _push.X < 0;
                    if (_recycleBin.Count > 0)
                    {
                        _recycleBin[0].Recycle(pos, vel, 5, null, _currentScale, left, !left, false);
                        _particles.Add(_recycleBin[0]);
                        _recycleBin.RemoveAt(0);
                    }
                    else _particles.Add(new Particle(pos, vel, 5, null, _currentScale, left, !left, false));
                }
                else if (_mode == Mode.Windy)
                {

                }
                else if (_mode == Mode.Random)
                {
                    var pos = new Vector2(rand.Next(0, _window.X), rand.Next(0, _window.Y));
                    var vel = new Vector2(MathD.RandomlySigned(rand, 25, 100), MathD.RandomlySigned(rand, 25, 100));
                    var acc = new Vector2(MathD.RandomlySigned(rand, 0, 20), MathD.RandomlySigned(rand, 0, 20));
                    if (_recycleBin.Count > 0)
                    {
                        _recycleBin[0].Recycle(pos, vel, 0, acc, _currentScale);
                        _particles.Add(_recycleBin[0]);
                        _recycleBin.RemoveAt(0);
                    }
                    else _particles.Add(new Particle(pos, vel, 0, acc, _currentScale));
                }
                else if (_mode == Mode.Stars)
                {

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
            if (_texture is null) return;
            foreach (var particle in _particles)
            {
                particle.Draw(spriteBatch, _texture, _color);
            }
        }

        public static void OnResize(Coordinate gameWindow)
        {
            _window = gameWindow.Copy();
            _currentScale = _rules.GetScale(gameWindow);
        }
    }
}
