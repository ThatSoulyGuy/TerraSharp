using TerraSharp.Core;
using GlmSharp;
using TerraSharp.Render;
using TerraSharp.Record;
using TerraSharp.World;
using TerraSharp.Thread;

namespace TerraSharp
{
    public class TerraSharp
    {
        public static RenderableObject renderableObject = new();
        public static Player.Player player = new();

        public static void Main(string[] args)
        {
            Logger.WriteConsole("Hello, TerraSharp!", LogLevel.INFO);

            Window.Initialize();

            ShaderManager.RegisterShader(ShaderObject.Register("shaders/default", "default"));
            TextureManager.RegisterTexture(Texture.Register("textures/block.png", "block"));
            TextureManager.RegisterTexture(Texture.Register("textures/terrain.png", "atlas"));

            Window.GenerateWindow("TerraCraft 0.1.7", new ivec2(750, 450), new vec3(0.0f, 0.45f, 0.75f));

            player.Initialize(new(0, 30, 0));

            World.World.StartUpdating();

            while (!Window.ShouldClose())
            {
                Window.UpdateColors();

                player.Update();
                
                MainThreadExecutor.UpdateTasks();

                World.World.playerPosition = player.Transform.position;
                Renderer.RenderObjects(player.Camera);

                Window.UpdateBuffers();
            }

            World.World.StopUpdating();
            Renderer.CleanUp();
            ShaderManager.CleanUp();
            TextureManager.CleanUp();
            Window.CleanUp();
        }
    }
}