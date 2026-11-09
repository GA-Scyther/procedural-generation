using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public MeshRenderer _meshRenderer;
    public MeshFilter _meshFilter;

    public Vector3 vertex1 = new Vector3(0, 0, 0);
    public Vector3 vertex2 = new Vector3(1, 0, 0);
    public Vector3 vertex3 = new Vector3(0, 0, 1);
    public Vector3 vertex4 = new Vector3(0, 1, 1);

    public int _size = 1;
    public float _scale = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _meshFilter.mesh = MeshGenerator.GenerateTri(vertex1, vertex2, vertex3);
    }


    private void OnValidate()
    {
        //_meshFilter.mesh = MeshGenerator.GenerateTri(vertex1, vertex2, vertex3);
        //_meshFilter.mesh = MeshGenerator.GenerateQuad(vertex1, vertex2, vertex3, vertex4);
        _meshFilter.mesh = MeshGenerator.GenerateWorldPlaneMesh(_size, _size, _scale);
    }


    public void GenerateMesh(float[,] matrix)
    {
        _meshFilter.mesh = MeshGenerator.ModifyWorldPlaneMeshFromMatrix(matrix, MeshGenerator.GenerateWorldPlaneMesh(_size, _size, _scale), _scale);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
