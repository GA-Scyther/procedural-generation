using Unity.Mathematics;
using UnityEngine;
using System;

public class MeshGeneratorGPU : MonoBehaviour
{
    public ComputeShader computeShader;

    public Mesh GenerateMeshGPU(int size)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[size * size];
        Vector3[] normals = new Vector3[size * size];
        Vector2[] uvs = new Vector2[size * size];
        // Number of squares * 3 Vertexs * 2 tris
        int[] triangles = new int[(size - 1) * (size - 1) * 3 * 2];

        uint[] triangleIndex = new uint[size * size];
        uint tempIndex = 0;
        for (int i = 0; i < size * size; i++)
        {
            if (((i + 1) % size != 0) && (i / size < size - 1))
            {
                triangleIndex[i] = tempIndex;
                tempIndex++;
            }
        }

        ComputeBuffer triangleIndexBuffer = new ComputeBuffer(size * size, sizeof(int));
        ComputeBuffer verticesBuffer = new ComputeBuffer(size * size, sizeof(float) * 3);
        ComputeBuffer normalsBuffer = new ComputeBuffer(size * size, sizeof(float) * 3);
        ComputeBuffer uvsBuffer = new ComputeBuffer(size * size, sizeof(float) * 2);
        ComputeBuffer trianglesBuffer = new ComputeBuffer((size - 1) * (size - 1) * 3 * 2, sizeof(uint));

        triangleIndexBuffer.SetData(triangleIndex);

        //inputBuffer.SetData(inputData);
        int kernelIndex = computeShader.FindKernel("CSMain");


        computeShader.SetBuffer(kernelIndex, "_triangleIndex", triangleIndexBuffer);
        computeShader.SetBuffer(kernelIndex, "_verticies", verticesBuffer);
        computeShader.SetBuffer(kernelIndex, "_normals", normalsBuffer);
        computeShader.SetBuffer(kernelIndex, "_uvs", uvsBuffer);
        computeShader.SetBuffer(kernelIndex, "_triangles", trianglesBuffer);
        computeShader.SetInt("size", size);
        computeShader.SetInt("limit", size*size);

        int threadGroups = Mathf.CeilToInt((float)size*size / 64f);
        //print(threadGroups);
        computeShader.Dispatch(kernelIndex, threadGroups, 1, 1);

        verticesBuffer.GetData(vertices);
        normalsBuffer.GetData(normals);
        uvsBuffer.GetData(uvs);
        trianglesBuffer.GetData(triangles);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        verticesBuffer.Release();
        normalsBuffer.Release();
        uvsBuffer.Release();
        trianglesBuffer.Release();

        return mesh;
    }
}
