using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MunsellTestManager))]
public class MunsellTestManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Regenerate Test"))
        {
            (target as MunsellTestManager).RegenerateTest();
        }
    }
}