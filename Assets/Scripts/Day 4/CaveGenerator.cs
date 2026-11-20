using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class CaveGenerator : MonoBehaviour
{

    private Cave2 _cave;
    private Cave3 _cave3;

    public int _seed = 23;
    public int _size = 64;
    [Range(0, 100)] public int _fillPercentage = 40;

    [Range(0, 20)] public int _passes = 1;

    public MeshRenderer _meshRenderer;
    public MeshFilter _meshFilter;
    public MeshCollider _meshCollider;
    public Tilemap _tileMap;

    public bool point = true;
    public bool cave3 = false;
    public bool autoUpdate = false;

    public bool coloured = false;
    public bool largestOnly = false;
    public bool invert = false;
    public bool generateOutline = false;
    public bool scaleQuad = false;
    public bool createTileMap = false;
    public bool createMesh = false;
    public float wallHeight = 3f;



    public TileSetReference tileSet;

    private Vector2Int lastTile;

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

                    matrix = _cave.MaskMatrix(matrix, largest + 1, invert, true);
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

                matrix = _cave.MaskMatrix(matrix, largest + 1, invert, true);
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


    public void GenerateOutline()
    {
        Gizmos.color = Color.purple;
        _cave.GenerateTileMap();
        byte[,] tileMap = _cave.GetTileMap();

        for (int x = 0; x < _size - 1; x++)
        {
            for (int y = 0; y < _size-1; y++)
            {
                DrawCaseLines(tileMap[x, y], x, y);
            }
        }
    }

    public void DrawCaseLines(byte c, int x, int y)
    {
        Vector3 l = new Vector3(x, y + 0.5f, 0);
        Vector3 b = new Vector3(x + 0.5f, y, 0);
        Vector3 r = new Vector3(x + 1f, y + 0.5f, 0);
        Vector3 t = new Vector3(x + 0.5f, y + 1f, 0);

        if (!scaleQuad)
        {
            l /= _size;
        }

        switch (c)
        {
            case 0b0000_0000:
                // Pass (Air)
                break;
            // Diagnals
            case 0b0000_0001:
                Gizmos.DrawLine(l, b);
                break;
            case 0b0000_0010:
                Gizmos.DrawLine(b, r);
                break;
            case 0b0000_0100:
                Gizmos.DrawLine(t, r);
                break;
            case 0b0000_1000:
                Gizmos.DrawLine(l, t);
                break;

            case 0b0000_1110:
                Gizmos.DrawLine(l, b);
                break;
            case 0b0000_1101:
                Gizmos.DrawLine(b, r);
                break;
            case 0b0000_1011:
                Gizmos.DrawLine(t, r);
                break;
            case 0b0000_0111:
                Gizmos.DrawLine(l, t);
                break;

            // Lines
            case 0b0000_0011:
                Gizmos.DrawLine(l, r);
                break;
            case 0b0000_0110:
                Gizmos.DrawLine(t, b);
                break;
            case 0b0000_1001:
                Gizmos.DrawLine(t, b);
                break;
            case 0b0000_1100:
                Gizmos.DrawLine(l, r);
                break;

            // Doubles
            case 0b0000_0101:
                Gizmos.DrawLine(l, t);
                Gizmos.DrawLine(b, r);
                break;
            case 0b0000_1010:
                Gizmos.DrawLine(t, r);
                Gizmos.DrawLine(l, b);
                break;

            default:
                break;
        }
    }


    public void GenerateMesh()
    {
        _cave.GenerateTileMap();
        byte[,] tileMap = _cave.GetTileMap();
        CaveMesh caveMesh = new CaveMesh(wallHeight);

        for (int x = 0; x < _size - 1; x++)
        {
            for (int y = 0; y < _size - 1; y++)
            {
                caveMesh.CreateWall(tileMap[x, y], x, y);
            }
        }

        Mesh mesh = caveMesh.GetMesh();
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    public void ScaleQuad()
    {
        transform.localScale = new Vector3(_size, _size, _size);
        transform.localPosition = new Vector3((_size - 1f) / 2f, (_size - 1f) / 2f, 0);
    }
    public void CreateTileMap()
    {
        _cave.GenerateTileMap();
        byte[,] tileMap = _cave.GetTileMap();

        _tileMap.ClearAllTiles();

        for (int x = 0; x < _size - 1; x++)
        {
            for (int y = 0; y < _size - 1; y++)
            {
                SetTile(tileMap[x, y], x, y);
            }
        }
    }


    public void RedrawTile(int x, int y)
    {
        byte[,] tileMap = _cave.GetTileMap();

        for (int oX = -1; oX < 2; oX++)
        {
            for (int oY = -1; oY < 2; oY++)
            {
                SetTile(tileMap[x + oX, y + oY], x + oX, y + oY);
            }
        }
    }

    public void SetTile(byte c, int x, int y)
    {
        _tileMap.SetTile(new Vector3Int(x, y, 0), tileSet.Get(c));
    }

    private void OnDrawGizmos()
    {
        if (generateOutline) GenerateOutline();
    }



    private void OnValidate()
    {
        if (autoUpdate) Generate();
        if (scaleQuad) ScaleQuad();

        if (createTileMap) CreateTileMap();
        if (createMesh) GenerateMesh();
        //if (cave3) Generate3();
    }

    private void Start()
    {

        if (cave3) Generate3();
        else Generate();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2Int tilePos = new Vector2Int((int)(pos.x + 0.5f), (int)(pos.y + 0.5f));

            if (lastTile != tilePos)
            {
                //lastTile = tilePos;

                if (!(tilePos.x <= 0 || tilePos.y <= 0 || tilePos.x >= _size - 1 || tilePos.y >= _size - 1))
                {
                    _cave.SwapTile(tilePos.x, tilePos.y);
                    RedrawTile(tilePos.x, tilePos.y);
                }
            }

        }
    }
}
