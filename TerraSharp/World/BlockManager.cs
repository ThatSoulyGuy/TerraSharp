using GlmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.World
{
    public enum BlockType
    {
        BLOCK_AIR = 0,
        BLOCK_TEST = 1,
        BLOCK_GRASS = 2,
        BLOCK_DIRT = 3,
        BLOCK_STONE = 4
    }

    public class BlockManager
    {
        public static readonly int AtlasSize = 256;
        public static readonly int TilePixelSize = 16;
        public static readonly float PADDING_RATIO = 1.0f / 256.0f;

        public static vec2[] GetTextureCoordinates(ivec2 position)
        {
            float perTextureSize = (float)TilePixelSize / AtlasSize;

            float u0 = position.x * perTextureSize + PADDING_RATIO;
            float v0 = position.y * perTextureSize + PADDING_RATIO;

            float u1 = u0 + perTextureSize - 2 * PADDING_RATIO;
            float v1 = v0 + perTextureSize - 2 * PADDING_RATIO;

            return
            [
                new(u0, v0),
                new(u1, v0),
                new(u1, v1),
                new(u0, v1)
            ];
        }

        public static vec2[] GetTextureCoordinates(ivec2 position, float rotation)
        {
            float perTextureSize = (float)TilePixelSize / AtlasSize;

            float u0 = position.x * perTextureSize + PADDING_RATIO;
            float v0 = position.y * perTextureSize + PADDING_RATIO;
            float u1 = u0 + perTextureSize - 2 * PADDING_RATIO;
            float v1 = v0 + perTextureSize - 2 * PADDING_RATIO;

            float centerX = (u0 + u1) / 2;
            float centerY = (v0 + v1) / 2;

            vec2[] coordinates =
            [
                new(u1, v0),
                new(u1, v1),
                new(u0, v1),
                new(u0, v0)
            ];

            float rad = (float)glm.Radians(rotation);

            for (int i = 0; i < coordinates.Length; i++)
            {
                float translatedX = coordinates[i].x - centerX;
                float translatedY = coordinates[i].y - centerY;

                float rotatedX = translatedX * (float)MathF.Cos(rad) - translatedY * (float)MathF.Sin(rad);
                float rotatedY = translatedX * (float)MathF.Sin(rad) + translatedY * (float)MathF.Cos(rad);

                coordinates[i].x = rotatedX + centerX;
                coordinates[i].y = rotatedY + centerY;
            }

            return coordinates;
        }

        public static ivec2[] GetBlockTexture(BlockType type)
        {
            ivec2[] coordinates = new ivec2[6];

            switch (type)
            {
                case BlockType.BLOCK_AIR:
                {
                    coordinates[0] = new ivec2(9, 2);
                    coordinates[1] = new ivec2(9, 2);
                    coordinates[2] = new ivec2(9, 2);
                    coordinates[3] = new ivec2(9, 2);
                    coordinates[4] = new ivec2(9, 2);
                    coordinates[5] = new ivec2(9, 2);
                    break;
                }

                case BlockType.BLOCK_TEST:
                {
                    coordinates[0] = new ivec2(0, 0);
                    coordinates[1] = new ivec2(0, 0);
                    coordinates[2] = new ivec2(0, 0);
                    coordinates[3] = new ivec2(0, 0);
                    coordinates[4] = new ivec2(0, 0);
                    coordinates[5] = new ivec2(0, 0);
                    break;
                }

                case BlockType.BLOCK_GRASS:
                {
                    coordinates[0] = new ivec2(0, 0);
                    coordinates[1] = new ivec2(2, 0);
                    coordinates[2] = new ivec2(3, 0);
                    coordinates[3] = new ivec2(3, 0);
                    coordinates[4] = new ivec2(3, 0);
                    coordinates[5] = new ivec2(3, 0);
                    break;
                }

                case BlockType.BLOCK_DIRT:
                {
                    coordinates[0] = new ivec2(2, 0);
                    coordinates[1] = new ivec2(2, 0);
                    coordinates[2] = new ivec2(2, 0);
                    coordinates[3] = new ivec2(2, 0);
                    coordinates[4] = new ivec2(2, 0);
                    coordinates[5] = new ivec2(2, 0);
                    break;
                }

                case BlockType.BLOCK_STONE:
                {
                    coordinates[0] = new ivec2(1, 0);
                    coordinates[1] = new ivec2(1, 0);
                    coordinates[2] = new ivec2(1, 0);
                    coordinates[3] = new ivec2(1, 0);
                    coordinates[4] = new ivec2(1, 0);
                    coordinates[5] = new ivec2(1, 0);
                    break;
                }
            }

            return coordinates;
        }
    }
}
