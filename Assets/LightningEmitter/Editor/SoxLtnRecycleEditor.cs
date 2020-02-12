using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SoxLtnRecycle))]
public class SoxLtnRecycleEditor : Editor
{

#if UNITY_EDITOR
    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            SoxLtnRecycle soxLthRecycle = (SoxLtnRecycle)target;
            GameObject.DestroyImmediate(soxLthRecycle.gameObject);
        }
    }
#endif

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
