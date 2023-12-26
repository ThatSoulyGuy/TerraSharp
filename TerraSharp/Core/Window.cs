using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using GLFW;
using static OpenGL.GL;

namespace TerraSharp.Core
{
    

    public class Window
    {
        public static string? Title;
        public static ivec2 Size;
        public static ivec2 Position;
        public static vec3 Color;
        public static GLFW.Window raw;

        private static void FSC(nint window, int width, int height)
        {
            glViewport(0, 0, width, height);
        }

        static void MouseCallback(nint window, double xposIn, double yposIn)
        {
            Input.mousePosition.x = (float)xposIn;
            Input.mousePosition.y = (float)yposIn;
        }

        public static void Initialize()
        {
            if (!Glfw.Init())
                throw new System.Exception("Failed to initialize GLFW");

            Glfw.WindowHint(Hint.Samples, 16);
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);
            Glfw.WindowHint(Hint.Resizable, true);
        }

        public static void GenerateWindow(string title, ivec2 size, vec3 color)
        {
            Title = title;
            Size = size;
            Position = new();
            Color = color;

            raw = Glfw.CreateWindow(size.x, size.y, title, GLFW.Monitor.None, GLFW.Window.None);

            Center();

            Glfw.MakeContextCurrent(raw);

            Glfw.SetFramebufferSizeCallback(raw, FSC);
            Glfw.SetCursorPositionCallback(raw, MouseCallback);

            Import(Glfw.GetProcAddress);

            Glfw.SwapInterval(1);

            glEnable(GL_MULTISAMPLE);
            glEnable(GL_DEPTH_TEST);
            glEnable(GL_CULL_FACE);
            glCullFace(GL_BACK);
        }

        public static void Center()
        {
            Glfw.GetWindowSize(raw, out Size.x, out Size.y);

            VideoMode video = Glfw.GetVideoMode(Glfw.PrimaryMonitor);

            Glfw.SetWindowPosition(raw, (video.Width - Size.x) / 2, (video.Height - Size.y) / 2);
            Glfw.GetWindowPosition(raw, out Position.x, out Position.y);
        }

        public static bool ShouldClose()
        {
            return Glfw.WindowShouldClose(raw);
        }

        public static void UpdateColors()
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            glClearColor(Color.x, Color.y, Color.z, 1.0f);

            Glfw.GetWindowSize(raw, out Size.x, out Size.y);
            Glfw.GetWindowPosition(raw, out Position.x, out Position.y);
        }

        public static void UpdateBuffers()
        {
            Glfw.SwapBuffers(raw);
            Glfw.PollEvents();
        }

        public static void CleanUp()
        {
            Glfw.DestroyWindow(raw);

            Glfw.Terminate();
        }
    }
}
