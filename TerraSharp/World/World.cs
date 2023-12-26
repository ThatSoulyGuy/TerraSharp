using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Core;

namespace TerraSharp.World
{
    public class World
    {
        public static Dictionary<ivec3, Chunk> LoadedChunks { get; private set; } = [];
        public static vec3 playerPosition = new(0, 0, 0); //TODO: Make this support multiple chunkloaders;
        private static CancellationTokenSource cancellationTokenSource;

        public static void StartUpdating()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Task.Run(() =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        Update();
                        Task.Delay(100, token).Wait(token); //TODO: Add a better update system
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Updating stopped.");
                }
            }, token);
        }

        public static void StopUpdating()
        {
            cancellationTokenSource?.Cancel();
        }

        public static void Update()
        {
            ivec3 playerChunkCoordinates = WorldToChunkCoordinates(playerPosition);

            for (int x = -Settings.ViewDistance; x <= Settings.ViewDistance; x++)
            {
                for (int z = -Settings.ViewDistance; z <= Settings.ViewDistance; z++)
                {
                    ivec3 chunkCoordinate = new(playerChunkCoordinates.x + x, 0, playerChunkCoordinates.z + z);

                    if (!LoadedChunks.ContainsKey(chunkCoordinate))
                    {
                        Chunk chunk = new();

                        ivec3 worldPosition = new(chunkCoordinate.x * Chunk.CHUNK_SIZE, 0, chunkCoordinate.z * Chunk.CHUNK_SIZE);

                        chunk.Initialize(worldPosition, false);

                        /*
                        if(chunks.containsKey(new Vector3i(chunkCoordinate.x, chunkCoordinate.y + 1, chunkCoordinate.z)))
                            chunkCheckList.add(new Vector3i(chunkCoordinate.x, chunkCoordinate.y + 1, chunkCoordinate.z));
                        if(chunks.containsKey(new Vector3i(chunkCoordinate.x, chunkCoordinate.y - 1, chunkCoordinate.z)))
                            chunkCheckList.add(new Vector3i(chunkCoordinate.x, chunkCoordinate.y - 1, chunkCoordinate.z));

                        if(chunks.containsKey(new Vector3i(chunkCoordinate.x, chunkCoordinate.y, chunkCoordinate.z + 1)))
                            chunkCheckList.add(new Vector3i(chunkCoordinate.x, chunkCoordinate.y, chunkCoordinate.z + 1));
                        if(chunks.containsKey(new Vector3i(chunkCoordinate.x, chunkCoordinate.y, chunkCoordinate.z - 1)))
                            chunkCheckList.add(new Vector3i(chunkCoordinate.x, chunkCoordinate.y, chunkCoordinate.z - 1));

                        if(chunks.containsKey(new Vector3i(chunkCoordinate.x + 1, chunkCoordinate.y, chunkCoordinate.z)))
                            chunkCheckList.add(new Vector3i(chunkCoordinate.x + 1, chunkCoordinate.y, chunkCoordinate.z));
                        if(chunks.containsKey(new Vector3i(chunkCoordinate.x - 1, chunkCoordinate.y, chunkCoordinate.z)))
                            chunkCheckList.add(new Vector3i(chunkCoordinate.x - 1, chunkCoordinate.y, chunkCoordinate.z));

                        for(Vector3i chunkChecklistCoordinate : chunkCheckList)
                            UpdateChunkOcclusion(chunks.get(chunkChecklistCoordinate));
                        */

                        LoadedChunks.Add(chunkCoordinate, chunk);
                    }
                }
            }

            HashSet<ivec3> chunkSet = new(LoadedChunks.Keys);
            foreach (ivec3 chunkCoordinate in chunkSet)
            {
                if ((MathF.Abs(chunkCoordinate.x - playerChunkCoordinates.x) > Settings.ViewDistance) && (MathF.Abs(chunkCoordinate.z - playerChunkCoordinates.z) > Settings.ViewDistance))
                {
                    LoadedChunks[chunkCoordinate].CleanUp();
                    LoadedChunks.Remove(chunkCoordinate);
                }
            }
        }

        public static void SetBlock(vec3 worldPosition, BlockType type)
        {
            ivec3 chunkCoordinates = WorldToChunkCoordinates(worldPosition);

            Chunk chunk = LoadedChunks[chunkCoordinates];

            if (chunk == null && type != BlockType.BLOCK_AIR)
            {
                chunk = new Chunk();
                chunk.Initialize(chunkCoordinates * Chunk.CHUNK_SIZE, true);

                LoadedChunks.Add(chunkCoordinates, chunk);
            }

            if (chunk != null)
            {
                ivec3 blockPosition = Chunk.WorldToBlockCoordinates(worldPosition);

                chunk.SetBlock(blockPosition, type);

                //UpdateAdjacentChunksIfNecessary(chunkCoordinates);
            }
        }

        public static void SetBlock(ivec3 worldPosition, BlockType type)
        {
            vec3 worldPositionFloat = new(worldPosition.x, worldPosition.y, worldPosition.z);
            SetBlock(worldPositionFloat, type);
        }

        private static void UpdateAdjacentChunksIfNecessary(ivec3 chunkCoordinates)
        {
            int[][] adjacentCoords = [[ 1, 0 ], [ -1, 0 ], [ 0, 1 ], [ 0, -1 ]];

            foreach (int[] coord in adjacentCoords)
            {
                ivec3 adjacentChunkCoord = new(chunkCoordinates.x + coord[0], chunkCoordinates.y, chunkCoordinates.z + coord[1]);

                if (LoadedChunks.ContainsKey(adjacentChunkCoord))
                    UpdateChunkOcclusion(LoadedChunks[adjacentChunkCoord]);
            }
        }

        private static void UpdateChunkOcclusion(Chunk chunk)
        {
            
        }

        public static Chunk GetChunk(vec3 worldPosition)
        {
            ivec3 position = WorldToChunkCoordinates(worldPosition);

            if (LoadedChunks.ContainsKey(position))
                return LoadedChunks[position];

            return null;
        }

        public static ivec3 WorldToChunkCoordinates(vec3 worldPosition)
        {
            return new(
                    (int)MathF.Floor(worldPosition.x / Chunk.CHUNK_SIZE),
                    (int)MathF.Floor(worldPosition.y / Chunk.CHUNK_SIZE),
                    (int)MathF.Floor(worldPosition.z / Chunk.CHUNK_SIZE)
            );
        }

        public static ivec3 WorldToChunkCoordinates(ivec3 worldPosition)
        {
            return new(
                    (int)MathF.Floor(worldPosition.x / Chunk.CHUNK_SIZE),
                    (int)MathF.Floor(worldPosition.y / Chunk.CHUNK_SIZE),
                    (int)MathF.Floor(worldPosition.z / Chunk.CHUNK_SIZE)
            );
        }
    }
}
