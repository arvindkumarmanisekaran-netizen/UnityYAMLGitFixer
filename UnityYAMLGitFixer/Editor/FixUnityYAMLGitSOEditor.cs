using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FixUnityYAMLGitSO))]
public class FixUnityYAMLGitSOEditor : Editor
{
    private GUIStyle statusLabelColor = new GUIStyle();

    private Color currentStatusColor = Color.white;

    void OnEnable()
    {
        statusLabelColor.normal.textColor = Color.red;
        statusLabelColor.fontSize = 10;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);

        string currentStatus = FixUnityYAMLGit.GetCurrentStatus(out currentStatusColor);

        statusLabelColor.normal.textColor = currentStatusColor;

        GUILayout.Label($"Status: {currentStatus}", statusLabelColor);

        GUILayout.Space(20);
    }
}
