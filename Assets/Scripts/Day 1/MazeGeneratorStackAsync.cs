using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class MazeGeneratorStackAsync : MonoBehaviour
{
    public enum Direction
    {
        UP,
        RIGHT,
        DOWN, 
        LEFT
    }




    [Header("Object References")]
    public MeshRenderer _renderer;

    [Header("Maze Stats")]
    public int _width = 20;
    public int _height = 20;

    public int _seed = 23;

    public int _stepSize = 2;


    [Header("Misc")]
    public Vector2Int _startPosition = new Vector2Int(1, 1);

    [SerializeField] private float _maxFrameTime = 1.5f;

    private System.Random _rng;

    private Texture2D _texture;


    private Stack<Vector2Int> _positionStack;
    private Stack<MazeNode> _nodeStack;

    private Vector2Int _currentPosition;
    private MazeNode _currentNode;

    private bool[,] _boolGrid;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Run();
    }


    public async void Run()
    {
        Init();

        Task carveMaze = CarveMaze();

        while (!carveMaze.IsCompleted)
        {
            _renderer.material.SetTexture("_BaseMap", TextureGenerator.TextureFromMatrix(_boolGrid));

            await Task.Yield();
        }
        
        await carveMaze;

        _renderer.material.SetTexture("_BaseMap", TextureGenerator.TextureFromMatrix(_boolGrid));
        print("Done");
    }

    private void Init()
    {
        // Initialise PRNG
        _rng = new System.Random(_seed);

        // Initialise Texture
        _texture = TextureGenerator.GenerateBlankTexture(_width, _height);

        // Set Texture to material
        _renderer.material.SetTexture("_BaseMap", _texture);


        // Initialise Stacks
        _positionStack = new Stack<Vector2Int>();
        _nodeStack = new Stack<MazeNode>();

        // Initialise Matrix Grid
        _boolGrid = new bool[_width, _height];

        // Initialise Starting Position
        _currentPosition = _startPosition;
        SetGridTile(_currentPosition, true);
        _currentNode = new MazeNode(_startPosition, null)
        {
            // Set Node Score
            _score = 0
        };
    }

    #region Maze Carving
    private async Task CarveMaze()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (_positionStack.Count > 0)
        {

            _currentPosition = _positionStack.Peek();
            _currentNode = _nodeStack.Peek();
            if (CanMove())
            {
                Move(GetDirection());
                _positionStack.Push(_currentPosition);
                _nodeStack.Push(_currentNode);
            }
            else
            {
                _positionStack.Pop();
                _nodeStack.Pop();
            }

            // Limit frame time
            if (stopwatch.ElapsedMilliseconds > _maxFrameTime)
            {
                await Awaitable.NextFrameAsync();
                stopwatch.Restart();
            }
        }
    }


    private bool GetTileInDirection(Vector2Int moveVector)
    {
        Vector2Int newPos = _currentPosition + (moveVector * _stepSize);
        if (!PositionInBounds(newPos)) return true;
        return _boolGrid[newPos.x, newPos.y];
    }

    private Vector2Int ClampPosition(Vector2Int position)
    {
        position.x =  Mathf.Clamp(position.x, 0, _width);
        position.y = Mathf.Clamp(position.y, 0, _height);

        return position;
    }
    private bool PositionInBounds(Vector2Int position)
    {
        return position.x < 0 || position.y < 0 || position.x > _width || position.y > _height;
    }

    private List<Vector2Int> GetAllPosibleDirections()
    {
        List<Vector2Int> possibleDirections = new List<Vector2Int>();

        // Check Up
        if (!GetTileInDirection(Vector2Int.up)) possibleDirections.Add(Vector2Int.up);
        if (!GetTileInDirection(Vector2Int.right)) possibleDirections.Add(Vector2Int.right);
        if (!GetTileInDirection(Vector2Int.down)) possibleDirections.Add(Vector2Int.down);
        if (!GetTileInDirection(Vector2Int.left)) possibleDirections.Add(Vector2Int.left);

        return possibleDirections;
    }

    private bool CanMove()
    {
        return GetAllPosibleDirections().Count != 0;
    }

    private void Move(Vector2Int moveVector)
    {
        for (int i = 0; i < _stepSize; i++)
        {
            _currentPosition += moveVector;
            _currentPosition = ClampPosition(_currentPosition);
            SetGridTile(_currentPosition, true);

            MazeNode newNode = new MazeNode(_currentPosition, _currentNode);
            newNode._score = _currentNode._score + 1;
            _currentNode.AddChild(newNode);
            _currentNode = newNode;
        }
    }

    public Vector2Int GetDirection()
    {
        List<Vector2Int> possibleDirections = GetAllPosibleDirections();


        int num = _rng.Next(0, possibleDirections.Count);

        return possibleDirections[num];
    }

    private void SetGridTile(int x, int y, bool value)
    {
        _boolGrid[x, y] = value;
    }
    private void SetGridTile(Vector2Int position, bool value)
    {
        _boolGrid[position.x, position.y] = value;
    }
    #endregion



}
