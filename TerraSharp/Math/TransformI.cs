using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Math
{
    public class TransformI
    {
        public ivec3 position;
        public ivec3 rotation;
        public ivec3 scale;
        public ivec3 pivot;
        public ivec3 up = new ivec3(0, 0, 0);

        public void Translate(ivec3 translation)
        {
            position += translation;
        }

        public void Rotate(ivec3 rotation)
        {
            this.rotation += rotation;
        }

        public TransformI Copy()
        {
            return Register(new ivec3(this.position), new ivec3(this.rotation), new ivec3(this.scale));
        }

        public Transform ToTransform()
        {
            return Transform.Register(position, rotation, scale); 
        }

        public static TransformI Register(ivec3 position)
        {
            return Register(position, new ivec3(0, 0, 0), new ivec3(1, 1, 1));
        }
        public static TransformI Register(ivec3 position, ivec3 rotation)
        {
            return Register(position, rotation, new ivec3(1, 1, 1));
        }

        public static TransformI Register(ivec3 position, ivec3 rotation, ivec3 scale)
        {
            TransformI transform = new()
            {
                position = position,
                rotation = rotation,
                pivot = new ivec3(position),
                scale = scale
            };

            return transform;
        }
    }
}
