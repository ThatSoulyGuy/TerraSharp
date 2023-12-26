using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;
using GLFW;
using static OpenGL.GL;
using TerraSharp.Core;
using TerraSharp.Util;
using OpenGL;

namespace TerraSharp.Render
{
    public enum ShaderType
    {
        VertexShader = 35633,
        FragmentShader = 35632
    }

    public static class ShaderTypeExtensions
    {
        public static int GetRaw(this ShaderType shaderType)
        {
            return (int)shaderType;
        }
    }

    public class ShaderObject : ICloneable
    {
        public static readonly uint SHADER_ERROR = 278246;

        public string Name { get; private set; } = "";

        public string OriginalPath { get; private set; } = "";
        public string VertexPath { get; private set; } = "";
        public string FragmentPath { get; private set; } = "";
        private string VertexData { get; set; } = "";
        private string FragmentData { get; set; } = "";

        public uint program;

        private uint GenerateShader(ShaderType type, string data)
        {
            uint id = glCreateShader(type.GetRaw());

            glShaderSource(id, data);
            glCompileShader(id);

            unsafe
            {
                int success;
                glGetShaderiv(id, GL_COMPILE_STATUS, &success);

                if (success == GL_FALSE)
                {
                    Console.Error.Write(type.ToString() + ": " + glGetShaderInfoLog(id));
                    return SHADER_ERROR;
                }
            }

            return id;
        }

        private uint LinkShaders(uint vertex, uint fragment)
        {
            glAttachShader(program, vertex);
            glAttachShader(program, fragment);

            glLinkProgram(program);

            unsafe
            {
                int success;
                glGetProgramiv(program, GL_LINK_STATUS, &success);
                if (success == GL_FALSE)
                {
                    Console.Error.Write("Program Linking: " + glGetProgramInfoLog(program));
                    return SHADER_ERROR;
                }

                glValidateProgram(program);

                glGetProgramiv(program, GL_VALIDATE_STATUS, &success); 
                if (success == GL_FALSE)
                {
                    Console.Error.Write("Program Validation: " + glGetProgramInfoLog(program));
                    return SHADER_ERROR;
                }
            }

            return 0;
        }

        public void Generate()
        {
            program = glCreateProgram();

            uint vertex = GenerateShader(ShaderType.VertexShader, VertexData);

            if (vertex == SHADER_ERROR)
                throw new System.Exception("Failed to generate vertex shader!");

            uint fragment = GenerateShader(ShaderType.FragmentShader, FragmentData);

            if (fragment == SHADER_ERROR)
                throw new System.Exception("Failed to generate fragment shader!");

            if (LinkShaders(vertex, fragment) == SHADER_ERROR)
                throw new System.Exception("Failed to link shaders!");

            glDeleteShader(vertex);
            glDeleteShader(fragment);
        }

        public void Use()
        {
            glUseProgram(program);
        }

        public int GetUniformLocation(string name)
        {
            return glGetUniformLocation(program, name);
        }

        public void SetUniform(string name, float value)
        {
            glUniform1f(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, int value)
        {
            glUniform1i(GetUniformLocation(name), value);
        }

        public void SetUniform(string name, vec2 value)
        {
            glUniform2f(GetUniformLocation(name), value.x, value.y);
        }

        public void SetUniform(string name, vec3 value)
        {
            glUniform3f(GetUniformLocation(name), value.x, value.y, value.z);
        }

        public void SetUniform(string name, mat4 value)
        {
            glUniformMatrix4fv(GetUniformLocation(name), 1, false, value.ToArray());
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public ShaderObject Copy()
        {
            return Register(OriginalPath, Name);
        }

        public void CleanUp()
        {
            glDeleteProgram(program);
        }

        public static ShaderObject Register(string localPath, string name)
        {
            return Register(localPath, name, Settings.DefaultDomain);
        }

        public static ShaderObject Register(string localPath, string name, string domain)
        {
            ShaderObject shader = new()
            {
                Name = name,

                OriginalPath = localPath,

                VertexPath = "assets/" + domain + "/" + localPath + "Vertex.glsl",
                FragmentPath = "assets/" + domain + "/" + localPath + "Fragment.glsl"
            };

            shader.VertexData = FileHelper.LoadFile(shader.VertexPath);
            shader.FragmentData = FileHelper.LoadFile(shader.FragmentPath);

            return shader;
        }
    }
}
