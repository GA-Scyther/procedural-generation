using UnityEngine;
using UnityEditor;

public class TerrainGeneratorGUI : EditorWindow
{
    public int size;
    public int seed;

    [MenuItem("Tools/Procedural Generation/Create Terrain...")]
    static void Open()
    {
        GetWindow<TerrainGeneratorGUI>("Worlder Creator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("World Settings", EditorStyles.boldLabel);
        size = EditorGUILayout.IntField("Size", size);
        seed = EditorGUILayout.IntField("Seed", seed);

        if (GUILayout.Button("Generate"))
        {

        }
    }
}
