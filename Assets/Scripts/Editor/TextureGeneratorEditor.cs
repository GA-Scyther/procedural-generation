using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureDisplayer))]
public class TextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw the default inspector fields (like variables)
        DrawDefaultInspector();

        // 2. Get a referenced link to the actual target script
        TextureDisplayer script = (TextureDisplayer)target;

        // 3. Create the spacing and button
        GUILayout.Space(10);
        if (GUILayout.Button("Generate Texure"))
        {
            script.Generate();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Generate Mesh"))
        {
            script.GenerateMesh();
        }
    }
}
