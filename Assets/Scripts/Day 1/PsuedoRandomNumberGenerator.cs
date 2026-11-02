using UnityEngine;

public class PsuedoRandomNumberGenerator : MonoBehaviour
{
    private System.Random _rng = new System.Random(23);




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print(_rng.Next(10));
        print(_rng.Next(10));
        print(_rng.Next(10));
        print(_rng.Next(10));
        print(_rng.Next(10));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
