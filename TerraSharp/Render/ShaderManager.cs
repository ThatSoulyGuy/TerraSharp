using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Core;

namespace TerraSharp.Render
{
    public class ShaderManager
    {
        public static Dictionary<string, ShaderObject> registeredShaders = new();

        public static void RegisterShader(ShaderObject shader)
        {
            Logger.WriteConsole("Registering ShaderObject: '" + shader.Name + "'.", LogLevel.INFO);

            registeredShaders.Add(shader.Name, shader);
        }

        public static ShaderObject GetShader(string name)
        {
            return registeredShaders[name].Copy();
        }

        public static void CleanUp()
        {
            foreach (ShaderObject shader in registeredShaders.Values)
                shader.CleanUp();
        }
    }
}
