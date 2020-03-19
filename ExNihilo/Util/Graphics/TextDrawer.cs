using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util.Graphics
{
    public static class TextDrawer
    {
        //These are the positions in the alphabet image: ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 -?!.,'"()[]%/;:+=_|<>\
        private static readonly Dictionary<char, int> _char = new Dictionary<char, int>
        {
            {'A', 0},  {'B', 1},  {'C', 2},  {'D', 3},  {'E', 4},  {'F', 5},  {'G', 6}, {'H', 7}, {'I', 8}, {'J', 9},
            {'K', 10}, {'L', 11}, {'M', 12}, {'N', 13}, {'O', 14}, {'P', 15}, {'Q', 16}, {'R', 17}, {'S', 18}, {'T', 19},
            {'U', 20}, {'V', 21}, {'W', 22}, {'X', 23}, {'Y', 24}, {'Z', 25},
            {'1', 26}, {'2', 27}, {'3', 28}, {'4', 29}, {'5', 30}, {'6', 31}, {'7', 32}, {'8', 33}, {'9', 34}, {'0', 35},
            {' ', 36}, {'-', 37}, {'?', 38}, {'!', 39}, {'.', 40}, {',', 41}, {'\'', 42}, {'\"', 43}, {'(', 44}, {')', 45},
            {'[', 46}, {']', 47}, {'%', 48}, {'/', 49}, {';', 50}, {':', 51}, {'+', 52}, {'=', 53}, {'_', 54}, {'|', 55},
            {'<', 56}, {'>', 57}, {'\\', 58}
        };

        public static int AlphaWidth, AlphaHeight, AlphaSpacer, LineSpacer; //In pixels
        private static Dictionary<char, Texture2D> _charTextures;
        private static Texture2D _underline;
        private static Vector2 _underlineOffset;
        private static int _metronome;

        public struct TextParameters
        {
            public bool ReducedSpaces;
            public float WaveIntensity, WaveSpeed;

            public TextParameters(bool reducedSpaces=false, float waveIntensity=10, float waveSpeed=7)
            {
                ReducedSpaces = reducedSpaces;
                WaveIntensity = waveIntensity;
                WaveSpeed = waveSpeed;
            }
        }

        private static readonly TextParameters _defaultParameters = new TextParameters();

        public static void Initialize(GraphicsDevice device, Texture2D alphabet)
        {
            _charTextures = new Dictionary<char, Texture2D>();
            var charWidth = alphabet.Width / _char.Count;
            foreach (var chr in _char)
            {
                var letter = new Rectangle(charWidth * chr.Value, 0, charWidth, alphabet.Height);
                _charTextures.Add(chr.Key, TextureUtilities.GetSubTexture(device, alphabet, letter));
            }
            
            AlphaWidth = _charTextures['A'].Width;
            AlphaHeight = _charTextures['A'].Height;
            AlphaSpacer = AlphaWidth/3;
            LineSpacer = 2*AlphaSpacer;
            _underlineOffset = new Vector2(-AlphaSpacer, AlphaHeight+AlphaSpacer);
            _underline = TextureUtilities.CreateSingleColorTexture(device, AlphaWidth + 2*AlphaSpacer, AlphaSpacer, Color.White);

            _metronome = UniversalTime.NewTimer(true, 2*Math.PI);
            UniversalTime.TurnOnTimer(_metronome);
        }

        public static Texture2D GetLetter(char let)
        {
            if (_charTextures.TryGetValue(let.ToString().ToUpper()[0], out var tex)) return tex;
            return _charTextures[' '];
        }       

        public static Coordinate DrawDumbText(SpriteBatch spriteBatch, Coordinate pos, string dumbText, float multiplier, ColorScale c)
        {
            var aPos = pos.Copy();
            var oldX = aPos.X;
            foreach (var t in dumbText)
            {
                if (t == '\n')
                {
                    aPos.Y = (int) Math.Round(aPos.Y + multiplier * (AlphaHeight + LineSpacer));
                    aPos.X = oldX;
                }
                else if (t == ' ')
                {
                    aPos.X = (int)Math.Round(aPos.X + multiplier * (AlphaWidth + AlphaSpacer));
                }
                else
                {
                    spriteBatch.Draw(GetLetter(t), (Vector2) aPos, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                    aPos.X = (int) Math.Round(aPos.X + multiplier * (AlphaSpacer + AlphaWidth));
                }
            }
            return aPos;
        }

        public static Coordinate DrawSmartText(SpriteBatch spriteBatch, Coordinate pos, string smartText, float multiplier, TextParameters? parameters = null, params ColorScale[] colors)
        {
            int underlining = 0;
            int wavy = 0;
            double sineOffset = 0;
            double metro = UniversalTime.GetCurrentTime(_metronome, true);

            //supports up to 10 different colors at a time
            //assumes line has already been split correctly, including buffers
            var aPos = pos.Copy();
            var oldX = aPos.X;
            var c = colors.Length > 0 ? colors[0] : ColorScale.Black;
            var p = parameters ?? _defaultParameters;
            for (var i = 0; i < smartText.Length; i++)
            {
                var t = smartText[i];
                if (t == '\n') //newline
                {
                    aPos.Y = (int)Math.Round(aPos.Y + multiplier * (AlphaHeight + LineSpacer));
                    aPos.X = oldX;
                    continue;
                }
                if (t == ' ') //space
                {
                    if (wavy > 0) wavy--;
                    if (underlining > 0)
                    {
                        underlining--;
                        spriteBatch.Draw(_underline, aPos + multiplier*_underlineOffset, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                    }
                    if (p.ReducedSpaces)
                        aPos.X = (int) Math.Round(aPos.X + multiplier * AlphaWidth);
                    else
                        aPos.X = (int) Math.Round(aPos.X + multiplier * (AlphaWidth + AlphaSpacer));
                    continue;
                }
                if (t == '@')//smart text
                {
                    if (i + 1 == smartText.Length) return aPos; //make sure there's actually something after
                    switch (smartText[i+1])
                    {
                        case 'c': //change color
                            i += 2; //fully consume @c
                            if (i == smartText.Length) return aPos; //make sure there's actually something after
                            if (char.IsDigit(smartText, i)) //Make sure it's actually a number after
                            {
                                var param = int.Parse(smartText[i].ToString());
                                if (colors.Length > param) c = colors[param]; //change color if there's actually a color at that position
                            }
                            break;
                        case 'h': //insert half space
                            i++; //fully consume @h
                            if (p.ReducedSpaces)
                                aPos.X = (int)Math.Round(aPos.X + 0.5 * multiplier * AlphaWidth);
                            else
                                aPos.X = (int)Math.Round(aPos.X + 0.5 * multiplier * (AlphaWidth + AlphaSpacer));
                            break;
                        case 'n': //insert half newline skip (can be done mid line as well)
                            i++; //fully consume @n
                            aPos.Y = (int) Math.Round(aPos.Y + 0.5f * multiplier * (AlphaHeight + LineSpacer));
                            break;
                        case 's': //sin wave
                            i += 2; //fully consume @s
                            string s = "";
                            if (i == smartText.Length) return aPos; //make sure there's actually something after
                            while (smartText[i] != '@')
                            {
                                s += smartText[i];
                                i++;
                                if (i == smartText.Length) return aPos; //ignore malformed strings
                            }
                            if (int.TryParse(s, out int lenS))
                            {
                                wavy = lenS;
                                sineOffset = 2*Math.PI / lenS;
                            }
                            break;
                        case 'u': //underline for some amount of characters
                            i += 2; //fully consume @u
                            string u = "";
                            if (i == smartText.Length) return aPos; //make sure there's actually something after
                            while (smartText[i] != '@')
                            {
                                u += smartText[i];
                                i++;
                                if (i == smartText.Length) return aPos; //ignore malformed strings
                            }
                            if (int.TryParse(u, out int lenU)) underlining = lenU;                          
                            break;
                        default:
                            i++;
                            break;
                    }
                    continue; //don't need to draw anything right after a format change
                }

                if (wavy > 0)
                {
                    wavy--;
                    var offset = new Vector2(0, (float) Math.Round(multiplier*5 * Math.Sin(7 * metro + wavy * sineOffset)));
                    spriteBatch.Draw(GetLetter(t), aPos+offset, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(GetLetter(t), (Vector2) aPos, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                }
                if (underlining > 0)
                {
                    underlining--;
                    spriteBatch.Draw(_underline, aPos + multiplier*_underlineOffset, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                }
                
                aPos.X = (int) Math.Round(aPos.X + multiplier * (AlphaSpacer + AlphaWidth));
            }

            return aPos; //return the would be placement of the next letter
        }

        public static string GetDeclutteredString(string smartText)
        {
            var result = new StringBuilder(smartText);
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != '@' && result[i] != '\n' && !_charTextures.ContainsKey(result[i]))
                {
                    result[i] = ' ';
                }
            }

            return result.ToString();
        }

        public static int GetSmartTextLength(string smartText)
        {
            int len = 0, maxLen = 0;
            for (var i = 0; i < smartText.Length; i++)
            {
                var t = smartText[i];
                if (t == '\n') //newline
                {
                    len = 0;
                    continue;
                }
                if (t == '@')//smart text
                {
                    if (i + 1 == smartText.Length) continue; //make sure there's actually something after
                    switch (smartText[i + 1])
                    {
                        case 'c':
                            i += 2; //consume c and number
                            break;
                        case 'h': //insert half space
                            i++; //consume h
                            len++; //Not sure about this
                            break;
                        case 'u': //underline for some amount of characters
                        case 's': //sine wave for some amount of characters
                            i++; //consume first @
                            while (smartText[i] != '@')
                            {
                                i++;
                                if (i == smartText.Length) break; //ignore malformed strings
                            }
                            break;
                        default:
                            i++;
                            break;
                    }
                }
                else
                {
                    len++;
                }

                if (len > maxLen) maxLen = len;
            }

            return maxLen;
        }
        public static Coordinate GetSmartTextSize(string smartText, float multiplier=1, bool reducedSpaces=false)
        {
            int currentX = 0;
            var pixelSize = new Coordinate();
            for (var i = 0; i < smartText.Length; i++)
            {
                var t = smartText[i];
                if (t == '\n') //newline
                {
                    pixelSize.Y += (int)Math.Round(multiplier * (AlphaHeight + LineSpacer));
                    pixelSize.X = Math.Max(pixelSize.X, (int) (currentX - multiplier * AlphaSpacer));
                    currentX = 0;
                    continue;
                }
                if (t == ' ') //space
                {
                    if (reducedSpaces)
                        currentX += (int)Math.Round(multiplier * AlphaWidth);
                    else
                        currentX += (int)Math.Round(multiplier * (AlphaWidth + AlphaSpacer));
                    continue;
                }
                if (t == '@')//smart text
                {
                    if (i + 1 == smartText.Length) continue; //make sure there's actually something after
                    switch (smartText[i + 1])
                    {
                        case 'c':
                            i+=2; //consume c and number
                            break;
                        case 'h': //insert half space
                            i++; //consume h
                            if (reducedSpaces)
                                currentX += (int)Math.Round(0.5 * multiplier * AlphaWidth);
                            else
                                currentX += (int)Math.Round(0.5 * multiplier * (AlphaWidth + AlphaSpacer));
                            break;
                        case 'n': //insert half newline skip (can be done mid line as well)
                            i++; //consume n
                            pixelSize.Y += (int)Math.Round(0.5f * multiplier * (AlphaHeight + LineSpacer));
                            break;
                        case 'u': //underline for some amount of characters
                        case 's': //sine wave for some amount of characters
                            i++; //consume initial @
                            while (smartText[i] != '@')
                            {
                                i++;
                                if (i == smartText.Length) break; //ignore malformed strings
                            }
                            break;
                        default:
                            i++;
                            break;
                    }
                }
                else
                {
                    currentX += (int)Math.Round(multiplier * (AlphaWidth + AlphaSpacer));
                }
            }

            pixelSize.Y += (int)Math.Round(multiplier * AlphaHeight);
            pixelSize.X = Math.Max(pixelSize.X, (int)(currentX - multiplier * AlphaSpacer));
            return pixelSize;
        }

        public static string GetSmartSplit(string smartText, int boxWidthInChars)
        {
            string GetNextSplit(string input)
            {
                var a = "";
                foreach (var c in input)
                {
                    a += c;
                    if (c == ' ' || c == '-' || c == '\n') break;
                }
                return a;
            }

            //Splits on spaces, new lines, and dashes
            var newSplit = "";
            var newLine = "";
            if (GetSmartTextLength(smartText) <= boxWidthInChars) return smartText;
            while (smartText.Length > 0)
            {
                var nextChunk = GetNextSplit(smartText);
                var nextLength = GetSmartTextLength(nextChunk);
                if (nextLength > boxWidthInChars)
                {
                    //next chunk is longer than a full line
                    var charsToUse = boxWidthInChars - GetSmartTextLength(newLine);
                    newLine += nextChunk.Substring(0, charsToUse);
                    smartText = smartText.Substring(charsToUse);
                }
                else if (GetSmartTextLength(newLine) + nextLength < boxWidthInChars)
                {
                    //Next chunk can fit on current line
                    newLine += nextChunk;
                    smartText = smartText.Substring(nextChunk.Length);
                    if (newLine.EndsWith("\n"))
                    {
                        //Time to split
                        newSplit += newLine;
                        newLine = "";
                    }
                }
                else
                {
                    //Time to split
                    newSplit += newLine.TrimEnd(' ') + '\n';
                    newLine = "";
                }
            }
            return newSplit + newLine;
        }

    }
}
