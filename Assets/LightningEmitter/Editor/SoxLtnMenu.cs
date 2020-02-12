using UnityEngine;
using UnityEditor;

public class SoxLtnMenu
{
    [MenuItem("GameObject/Effects/Lightning Emitter", false, 12)]
    private static void CreateLtn(MenuCommand sel)
    {
        GameObject newLtn = new GameObject("LightningEmitter");
        SoxLtn soxLtn = newLtn.AddComponent<SoxLtn>();
        SoxLtnUtilEditor.CheckNodes(soxLtn);
        soxLtn.version = soxLtn.versionNow;

        GameObject selObj = (GameObject)sel.context;

        if (selObj != null)
        {
            newLtn.transform.SetParent(selObj.transform);
            newLtn.transform.localPosition = Vector3.zero;
            newLtn.transform.localEulerAngles = Vector3.zero;
            newLtn.transform.localScale = Vector3.one;
        }

        if (Selection.gameObjects.Length <= 1)
        {
            Selection.activeGameObject = newLtn;
        }

        Undo.RegisterCreatedObjectUndo(newLtn, "Create Lightning Emitter");
    }
}
