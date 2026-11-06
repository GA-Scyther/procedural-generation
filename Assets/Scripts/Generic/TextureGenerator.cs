using NUnit.Framework.Internal.Commands;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{

    public static Texture2D GenerateBlankTexture(int width, int height, bool apply = true)
    {
        Texture2D texture = new Texture2D(width, height);
        texture = Texture2D.whiteTexture;
        texture.filterMode = FilterMode.Point;

        if (apply) texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromMatrix(bool[,] matrix, Color trueColor, Color falseColor, bool apply = true)
    {
        Texture2D texture = new Texture2D(matrix.GetLength(0), matrix.GetLength(1));

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (matrix[x, y])
                {
                    texture.SetPixel(x, y, trueColor);
                }
                else
                {
                    texture.SetPixel(x, y, falseColor);
                }
            }
        }


        texture.filterMode = FilterMode.Point;
        if (apply) texture.Apply();
        return texture;
    }
    
    public static Texture2D TextureFromMatrix(bool[,] matrix, bool apply = true)
    {
        return TextureFromMatrix(matrix, Color.white, Color.black, apply);
    }

    public static Texture2D TextureFromMatrix(bool[,] matrix, Color trueColor, bool apply = true)
    {
        return TextureFromMatrix(matrix, trueColor, Color.black, apply);
    }


    public static Texture2D TextureFromMatrix(int[,] matrix, int max, Color minColor, Color maxColor, bool blackNegatives = true, bool whiteZero = false, bool apply = true)
    {
        Texture2D texture = new Texture2D(matrix.GetLength(0), matrix.GetLength(1));

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (matrix[x, y] < 0 && blackNegatives)
                {
                    texture.SetPixel(x, y, Color.black);
                }
                else if (matrix[x, y] == 0 && whiteZero)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.Lerp(minColor, maxColor, (float)matrix[x, y] / max));
                }
            }
        }

        texture.filterMode = FilterMode.Point;
        if (apply) texture.Apply();
        return texture;
    }
    public static Texture2D TextureFromMatrix(int[,] matrix, int max, bool blackNegatives = true, bool whiteZero = false, bool apply = true)
    {
        Texture2D texture = new Texture2D(matrix.GetLength(0), matrix.GetLength(1));

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (matrix[x, y] < 0 && blackNegatives)
                {
                    texture.SetPixel(x, y, Color.black);
                }
                else if (matrix[x, y] == 0 && whiteZero)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    Color color = Color.HSVToRGB((float)matrix[x, y] / max, 1, 1);
                    texture.SetPixel(x, y, color);
                }
            }
        }

        texture.filterMode = FilterMode.Point;
        if (apply) texture.Apply();
        return texture;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
