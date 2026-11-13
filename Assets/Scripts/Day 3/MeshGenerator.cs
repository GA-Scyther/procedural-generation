using System.Security.Cryptography;
using UnityEngine;

public static class MeshGenerator
{
    // Define a matching struct layout for memory safety
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct VertexData
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    public static Mesh GenerateTri(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        Mesh mesh = new Mesh();
        Vector3[] verticies = { vertex1, vertex2, vertex3 };
        Vector3[] normals = { Vector3.up, Vector3.up, Vector3.up };
        int[] triangles = {0 , 1, 2};
        Vector2[] uvs = {new Vector2(vertex1.normalized.x, vertex1.normalized.z), new Vector2(vertex2.normalized.x, vertex2.normalized.z) , new Vector2(vertex3.normalized.x, vertex3.normalized.z) };

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.normals = normals;
        //mesh.uv = uvs;
        mesh.SetUVs(0, uvs);
        //mesh.RecalculateNormals();
        //mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }

    public static Mesh GenerateQuad(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 vertex4)
    {
        Mesh mesh = new Mesh();
        Vector3[] verticies = { vertex1, vertex2, vertex3, vertex4 };
        Vector3[] normals = { Vector3.up, Vector3.up, Vector3.up, Vector3.up};
        int[] triangles = { 0, 1, 2, 0, 2, 3 };
        Vector2[] uvs = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        //mesh.SetUVs(0, uvs);
        //mesh.RecalculateNormals();
        //mesh.RecalculateUVDistributionMetrics();

        return mesh;
    }


    public static Mesh GenerateWorldPlaneMesh(int _width, int _height, float scale)
    {
        Mesh mesh = new Mesh();


        Vector3[] vertices = new Vector3[_width * _height];
        Vector3[] normals = new Vector3[_width * _height];
        Vector2[] uvs = new Vector2[_width * _height];
        // Number of squares * 3 Vertexs * 2 tris
        int[] triangles = new int[(_width-1) * (_height-1) * 3 * 2];
        int triIndex = 0;

        // Generate Mesh
        for (int i = 0; i < _width * _height; i++)
        {
            // Create Vertex
            //vertices[i] = new Vector3((i - _width) * scale, 0, Mathf.Floor(i / _width) * scale);
            int x = (int)Mathf.Floor(i / _width);
            int y = (i % _width);

            vertices[i] = new Vector3(x * scale, 0, y * scale);
            normals[i] = Vector3.up;
            uvs[i] = new Vector2(x / (float)(_width), y / (float)(_height));
            //(y * width) + x

            // Check point is not at edge
            if (((i+1) % _width != 0) && (i / _width < _height -1))
            //if (i / _width < _height || i )
            {
                // Tri 1
                triangles[(triIndex * 6) + 2] = i;
                triangles[(triIndex * 6) + 1] = i + _width;
                triangles[(triIndex * 6) + 0] = i + _width + 1;
                //triIndex++;

                //// Tri 2
                triangles[(triIndex * 6) + 5] = i;
                triangles[(triIndex * 6) + 4] = i + _width + 1;
                triangles[(triIndex * 6) + 3] = i + 1;
                triIndex++;
            }

        }


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        return mesh;
    }
    public static Mesh GenerateWorldPlaneMesh(float scale, int lod, float[,] matrix, float heightMultiplier)
    {
        Mesh mesh = new Mesh();

        int increment = 1 << lod;

        int size = matrix.GetLength(0);

        int verticesPerLine = size - 1 / increment + 1;

        Vector3[] vertices = new Vector3[verticesPerLine * verticesPerLine];
        Vector3[] normals = new Vector3[verticesPerLine * verticesPerLine];
        Vector2[] uvs = new Vector2[verticesPerLine * verticesPerLine];
        // Number of squares * 3 Vertexs * 2 tris
        int[] triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 3 * 2];
        int triIndex = 0;

        // Generate Mesh
        for (int i = 0; i < verticesPerLine * verticesPerLine; i+=increment)
        {
            // Create Vertex
            //vertices[i] = new Vector3((i - verticesPerLine) * scale, 0, Mathf.Floor(i / verticesPerLine) * scale);
            int x = (int)Mathf.Floor(i / verticesPerLine);
            int y = (i % verticesPerLine);

            vertices[i] = new Vector3(x * scale, matrix[x, y] * heightMultiplier, y * scale);
            normals[i] = Vector3.up;
            uvs[i] = new Vector2(x / (float)(verticesPerLine), y / (float)(verticesPerLine));
            //(y * width) + x

            // Check point is not at edge
            if (((i + 1) % verticesPerLine != 0) && (i / verticesPerLine < verticesPerLine - 1))
            //if (i / verticesPerLine < _height || i )
            {
                // Tri 1
                triangles[(triIndex * 6) + 2] = i;
                triangles[(triIndex * 6) + 1] = i + verticesPerLine;
                triangles[(triIndex * 6) + 0] = i + verticesPerLine + 1;
                //triIndex++;

                //// Tri 2
                triangles[(triIndex * 6) + 5] = i;
                triangles[(triIndex * 6) + 4] = i + verticesPerLine + 1;
                triangles[(triIndex * 6) + 3] = i + 1;
                triIndex++;
            }
        }


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        return mesh;
    }

    public static Mesh ModifyWorldPlaneMeshFromMatrix(float[,] matrix, Mesh mesh, float scale, int lod = 0)
    {
        int increment = 1 << lod;

        int width = matrix.GetLength(0);
        //int height = matrix.GetLength(1);
        Vector3[] vertices = new Vector3[mesh.vertices.Length];

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 tri = mesh.vertices[i];
            int x = (int)Mathf.Floor(i / width);
            int y = (i % width);
            tri.y = matrix[x * increment, y * increment] * scale;
            //Debug.Log(matrix[x, y]);
            vertices[i] = tri;
        }

        mesh.vertices = vertices;

        return mesh;
    }


    //public static Mesh GenerateGenerateWorldPlaneMeshCompute(ComputeShader shader, int size)
    //{
    //    Mesh mesh = new Mesh();

    //    int kernelIndex = shader.FindKernel("CSMain");

    //    int verticiesSize = size * size;
    //    int triangleSize = (size - 1) * (size - 1) * 6;

    //    ComputeBuffer vertexBuffer = new ComputeBuffer(verticiesSize, sizeof(float) * 5);
    //    ComputeBuffer triangleBuffer = new ComputeBuffer(triangleSize, sizeof(float));

    //    int threadGroupsX = Mathf.CeilToInt((float)size / 8f);
    //    int threadGroupsY = Mathf.CeilToInt((float)size / 8f);

    //    shader.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);



    //    return mesh;
    //}


}
