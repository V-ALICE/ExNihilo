using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ExNihilo.Util
{
    public static class TypingKeyboard
    {
        // ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 -?!.,'"()[]%/;:+=_|<>\
        // No @#$^&*{}~`
        private static readonly Dictionary<Keys, char> _defaultKeyboard = new Dictionary<Keys, char>
        {
            {Keys.A, 'a'}, {Keys.B, 'b'}, {Keys.C, 'c'}, {Keys.D, 'd'}, {Keys.E, 'e'}, {Keys.F, 'f'}, {Keys.G, 'g'},
            {Keys.H, 'h'}, {Keys.I, 'i'}, {Keys.J, 'j'}, {Keys.K, 'k'}, {Keys.L, 'l'}, {Keys.M, 'm'}, {Keys.N, 'n'},
            {Keys.O, 'o'}, {Keys.P, 'p'}, {Keys.Q, 'q'}, {Keys.R, 'r'}, {Keys.S, 's'}, {Keys.T, 't'}, {Keys.U, 'u'},
            {Keys.V, 'v'}, {Keys.W, 'w'}, {Keys.X, 'x'}, {Keys.Y, 'y'}, {Keys.Z, 'z'},
            {Keys.D1, '1'}, {Keys.D2, '2'}, {Keys.D3, '3'}, {Keys.D4, '4'}, {Keys.D5, '5'}, {Keys.D6, '6'}, {Keys.D7, '7'},
            {Keys.D8, '8'}, {Keys.D9, '9'}, {Keys.D0, '0'},
            /*{Keys.OemTilde, '`'},*/ {Keys.OemMinus, '-'}, {Keys.OemPlus, '='}, {Keys.OemOpenBrackets, '['}, {Keys.OemCloseBrackets, ']'},
            {Keys.OemBackslash, '\\'}, {Keys.OemSemicolon, ';'}, {Keys.OemQuotes, '\''}, {Keys.OemComma, ','}, {Keys.OemPeriod, '.'},
            {Keys.OemQuestion, '/'}, {Keys.Space, ' '}
        };

        private static readonly Dictionary<Keys, char> _shiftedKeyboard = new Dictionary<Keys, char>
        {
            {Keys.A, 'A'}, {Keys.B, 'B'}, {Keys.C, 'C'}, {Keys.D, 'D'}, {Keys.E, 'E'}, {Keys.F, 'F'}, {Keys.G, 'G'},
            {Keys.H, 'H'}, {Keys.I, 'I'}, {Keys.J, 'J'}, {Keys.K, 'K'}, {Keys.L, 'L'}, {Keys.M, 'M'}, {Keys.N, 'N'},
            {Keys.O, 'O'}, {Keys.P, 'P'}, {Keys.Q, 'Q'}, {Keys.R, 'R'}, {Keys.S, 'S'}, {Keys.T, 'T'}, {Keys.U, 'U'},
            {Keys.V, 'V'}, {Keys.W, 'W'}, {Keys.X, 'X'}, {Keys.Y, 'Y'}, {Keys.Z, 'Z'},
            {Keys.D1, '!'}, /*{Keys.D2, '@'}, {Keys.D3, '#'}, {Keys.D4, '$'},*/ {Keys.D5, '%'}, /*{Keys.D6, '^'}, {Keys.D7, '&'},*/
            /*{Keys.D8, '*'},*/ {Keys.D9, '('}, {Keys.D0, ')'},
            /*{Keys.OemTilde, '~'},*/ {Keys.OemMinus, '_'}, {Keys.OemPlus, '+'}, /*{Keys.OemOpenBrackets, '{'}, {Keys.OemCloseBrackets, '}'},*/
            {Keys.OemBackslash, '|'}, {Keys.OemSemicolon, ':'}, {Keys.OemQuotes, '\"'}, {Keys.OemComma, '<'}, {Keys.OemPeriod, '>'},
            {Keys.OemQuestion, '?'}, {Keys.Space, ' '}
        };

        private static readonly Dictionary<Keys, char> _capsLockKeyboard = new Dictionary<Keys, char>
        {
            {Keys.A, 'A'}, {Keys.B, 'B'}, {Keys.C, 'C'}, {Keys.D, 'D'}, {Keys.E, 'E'}, {Keys.F, 'F'}, {Keys.G, 'G'},
            {Keys.H, 'H'}, {Keys.I, 'I'}, {Keys.J, 'J'}, {Keys.K, 'K'}, {Keys.L, 'L'}, {Keys.M, 'M'}, {Keys.N, 'N'},
            {Keys.O, 'O'}, {Keys.P, 'P'}, {Keys.Q, 'Q'}, {Keys.R, 'R'}, {Keys.S, 'S'}, {Keys.T, 'T'}, {Keys.U, 'U'},
            {Keys.V, 'V'}, {Keys.W, 'W'}, {Keys.X, 'X'}, {Keys.Y, 'Y'}, {Keys.Z, 'Z'},
            {Keys.D1, '1'}, {Keys.D2, '2'}, {Keys.D3, '3'}, {Keys.D4, '4'}, {Keys.D5, '5'}, {Keys.D6, '6'}, {Keys.D7, '7'},
            {Keys.D8, '8'}, {Keys.D9, '9'}, {Keys.D0, '0'},
            /*{Keys.OemTilde, '`'},*/ {Keys.OemMinus, '-'}, {Keys.OemPlus, '='}, {Keys.OemOpenBrackets, '['}, {Keys.OemCloseBrackets, ']'},
            {Keys.OemBackslash, '\\'}, {Keys.OemSemicolon, ';'}, {Keys.OemQuotes, '\''}, {Keys.OemComma, ','}, {Keys.OemPeriod, '.'},
            {Keys.OemQuestion, '/'}, {Keys.Space, ' '}
        };

        private static KeyboardState _oldState;
        public static string GetText()
        {
            var list = Keyboard.GetState();
            var text = "";
            foreach (var key in list.GetPressedKeys())
            {
                if (_oldState.IsKeyDown(key)) continue;
                try
                {
                    if (list.IsKeyDown(Keys.LeftShift) || list.IsKeyDown(Keys.RightShift))
                    {
                        text += _shiftedKeyboard[key];
                    }
                    else if (Control.IsKeyLocked(System.Windows.Forms.Keys.CapsLock))
                    {
                        text += _capsLockKeyboard[key];
                    }
                    else
                    {
                        text += _defaultKeyboard[key];
                    }
                }
                catch (KeyNotFoundException) {}
            }

            _oldState = list;
            return text;
        }
    }
}