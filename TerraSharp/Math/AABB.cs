using GlmSharp;
using System.Diagnostics;
using TerraSharp.Record;
using TerraSharp.Render;
using TerraSharp.Thread;

namespace TerraSharp.Math
{
    public class AABB
    {
        public vec3 min;
        public vec3 max;
        public vec3 dimensions;
        public vec3 position;
        //public required RenderableObject debugOverlay;

        public bool IsColliding(AABB other)
        {
            return (this.min.x <= other.max.x && this.max.x >= other.min.x) &&
                    (this.min.y <= other.max.y && this.max.y >= other.min.y) &&
                    (this.min.z <= other.max.z && this.max.z >= other.min.z);
        }

        public void Update(vec3 position)
        {
            min = new(
                    position.x - dimensions.x / 2,
                    position.y,
                    position.z - dimensions.z / 2
            );

            max = new(
                    position.x + dimensions.x / 2,
                    position.y + dimensions.y,
                    position.z + dimensions.z / 2
            );

            this.position = position;
        }

        public vec3 GetHalfSize()
        {
            return new(dimensions.x / 2, dimensions.y / 2, dimensions.z / 2);
        }

        public static AABB Register(vec3 dimensions)
        {
            return Register(new(0, 0, 0), dimensions);
        }

        public static AABB Register(vec3 position, vec3 dimensions)
        {
            AABB aabb = new()
            {
                dimensions = dimensions,
                //debugOverlay = RenderableObject.Register(NameIDTag.Register($"BlockOverlay: [{position}]", position), [], [])
            };

            aabb.Update(position);

            //MainThreadExecutor.QueueTask(aabb.debugOverlay.GenerateCube);
            //aabb.debugOverlay.transform.position = position;

            //MainThreadExecutor.QueueTask(() =>
            //    Renderer.RegisterRenderableObject(aabb.debugOverlay));

            return aabb;
        }
    }
}
