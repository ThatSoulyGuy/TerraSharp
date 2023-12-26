using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.World;

namespace TerraSharp.Math
{
    public class Raycast
    {
        public vec3 Origin { get; private set; }
        public vec3 Direction { get; private set; }

        public static Raycast Register(vec3 origin, vec3 direction)
        {
            Raycast raycast = new()
            {
                Origin = origin,
                Direction = direction.NormalizedSafe
            };

            return raycast;
        }

        public bool IntersectAABB(AABB aabb, out vec3 intersectionPoint)
        {
            intersectionPoint = new vec3();
            vec3 dirFrac = new(1.0f / Direction.x, 1.0f / Direction.y, 1.0f / Direction.z);

            float t1 = (aabb.min.x - Origin.x) * dirFrac.x;
            float t2 = (aabb.max.x - Origin.x) * dirFrac.x;
            float t3 = (aabb.min.y - Origin.y) * dirFrac.y;
            float t4 = (aabb.max.y - Origin.y) * dirFrac.y;
            float t5 = (aabb.min.z - Origin.z) * dirFrac.z;
            float t6 = (aabb.max.z - Origin.z) * dirFrac.z;

            float tmin = MathF.Max(MathF.Max(MathF.Min(t1, t2), MathF.Min(t3, t4)), MathF.Min(t5, t6));
            float tmax = MathF.Min(MathF.Min(MathF.Max(t1, t2), MathF.Max(t3, t4)), MathF.Max(t5, t6));

            if (tmax < 0)
                return false;

            if (tmin > tmax)
                return false;

            intersectionPoint = Origin + tmin * Direction;
            return true;
        }

        public static ivec3 Shoot(vec3 origin, vec3 direction, float maxDistance)
        {
            vec3 ray = new(origin);
            vec3 step = direction.NormalizedSafe;

            for (float t = 0; t < maxDistance; t += 1.0f)
            {
                ray += step;

                Chunk chunk = World.World.GetChunk(ray);
                if (chunk != null)
                {
                    ivec3 blockPos = Chunk.WorldToBlockCoordinates(ray);

                    if (chunk.HasBlock(blockPos))
                        return new((int)MathF.Floor(ray.x), (int)MathF.Floor(ray.y), (int)MathF.Floor(ray.z));
                }
            }

            return new(-1, -1, -1);
        }
    }
}
