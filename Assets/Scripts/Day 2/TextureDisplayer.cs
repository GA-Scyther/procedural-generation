using System;
using System.Diagnostics;
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

    public int _meshLod = 0;
    public int _collisionLod = 0;

    private System.Random _rng;

    public MeshRenderer _renderer;
    public MeshFilter _meshFilter;
    public MeshCollider _meshColldier;

    public enum RenderMode
    {
        GREYSCALE,
        HSV,
        LAYERS
    }

    public RenderMode _renderMode = RenderMode.GREYSCALE;


    [Serializable]
    public struct Layer
    {
        public string _name;
        public float _maxHeight;
        public Color _color;
        //public AnimationCurve _heightCurve;
        //public float _heightMultiplier;
    }

    public AnimationCurve _fallofCurve;

    public Layer[] _layers;

    public enum MeshGenerationMode
    {
        CPU,
        GPU
    }

    public MeshGenerationMode _meshGenerationMode = MeshGenerationMode.CPU;

    public float _meshScale = 1;
    public float _heightScale = 1;

    public MeshGeneratorGPU _meshGenGPU;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
        //GenerateMesh();
    }

    public void Generate()
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


        switch (_renderMode)
        {
            case RenderMode.GREYSCALE:
                _renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(noise, Color.black, Color.white, point: false);
                break;
            case RenderMode.HSV:
                _renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(noise, point: true);
                break;
            case RenderMode.LAYERS:
                _renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(coloredMatrix, point: true);
                break;
            default:
                _renderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(coloredMatrix, point: true);
                break;
        }

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

    //private float[,] ModifyHeightFromLayers(float[,] matrix, Layer[] layers)
    //{
    //    float[,] floatMatrix = new float[matrix.GetLength(0), matrix.GetLength(1)];
    //    floatMatrix = matrix;

    //    for (int x = 0; x < matrix.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < matrix.GetLength(1); y++)
    //        {
    //            for (int i = 0; i < layers.Length; i++)
    //            {
    //                if (matrix[x, y] <= layers[i]._maxHeight)
    //                {
    //                    if (i == 0) floatMatrix[x, y] = matrix[x, y] + layers[i]._heightCurve.Evaluate(Mathf.InverseLerp(0, layers[i]._maxHeight, matrix[x, y])) * layers[i]._heightMultiplier;
    //                    else floatMatrix[x, y] = matrix[x,y] + layers[i]._heightCurve.Evaluate(Mathf.InverseLerp(layers[i-1]._maxHeight, layers[i]._maxHeight, matrix[x, y])) * layers[i]._heightMultiplier;
    //                    break;
    //                }
    //                else
    //                {
    //                    continue;
    //                }
    //            }
    //        }
    //    }

    //    return floatMatrix;
    //    }


    public void GenerateMesh()
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
        //noise = ModifyHeightFromLayers(noise, _layers);

        Stopwatch stopwatch = Stopwatch.StartNew();


        switch (_meshGenerationMode)
        {
            case MeshGenerationMode.CPU:
                //_meshFilter.mesh = MeshGenerator.ModifyWorldPlaneMeshFromMatrix(noise, MeshGenerator.GenerateWorldPlaneMesh(_width, _height, _meshScale, _meshLod), _heightScale);
                //_meshColldier.sharedMesh = MeshGenerator.ModifyWorldPlaneMeshFromMatrix(noise, MeshGenerator.GenerateWorldPlaneMesh(_width, _height, _meshScale, _collisionLod), _heightScale);
                //_meshFilter.mesh = MeshGenerator.GenerateWorldPlaneMesh(_width, _height, _meshScale, _meshLod, noise, _heightScale);
                //_meshColldier.sharedMesh = MeshGenerator.GenerateWorldPlaneMesh(_width, _height, _meshScale, _collisionLod, noise, _heightScale);
                break;
            case MeshGenerationMode.GPU:
                _meshFilter.mesh = MeshGenerator.ModifyWorldPlaneMeshFromMatrix(noise, _meshGenGPU.GenerateMeshGPU(_width), _heightScale);
                break;
            default:
                _meshFilter.mesh = MeshGenerator.ModifyWorldPlaneMeshFromMatrix(noise, _meshGenGPU.GenerateMeshGPU(_width), _heightScale);
                break;
        }

        //_meshFilter.mesh = _meshGenGPU.GenerateMeshGPU(_width);

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Complete Time: {stopwatch.Elapsed}");
    }
}
