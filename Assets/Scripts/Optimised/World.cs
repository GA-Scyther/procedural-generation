using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{

    private Vector2[] _octaves;
    private float _persistance;
    private float _lacunarity;

    private float _scale;
    private float _heightScale;

    private float _minValue;
    private float _maxValue;

    private int _size;

    public Vector2Int offset = Vector2Int.zero;
    public AnimationCurve _fallofCurve;

    //private Dictionary<Vector2, float> _noiseCache;

    public struct CombinedWorldData
    {
        public WorldData meshData;
        public WorldData collisionData;
    }

    public struct WorldData
    {
        public Vector3[] verticies;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int[] triangles;

        public Texture2D texture;

        public int size;

        public WorldData(int vertexCount, int triCount, int _size)
        {
            verticies = new Vector3[vertexCount];
            normals = new Vector3[vertexCount];
            uvs = new Vector2[vertexCount];

            triangles = new int[triCount];

            size = _size;

            texture = new Texture2D(size, size);
        }

        public WorldData(int _size)
        {
            verticies = new Vector3[_size * _size];
            normals = new Vector3[_size * _size];
            uvs = new Vector2[_size * _size];

            triangles = new int[(_size - 1) * (_size - 1) * 3 * 2];

            size = _size;

            texture = new Texture2D(size, size);
        }

        public readonly Mesh GetMesh()
        {
            Mesh mesh = new()
            {
                vertices = verticies,
                normals = normals,
                uv = uvs,
                triangles = triangles
            };

            return mesh;
        }

        public readonly Texture2D GetTexture()
        {
            texture.Apply();

            return texture;
        }

        public void AddVertex(int x, int y, float height, float positionMultiplier = 1)
        {
            verticies[y * size + x] = new Vector3(x * positionMultiplier, height, y * positionMultiplier);
            normals[y * size + x] = new Vector3(0, 1, 0);
            uvs[y * size + x] = new Vector2((float)x / (float)size - 1f, (float)y / (float)size - 1f);
        }

        // SetPixel on texure
        public void SetPixel(int x, int y, Color color)
        {
            if (x > size || y > size || x < 0 || y < 0) return;


            texture.SetPixel(x, y, color);
        }

        /// <summary>
        /// Add cube base on x/y of bottom left point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddSquare(int x, int y)
        {

            // Calculate triangle index from x/y
            // x + size-1 * y
            int triIndex = x + ((size - 1) * y);
            triIndex *= 6;

            // Calculate 1d vertex index
            int vertexIndex = y * size + x;

            // Triangle 1
            triangles[triIndex] = vertexIndex;
            triangles[triIndex + 1] = vertexIndex + size;
            triangles[triIndex + 2] = vertexIndex + size + 1;

            // Triangle 2
            triangles[triIndex + 3] = vertexIndex;
            triangles[triIndex + 4] = vertexIndex + size + 1;
            triangles[triIndex + 5] = vertexIndex + 1;
        }
    }


    public struct TileData
    {
        public float height;
    }

    public World(int size, Vector2[] octaves, float persistance, float lacunarity, float scale, float heightScale, AnimationCurve fallofCurve)
    {
        _size = size;
        _octaves = octaves;
        _persistance = persistance;
        _lacunarity = lacunarity;
        _scale = scale;
        _heightScale = heightScale;
        _fallofCurve = fallofCurve;
        _scale = scale;


        float amplitude = 1, frequency = 1;
        for (int i = 0; i < octaves.Length; i++)
        {
            float min = 0 * 2 - 1;
            float max = 1 * 2 - 1;

            _minValue += min * amplitude;
            _maxValue += max * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }
    }



    public TileData GetTileData(float x, float y, bool falloff = false)
    {
        TileData tileData = new();

        float amplitude = 1, frequency = 1;
        float value = 0;

        // Calculate Height Data
        foreach (Vector2 octave in _octaves)
        {
            //float sampleX = ((x - width / 2) + offset.x) / scale * frequency + octave.x;
            //float sampleY = ((y - width / 2) + offset.y) / scale * frequency + octave.y;

            float sampleX = x / _scale * frequency + octave.x;
            float sampleY = y / _scale * frequency + octave.y;

            float noise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

            value += noise * amplitude;
            amplitude *= _persistance;
            frequency *= _lacunarity;

            //if (value < minValue) minValue = value;
            //if (value > maxValue) maxValue = value;
        }


        value = Mathf.InverseLerp(_minValue, _maxValue, value);


        if (falloff)
        {
            float fallof = Mathf.Max(Mathf.Abs(x * 2f - 1f), Mathf.Abs(y * 2f - 1f));
            value = SubtractWithCurve(value, fallof);
        }

        tileData.height = value;

        return tileData;
    }

    public float SubtractWithCurve(float a, float b)
    {
        return _fallofCurve.Evaluate(a - b);
    }

    public CombinedWorldData GenerateWorld(int lod, bool falloff = false)
    {
        int increment = 1 << lod;
        while ((_size - 1) % increment != 0) { increment = increment / 2; }
        int verticiesPerLine = (_size - 1) / increment + 1;

        //_noiseCache = new Dictionary<Vector2, float>();

        WorldData worldData = new WorldData(_size);
        WorldData collisionData = new WorldData(verticiesPerLine);

        CombinedWorldData output = new CombinedWorldData();

        for (int x = 0; x < _size; x++)
        {
            for (int y = 0; y < _size; y++)
            {
                // Tile Processing
                float sampleX = ((x - _size / 2) + offset.x);
                float sampleY = ((y - _size / 2) + offset.y);
                TileData tileData = GetTileData(sampleX, sampleY);


                // Fallof Map
                if (falloff)
                {
                    float fallof = Mathf.Max(Mathf.Abs(x / (float)(_size - 1) * 2 - 1), Mathf.Abs(y / (float)(_size - 1) * 2 - 1));
                    //tileData.height *= fallof;
                    tileData.height = SubtractWithCurve(tileData.height, fallof);
                }


                if (x % increment == 0 && y % increment == 0)
                {
                    collisionData.AddVertex(x / increment, y / increment, tileData.height * _heightScale, increment);

                    if (x < _size - 1 && y < _size - 1 && x >= 0 && y >= 0)
                    {
                        collisionData.AddSquare(x / increment, y / increment);
                    }
                }

                // World Mesh Adding
                worldData.AddVertex(x, y, tileData.height * _heightScale);

                // Set Color
                worldData.SetPixel(x, y, Color.Lerp(Color.black, Color.white, tileData.height));

                // Add Squares
                if (x < _size - 1 && y < _size - 1 && x >= 0 && y >= 0)
                {
                    worldData.AddSquare(x, y);
                }
            }
        }

        //for (int x = 0; x < _size; x+= increment)
        //{
        //    for (int y = 0; y < _size; y+= increment)
        //    {
        //        // Tile Processing
        //        float sampleX = ((x - _size / 2) + offset.x);
        //        float sampleY = ((y - _size / 2) + offset.y);
        //        TileData tileData = GetTileData(sampleX, sampleY);


        //        // Fallof Map
        //        if (falloff)
        //        {
        //            float fallof = Mathf.Max(Mathf.Abs(x / (float)(_size - 1) * 2 - 1), Mathf.Abs(y / (float)(_size - 1) * 2 - 1));
        //            //tileData.height *= fallof;
        //            tileData.height = SubtractWithCurve(tileData.height, fallof);
        //        }

        //        // World Mesh Adding
        //        collisionData.AddVertex(x / increment, y / increment, tileData.height * _heightScale, increment);

        //        // Set Color
        //        //collisionData.SetPixel(x / increment, y / increment, Color.Lerp(Color.black, Color.white, tileData.height));

        //        // Add Squares
        //        if (x < _size - 1 && y < _size - 1 && x >= 0 && y >= 0)
        //        {
        //            collisionData.AddSquare(x / increment, y / increment);
        //        }
        //    }
        //}

        output.meshData = worldData;
        output.collisionData = collisionData;

        return output;
    }

    public Color CalculateColorFromLayers(float height)
    {
        Color color = new Color();





        return color;
    }

}
