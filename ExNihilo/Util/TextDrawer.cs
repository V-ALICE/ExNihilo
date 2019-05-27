using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExNihilo.Util
{
    public static class TextDrawer
    {
        //These are the positions in the alphabet image: ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 -?!.,'"()[]%/;:+=_|<>\
        private static readonly Dictionary<char, int> Char = new Dictionary<char, int>
        {
            {'A', 0},  {'B', 1},  {'C', 2},  {'D', 3},  {'E', 4},  {'F', 5},  {'G', 6}, {'H', 7}, {'I', 8}, {'J', 9},
            {'K', 10}, {'L', 11}, {'M', 12}, {'N', 13}, {'O', 14}, {'P', 15}, {'Q', 16}, {'R', 17}, {'S', 18}, {'T', 19},
            {'U', 20}, {'V', 21}, {'W', 22}, {'X', 23}, {'Y', 24}, {'Z', 25},
            {'1', 26}, {'2', 27}, {'3', 28}, {'4', 29}, {'5', 30}, {'6', 31}, {'7', 32}, {'8', 33}, {'9', 34}, {'0', 35},
            {' ', 36}, {'-', 37}, {'?', 38}, {'!', 39}, {'.', 40}, {',', 41}, {'\'', 42}, {'\"', 43}, {'(', 44}, {')', 45},
            {'[', 46}, {']', 47}, {'%', 48}, {'/', 49}, {';', 50}, {':', 51}, {'+', 52}, {'=', 53}, {'_', 54}, {'|', 55},
            {'<', 56}, {'>', 57}, {'\\', 58}
        };

        public static int AlphaWidth, AlphaHeight, AlphaSpacer; //In pixels
        private static Dictionary<char, Texture2D> CharTextures;

        public static void Initialize(GraphicsDevice device, Texture2D alphabet)
        {
            ConfigureTextSize(2);
            CharTextures = new Dictionary<char, Texture2D>();
            foreach (var chr in Char)
            {
                var letter = new Rectangle(AlphaWidth * chr.Value, 0, AlphaWidth, AlphaHeight);
                CharTextures.Add(chr.Key, TextureUtilities.GetSubTexture(device, alphabet, letter));
            }
        }

        public static void ConfigureTextSize(int multiplier)
        {
            AlphaWidth = 3 * multiplier;
            AlphaHeight = 5 * multiplier;
            AlphaSpacer = multiplier;
        }

        public static Texture2D GetLetter(char let)
        {
            try
            {
                return CharTextures[let.ToString().ToUpper()[0]];
            }
            catch (KeyNotFoundException)
            {
                return CharTextures[' '];
            }
        }       

        public static Vector2 DrawDumbText(SpriteBatch spriteBatch, Vector2 pos, string text, int multiplier, Color c)
        {
            var aPos = Utilities.Copy(pos);
            foreach (var t in text)
            {
                spriteBatch.Draw(GetLetter(t), aPos, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                aPos.X = (int) Math.Round(aPos.X + multiplier * (AlphaSpacer + AlphaWidth));
            }
            return aPos;
        }

        public static Vector2 DrawSmartText(SpriteBatch spriteBatch, Vector2 pos, string smartText, int multiplier, 
            bool reducedSpaces, params Color[] colors)
        {
            //supports up to 10 different colors at a time
            //assumes line has already been split correctly, including buffers
            var aPos = Utilities.Copy(pos);
            var oldX = aPos.X;
            var c = colors.Length > 0 ? colors[0] : Color.Black;
            for (var i = 0; i < smartText.Length; i++)
            {
                var t = smartText[i];
                if (t == '\n') //newline
                {
                    aPos.Y = (int)Math.Round(aPos.Y + multiplier * (AlphaHeight + 2 * AlphaSpacer));
                    aPos.X = oldX;
                    continue;
                }
                if (t == ' ') //space
                {
                    if (reducedSpaces)
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
                            i++; //consume c
                            if (i + 1 == smartText.Length) return aPos; //make sure there's actually something after
                            if (char.IsDigit(smartText, i+1)) //Make sure it's actually a number after
                            {
                                i++; //consume digit
                                var param = int.Parse(smartText[i].ToString());
                                if (colors.Length > param) c = colors[param]; //change color if there's actually a color at that position
                            }
                            break;
                        case 'h': //insert half space
                            i++; //consume h
                            if (reducedSpaces)
                                aPos.X = (int)Math.Round(aPos.X + 0.5 * multiplier * AlphaWidth);
                            else
                                aPos.X = (int)Math.Round(aPos.X + 0.5 * multiplier * (AlphaWidth + AlphaSpacer));
                            break;
                        case 'n': //insert half newline skip (can be done mid line as well)
                            i++; //consume n
                            aPos.Y = (int) Math.Round(aPos.Y + 0.5f * multiplier * (AlphaHeight + 2 * AlphaSpacer));
                            break;
                    }
                    continue; //don't need to draw anything right after a format change
                }
                spriteBatch.Draw(GetLetter(t), aPos, null, c, 0, Vector2.Zero, multiplier, SpriteEffects.None, 0);
                aPos.X = (int) Math.Round(aPos.X + multiplier * (AlphaSpacer + AlphaWidth));
            }

            return aPos; //return the would be placement of the next letter
        }

    }
}
