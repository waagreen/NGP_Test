using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveDataManager))]
public class DataManagerCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveDataManager data = (SaveDataManager)target;
        GUIStyle style = new(GUI.skin.button)
        {
            fontSize = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            fixedWidth = 200,
            margin = new RectOffset(100, 0, 15, 0),
            padding = new RectOffset(10, 10, 5, 5),
            richText = true,
        };

        if (GUILayout.Button($"<color=#F03C32>CLEAR SAVE DATA!</color>", style))
        {
            data.ClearGameData();
        }
    }
}
