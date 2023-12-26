using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Math;
using TerraSharp.Player;
using TerraSharp.Record;
using TerraSharp.Render;
using TerraSharp.Thread;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerraSharp.World
{
    public class Chunk
    {
        public static readonly byte CHUNK_SIZE = 16;

        public List<Vertex> Vertices { get; private set; } = [];
        public List<uint> Indices { get; private set; } = [];

        public TransformI Transform { get; private set; } = TransformI.Register(new(0, 0, 0));

        public Dictionary<ivec3, AABB> BlockAABBs { get; private set; } = [];

        public bool firstRebuild = true;
        
        public uint[,,] blocks = new uint[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        
        private RenderableObject Mesh { get; set; } = new();
        private uint IndicesIndex { get; set; } = 0;

        public void Initialize(ivec3 position, bool generateNothing)
        {
            Transform = TransformI.Register(position);

            if (generateNothing)
            {
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        for (int z = 0; z < CHUNK_SIZE; z++)
                            blocks[x, y, z] = (int)BlockType.BLOCK_AIR;
                    }
                }
            }
            else
            {
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        for (int z = 0; z < CHUNK_SIZE; z++)
                        {
                            if (y < 15)
                                blocks[x, y, z] = (uint)BlockType.BLOCK_DIRT;

                            if (y < 10)
                                blocks[x, y, z] = (uint)BlockType.BLOCK_STONE;

                            blocks[x, 15, z] = (uint)BlockType.BLOCK_GRASS;
                        }
                    }
                }
            }

            Mesh = RenderableObject.Register(NameIDTag.Register($"Chunk_{position.x}_{position.y}_{position.z}", Mesh), [], []);
            Mesh.transform = Transform.ToTransform();

            MainThreadExecutor.QueueTask(() =>
                Mesh.RegisterTexture("atlas"));

            Rebuild();
        }

        public bool HasBlock(ivec3 position)
        {
            if (IsPositionInsideChunk(position))
                return blocks[position.x, position.y, position.z] != (uint)BlockType.BLOCK_AIR;
            else
            {
                vec3 worldPosition = LocalToWorldCoordinates(position);
                Chunk adjacentChunk = World.GetChunk(worldPosition);
                if (adjacentChunk == null)
                    return false;

                ivec3 localPosition = WorldToLocalCoordinates(worldPosition);
                return adjacentChunk.HasBlock(localPosition);
            }
        }

        private vec3 LocalToWorldCoordinates(ivec3 localPosition)
        {
            return new vec3(
                Transform.position.x * CHUNK_SIZE + localPosition.x,
                Transform.position.y * CHUNK_SIZE + localPosition.y,
                Transform.position.z * CHUNK_SIZE + localPosition.z
            );
        }

        private bool IsPositionInsideChunk(ivec3 position)
        {
            return position.x >= 0 && position.x < CHUNK_SIZE &&
                   position.y >= 0 && position.y < CHUNK_SIZE &&
                   position.z >= 0 && position.z < CHUNK_SIZE;
        }

        public void SetBlock(ivec3 position, BlockType type)
        {
            if (position.x < 0 || position.x >= CHUNK_SIZE)
                return;

            if (position.y < 0 || position.y >= CHUNK_SIZE)
                return;

            if (position.z < 0 || position.z >= CHUNK_SIZE)
                return;

            if (blocks[position.x, position.y, position.z] == (uint)type)
                return;

            blocks[position.x, position.y, position.z] = (uint)type;

            Rebuild();
        }

        public void Rebuild()
        {
            Vertices.Clear();
            Indices.Clear(); 
            BlockAABBs.Clear();
            IndicesIndex = 0;

            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if (blocks[x, y, z] == (int)BlockType.BLOCK_AIR)
                            continue;

                        ivec2[] textureCoordinates = BlockManager.GetBlockTexture((BlockType)blocks[x, y, z]);

                        if (ShouldRenderFace(x, y, z, "top"))
                            GenerateTopFace(new(x, y, z), BlockManager.GetTextureCoordinates(textureCoordinates[0]));

                        if (ShouldRenderFace(x, y, z, "bottom"))
                            GenerateBottomFace(new(x, y, z), BlockManager.GetTextureCoordinates(textureCoordinates[1]));

                        if (ShouldRenderFace(x, y, z, "front"))
                            GenerateFrontFace(new(x, y, z), BlockManager.GetTextureCoordinates(textureCoordinates[2], 90));

                        if (ShouldRenderFace(x, y, z, "back"))
                            GenerateBackFace(new(x, y, z), BlockManager.GetTextureCoordinates(textureCoordinates[3], 90));

                        if (ShouldRenderFace(x, y, z, "right"))
                            GenerateRightFace(new(x, y, z), BlockManager.GetTextureCoordinates(textureCoordinates[4], 90));

                        if (ShouldRenderFace(x, y, z, "left"))
                            GenerateLeftFace(new(x, y, z), BlockManager.GetTextureCoordinates(textureCoordinates[5], 90));

                        
                        if (!BlockAABBs.ContainsKey(new(x, y, z)) && BlockExposed(x, y, z))
                        {
                            ivec3 blockPos = new(x, y, z);
                            AABB blockAABB = GenerateBlockAABB(blockPos);

                            BlockAABBs.Add(blockPos, blockAABB);
                        }
                        else
                            BlockAABBs.Remove(new(x, y, z));
                    }
                }
            }

            

            Mesh.RegisterData(Vertices, Indices);

            if (firstRebuild)
            {
                MainThreadExecutor.QueueTask(() =>
                    Mesh.Generate());
                firstRebuild = false;
            }
            else
            {
                MainThreadExecutor.QueueTask(() =>
                    Mesh.ReGenerate());
            }

            MainThreadExecutor.QueueTask(() =>
                Renderer.RegisterRenderableObject(Mesh));
        }

        private bool ShouldRenderFace(int x, int y, int z, string face)
        {
            switch (face)
            {
                case "top":
                    return y == CHUNK_SIZE - 1 || blocks[x, y + 1, z] == (int)BlockType.BLOCK_AIR;
                case "bottom":
                    return y == 0 || blocks[x, y - 1, z] == (int)BlockType.BLOCK_AIR;
                case "front":
                    return z == CHUNK_SIZE - 1 || blocks[x, y, z + 1] == (int)BlockType.BLOCK_AIR;
                case "back":
                    return z == 0 || blocks[x, y, z - 1] == (int)BlockType.BLOCK_AIR;
                case "right":
                    return x == CHUNK_SIZE - 1 || blocks[x + 1, y, z] == (int)BlockType.BLOCK_AIR;
                case "left":
                    return x == 0 || blocks[x - 1, y, z] == (int)BlockType.BLOCK_AIR;
                default:
                    return false;
            }
        }

        private AABB GenerateBlockAABB(ivec3 blockPosition)
        {
            vec3 worldPosition = new(blockPosition.x + Transform.position.x + 0.5f, blockPosition.y + Transform.position.y + 0.5f, blockPosition.z + Transform.position.z + 0.5f);
            vec3 blockDimensions = new(1.0f, 1.0f, 1.0f);

            return AABB.Register(worldPosition, blockDimensions);
        }

        private bool BlockExposed(int x, int y, int z)
        {
            return IsAir(x - 1, y, z) || IsAir(x + 1, y, z) ||
                    IsAir(x, y - 1, z) || IsAir(x, y + 1, z) ||
                    IsAir(x, y, z - 1) || IsAir(x, y, z + 1);
        }

        public bool IsAir(int x, int y, int z)
        {
            ivec3 position = new(x, y, z);

            if (IsPositionInsideChunk(position))
                return blocks[x, y, z] == (uint)BlockType.BLOCK_AIR;
            else
            {
                vec3 worldPosition = LocalToWorldCoordinates(position);
                Chunk adjacentChunk = World.GetChunk(worldPosition);
                if (adjacentChunk == null)
                    return true;

                ivec3 localPosition = WorldToLocalCoordinates(worldPosition);
                return adjacentChunk.IsAir(localPosition.x, localPosition.y, localPosition.z);
            }
        }

        public static ivec3 WorldToLocalCoordinates(vec3 worldPosition)
        {
            return new ivec3(
                (int)MathF.Floor(worldPosition.x) % CHUNK_SIZE,
                (int)MathF.Floor(worldPosition.y) % CHUNK_SIZE,
                (int)MathF.Floor(worldPosition.z) % CHUNK_SIZE
            );
        }

        public static vec3 BlockToWorldCoordinates(Chunk chunk, ivec3 blockPosition)
        {
            return chunk.Transform.position + blockPosition;
        }

        public static ivec3 WorldToBlockCoordinates(vec3 worldPosition)
        {
            int blockX = (int)MathF.Floor(worldPosition.x) % Chunk.CHUNK_SIZE;
            int blockY = (int)MathF.Floor(worldPosition.y) % Chunk.CHUNK_SIZE;
            int blockZ = (int)MathF.Floor(worldPosition.z) % Chunk.CHUNK_SIZE;

            blockX = (blockX < 0) ? Chunk.CHUNK_SIZE + blockX : blockX;
            blockY = (blockY < 0) ? Chunk.CHUNK_SIZE + blockY : blockY;
            blockZ = (blockZ < 0) ? Chunk.CHUNK_SIZE + blockZ : blockZ;

            return new(blockX, blockY, blockZ);
        }

        public static ivec3 WorldToBlockCoordinates(ivec3 worldPosition)
        {
            int blockX = (int)MathF.Floor(worldPosition.x) % Chunk.CHUNK_SIZE;
            int blockY = (int)MathF.Floor(worldPosition.y) % Chunk.CHUNK_SIZE;
            int blockZ = (int)MathF.Floor(worldPosition.z) % Chunk.CHUNK_SIZE;

            blockX = (blockX < 0) ? Chunk.CHUNK_SIZE + blockX : blockX;
            blockY = (blockY < 0) ? Chunk.CHUNK_SIZE + blockY : blockY;
            blockZ = (blockZ < 0) ? Chunk.CHUNK_SIZE + blockZ : blockZ;

            return new(blockX, blockY, blockZ);
        }

        private void GenerateTopFace(vec3 position, vec2[] uvs)
        {
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 1.0f + position.y, 1.0f + position.z), uvs[0]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 1.0f + position.y, 0.0f + position.z), uvs[1]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 1.0f + position.y, 0.0f + position.z), uvs[2]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 1.0f + position.y, 1.0f + position.z), uvs[3]));

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 2);
            Indices.Add(IndicesIndex + 1);

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 3);
            Indices.Add(IndicesIndex + 2);

            IndicesIndex += 4;
        }

        private void GenerateBottomFace(vec3 position, vec2[] uvs)
        {
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 0.0f + position.y, 0.0f + position.z), uvs[0]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 0.0f + position.y, 0.0f + position.z), uvs[1]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 0.0f + position.y, 1.0f + position.z), uvs[2]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 0.0f + position.y, 1.0f + position.z), uvs[3]));

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 1);
            Indices.Add(IndicesIndex + 2);

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 2);
            Indices.Add(IndicesIndex + 3);

            IndicesIndex += 4;
        }

        private void GenerateFrontFace(vec3 position, vec2[] uvs)
        {
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 0.0f + position.y, 1.0f + position.z), uvs[0]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 0.0f + position.y, 1.0f + position.z), uvs[1]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 1.0f + position.y, 1.0f + position.z), uvs[2]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 1.0f + position.y, 1.0f + position.z), uvs[3]));

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 1);
            Indices.Add(IndicesIndex + 2);

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 2);
            Indices.Add(IndicesIndex + 3);

            IndicesIndex += 4;
        }

        private void GenerateBackFace(vec3 position, vec2[] uvs)
        {
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 0.0f + position.y, 0.0f + position.z), uvs[0]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 0.0f + position.y, 0.0f + position.z), uvs[1]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 1.0f + position.y, 0.0f + position.z), uvs[2]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 1.0f + position.y, 0.0f + position.z), uvs[3]));

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 1);
            Indices.Add(IndicesIndex + 2);

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 2);
            Indices.Add(IndicesIndex + 3);

            IndicesIndex += 4;
        }

        private void GenerateRightFace(vec3 position, vec2[] uvs)
        {
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 0.0f + position.y, 1.0f + position.z), uvs[0]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 0.0f + position.y, 0.0f + position.z), uvs[1]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 1.0f + position.y, 0.0f + position.z), uvs[2]));
            Vertices.Add(Vertex.Register(new(1.0f + position.x, 1.0f + position.y, 1.0f + position.z), uvs[3]));

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 1);
            Indices.Add(IndicesIndex + 2);

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 2);
            Indices.Add(IndicesIndex + 3);

            IndicesIndex += 4;
        }

        private void GenerateLeftFace(vec3 position, vec2[] uvs)
        {
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 0.0f + position.y, 0.0f + position.z), uvs[0]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 0.0f + position.y, 1.0f + position.z), uvs[1]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 1.0f + position.y, 1.0f + position.z), uvs[2]));
            Vertices.Add(Vertex.Register(new(0.0f + position.x, 1.0f + position.y, 0.0f + position.z), uvs[3]));

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 1);
            Indices.Add(IndicesIndex + 2);

            Indices.Add(IndicesIndex);
            Indices.Add(IndicesIndex + 2);
            Indices.Add(IndicesIndex + 3);

            IndicesIndex += 4;
        }

        public void CleanUp()
        {
            Vertices.Clear();
            Indices.Clear();
            IndicesIndex = 0;

            MainThreadExecutor.QueueTask(() =>
                Renderer.RemoveRenderableObject(Mesh.Name));
        }
    }
}
