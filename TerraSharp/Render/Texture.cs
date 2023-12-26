using GLFW;
using GlmSharp;
using static OpenGL.GL;
using StbiSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TerraSharp.Core;

namespace TerraSharp.Render
{
    public enum TextureWrapping
    {
        REPEAT = GL_REPEAT,
        MIRRORED_REPEAT = GL_MIRRORED_REPEAT,
        CLAMP_TO_EDGE = GL_CLAMP_TO_EDGE,
        CLAMP_TO_BORDER = GL_CLAMP_TO_BORDER
    }

    public enum TextureFiltering
    {
        NEAREST = GL_NEAREST,
        LINEAR = GL_LINEAR
    }

    public struct TextureProperties
    {
        public TextureWrapping Wrapping;
        public TextureFiltering Filtering;

        public static TextureProperties Register(TextureWrapping wrapping, TextureFiltering filtering)
        {
            TextureProperties properties = new()
            {
                Wrapping = wrapping,
                Filtering = filtering
            };

            return properties;
        }
    }

    public class Texture : ICloneable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Comp { get; private set; }

        public string Name { get; private set; } = "";
        public string OriginalPath { get; private set; } = "";
        public string LocalPath { get; private set; } = "";
        private unsafe byte* Image { get; set; } = null;

        public uint Id { get; private set; }

        public void Generate(TextureProperties properties)
        {
            Id = glGenTexture();
            glBindTexture(GL_TEXTURE_2D, Id);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)properties.Wrapping);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)properties.Wrapping);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)properties.Filtering);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)properties.Filtering);

            unsafe
            {
                int format = GL_RGBA;
                glTexImage2D(GL_TEXTURE_2D, 0, format, Width, Height, 0, format, GL_UNSIGNED_BYTE, Image);
                glGenerateMipmap(GL_TEXTURE_2D);
                
                StbiImageFreee(this.Image);
            }
        }
        
        public unsafe static void StbiImageFreee(byte* image)
        {
            unsafe
            {
                if (image != null)
                {
                    Stbi.Free(image);
                }
            }
        }

        public void CleanUp()
        {
            glDeleteTexture(Id);
        }

        public static Texture Register(string localPath, string name)
        {
            var texture = new Texture
            {
                Name = name,
                OriginalPath = localPath,
                LocalPath = $"assets/{Settings.DefaultDomain}/{localPath}"
            };

            try
            {
                var imageBuffer = File.ReadAllBytes(texture.LocalPath);
                unsafe
                {
                    fixed (byte* imageBufferPtr = imageBuffer)
                    {
                        texture.Image = Stbi.LoadFromMemory(imageBufferPtr, imageBuffer.Length, out int width, out int height, out int comp, 4);
                        if (texture.Image == null)
                            throw new InvalidOperationException("Failed to load texture: " + Stbi.FailureReason());

                        texture.Width = width;
                        texture.Height = height;
                        texture.Comp = comp;
                    }
                }
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Failed to load resource: " + texture.LocalPath, e);
            }

            return texture;
        }

        public object Clone()
        {
            return base.MemberwiseClone();
        }

        public Texture Copy()
        {
            return Register(OriginalPath, Name);
        }
    }
}