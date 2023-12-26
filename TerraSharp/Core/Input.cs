using GLFW;
using GlmSharp;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Core
{
    public class Input
    {
        public static vec2 mousePosition = new();

        public static bool GetKey(Keys key, InputState state)
        {
            return Glfw.GetKey(Window.raw, key) == state;
        }

        public static bool GetMouseButton(MouseButton button, InputState state)
        {
            return Glfw.GetMouseButton(Window.raw, button) == state;
        }

        public static vec2 GetMousePosition()
        {
            return mousePosition;
        }

        public static void SetMouseMode(CursorMode mode)
        {
            Glfw.SetInputMode(Window.raw, InputMode.Cursor, (int)mode);
        }
    }
}
