using System;
using System.Collections.Generic;
using ExNihilo.Input.Commands;
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
        public readonly bool Command;
        public int LineCount { get; private set; }

        public Message(string message, float timeToLive, string header, bool command)
        {
            Command = command;
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
            var initialBuffer = Command ? " " : "@c1 ";

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

        public void AddMessage(string starter, string text, bool command=false)
        {
            var tmp = new Message(text, _lineLiveTime, starter, command);
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

    public class ConsoleHandler : IUI
    {
        private readonly ConsoleBox _console;
        private readonly CommandHandler _handler;
        private Texture2D _backdrop;
        private string _activeText;
        private Vector2 _backdropPos, _activeMessagePosition, _oldMessagePosition;
        private int _maxCharacterCount, _maxLineCount;
        private float _currentScale;
        private ScaleRuleSet _rules;

        private readonly int _backspaceTimerID;
        private const float _backspaceDelay = 0.05f, _backspaceDelayExtended = 0.35f;
        private bool _backspace, _firstBackspace;
        private string _lastMessage;

        public bool Active;

        public ConsoleHandler()
        {
            _handler = new CommandHandler();
            _handler.Initialize(this);
            _backspaceTimerID = UniversalTime.NewTimer(true);
            _console = new ConsoleBox(_maxLineCount, _maxCharacterCount);
            _lastMessage = "";
        }

        public void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _currentScale = 1;
            _rules = UILibrary.ReducedScaleRuleSet;
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

            _activeText += TypingKeyboard.GetText();
            _handler.UpdateInput();

            if (_backspace)
            {
                if (_firstBackspace && UniversalTime.GetCurrentTime(_backspaceTimerID) > _backspaceDelayExtended)
                {
                    //pressing backspace for significant time (0.35 sec) to engage auto
                    _firstBackspace = false;
                    UniversalTime.ResetTimer(_backspaceTimerID);
                }
                else if (!_firstBackspace && UniversalTime.GetCurrentTime(_backspaceTimerID) > _backspaceDelay)
                {
                    //auto is engaged and enough time has passed to go again
                    UniversalTime.ResetTimer(_backspaceTimerID);
                    if (_activeText.Length > 0) _activeText = _activeText.Substring(0, _activeText.Length - 1);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                spriteBatch.Draw(_backdrop, _backdropPos, Color.White);
                string message = "> ";
                if (_activeText.Length + 2 > _maxCharacterCount) message += _activeText.Substring(_activeText.Length + 2 - _maxCharacterCount);
                else message += _activeText;

                TextDrawer.DrawDumbText(spriteBatch, _activeMessagePosition, message, _currentScale, Color.White);
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

                var c = message.Command ? new ColorScale[] {Color.DarkOrange} : new ColorScale[] { Color.DeepSkyBlue, Color.White };
                pos = TextDrawer.DrawSmartText(spriteBatch, pos, message.SplitMessage, _currentScale, false, c);
            }
        }

        public void OpenConsole(string initMessage)
        {
            if (Active) return;
            Active = true;
            _activeText = initMessage;
        }

        public void CloseConsole()
        {
            _activeText = "";
            Active = false;
        }

        public void PushConsole()
        {
            if (_activeText.Length == 0) return;

            if (_activeText.StartsWith("/")) Asura.Handle(this, _activeText);
            else _console.AddMessage("<Player>", _activeText);

            //Active = false;
            _lastMessage = _activeText;
            _activeText = "";
        }

        public void ForceMessage(string starter, string message)
        {
            _console.AddMessage(starter, message, true);
        }

        public void GetLastMessage()
        {
            _activeText = _lastMessage;
        }

        public void ClearOutMessage()
        {
            _activeText = "";
        }

        public void Backspace()
        {
            UniversalTime.TurnOnTimer(_backspaceTimerID);
            _firstBackspace = _backspace = true;
            if (_activeText.Length > 0) _activeText = _activeText.Substring(0, _activeText.Length - 1);
        }

        public void UnBackspace()
        {
            UniversalTime.ResetTimer(_backspaceTimerID);
            UniversalTime.TurnOffTimer(_backspaceTimerID);
            _firstBackspace = _backspace = false;
        }
    }
}