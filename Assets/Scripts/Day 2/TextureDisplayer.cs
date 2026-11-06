using System;
using Unity.VectorGraphics;
using UnityEngine;

public class TextureDisplayer : MonoBehaviour
{

    public int _width = 256;
    public int _height = 256;

    public float _scale = 1f;

    public int _seed = 23;

    public Vector2 _offset;


    public int _octavesQuantity = 4;
    public float _persistance = 0.5f;
    public float _lacunarity = 2f;

    private System.Random _rng;

    public MeshRenderer _renderer;

    [Serializable]
    public struct Layer
    {
        public string _name;
        public float _maxHeight;
        public Color _color;
    }

    public AnimationCurve _fallofCurve;

    public Layer[] _layers;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }

    private void Generate()
    {
        _rng = new System.Random(_seed);

        Vector2[] octaves = new Vector2[_octavesQuantity];
        for (int i = 0; i < _octavesQuantity; i++)
        {
            octaves[i] = new Vector2(_rng.Next(-1000, 1000), _rng.Next(-1000, 1000));
        }

        float[,] noise = PerlinNoiseGenerator.GeneratePerlinNoiseMatrixOctaves(_width, _height, octaves, _scale, _persistance, _lacunarity, _offset);
        float[,] fallof = GenerateFallofMap(_width, _height);
        noise = SubtractMatrix(noise, fallof);

        Color[,] coloredMatrix = FloatMatrixToLayers(noise, _layers);

        //_renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(noise, Color.black, Color.white, point: false);
        //_renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(noise, point: true);
        _renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(coloredMatrix, point: true);
    }


    private Color[,] FloatMatrixToLayers(float[,] matrix, Layer[] layers)
    {
        Color[,] colorMatrix = new Color[matrix.GetLength(0), matrix.GetLength(1)];


        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                foreach (Layer layer in layers)
                {
                    if (matrix[x, y] <= layer._maxHeight)
                    {
                        colorMatrix[x, y] = layer._color;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        return colorMatrix;
    }

    private float[,] GenerateFallofMap(int width, int height)
    {
        float[,] matrix = new float[width, height];



        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                matrix[x, y] = Mathf.Max(Mathf.Abs(x / (float)(width - 1) * 2 - 1), Mathf.Abs(y / (float)(height - 1) * 2 - 1));
            }
        }



        return matrix;
    }

    private float[,] SubtractMatrix(float[,] a, float[,] b)
    {
        float[,] matrix = new float[a.GetLength(0), a.GetLength(1)];

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                matrix[x, y] = _fallofCurve.Evaluate(a[x, y] - b[x, y]);
            }
        }

        return matrix;
    }
}
