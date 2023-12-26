using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Core;
using TerraSharp.Math;
using TerraSharp.World;

namespace TerraSharp.Player
{
    public class Player
    {
        private const int CollisionCheckRadius = 5;

        public Camera Camera { get; private set; } = new Camera();

        public float MouseSensitivity { get; private set; } = 0.08f;
        public float MoveSpeed { get; private set; } = 0.1f;

        public Transform Transform { get; private set; } = Transform.Register(new(0, 0, 0));

        public AABB BoundingBox { get; private set; } = AABB.Register(new(0.4f, 0.4f, 0.4f));

        private vec2 oldMouse = new(0, 0), newMouse = new(0, 0);

        public void Initialize(vec3 position)
        {
            Camera.Initialize(position);

            Input.SetMouseMode(GLFW.CursorMode.Disabled);
            Transform.position = position;
        }

        public void Update()
        {
            Camera.Update(Transform.position, out Transform.rotation);

            UpdateControls();
            UpdateMouseLook();
            UpdateMovement();

            BoundingBox.Update(Transform.position);
        }

        private void UpdateControls()
        {
            if (Input.GetMouseButton(GLFW.MouseButton.Left, GLFW.InputState.Press))
            {
                ivec3 blockPosition = Raycast.Shoot(Camera.transform.position, Camera.transform.rotation, 5.0f);

                if (blockPosition != new ivec3(-1, -1, -1))
                    World.World.SetBlock(blockPosition, BlockType.BLOCK_AIR);
            }
        }

        private void UpdateMovement()
        {
            vec3 movementVector = new(0, 0, 0);

            if (Input.GetKey(GLFW.Keys.W, GLFW.InputState.Press))
                movementVector += MoveSpeed * Transform.rotation;

            if (Input.GetKey(GLFW.Keys.S, GLFW.InputState.Press))
                movementVector -= MoveSpeed * Transform.rotation;

            if (Input.GetKey(GLFW.Keys.A, GLFW.InputState.Press))
                movementVector += MoveSpeed * Camera.Right;

            if (Input.GetKey(GLFW.Keys.D, GLFW.InputState.Press))
                movementVector -= MoveSpeed * Camera.Right;

            HandleAxisCollision(ref movementVector.x, new vec3(1, 0, 0));
            HandleAxisCollision(ref movementVector.y, new vec3(0, 1, 0));
            HandleAxisCollision(ref movementVector.z, new vec3(0, 0, 1));

            Transform.position += movementVector;
            BoundingBox.Update(Transform.position);
        }

        private void HandleAxisCollision(ref float movementComponent, vec3 axis)
        {
            float originalComponent = movementComponent;

            float sign = MathF.Sign(movementComponent);

            while (movementComponent != 0)
            {
                if (!IsCollision(Transform.position + axis * movementComponent))
                    break;
                
                movementComponent = AdjustTowardsZero(movementComponent);

                if (MathF.Abs(movementComponent) < 0.01f)
                {
                    movementComponent = 0;
                    break;
                }
            }

            if (originalComponent != movementComponent)
            {
                AdjustForEdgeCases(ref movementComponent, axis);
            }
        }

        private void AdjustForEdgeCases(ref float movementComponent, vec3 axis)
        {
            const float edgeThreshold = 0.1f;

            for (float offset = -edgeThreshold; offset <= edgeThreshold; offset += edgeThreshold)
            {
                vec3 offsetVector = new vec3(0, 0, 0);
                if (axis.x != 0) offsetVector.y = offsetVector.z = offset;
                if (axis.y != 0) offsetVector.x = offsetVector.z = offset;
                if (axis.z != 0) offsetVector.x = offsetVector.y = offset;

                if (IsCollision(Transform.position + axis * movementComponent + offsetVector))
                {
                    movementComponent = AdjustTowardsZero(movementComponent);
                    if (MathF.Abs(movementComponent) < 0.01f)
                    {
                        movementComponent = 0;
                        break;
                    }
                }
            }
        }

        private void HandleCollision(ref vec3 movementVector)
        {
            vec3 newPosition = Transform.position + new vec3(movementVector.x, 0, 0);

            if (IsCollision(newPosition))
                movementVector.x = 0;

            newPosition = Transform.position + new vec3(0, movementVector.y, 0);

            if (IsCollision(newPosition))
                movementVector.y = 0;

            newPosition = Transform.position + new vec3(0, 0, movementVector.z);

            if (IsCollision(newPosition))
                movementVector.z = 0;
        }

        private float AdjustTowardsZero(float value)
        {
            if (value > 0) return value - 0.01f;
            if (value < 0) return value + 0.01f;
            return 0;
        }

        private bool IsCollision(vec3 newPosition)
        {
            AABB tempAABB = new()
            {
                dimensions = BoundingBox.dimensions,
                //debugOverlay = BoundingBox.debugOverlay
            };

            tempAABB.Update(newPosition);

            foreach (var other in GetAllOtherAABBs())
            {
                if (tempAABB.IsColliding(other))
                    return true;
            }

            return false;
        }

        private IEnumerable<AABB> GetAllOtherAABBs()
        {
            var currentChunk = World.World.GetChunk(Transform.position);
            if (currentChunk == null)
                return Enumerable.Empty<AABB>();

            List<AABB> nearbyAABBs = [];
            ivec3 playerBlockPosition = Chunk.WorldToBlockCoordinates(Transform.position);

            foreach (var blockAABBPair in currentChunk.BlockAABBs)
            {
                if (IsWithinRadius(playerBlockPosition, blockAABBPair.Key, CollisionCheckRadius))
                    nearbyAABBs.Add(blockAABBPair.Value);            
            }

            return nearbyAABBs;
        }

        private static bool IsWithinRadius(ivec3 center, ivec3 point, int radius)
        {
            int dx = center.x - point.x;
            int dy = center.y - point.y;
            int dz = center.z - point.z;
            return (dx * dx + dy * dy + dz * dz) <= radius * radius;
        }

        private void UpdateMouseLook()
        {
            newMouse.x = Input.mousePosition.x;
            newMouse.y = Input.mousePosition.y;

            float dx = (newMouse.x - oldMouse.x);
            float dy = (newMouse.y - oldMouse.y);

            Camera.Yaw += dx * MouseSensitivity;
            Camera.Pitch -= dy * MouseSensitivity;

            if (Camera.Pitch > 90)
                Camera.Pitch = 89.99f;

            if (Camera.Pitch < -90)
                Camera.Pitch = -89.99f;

            oldMouse.x = newMouse.x;
            oldMouse.y = newMouse.y;
        }
    }
}
