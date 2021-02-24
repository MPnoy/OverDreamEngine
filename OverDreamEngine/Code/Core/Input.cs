using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ODEngine.Core
{
    public static class Input
    {
        public static List<MouseButtonEventArgs> mouseEvents = new List<MouseButtonEventArgs>();
        public static List<MouseButton> mouseDowns = new List<MouseButton>();
        public static List<MouseButton> mouseUps = new List<MouseButton>();
        public static bool consoleOpened = false;

        private static readonly bool[] stateKeys = new bool[(int)Keys.LastKey];
        private static readonly bool[] stateKeysPrevious = new bool[(int)Keys.LastKey];
        public static List<Keys> downedKeys = new List<Keys>((int)Keys.LastKey);

        private static readonly bool[] stateMouse = new bool[(int)MouseButton.Last];
        private static readonly bool[] stateMousePrevious = new bool[(int)MouseButton.Last];

        public static float mouseWheelDelta;
        public static Vector3 mousePos;

        public static void MouseMove(MouseMoveEventArgs e)
        {
            mousePos = new Vector3(e.X, e.Y, 0);
        }

        public static void KeyChange(MouseButtonEventArgs e)
        {
            mouseEvents.Add(e);
            if (e.IsPressed)
            {
                mouseDowns.Add(e.Button);
            }
            else
            {
                mouseUps.Add(e.Button);
            }
            stateMouse[(int)e.Button] = e.IsPressed;
        }

        public static void KeyChange(KeyboardKeyEventArgs e, bool isDown)
        {
            stateKeys[(int)e.Key] = isDown;
            if (isDown)
            {
                downedKeys.Add(e.Key);
            }
        }

        public static void Update()
        {
            mouseEvents.Clear();
            mouseDowns.Clear();
            mouseUps.Clear();
            stateKeys.CopyTo(stateKeysPrevious, 0);
            stateMouse.CopyTo(stateMousePrevious, 0);
            downedKeys.Clear();
        }

        public static bool GetKey(MouseButton mouseButton)
        {
            return stateMouse[(int)mouseButton];
        }

        public static bool GetKey(Keys key, bool ignoreConsole = false)
        {
            return consoleOpened && !ignoreConsole ? false : stateKeys[(int)key];
        }

        public static bool GetKeyDown(MouseButton mouseButton)
        {
            return stateMouse[(int)mouseButton] && !stateMousePrevious[(int)mouseButton];
        }

        public static bool GetKeyDown(Keys key, bool ignoreConsole = false, bool removeAfterReturn = true)
        {
            var ret = consoleOpened && !ignoreConsole ? false : stateKeys[(int)key] && !stateKeysPrevious[(int)key];
            if (ret && removeAfterReturn)
            {
                stateKeys[(int)key] = false;
            }
            return ret;
        }

        public static bool GetKeyUp(MouseButton mouseButton)
        {
            return !stateMouse[(int)mouseButton] && stateMousePrevious[(int)mouseButton];
        }

        public static bool GetKeyUp(Keys key, bool ignoreConsole = false, bool removeAfterReturn = true)
        {
            var ret = consoleOpened && !ignoreConsole ? false : !stateKeys[(int)key] && stateKeysPrevious[(int)key];
            if (ret && removeAfterReturn)
            {
                stateKeysPrevious[(int)key] = false;
            }
            return ret;
        }

        public static float GetMouseWheel()
        {
            return mouseWheelDelta;
        }
    }
}