using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MazeNode
{
    public Vector2Int _mazePosition = Vector2Int.zero;

    public List<MazeNode> _children = new List<MazeNode>();
    public MazeNode _parent = null;


    public int _score = 0;


    public MazeNode(Vector2Int mazePosition, MazeNode parent)
    {
        _mazePosition = mazePosition;
        _parent = parent;
    }

    public void AddChild(MazeNode child)
    {
        _children.Add(child);
    }


    // BFS Search for node with highest score
    public static MazeNode SearchNodesForHighestScore(MazeNode node)
    {
        Queue<MazeNode> queue = new Queue<MazeNode>();
        queue.Enqueue(node);
        MazeNode highestScore = node;

        MazeNode current = node;
        while (queue.Count > 0)
        {
            current = queue.Peek();
            foreach (MazeNode child in current._children)
            {
                queue.Enqueue(child);
            }
            if (current._score > highestScore._score)
            {
                highestScore = current;
            }
            queue.Dequeue();
        }

        return highestScore;
    }
}
