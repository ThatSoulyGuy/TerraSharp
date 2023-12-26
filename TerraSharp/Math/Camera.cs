using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Core;

namespace TerraSharp.Math
{
    public class Camera
    {
        public float Fov { get; set; }
        public float NearPlane { get; set; }
        public float FarPlane { get; set; }

        public Transform transform = Transform.Register(new(0.0f, 0.0f, 0.0f));
        public vec3 ForwardByYaw { get; private set; }
        public vec3 RightByYaw { get; private set; }

        public vec3 Right { get; private set; }

        public float Pitch { get; set; } = 0;
        public float Yaw { get; set; } = 0;

        public mat4 projection = mat4.Identity;
        public mat4 view = mat4.Identity;

        public void Initialize(vec3 position)
        {
            Initialize(position, 45.0f, 0.004f, 500.0f);
        }

        public void Initialize(vec3 position, float fov, float nearPlane, float farPlane)
        {
            transform.position = position;
            Fov = fov;
            NearPlane = nearPlane;
            FarPlane = farPlane;

            projection = mat4.Perspective(glm.Radians(Fov), Window.Size.x / (float)Window.Size.y, NearPlane, FarPlane);
        }

        public void Update(vec3 position, out vec3 rotation)
        {
            projection = mat4.Perspective(glm.Radians(Fov), Window.Size.x / (float)Window.Size.y, NearPlane, FarPlane);

            vec3 direction = new(
                    MathF.Cos(glm.Radians(Yaw)) * (float)MathF.Cos(glm.Radians(Pitch)),
                    MathF.Sin(glm.Radians(Pitch)),
                    MathF.Sin(glm.Radians(Yaw)) * (float)MathF.Cos(glm.Radians(Pitch))
            );

            transform.rotation = direction.Normalized;

            transform.position = position;
            rotation = transform.rotation;

            float yawRadians = transform.rotation.y;
            vec3 forward = new vec3(
                    (float)-MathF.Sin(yawRadians),
                    0,
                    (float)MathF.Cos(yawRadians)
            ).Normalized;

            vec3 right = new vec3(
                    forward.z,
                    0,
                    -forward.x
            ).Normalized;

            ForwardByYaw = forward;
            RightByYaw = right;

            Right = glm.Cross(transform.up, direction).Normalized;

            view = mat4.LookAt(transform.position, transform.position + direction, transform.up);
        }
    }
}
