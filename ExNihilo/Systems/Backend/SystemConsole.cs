using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using ExNihilo.Input.Commands;
using ExNihilo.Systems.Backend.Network;
using ExNihilo.Systems.Bases;
using ExNihilo.UI.Bases;
using ExNihilo.Util;
using ExNihilo.Util.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Systems.Backend
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
            var tmp = new Message(TextDrawer.GetDeclutteredString(text.ToUpper()), _lineLiveTime, TextDrawer.GetDeclutteredString(starter.ToUpper()), startColor, messageColor);
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

    public static class SystemConsole
    {
        private class ConsoleReceiver : ITypable
        {
            public void ReceiveInput(string input)
            {
                _activeText += input;
            }

            public void Backspace(int len)
            {
                if (_activeText.Length >= len) _activeText = _activeText.Substring(0, _activeText.Length - len);
            }
        }

        private static readonly ConsoleReceiver _receiver = new ConsoleReceiver();
        private static ConsoleBox _console;
        private static readonly CommandHandler _handler = new CommandHandler();
        private static Texture2D _backdrop;
        private static Vector2 _backdropPos;
        private static Coordinate _activeMessagePosition, _oldMessagePosition;
        private static int _maxCharacterCount, _maxLineCount;
        private static float _currentScale;
        private static ScaleRuleSet _rules;
        private static string _lastMessage="", _activeText="";

        public static bool Active, Ready;

        private static Color _myColor;
        public static Color MyColor
        {
            get => _myColor;
            set
            {
                _myColor = value;
                SaveHandler.Parameters.R = value.R;
                SaveHandler.Parameters.G = value.G;
                SaveHandler.Parameters.B = value.B;
            }
        }

        public static void Initialize(GameContainer g)
        {
            _console = new ConsoleBox(_maxLineCount, _maxCharacterCount);
            _handler.InitializeConsole(g);
        }

        public static void LoadContent(GraphicsDevice graphics, ContentManager content)
        {
            _currentScale = 1;
            _rules = TextureLibrary.ReducedScaleRuleSet;
        }

        public static void OnResize(GraphicsDevice graphics, Coordinate gameWindow)
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
            _activeMessagePosition = new Coordinate(TextDrawer.AlphaSpacer, gameWindow.Y - TextDrawer.LineSpacer - TextDrawer.AlphaHeight);
            _oldMessagePosition = new Coordinate(TextDrawer.AlphaSpacer, gameWindow.Y - _backdrop.Height + TextDrawer.LineSpacer);
            _currentScale = _rules.GetScale(gameWindow);

            Ready = true;
            foreach (var message in queue)
            {
                ForceMessage(message.Item1, message.Item2, message.Item3, message.Item4);
            }
        }

        public static void Update()
        {
            _console.Update();
            if (!Active) return;

            _handler.UpdateInput();
        }

        public static void Draw(SpriteBatch spriteBatch)
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
            var pos = _oldMessagePosition.Copy();
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

        public static void OpenConsole(string initMessage)
        {
            if (Active || TypingKeyboard.Active) return;
            Active = true;
            if (_activeText.Length == 0) _activeText = initMessage;
            TypingKeyboard.Lock(_receiver);
        }

        public static void CloseConsole()
        {
            Active = false;
            TypingKeyboard.Unlock(_receiver);
        }

        public static void PushConsole(string name, bool loading)
        {
            if (_activeText.StartsWith("/"))
            {
                if (loading)
                {
                    ForceMessage("<error>", "Cannot execute commands during loading sequence", Color.DarkRed, Color.White);
                }
                else Asura.Handle(_activeText);
            }
            else if (_activeText.Length != 0)
            {
                _console.AddMessage("<"+name+">", _activeText, MyColor, Color.White);
                NetworkManager.SendMessage(new ConsoleMessage(NetworkManager.MyUniqueID, _activeText));
            }

            CloseConsole();
            _lastMessage = _activeText;
            _activeText = "";
        }

        private static List<Tuple<string, string, ColorScale, ColorScale>> queue = new List<Tuple<string, string, ColorScale, ColorScale>>();
        public static void ForceMessage(string starter, string message, ColorScale startColor, ColorScale messageColor)
        {
            if (!Ready)
            {
                queue.Add(new Tuple<string, string, ColorScale, ColorScale>(starter, message, startColor, messageColor));
                return;
            }
            _console.AddMessage(starter, message, startColor, messageColor);
        }

        public static void ClearConsole()
        {
            _console.ClearMemory();
        }

        public static void GetLastMessage()
        {
            _activeText = _lastMessage;
        }

        public static void ClearOutMessage()
        {
            _activeText = "";
        }
    }
}