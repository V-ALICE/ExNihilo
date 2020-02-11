using System;
using System.Collections.Generic;
using ExNihilo.Input.Commands;
using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems
{
    public class Message
    {
        private readonly float _maxLiveTime; //in seconds
        private readonly string _starter;
        private double _liveTime;

        public readonly string Text;
        public string SplitMessage;
        public bool Dead;
        public readonly ColorScale StarterColor, MessageColor;
        public int LineCount { get; private set; }

        public Message(string message, float timeToLive, string header, ColorScale startColor, ColorScale messageColor)
        {
            StarterColor = startColor;
            MessageColor = messageColor;
            _maxLiveTime = timeToLive;
            Text = message;
            _starter = header;
        }

        private static int ChooseBestSplit(string input, int maxLength)
        {
            var splitOnNewLine = input.IndexOf('\n', 0, Math.Min(maxLength, input.Length));
            if (splitOnNewLine >= 0) return splitOnNewLine;
            var splitOnSpace = input.LastIndexOf(' ', maxLength, maxLength);
            var splitOnDash = input.LastIndexOf('-', maxLength - 1, maxLength - 1);
            if (splitOnDash < 0 && splitOnSpace < 0) return maxLength;
            return Math.Max(splitOnDash, splitOnSpace);
        }
        public void SmartSplit(int maxLength)
        {
            var fullText = Text.Trim();
            SplitMessage = "";
            LineCount = 0;
            var lengthOfCompare = maxLength - _starter.Length;
            var fakeStarter = new string(' ', _starter.Length+1);
            var initialBuffer = "@c1 ";

            //Single line messages
            if (fullText.Length <= lengthOfCompare && !fullText.Contains("\n"))
            {
                SplitMessage += _starter + initialBuffer + fullText + "\n";
                LineCount++;
                return;
            }

            while (fullText.Length > lengthOfCompare || fullText.Contains("\n"))
            {
                if (SplitMessage.Length > 0) SplitMessage += "\n" + fakeStarter;
                else SplitMessage += _starter + initialBuffer;

                var point = ChooseBestSplit(fullText, lengthOfCompare - 1);
                SplitMessage += fullText.Substring(0, point);
                fullText = fullText.Substring(point).TrimStart();
                LineCount++;
            }

            //Add the rest of the message if there is any
            if (fullText.Length > 0)
            {
                SplitMessage += "\n" + fakeStarter + fullText;
                LineCount++;
            }

            SplitMessage += "\n";
        }

        public void Tick(double seconds)
        {
            _liveTime += seconds;
            Dead = _liveTime > _maxLiveTime;
        }

    }

    public class ConsoleBox
    {
        private const int _memoryMax = 25;      //Lines to keep in memory
        private const float _lineLiveTime = 10; //Time to display a message (in seconds)
        private int _maxLines, _maxLineLength; //Lines to display and characters per line
        private readonly int _timerID;
        private readonly List<Message> _messages;

        public ConsoleBox(int lines, int characters)
        {
            _maxLines = lines;
            _maxLineLength = characters;
            _messages = new List<Message>();
            _timerID = UniversalTime.NewTimer(true);
            UniversalTime.TurnOnTimer(_timerID);
        }

        public void ClearMemory()
        {
            _messages.Clear();
        }

        public void AddMessage(string starter, string text, ColorScale startColor, ColorScale messageColor)
        {
            var tmp = new Message(text, _lineLiveTime, starter, startColor, messageColor);
            tmp.SmartSplit(_maxLineLength);
            _messages.Add(tmp);
            while (_messages.Count > _memoryMax) _messages.RemoveAt(0);
        }

        public void Update()
        {
            var tick = UniversalTime.GetLastTickTime(_timerID);
            foreach (var m in _messages) m.Tick(tick);
        }

        public void Resize(int lines, int characters)
        {
            _maxLines = lines;
            _maxLineLength = characters;
            foreach (var m in _messages) m.SmartSplit(_maxLineLength);
        }

        public List<Message> GetAllMessages()
        {
            //return up to _maxLines lines in oldest to newest order
            var list = new List<Message>();
            var len = _messages.Count - 1;
            var count = 0;
            while (len >= 0)
            {
                var line = _messages[len--];
                if (count + line.LineCount > _maxLines) break;
                list.Insert(0, line);
                count += line.LineCount;
            }
            return list;
        }

    }

    public class ConsoleHandler : IUI, ITypable
    {
        private readonly ConsoleBox _console;
        private readonly CommandHandler _handler;
        private Texture2D _backdrop;
        private string _activeText;
        private Vector2 _backdropPos, _activeMessagePosition, _oldMessagePosition;
        private int _maxCharacterCount, _maxLineCount;
        private float _currentScale;
        private ScaleRuleSet _rules;
        private string _lastMessage;

        public bool Active;

        public ConsoleHandler()
        {
            _handler = new CommandHandler();
            _handler.Initialize(this);
            _console = new ConsoleBox(_maxLineCount, _maxCharacterCount);
            _lastMessage = "";
            _activeText = "";
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _currentScale = 1;
            _rules = TextureLibrary.ReducedScaleRuleSet;
        }

        public void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
        {
            //Values
            int height = gameWindow.Y / 4;
            int width = gameWindow.X / 2;
            _maxCharacterCount = width / (TextDrawer.AlphaWidth + TextDrawer.AlphaSpacer);
            _maxLineCount = height / (TextDrawer.AlphaHeight + TextDrawer.LineSpacer) - 1;
            _console?.Resize(_maxLineCount, _maxCharacterCount);

            //UI elements
            _backdrop?.Dispose();
            var border = 2 * TextDrawer.AlphaSpacer;
            _backdrop = TextureUtilities.CreateSingleColorTexture(graphics, width + border, height + border, new Color(0, 0, 0, 0.75f));

            //UI positions
            _backdropPos = new Vector2(0, gameWindow.Y - _backdrop.Height);
            _activeMessagePosition = new Vector2(TextDrawer.AlphaSpacer, gameWindow.Y - TextDrawer.LineSpacer - TextDrawer.AlphaHeight);
            _oldMessagePosition = new Vector2(TextDrawer.AlphaSpacer, gameWindow.Y - _backdrop.Height + TextDrawer.LineSpacer);
            _currentScale = _rules.GetScale(gameWindow);
        }

        public void Update()
        {
            _console.Update();
            if (!Active) return;

            _handler.UpdateInput();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(_backdrop, _backdropPos, Color.White);
                string message = "> ";
                if (_activeText.Length + 2 > _maxCharacterCount) message += _activeText.Substring(_activeText.Length + 2 - _maxCharacterCount);
                else message += _activeText;

                TextDrawer.DrawSmartText(spriteBatch, _activeMessagePosition, message, _currentScale, null, Color.White);
            }

            List<Message> pile = _console.GetAllMessages(); //oldest to newest
            var pos = Utilities.Copy(_oldMessagePosition);
            foreach (var message in pile)
            {
                if (!Active && message.Dead)
                {
                    pos.Y += message.LineCount*(TextDrawer.AlphaHeight + TextDrawer.LineSpacer);
                    continue;
                }

                pos = TextDrawer.DrawSmartText(spriteBatch, pos, message.SplitMessage, _currentScale, null, message.StarterColor, message.MessageColor);
            }
        }

        public void OpenConsole(string initMessage)
        {
            if (Active || TypingKeyboard.Active) return;
            Active = true;
            if (_activeText.Length == 0) _activeText = initMessage;
            TypingKeyboard.Lock(this);
        }

        public void CloseConsole()
        {
            Active = false;
            TypingKeyboard.Unlock(this);
        }

        public void PushConsole(string name="Player")
        {
            if (_activeText.StartsWith("/")) Asura.Handle(this, _activeText);
            else if (_activeText.Length != 0) _console.AddMessage("<"+name+">", _activeText, Color.DeepSkyBlue, Color.White);

            CloseConsole();
            _lastMessage = _activeText;
            _activeText = "";
        }

        public void ForceMessage(string starter, string message, ColorScale startColor, ColorScale messageColor)
        {
            _console.AddMessage(starter, message, startColor, messageColor);
        }

        public void GetLastMessage()
        {
            _activeText = _lastMessage;
        }

        public void ClearOutMessage()
        {
            _activeText = "";
        }

        public void Backspace(int len)
        {
            if (_activeText.Length >= len) _activeText = _activeText.Substring(0, _activeText.Length - len);
        }

        public void ReceiveInput(string input)
        {
            _activeText += input;
        }
    }
}