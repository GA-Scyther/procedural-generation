using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{

    private Cave2 _cave;
    private Cave3 _cave3;

    public int _seed = 23;
    public int _size = 64;
    [Range(0, 100)] public int _fillPercentage = 40;

    [Range(0, 20)] public int _passes = 1;

    public MeshRenderer _meshRenderer;

    public bool point = true;
    public bool cave3 = false;
    public bool autoUpdate = false;

    public bool coloured = false;
    public bool largestOnly = false;
    public bool invert = false;

    public void Generate()
    {
        _cave = new Cave2(_size, _fillPercentage, _seed);

        _cave.GenerateMatrix();

        for (int i = 0; i < _passes; i++)
        {
            _cave.Pass();
        }


        if (!coloured)
        {
            int[,] matrix = _cave.GetMatrix();

            if (largestOnly)
            {
                int max;
                List<int> sizes;
                matrix = _cave.NumberCaves(out max, out sizes);
                if (largestOnly)
                {
                    int largest = 0;
                    for (int i = 0; i < max; i++)
                    {
                        if (sizes[i] > sizes[largest])
                        {
                            largest = i;
                        }
                    }

                    matrix = Cave2.MaskMatrix(matrix, largest + 1, invert);
                }
                _meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(matrix, 1, Color.black, Color.white, point: point);
            }
            else _meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(matrix, 1, Color.white, Color.black, point: point);
        }

        else
        {
            int max;
            List<int> sizes;
            int[,] matrix = _cave.NumberCaves(out max, out sizes);
            if (largestOnly)
            {
                int largest = 0;
                for (int i = 0; i < max; i++)
                {
                    if (sizes[i] > sizes[largest])
                    {
                        largest = i;
                    }
                }

                matrix = Cave2.MaskMatrix(matrix, largest + 1, invert);
            }
            _meshRenderer.sharedMaterial.mainTexture = TextureGenerator.TextureFromMatrix(matrix, max, whiteZero: true, point: point);
        }
    }

    public void Generate3()
    {
        _cave3 = new Cave3(_size, _fillPercentage, _seed);

        _cave3.GenerateMatrix();

        for (int i = 0; i < _passes; i++)
        {
            _cave3.Pass();
        }

        int[,,] matrix = _cave3.GetMatrix();


        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                for (int z = 0; z < _size; z++)
                {
                    if (matrix[x, y, z] != 0)
                    {
                        if (!(x == 0 || y == 0 || z == 0 || x == _size - 1 || y == _size - 1 || z == _size - 1) && (_cave3.CountWallNeighbours(x, y, z) == 26)) continue;
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.SetParent(transform, false);
                        cube.transform.localPosition = new Vector3(x, y, z);
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        if (autoUpdate) Generate();
        //if (cave3) Generate3();
    }

    private void Start()
    {
        if (cave3) Generate3();
        else Generate();
    }
}
