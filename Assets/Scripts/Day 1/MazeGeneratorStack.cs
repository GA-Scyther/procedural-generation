using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class MazeGeneratorStack : MonoBehaviour
{

    public GameObject _textNodePrefab;

    [Description("Width of maze, Do not go over 100 if using ADVANCED_TEXTURE")] public int _width = 20;
    [Description("Width of maze, Do not go over 100 if using ADVANCED_TEXTURE")] public int _height = 20;

    public int _steps = 100;
    public int _seed = 23;

    public int _stepSize = 2;

    public bool _rngStepSize;
    public int _stepMaxSize = 6;

    public MeshRenderer _renderer;

    public enum MazeGenerationType
    {
        CUBE,
        BASIC_TEXTURE,
        ADVANCED_TEXTURE,
        ADVANCED_TEXTURE_HSV
    }

    public MazeGenerationType _mazeGenerationType = MazeGenerationType.ADVANCED_TEXTURE;
    public bool _singlePath = false;


    public enum Direction
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    private System.Random _rng;

    private bool[,] _grid;
    private int[,] _intGrid;
    private Vector2Int _position;

    private Stack<Vector2Int> _stack;

    private MazeNode _nodes;
    private MazeNode _currentNode;
    private Stack<MazeNode> _nodesStack;

    private void Step()
    {
        Move(GetDirection());
    }

    private void CarveMaze()
    {
        /*for (int i = 0; i < _steps; i++)
        {
            if (_stack.Count == 0) break;

            if (CanMove())
            {
                Step();
                if (_stack.Peek() != _position) _stack.Push(_position);
            }
            else
            {
                _stack.Pop();
            }

        }*/

        while (_stack.Count > 0)
        {

            _position = _stack.Peek();
            _currentNode = _nodesStack.Peek();
            if (CanMove())
            {
                Step();
                _stack.Push(_position);
                _nodesStack.Push(_currentNode);
            }
            else
            {
                _stack.Pop();
                _nodesStack.Pop();
            }
        }
    }

    private bool CheckDirection(Direction direction)
    {
        Vector2Int newPosition = Vector2Int.zero;

        switch (direction)
        {
            case Direction.NORTH:
                newPosition = _position + (Vector2Int.up * _stepSize);
                break;
            case Direction.EAST:
                newPosition = _position + (Vector2Int.right * _stepSize);
                break;
            case Direction.SOUTH:
                newPosition = _position + (Vector2Int.down * _stepSize);
                break;
            case Direction.WEST:
                newPosition = _position + (Vector2Int.left * _stepSize);
                break;
        }

        //newPosition.Clamp(Vector2Int.zero, new Vector2Int(_width - 1, _height - 1));

        if (newPosition.x > _width - 1 || newPosition.y > _height - 1 || newPosition.x < 0 || newPosition.y < 0) return true;

        return _grid[newPosition.x, newPosition.y];
    }

    private bool CanMove()
    {
        return GetPossibleDirections().Count != 0;
    }

    private List<Direction> GetPossibleDirections()
    {
        List<Direction> possibleDirections = new List<Direction>();

        if (!CheckDirection(Direction.NORTH)) possibleDirections.Add(Direction.NORTH);
        if (!CheckDirection(Direction.EAST)) possibleDirections.Add(Direction.EAST);
        if (!CheckDirection(Direction.SOUTH)) possibleDirections.Add(Direction.SOUTH);
        if (!CheckDirection(Direction.WEST)) possibleDirections.Add(Direction.WEST);

        return possibleDirections;
    }


    private Vector2Int GetMoveVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.NORTH:
                return Vector2Int.up;
            case Direction.EAST:
                return Vector2Int.right;
            case Direction.SOUTH:
                return Vector2Int.down;
            case Direction.WEST:
                return Vector2Int.left;
            default:
                return Vector2Int.zero;
        }
    }


    private void Move(Direction direction)
    {
        Vector2Int moveVector = GetMoveVector(direction);

        for (int i = 0; i < _stepSize; i++)
        {
            _position += moveVector;
            _position.Clamp(Vector2Int.zero, new Vector2Int(_width - 1, _height - 1));
            _grid[_position.x, _position.y] = true;

            MazeNode newNode = new MazeNode(_position, _currentNode);
            newNode._score = _currentNode._score + 1;
            _currentNode.AddChild(newNode);
            _currentNode = newNode;
        }
    }


    public Direction GetDirection()
    {
        List<Direction> possibleDirections = GetPossibleDirections();


        int num = _rng.Next(0, possibleDirections.Count);

        return possibleDirections[num];
    }



    public void Init()
    {
        _rng = new System.Random(_seed);

        //_position = new Vector2Int(Mathf.FloorToInt(_width / 2), Mathf.FloorToInt(_height / 2));
        // Start Position
        _position = new Vector2Int(1, 1);

        // Create Grid
        _grid = new bool[_width, _height];

        // Set Starting Position to true
        _grid[_position.x, _position.y] = true;

        _stack = new Stack<Vector2Int>();
        _stack.Push(_position);


        _nodes = new MazeNode(_position, null);
        _currentNode = _nodes;
        _nodesStack = new Stack<MazeNode>();
        _nodesStack.Push(_currentNode);
    }


    public void CreateMaze()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (!_grid[x, y])
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(x, 0, y);
                    cube.transform.parent = transform;
                }
            }
        }

        GameObject text = Instantiate(_textNodePrefab, transform);
        text.transform.position = new Vector3(_nodes._mazePosition.x, 0, _nodes._mazePosition.y);
        text.GetComponent<TextMeshPro>().text = $" {_nodes._score.ToString()} ";

        CreateFloorsFromNode(_nodes);
        CreateEndPointCube();
        CreateStartPointCube();
    }

    public void CreateEndPointCube()
    {
        MazeNode endNode = MazeNode.SearchNodesForHighestScore(_nodes);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.position = new Vector3(endNode._mazePosition.x, -1, endNode._mazePosition.y);
        cube.transform.parent = transform;
        cube.GetComponent<MeshRenderer>().material.color = Color.yellowNice;
    }

    public void CreateStartPointCube()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.position = new Vector3(_nodes._mazePosition.x, -1, _nodes._mazePosition.y);
        cube.transform.parent = transform;
        cube.GetComponent<MeshRenderer>().material.color = Color.limeGreen;
    }

    public void CreateMazeTexture()
    {
        _renderer.material.SetTexture("_BaseMap", SetStartPointTexture(SetEndPointTexture(TextureGenerator.TextureFromMatrix(_grid, apply: false), apply: false)));
    }

    public void CreateAdvancedTexture()
    {
        _intGrid = new int[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (!_grid[x, y])
                {
                    _intGrid[x, y] = -1;
                }
                else
                {
                    _intGrid[x, y] = 0;
                }

            }
        }

        //_currentNode = _nodes;

        if (!_singlePath)
        {
            AdvancedTextureNodes(_nodes);
        }
        else
        {
            AdvancedTextureNodesSinglePath(MazeNode.SearchNodesForHighestScore(_nodes));
        }

        int max = 0;
        foreach (int i in _intGrid)
        {
            if (i > max)
            {
                max = i;
            }
        }

        switch (_mazeGenerationType)
        {
            /*case MazeGenerationType.CUBE:
                break;
            case MazeGenerationType.BASIC_TEXTURE:
                break;*/
            case MazeGenerationType.ADVANCED_TEXTURE:
                _renderer.material.SetTexture("_BaseMap", SetStartPointTexture(SetEndPointTexture(TextureGenerator.TextureFromMatrix(_intGrid, max, Color.yellow, Color.red, true, _singlePath ,apply: false), apply: false)));

                break;
            case MazeGenerationType.ADVANCED_TEXTURE_HSV:
                _renderer.material.SetTexture("_BaseMap", SetStartPointTexture(SetEndPointTexture(TextureGenerator.TextureFromMatrix(_intGrid, max, true, _singlePath, apply: false), apply: false)));
                
                break;
            default:
                _renderer.material.SetTexture("_BaseMap", SetStartPointTexture(SetEndPointTexture(TextureGenerator.TextureFromMatrix(_intGrid, max, Color.yellow, Color.red, true, _singlePath, apply: false), apply: false)));
                break;
        }
    }

    // Causes stack overflow exception on larger sizes. 
    /*private void AdvancedTextureNodesOld()
    {
        foreach (MazeNode child in _currentNode._children)
        {
            _intGrid[child._mazePosition.x, child._mazePosition.y] = child._score;
            _currentNode = child;
            AdvancedTextureNodesOld();
        }
    }*/

    // Doesn't call itself, should stop stack overflow exception
    /// <summary>
    ///  Sets values in the _intGrid matrix to the scores of the nodes starting from starting node
    /// </summary>
    /// <param name="startingNode"></param>
    private void AdvancedTextureNodes(MazeNode startingNode)
    {
        Stack<MazeNode> nodeStack = new Stack<MazeNode>();
        nodeStack.Push(startingNode);

        MazeNode node;

        while (nodeStack.Count > 0)
        {
            node = nodeStack.Pop();
            _intGrid[node._mazePosition.x, node._mazePosition.y] = node._score;

            foreach (MazeNode child in node._children)
            {
                nodeStack.Push(child);
            }

        }
    }


    private void AdvancedTextureNodesSinglePath(MazeNode startingNode)
    {
        MazeNode node = startingNode;

        while (node._parent != null)
        {
            _intGrid[node._mazePosition.x, node._mazePosition.y] = node._score;
            node = node._parent;
        }
    }



    private void CreateFloorsFromNode(MazeNode node)
    {
        foreach (MazeNode child in node._children)
        {
            GameObject text = Instantiate(_textNodePrefab, transform);
            text.transform.position = new Vector3(child._mazePosition.x, 0, child._mazePosition.y);
            text.GetComponent<TextMeshPro>().text = $" {child._score.ToString()} ";

            CreateFloorsFromNode(child);
        }
    }

    public Texture2D SetEndPointTexture(Texture2D texture, bool apply = true)
    {
        MazeNode endNode = MazeNode.SearchNodesForHighestScore(_nodes);
        texture.SetPixel(endNode._mazePosition.x, endNode._mazePosition.y, Color.yellowNice);
        if (apply) texture.Apply();
        return texture;
    }

    public Texture2D SetStartPointTexture(Texture2D texture, bool apply = true)
    {
        texture.SetPixel(_nodes._mazePosition.x, _nodes._mazePosition.y, Color.limeGreen);
        if (apply) texture.Apply();
        return texture;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Run();
    }


    public void Run()
    {
        Init();

        CarveMaze();

        switch (_mazeGenerationType)
        {
            case MazeGenerationType.CUBE:
                CreateMaze();
                break;
            case MazeGenerationType.BASIC_TEXTURE:
                CreateMazeTexture();
                break;
            case MazeGenerationType.ADVANCED_TEXTURE:
                CreateAdvancedTexture();
                break;
            case MazeGenerationType.ADVANCED_TEXTURE_HSV:
                CreateAdvancedTexture();
                break;
            default:
                CreateAdvancedTexture();
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
