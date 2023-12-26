using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Math
{
    public class Transform
    {
        public vec3 position;
        public vec3 rotation;
        public vec3 scale;
        public vec3 pivot;
        public vec3 up = new vec3(0.0f, 1.0f, 0.0f);

        public void Translate(vec3 translation)
        {
            position += translation;
        }

        public void Rotate(vec3 rotation)
        {
            this.rotation += rotation;
        }

        public Transform Copy()
        {
            return Register(new vec3(this.position), new vec3(this.rotation), new vec3(this.scale));
        }


        public static Transform Register(vec3 position)
        {
            return Register(position, new vec3(0, 0, 0), new vec3(1.0f, 1.0f, 1.0f));
        }
        public static Transform Register(vec3 position, vec3 rotation)
        {
            return Register(position, rotation, new vec3(1.0f, 1.0f, 1.0f));
        }

        public static Transform Register(vec3 position, vec3 rotation, vec3 scale)
        {
            Transform transform = new()
            {
                position = position,
                rotation = rotation,
                pivot = new vec3(position),
                scale = scale
            };

            return transform;
        }
    }
}
