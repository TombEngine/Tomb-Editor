#nullable enable

using Keys = TombLib.Input.EditorKey;
using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace TombEditor
{
    public enum KeyboardLayout
    {
        Qwerty,
        Qwertz,
        Azerty
    }

    public static class KeyboardLayoutDetector
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Alternative route that failed:
        // https://stackoverflow.com/questions/23616205/how-to-detect-keyboard-layout

        private const int KL_NAMELENGTH = 9;
        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(int uCode, int uMapType);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint threadId);
        private const int MAPVK_VK_TO_VSC = 0;
        private const int MAPVK_VSC_TO_VK = 1;

        private const int _keyCount = 13;
        private static readonly int[] _scancodes = new int[_keyCount]
        {
            16, // On english qwerty keyboard: Q
            30, // On english qwerty keyboard: A
            17, // On english qwerty keyboard: W
            31, // On english qwerty keyboard: S
            18, // On english qwerty keyboard: E
            32, // On english qwerty keyboard: D
            19, // On english qwerty keyboard: R
            33, // On english qwerty keyboard: F
            20, // On english qwerty keyboard: T
            34, // On english qwerty keyboard: G
            21, // On english qwerty keyboard: Y
            35, // On english qwerty keyboard: H
            44 // On english qwerty keyboard: Z
        };

        private static readonly Keys[] _qwertyKeys = new Keys[_keyCount]
        {
            Keys.Q, // On english qwerty keyboard: Q
            Keys.A, // On english qwerty keyboard: A
            Keys.W, // On english qwerty keyboard: W
            Keys.S, // On english qwerty keyboard: S
            Keys.E, // On english qwerty keyboard: E
            Keys.D, // On english qwerty keyboard: D
            Keys.R, // On english qwerty keyboard: R
            Keys.F, // On english qwerty keyboard: F
            Keys.T, // On english qwerty keyboard: T
            Keys.G, // On english qwerty keyboard: G
            Keys.Y, // On english qwerty keyboard: Y
            Keys.H, // On english qwerty keyboard: H
            Keys.Z // On english qwerty keyboard: Z
        };

        private static readonly Keys[] _qwertzKeys = new Keys[_keyCount]
        {
            Keys.Q, // On english qwerty keyboard: Q
            Keys.A, // On english qwerty keyboard: A
            Keys.W, // On english qwerty keyboard: W
            Keys.S, // On english qwerty keyboard: S
            Keys.E, // On english qwerty keyboard: E
            Keys.D, // On english qwerty keyboard: D
            Keys.R, // On english qwerty keyboard: R
            Keys.F, // On english qwerty keyboard: F
            Keys.T, // On english qwerty keyboard: T
            Keys.G, // On english qwerty keyboard: G
            Keys.Z, // On english qwerty keyboard: Y
            Keys.H, // On english qwerty keyboard: H
            Keys.Y // On english qwerty keyboard: Z
        };

        private static readonly Keys[] _azertyKeys = new Keys[_keyCount]
        {
            Keys.A, // On english qwerty keyboard: Q
            Keys.Q, // On english qwerty keyboard: A
            Keys.Z, // On english qwerty keyboard: W
            Keys.S, // On english qwerty keyboard: S
            Keys.E, // On english qwerty keyboard: E
            Keys.D, // On english qwerty keyboard: D
            Keys.R, // On english qwerty keyboard: R
            Keys.F, // On english qwerty keyboard: F
            Keys.T, // On english qwerty keyboard: T
            Keys.G, // On english qwerty keyboard: G
            Keys.Y, // On english qwerty keyboard: Y
            Keys.H, // On english qwerty keyboard: H
            Keys.W // On english qwerty keyboard: Z
        };

        static KeyboardLayoutDetector()
        {
            ReloadKeyboardLayout();
        }

        private static void ReloadKeyboardLayout()
        {
            // Get keyboard layout information
            Keys[] keys;
            try
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        keys = new Keys[_keyCount];
                        for (int i = 0; i < keys.Length; ++i)
                            keys[i] = (Keys)MapVirtualKey(_scancodes[i], MAPVK_VSC_TO_VK);
                        break;
                    default:
                        logger.Warn("Getting keyboard layout information not supported for platform '" + Environment.OSVersion.Platform + "'.");
                        keys = _qwertyKeys;
                        break;
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Getting keyboard information failed on platform '" + Environment.OSVersion.Platform + "'.");
                keys = _qwertyKeys;
            }

            // Recognize keyboard layout
            if (keys.SequenceEqual(_qwertyKeys))
                _currentLayout = KeyboardLayout.Qwerty;
            else if (keys.SequenceEqual(_qwertzKeys))
                _currentLayout = KeyboardLayout.Qwertz;
            else if (keys.SequenceEqual(_azertyKeys))
                _currentLayout = KeyboardLayout.Azerty;
            else
                _currentLayout = KeyboardLayout.Qwerty;

            // Debug output
            string keysString = "{";
            for (int i = 0; i < keys.Length; ++i)
                keysString += (i == 0 ? "" : ", ") + keys[i];
            keysString += "}";

            logger.Info("Keyboard layout chosen: '" + _currentLayout + "' (" +
                "Language: " + CultureInfo.CurrentCulture.EnglishName +
                ", Input Language: " + GetInputLanguageName() + ", Decision key map " + keysString + ")");
        }

        private static string GetInputLanguageName()
        {
            try
            {
                return CultureInfo.GetCultureInfo((int)(GetKeyboardLayout(0).ToInt64() & 0xffff)).DisplayName;
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.CurrentCulture.DisplayName;
            }
        }

        private static KeyboardLayout _currentLayout;

        public static KeyboardLayout KeyboardLayout => _currentLayout;
    }
}
