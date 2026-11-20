using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileSetReference", menuName = "Scriptable Objects/TileSetReference")]
public class TileSetReference : ScriptableObject
{
    public TileBase[] tiles;

    public TileBase Get(byte i)
    {
        return tiles[(int)i];
    }
}
