using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public int _width = 20;
    public int _height = 20;

    public int _steps = 100;
    public int _seed = 23;

    private System.Random _rng;

    private bool[,] _grid;
    private Vector2Int _position;
    
    private void Step()
    {
        int direction = GetDirection();
        print(direction);
        //print(_position);
        _grid[_position.x, _position.y] = true;
        Move(direction);
    }


    private void Move(int direction)
    {
        switch (direction)
        {
            case 1: // Right
                if (_position.x + 1 < _width) _position.x += 1;
                break;
            case 0: // Up
                if (_position.y + 1 < _height) _position.y += 1;
                break;
            case 3: // Left 
                if (_position.x - 1 > 0) _position.x -= 1;
                break;
            case 2: // Down
                if (_position.y - 1 > 0) _position.y -= 1;
                break;

        }
    }

    private void CarveMaze()
    {
        for (int i = 0; i < _steps; i++)
        {
            Step();
        }
    }

    // 0 = North
    public int GetDirection()
    {
        return _rng.Next(0, 4);
    }



    public void Init()
    {
        _rng = new System.Random(_seed);

        _position = new Vector2Int(Mathf.FloorToInt(_width/2), Mathf.FloorToInt(_height / 2));

        _grid = new bool[_width, _height];
    }


    public void CreateMaze()
    {
        for (int x  = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y])
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(x, 0, y);
                    cube.transform.parent = transform;
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        CarveMaze();
        CreateMaze();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
