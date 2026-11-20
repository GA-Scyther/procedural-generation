using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CaveMesh
{
    private List<Vector3> _verticies;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;
    private List<int> _triangles;

    private int _vIndex;
    private int _tIndex;

    private float _wallHeight;

    public CaveMesh(float wallHeight)
    {
        _verticies = new List<Vector3>();
        _normals = new List<Vector3>();
        _uvs = new List<Vector2>();
        _triangles = new List<int>();

        _wallHeight = wallHeight;
    }

    public void CreateWall(byte c, int x, int y)
    {
        Vector3 l = new Vector3(x, 0, y + 0.5f);
        Vector3 b = new Vector3(x + 0.5f, 0, y);
        Vector3 r = new Vector3(x + 1f, 0, y + 0.5f);
        Vector3 t = new Vector3(x + 0.5f, 0, y + 1f);

        switch (c)
        {
            case 0b0000_0000:
                // Pass (Air)
                break;
            // Diagnals
            case 0b0000_0001:
                CreateQuad(b, l);
                break;
            case 0b0000_0010:
                CreateQuad(r, b);
                break;
            case 0b0000_0100:
                CreateQuad(t, r);
                break;
            case 0b0000_1000:
                CreateQuad(l, t);
                break;

            case 0b0000_1110:
                CreateQuad(l, b);
                break;
            case 0b0000_1101:
                CreateQuad(b, r);
                break;
            case 0b0000_1011:
                CreateQuad(r, t);
                break;
            case 0b0000_0111:
                CreateQuad(t, l);
                break;

            // Lines
            case 0b0000_0011:
                CreateQuad(r, l);
                break;
            case 0b0000_0110:
                CreateQuad(t, b);
                break;
            case 0b0000_1001:
                CreateQuad(b, t);
                break;
            case 0b0000_1100:
                CreateQuad(l, r);
                break;

            // Doubles
            case 0b0000_0101:
                CreateQuad(l, t);
                CreateQuad(b, r);
                break;
            case 0b0000_1010:
                CreateQuad(t, r);
                CreateQuad(l, b);
                break;

            default:
                break;
        }
    }

    public void CreateQuad(Vector3 start, Vector3 end)
    {
        AddTriangle(start, end, end + (Vector3.up * _wallHeight));
        AddTriangle(end + (Vector3.up * _wallHeight), start + (Vector3.up * _wallHeight), start);
    }

    public void AddVertex(Vector3 position)
    {
        _verticies.Add(position);
        _vIndex++;
    }

    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        _triangles.Add(_vIndex);
        AddVertex(c);
        _triangles.Add(_vIndex);
        AddVertex(b);
        _triangles.Add(_vIndex);
        AddVertex(a);
    }

    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = _verticies.ToArray();
        mesh.triangles = _triangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }
}
