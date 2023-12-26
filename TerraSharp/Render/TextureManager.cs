using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Core;

namespace TerraSharp.Render
{
    public class TextureManager
    {
        public static Dictionary<string, Texture> RegisteredTextures { get; private set; } = [];

        public static void RegisterTexture(Texture texture)
        {
            Logger.WriteConsole($"Registering Texture: '{texture.Name}'", LogLevel.INFO);

            RegisteredTextures.Add(texture.Name, texture);
        }

        public static Texture GetTexture(string name)
        {
            return RegisteredTextures[name].Copy();
        }

        public static void CleanUp()
        {
            foreach (Texture texture in RegisteredTextures.Values)
                texture.CleanUp();

            RegisteredTextures.Clear();
        }
    }
}
