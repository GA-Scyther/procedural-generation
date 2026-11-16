using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEditor.Search;
using UnityEngine;

public class Cave2
{
    // Wall = 1, Empty = 0
    private int[,] _matrix;
    private int _fillPercentage;

    private int _seed;
    private System.Random _rng;

    public Cave2(int size, int fillPercentage, int seed)
    {
        _matrix = new int[size, size];
        _fillPercentage = fillPercentage;
        
        _seed = seed;
        _rng = new System.Random(_seed);
    }

    // Get Cell Neighbours
    public int CountWallNeighbours(int x, int y)
    {
        //int count = 0;

        //// Y - 1
        //count += _matrix[x - 1, y - 1];
        //count += _matrix[x, y - 1];
        //count += _matrix[x + 1, y - 1];

        //// Y
        //count += _matrix[x - 1, y];
        //count += _matrix[x + 1, y];

        //// Y + 1
        //count += _matrix[x - 1, y + 1];
        //count += _matrix[x, y + 1];
        //count += _matrix[x + 1, y + 1];


        int count = -_matrix[x, y];

        for (int oX = -1; oX < 2; oX++)
        {
            for (int oY = -1; oY < 2; oY++)
            {
                count += _matrix[x + oX, y + oY];
            }
        }

        return count;
    }

    public void Pass()
    {
        int size = _matrix.GetLength(0);
        //int[,] matrix = _matrix;
        int[,] matrix = new int[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                {
                    matrix[x, y] = 1;
                    continue;
                }

                int neighbours = CountWallNeighbours(x, y);
                if (neighbours < 4)
                {
                    matrix[x, y] = 0;
                }
                else if (neighbours > 4)
                {
                    matrix[x, y] = 1;
                }
                else
                {
                    matrix[x, y] = _matrix[x, y];
                }
            }
        }

        _matrix = matrix;
    }

    public void GenerateMatrix()
    {
        int size = _matrix.GetLength(0);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x == 0 || y == 0 || x == size-1 || y == size-1)
                {
                    _matrix[x, y] = 1;
                    continue;
                }

                int rand = _rng.Next(0, 100);
                if (rand < _fillPercentage) _matrix[x, y] = 1;
            }
        }
    }

    public int[,] NumberCaves(out int counter, out List<int> sizes)
    {
        int size = _matrix.GetLength(0);
        int[,] matrix = new int[size, size];

        counter = 0;

        sizes = new List<int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // If empty and undiscovered
                if (_matrix[x, y] == 0 && matrix[x,y] == 0)
                {
                    counter++;
                    sizes.Add(0);
                    Queue<int2> queue = new Queue<int2>();

                    queue.Enqueue(new(x,y));

                    while (queue.Count > 0)
                    {
                        // Get Current Position
                        int2 position = queue.Peek();
                        sizes[counter-1] += 1;

                        // Set Position to Counter
                        matrix[position.x, position.y] = counter;


                        int2 checkPosition = new(position.x + 1, position.y);
                        if (_matrix[checkPosition.x, checkPosition.y] == 0 && matrix[checkPosition.x, checkPosition.y] == 0)
                        {
                            queue.Enqueue(checkPosition);
                            matrix[checkPosition.x, checkPosition.y] = -1;
                        }

                        checkPosition = new(position.x - 1, position.y);
                        if (_matrix[checkPosition.x, checkPosition.y] == 0 && matrix[checkPosition.x, checkPosition.y] == 0)
                        {
                            queue.Enqueue(checkPosition);
                            matrix[checkPosition.x, checkPosition.y] = -1;
                        }


                        checkPosition = new(position.x, position.y + 1);
                        if (_matrix[checkPosition.x, checkPosition.y] == 0 && matrix[checkPosition.x, checkPosition.y] == 0)
                        {
                            queue.Enqueue(checkPosition);
                            matrix[checkPosition.x, checkPosition.y] = -1;
                        }

                        checkPosition = new(position.x, position.y - 1);
                        if (_matrix[checkPosition.x, checkPosition.y] == 0 && matrix[checkPosition.x, checkPosition.y] == 0)
                        {
                            queue.Enqueue(checkPosition);
                            matrix[checkPosition.x, checkPosition.y] = -1;
                        }


                        queue.Dequeue();
                    }
                }
            }
        }

        return matrix;
    }

    public static int[,] MaskMatrix(int[,] matrix, int desired, bool invert = false)
    {
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (matrix[x, y] != desired && !invert)
                {
                    matrix[x, y] = 0;
                }
                else if (matrix[x, y] == desired && invert)
                {
                    matrix[x, y] = 0;
                }
            }
        }

        return matrix;
    }

    public int GetNeighbourValue(int x, int y, int[,] matrix)
    {
        if (matrix[x - 1, y] != 0) return matrix[x - 1, y];

        if (matrix[x, y - 1] != 0) return matrix[x, y - 1];

        if (matrix[x + 1, y] != 0) return matrix[x + 1, y];

        if (matrix[x, y + 1] != 0) return matrix[x, y + 1];

        return 0;
    }

    public int[,] GetMatrix()
    {
        return _matrix;
    }

    public int GetCell(int x, int y)
    {
        return _matrix[x, y];
    }

}


public class Cave3
{
    // Wall = 1, Empty = 0
    private int[,,] _matrix;
    private int _fillPercentage;

    private int _seed;
    private System.Random _rng;

    public Cave3(int size, int fillPercentage, int seed)
    {
        _matrix = new int[size, size, size];
        _fillPercentage = fillPercentage;

        _seed = seed;
        _rng = new System.Random(_seed);
    }

    // Get Cell Neighbours
    public int CountWallNeighbours(int x, int y, int z)
    {
        int count = - _matrix[x, y, z];

        for (int oX = -1; oX < 2; oX++)
        {
            for (int oY = -1; oY < 2; oY++)
            {
                for (int oZ = -1; oZ < 2; oZ++)
                {
                    count += _matrix[x + oX, y + oY, z + oZ];
                }
            }
        }

        return count;
    }

    public void Pass()
    {
        int size = _matrix.GetLength(0);
        int[,,] matrix = _matrix;

        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                for (int z = 1; z < size - 1; z++)
                {
                    if (x == 0 || y == 0 || z == 0 || x == size - 1 || y == size - 1 || z == size - 1)
                    {
                        _matrix[x, y, z] = 1;
                        continue;
                    }

                    int neighbours = CountWallNeighbours(x, y, z);
                    if (neighbours < 9)
                    {
                        matrix[x, y, z] = 0;
                    }
                    else if (neighbours > 15)
                    {
                        matrix[x, y, z] = 1;
                    }
                    else
                    {
                        matrix[x, y, z] = _matrix[x, y, z];
                    }
                }
            }
        }

        _matrix = matrix;
    }

    public void GenerateMatrix()
    {
        int size = _matrix.GetLength(0);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (x == 0 || y == 0 || z == 0|| x == size - 1 || y == size - 1 || z == size - 1)
                    {
                        _matrix[x, y, z] = 1;
                        continue;
                    }

                    int rand = _rng.Next(0, 100);
                    if (rand < _fillPercentage) _matrix[x, y, z] = 1;
                }
            }
        }
    }



    public int[,,] GetMatrix()
    {
        return _matrix;
    }

    public int GetCell(int x, int y, int z)
    {
        return _matrix[x, y, z];
    }

}