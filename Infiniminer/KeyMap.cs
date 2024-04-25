using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; 
using System.Text;
using System.Diagnostics;
using LibreLancer;

namespace Infiniminer
{
    public class KeyMap
    {
        Dictionary<Keys, string> keyMap;

        public KeyMap()
        {
            keyMap = new Dictionary<Keys, string>();
            keyMap.Add(Keys.A, "aA");
            keyMap.Add(Keys.B, "bB");
            keyMap.Add(Keys.C, "cC");
            keyMap.Add(Keys.D, "dD");
            keyMap.Add(Keys.E, "eE");
            keyMap.Add(Keys.F, "fF");
            keyMap.Add(Keys.G, "gG");
            keyMap.Add(Keys.H, "hH");
            keyMap.Add(Keys.I, "iI");
            keyMap.Add(Keys.J, "jJ");
            keyMap.Add(Keys.K, "kK");
            keyMap.Add(Keys.L, "lL");
            keyMap.Add(Keys.M, "mM");
            keyMap.Add(Keys.N, "nN");
            keyMap.Add(Keys.O, "oO");
            keyMap.Add(Keys.P, "pP");
            keyMap.Add(Keys.Q, "qQ");
            keyMap.Add(Keys.R, "rR");
            keyMap.Add(Keys.S, "sS");
            keyMap.Add(Keys.T, "tT");
            keyMap.Add(Keys.U, "uU");
            keyMap.Add(Keys.V, "vV");
            keyMap.Add(Keys.W, "wW");
            keyMap.Add(Keys.X, "xX");
            keyMap.Add(Keys.Y, "yY");
            keyMap.Add(Keys.Z, "zZ");
            keyMap.Add(Keys.D0, "0)");
            keyMap.Add(Keys.D1, "1!");
            keyMap.Add(Keys.D2, "2@");
            keyMap.Add(Keys.D3, "3#");
            keyMap.Add(Keys.D4, "4$");
            keyMap.Add(Keys.D5, "5%");
            keyMap.Add(Keys.D6, "6^");
            keyMap.Add(Keys.D7, "7&");
            keyMap.Add(Keys.D8, "8*");
            keyMap.Add(Keys.D9, "9(");
            keyMap.Add(Keys.Keypad0, "00");
            keyMap.Add(Keys.Keypad1, "11");
            keyMap.Add(Keys.Keypad2, "22");
            keyMap.Add(Keys.Keypad3, "33");
            keyMap.Add(Keys.Keypad4, "44");
            keyMap.Add(Keys.Keypad5, "55");
            keyMap.Add(Keys.Keypad6, "66");
            keyMap.Add(Keys.Keypad7, "77");
            keyMap.Add(Keys.Keypad8, "88");
            keyMap.Add(Keys.Keypad9, "99");
            keyMap.Add(Keys.Space, "  ");
            keyMap.Add(Keys.Minus, "-_");
            keyMap.Add(Keys.Equals, "=+");
            keyMap.Add(Keys.Backslash, "\\|");
            keyMap.Add(Keys.RightBracket, "]}");
            keyMap.Add(Keys.Comma, ",<");
            keyMap.Add(Keys.KeypadMinus, "-_");
            keyMap.Add(Keys.LeftBracket, "[{");
            keyMap.Add(Keys.Period, ".>");
            keyMap.Add(Keys.Backslash, "\\|");
            keyMap.Add(Keys.KeypadPlus, "=+");
            keyMap.Add(Keys.Slash, "/?");
            keyMap.Add(Keys.Apostrophe, "'\"");
            keyMap.Add(Keys.Semicolon, ";:");
            //keyMap.Add(Keys., "`~");
        }

        public bool IsKeyMapped(Keys key)
        {
            return keyMap.ContainsKey(key);
        }

        public string TranslateKey(Keys key, bool shiftDown)
        {
            if (!IsKeyMapped(key))
                return "";

            if (shiftDown)
                return keyMap[key].Substring(1, 1);
            else
                return keyMap[key].Substring(0, 1);
        }
    }
}