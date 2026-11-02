using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;

public class MazeGeneratorStack : MonoBehaviour
{

    public GameObject _textNodePrefab;

    public int _width = 20;
    public int _height = 20;

    public int _steps = 100;
    public int _seed = 23;

    public int _stepSize = 2;

    public bool _rngStepSize;
    public int _stepMaxSize = 6;

    public enum Direction
    {
        NORTH,
        EAST,
        SOUTH,
        WEST
    }

    private System.Random _rng;

    private bool[,] _grid;
    private Vector2Int _position;

    private Stack<Vector2Int> _stack;

    class Node
    {
        public Vector2Int position;
        public int score;


        public List<Node> children = new List<Node>();
        public Node parent;

        public Node(Vector2Int newPosition, int newScore)
        {
            position = newPosition;
            score = newScore;
        }
        public Node(Vector2Int newPosition, int newScore, Node newParent)
        {
            position = newPosition;
            score = newScore;
            parent = newParent;
        }

        public void AddChild(Node child)
        {
            children.Add(child);
        }
    }

    private Node _nodes;
    private Node _currentNode;

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
            if (_rngStepSize) NewStepSize();

            _position = _stack.Peek();
            if (CanMove())
            {
                Step();
                _stack.Push(_position);
            }
            else
            {
                Vector2Int nextPos = _stack.Pop();
                bool found = false;
                while (!found)
                {
                    if (_currentNode.position == nextPos)
                    {
                        found = true;
                    }
                    else
                    {
                        _currentNode = _currentNode.parent;
                    }
                }
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

            Node newNode = new Node(_position, _currentNode.score + 1, _currentNode);
            _currentNode.AddChild(newNode);
            _currentNode = newNode;
        }
    }


    private void NewStepSize()
    {
        _stepSize = (_rng.Next(1, _stepMaxSize / 2)) * 2;
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
        _position = new Vector2Int(1, 1);

        _grid = new bool[_width, _height];

        _stack = new Stack<Vector2Int>();
        _stack.Push(_position);

        if (_rngStepSize) NewStepSize();

        _nodes = new Node(_position, 0);
        _currentNode = _nodes;
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

        CreateFloorsFromNode(_nodes);
    }

    private void CreateFloorsFromNode(Node node)
    {
        foreach (Node child in node.children)
        {
            GameObject text = Instantiate(_textNodePrefab, transform);
            text.transform.position = new Vector3(child.position.x, 0, child.position.y);
            text.GetComponent<TextMeshPro>().text = $" {child.score.ToString()} ";

            CreateFloorsFromNode(child);
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
