using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public class PerlinNoiseGenerator : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }




    //private float[,] GeneratePerlinNoiseMatrix(float scale, float xOrigin, float yOrigin, Vector2 position)
    //{
    //    float[,] matrix = new float[_width, _height];

    //    for (int x = 0; x < _width; x++)
    //    {
    //        for (int y = 0; y < _height; y++)
    //        {
    //            matrix[x, y] = Mathf.Clamp01(Mathf.PerlinNoise(xOrigin + position.x + x / scale, (float)yOrigin + position.y + y / scale));
    //        }
    //    }

    //    return matrix;
    //}

    static public float[,] GeneratePerlinNoiseMatrixOctaves(int width, int height, Vector2[] octaves, float scale, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] matrix = new float[width, height];
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1, frequency = 1;
                float value = 0;

                foreach (var octave in octaves)
                {
                    float sampleX = ((x - width / 2) + offset.x) / scale * frequency + octave.x;
                    float sampleY = ((y - width / 2) + offset.y) / scale * frequency + octave.y;
                    float noise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    value += noise * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                    matrix[x, y] = value;

                    if (value < minValue) minValue = value;
                    if (value > maxValue) maxValue = value;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                matrix[x, y] = Mathf.InverseLerp(minValue, maxValue, matrix[x, y]);
            }
        }

        return matrix;
    }
}
