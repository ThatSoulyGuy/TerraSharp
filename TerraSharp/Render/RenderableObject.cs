using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using GLFW;
using static OpenGL.GL;
using TerraSharp.Math;
using TerraSharp.Record;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerraSharp.Render
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public vec3 position;
        public vec3 color;
        public vec2 textureCoordinates;

        public static Vertex Register(vec3 position, vec2 textureCoordinates)
        {
            return Vertex.Register(position, new(1.0f, 1.0f, 1.0f), textureCoordinates);
        }

        public static Vertex Register(vec3 position, vec3 color, vec2 textureCoordinates)
        {
            Vertex vertex = new()
            {
                position = position,
                color = color,
                textureCoordinates = textureCoordinates
            };

            return vertex;
        }
    }

    public class RenderableObject
    {
        public NameIDTag Name { get; private set; } = NameIDTag.Register(null);
        public List<Vertex> Vertices { get; private set; } = [];
        public List<uint> Indices { get; private set; } = [];

        public ShaderObject Shader { get; private set; } = new();

        public Transform transform = Transform.Register(new(0.0f, 0.0f, 0.0f));
        public Dictionary<string, Texture> Textures { get; private set; } = [];

        public Dictionary<string, uint> buffers = new()
        {
            {"VAO", 0},
            {"VBO", 0},
            {"EBO", 0}
        };

        public void GenerateCube()
        {
            List<Vertex> vertices =
            [
                Vertex.Register(new (-0.5f, 0.5f, 0.5f), new (0.0f, 0.0f)),
                Vertex.Register(new (-0.5f, 0.5f, -0.5f), new (0.0f, 1.0f)),
                Vertex.Register(new (0.5f, 0.5f, -0.5f), new (1.0f, 1.0f)),
                Vertex.Register(new (0.5f, 0.5f, 0.5f), new (1.0f, 0.0f)),

                Vertex.Register(new (-0.5f, -0.5f, 0.5f), new (0.0f, 0.0f)),
                Vertex.Register(new (-0.5f, -0.5f, -0.5f), new (0.0f, 1.0f)),
                Vertex.Register(new (0.5f, -0.5f, -0.5f), new (1.0f, 1.0f)),
                Vertex.Register(new (0.5f, -0.5f, 0.5f), new (1.0f, 0.0f)),

                Vertex.Register(new (-0.5f, 0.5f, 0.5f), new (0.0f, 0.0f)),
                Vertex.Register(new (-0.5f, -0.5f, 0.5f), new (0.0f, 1.0f)),
                Vertex.Register(new (0.5f, -0.5f, 0.5f), new (1.0f, 1.0f)),
                Vertex.Register(new (0.5f, 0.5f, 0.5f), new (1.0f, 0.0f)),

                Vertex.Register(new (-0.5f, 0.5f, -0.5f), new (0.0f, 0.0f)),
                Vertex.Register(new (-0.5f, -0.5f, -0.5f), new (0.0f, 1.0f)),
                Vertex.Register(new (0.5f, -0.5f, -0.5f), new (1.0f, 1.0f)),
                Vertex.Register(new (0.5f, 0.5f, -0.5f), new (1.0f, 0.0f)),

                Vertex.Register(new (0.5f, 0.5f, -0.5f), new (0.0f, 0.0f)),
                Vertex.Register(new (0.5f, -0.5f, -0.5f), new (0.0f, 1.0f)),
                Vertex.Register(new (0.5f, -0.5f, 0.5f), new (1.0f, 1.0f)),
                Vertex.Register(new (0.5f, 0.5f, 0.5f), new (1.0f, 0.0f)),

                Vertex.Register(new (-0.5f, 0.5f, -0.5f), new (0.0f, 0.0f)),
                Vertex.Register(new (-0.5f, -0.5f, -0.5f), new (0.0f, 1.0f)),
                Vertex.Register(new (-0.5f, -0.5f, 0.5f), new (1.0f, 1.0f)),
                Vertex.Register(new (-0.5f, 0.5f, 0.5f), new (1.0f, 0.0f)),
            ];

            List<uint> indices =
            [
                0, 2, 1,
                0, 3, 2,
                
                4, 5, 6,
                4, 6, 7,
                
                8, 9, 10,
                8, 10, 11,
                
                12, 14, 13,
                12, 15, 14,
                
                16, 18, 17, 
                16, 19, 18,
                
                20, 21, 22,
                20, 22, 23,
            ];

            RegisterTexture("block");
            RegisterData(vertices, indices);
            Generate();
        }

        public void GenerateSquare()
        {
            List<Vertex> vertices =
            [
                Vertex.Register(new(-0.5f, 0.5f, 0.5f), new(0.0f, 0.0f)),
                Vertex.Register(new(-0.5f, -0.5f, 0.5f), new(0.0f, 1.0f)),
                Vertex.Register(new(0.5f, -0.5f, 0.5f), new(1.0f, 1.0f)),
                Vertex.Register(new(0.5f, 0.5f, 0.5f), new(1.0f, 0.0f))
            ];

            List<uint> indices = [0, 1, 3, 3, 1, 2];

            RegisterTexture("block");
            RegisterData(vertices, indices);
            Generate();
        }

        public void Generate()
        {
            Shader.Generate();

            buffers["VAO"] = glGenVertexArray();
            buffers["VBO"] = glGenBuffer();
            buffers["EBO"] = glGenBuffer();

            glBindVertexArray(buffers["VAO"]);

            glBindBuffer(GL_ARRAY_BUFFER, buffers["VBO"]);

            unsafe
            {
                fixed (Vertex* vertex = &Vertices.ToArray()[0])
                    glBufferData(GL_ARRAY_BUFFER, Marshal.SizeOf(typeof(Vertex)) * Vertices.Count, vertex, GL_DYNAMIC_DRAW);
            

                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, buffers["EBO"]);

            
                fixed (uint* index = &Indices.ToArray()[0])
                    glBufferData(GL_ELEMENT_ARRAY_BUFFER, Marshal.SizeOf(typeof(uint)) * Indices.Count, index, GL_DYNAMIC_DRAW);
            
                glVertexAttribPointer(0, 3, GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "position"));
                glEnableVertexAttribArray(0);

                glVertexAttribPointer(1, 3, GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "color"));
                glEnableVertexAttribArray(1);

                glVertexAttribPointer(2, 2, GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "textureCoordinates"));
                glEnableVertexAttribArray(2);
            }

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            Shader.Use();
            Shader.SetUniform("diffuse", 0);
        }

        public void ReGenerate()
        {
            buffers["VAO"] = glGenVertexArray();
            buffers["VBO"] = glGenBuffer();
            buffers["EBO"] = glGenBuffer();

            glBindVertexArray(buffers["VAO"]);

            glBindBuffer(GL_ARRAY_BUFFER, buffers["VBO"]);

            unsafe
            {
                fixed (Vertex* vertex = &Vertices.ToArray()[0])
                    glBufferData(GL_ARRAY_BUFFER, Marshal.SizeOf(typeof(Vertex)) * Vertices.Count, vertex, GL_DYNAMIC_DRAW);


                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, buffers["EBO"]);


                fixed (uint* index = &Indices.ToArray()[0])
                    glBufferData(GL_ELEMENT_ARRAY_BUFFER, Marshal.SizeOf(typeof(uint)) * Indices.Count, index, GL_DYNAMIC_DRAW);

                glVertexAttribPointer(0, 3, GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "position"));
                glEnableVertexAttribArray(0);

                glVertexAttribPointer(1, 3, GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "color"));
                glEnableVertexAttribArray(1);

                glVertexAttribPointer(2, 2, GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), Marshal.OffsetOf(typeof(Vertex), "textureCoordinates"));
                glEnableVertexAttribArray(2);
            }

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }

        public void RegisterData(List<Vertex> vertices, List<uint> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public void RegisterTexture(string texture)
        {
            Texture textureReference = TextureManager.GetTexture(texture);

            Textures.Add(textureReference.Name, textureReference);

            Textures[texture].Generate(TextureProperties.Register(TextureWrapping.REPEAT, TextureFiltering.NEAREST));
        }

        public void CleanUp()
        {
            glDeleteVertexArray(buffers["VAO"]);

            foreach (string key in buffers.Keys)
            {
                if (key == "VAO")
                    continue;
                
                glDeleteBuffer(buffers[key]);
            }    

            buffers.Clear();

            foreach (Texture texture in Textures.Values)
                texture.CleanUp();

            Textures.Clear();

            Shader.CleanUp();
        }

        public static RenderableObject Register(NameIDTag name, List<Vertex> vertices, List<uint> indices)
        {
            return Register(name, vertices, indices, "default");
        }

        public static RenderableObject Register(NameIDTag name, List<Vertex> vertices, List<uint> indices, string shader)
        {
            RenderableObject renderable = new()
            {
                Name = name,
                Vertices = vertices,
                Indices = indices,

                Shader = ShaderManager.GetShader(shader)
            };

            return renderable;
        }
    }
}
