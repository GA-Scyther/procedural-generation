using NUnit.Framework;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PoissonDisc : MonoBehaviour
{

    public int attempts = 30;
    public int seed = 23;

    public int width = 32;
    public int height = 32;

    public float radius = 1f;

    private List<Vector2> _points;
    private List<Vector2> _active;
    private System.Random _rng;


    public void Init()
    {
        _rng = new System.Random(seed);

        _points = new List<Vector2>();
        _active = new List<Vector2>();

    }

    public void Generate()
    {
        Vector2 firstPoint = new Vector2(_rng.Next(0, width), _rng.Next(0, height));
        _points.Add(firstPoint);
        _active.Add(firstPoint);

        while (_active.Count > 0)
        {
            int i = _rng.Next(_active.Count);
            Vector2 centre = _active[i];

            for (int a = 0; a < attempts; a++)
            {
                float angle = (float)_rng.NextDouble() * Mathf.PI * 2;
                float distance = radius + (float)_rng.NextDouble() * radius;

                Vector2 candidate = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                if (candidate.x > width || candidate.y > width)
                {
                    continue;
                }
            }
        }

    }

    public List<Vector2> GetPoints()
    {
        return _points;
    }
}
