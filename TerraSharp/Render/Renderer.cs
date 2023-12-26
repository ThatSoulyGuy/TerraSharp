using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using GLFW;
using static OpenGL.GL;
using TerraSharp.Record;
using TerraSharp.Math;
using TerraSharp.Core;

namespace TerraSharp.Render
{
    public class Renderer
    {
        private static Dictionary<NameIDTag, RenderableObject> RegisteredObjects { get; set; } = [];
        private static mat4 translation = mat4.Identity;

        public static void RegisterRenderableObject(RenderableObject renderableObject)
        {
            if(RegisteredObjects.ContainsKey(renderableObject.Name))
            {
                RegisteredObjects[renderableObject.Name] = renderableObject;
                return;
            }

            RegisteredObjects.Add(renderableObject.Name, renderableObject);
        }

        public static void RemoveRenderableObject(NameIDTag name)
        {
            RegisteredObjects[name].CleanUp();
            RegisteredObjects.Remove(name);
        }

        public static RenderableObject GetObject(NameIDTag name)
        {
            return RegisteredObjects[name];
        }

        public static void RenderObjects(Camera camera)
        {
            unsafe
            {
                foreach (RenderableObject renderableObject in RegisteredObjects.Values)
                {
                    translation = mat4.Identity;

                    mat4 rotationX = mat4.RotateX(glm.Radians(renderableObject.transform.rotation.x));
                    mat4 rotationY = mat4.RotateY(glm.Radians(renderableObject.transform.rotation.y));
                    mat4 rotationZ = mat4.RotateZ(glm.Radians(renderableObject.transform.rotation.z));
                    mat4 rotation = rotationX * rotationY * rotationZ;

                    translation = mat4.Translate(renderableObject.transform.position);

                    mat4 model = translation * rotation;

                    renderableObject.Shader.Use();
                    renderableObject.Shader.SetUniform("projection", camera.projection);
                    renderableObject.Shader.SetUniform("view", camera.view);
                    renderableObject.Shader.SetUniform("model", model);

                    translation = mat4.Identity;

                    glBindVertexArray(renderableObject.buffers["VAO"]);
                    glDrawElements(GL_TRIANGLES, renderableObject.Indices.Count, GL_UNSIGNED_INT, NULL);

                    int error = GetError();

                    if (error != GL_NO_ERROR)
                        Logger.WriteConsole($"OpenGL error detected: {error}", LogLevel.ERROR);
                }
            }
        }

        public static void CleanUp()
        {
            foreach (RenderableObject renderableObject in RegisteredObjects.Values)
                renderableObject.CleanUp();
            
            RegisteredObjects.Clear();
        }
    }
}
