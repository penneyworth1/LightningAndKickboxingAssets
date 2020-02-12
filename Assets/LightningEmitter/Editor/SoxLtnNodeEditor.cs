using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoxLtnNode))][CanEditMultipleObjects]
public class SoxLtnNodeEditor : Editor
{
    private bool ifEditorAnimationPlaying = false;

    public override void OnInspectorGUI()
    {
        SoxLtnNode soxLtnNode = target as SoxLtnNode;

        if (ifEditorAnimationPlaying)
        {
            EditorGUILayout.HelpBox("Nodes do not rotate automatically in Animation mode.", MessageType.Info);
        }

        DrawDefaultInspector();
        if (soxLtnNode.meIndex == 0 || soxLtnNode.meIndex == (soxLtnNode.nodeCount - 1))
        {
            soxLtnNode.autoMiddlePosition = false;
            GUI.enabled = false;
        }
        soxLtnNode.autoMiddlePosition = EditorGUILayout.Toggle("Auto middle position", soxLtnNode.autoMiddlePosition);
        GUI.enabled = true;
    }

    void OnSceneGUI()
    {
        SoxLtnNode soxLtnNode = target as SoxLtnNode;
        if (soxLtnNode.soxLtn == null) return;

        ifEditorAnimationPlaying = AnimationMode.InAnimationMode(); // 애니메이터 창이 애니메이션모드인지를 노드에 세팅해줌
        soxLtnNode.ifEditorAnimationPlaying = ifEditorAnimationPlaying;

        SoxLtnNode[] soxLtnNodes = soxLtnNode.soxLtn.GetSoxLtnNodesInChildren();
        if (!soxLtnNode) return;
        if (!soxLtnNode.soxLtn) return;

        if (soxLtnNode.circleArea < 0)
            soxLtnNode.circleArea = 0;

        // scaledDotSize 는 soxLtn 노드의 스케일이 변경될 때 CubeCap 역시 같이 스케일이 변하도록 하기 위한 변수
        float scaledDotSize = (
            Mathf.Abs(soxLtnNode.transform.lossyScale.x) +
            Mathf.Abs(soxLtnNode.transform.lossyScale.y) +
            Mathf.Abs(soxLtnNode.transform.lossyScale.z)
            ) / 3.0f;
        Handles.color = Color.yellow;
        Handles.CubeHandleCap(0, soxLtnNode.transform.position, soxLtnNode.transform.rotation, soxLtnNode.soxLtn.dotSize * scaledDotSize, EventType.Repaint);

        switch (soxLtnNode.nodeType)
        {
            case SoxLtnNode.NodeType.CIRCLE:
                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.CircleHandleCap(0, soxLtnNode.transform.position + new Vector3(0, 0, 0), soxLtnNode.transform.rotation, soxLtnNode.circleArea * scaledDotSize, EventType.Repaint);
                break;
            case SoxLtnNode.NodeType.SPHERE:
                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.CircleHandleCap(0, soxLtnNode.transform.position + new Vector3(0, 0, 0), soxLtnNode.transform.rotation, soxLtnNode.circleArea * scaledDotSize, EventType.Repaint);

                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.CircleHandleCap(0, soxLtnNode.transform.position + new Vector3(0, 0, 0), soxLtnNode.transform.rotation * Quaternion.AngleAxis(90, Vector3.up), soxLtnNode.circleArea * scaledDotSize, EventType.Repaint);

                Handles.color = new Color(1, 1, 1, 0.5f);
                Handles.CircleHandleCap(0, soxLtnNode.transform.position + new Vector3(0, 0, 0), soxLtnNode.transform.rotation * Quaternion.AngleAxis(90, Vector3.right), soxLtnNode.circleArea * scaledDotSize, EventType.Repaint);
                break;
            default:
                break;
        }//Switch end

        // nodeCount와 meIndex 적용, 각 노드마다 이 처리가 있는 이유는, SoxLtn 루트가 아닌 자식 노드의 수동 복제도 있을 수 있기 때문
        soxLtnNode.nodeCount = soxLtnNodes.Length;
        for (int i = 0; i < soxLtnNode.nodeCount; i++)
        {
            if (soxLtnNode.soxLtn.soxLtnNodes[i] != null)
            {
                if (soxLtnNode.soxLtn.soxLtnNodes[i].transform == soxLtnNode.transform)
                {
                    soxLtnNode.meIndex = i;
                }
            }
        }
        
        if (soxLtnNode.nodeCount >= 2)
        {
            Vector3 camPos;
            camPos = SceneView.lastActiveSceneView.camera.transform.position;

            if ((soxLtnNode.meIndex == 0) || ((soxLtnNode.meIndex + 1) == soxLtnNode.nodeCount))
            {
                // 처음 혹은 마지막 노드인 경우
                if (soxLtnNode.meIndex == 0)
                {   // 처음 노드
                    soxLtnNode.editorCamPos = camPos;
                    soxLtnNode.Update();
                    soxLtnNodes[1].editorCamPos = camPos;
                    soxLtnNodes[1].Update();
                }
                else
                {
                    // 마지막 노드인 경우
                    soxLtnNode.editorCamPos = camPos;
                    soxLtnNode.Update();
                    soxLtnNodes[soxLtnNode.meIndex - 1].editorCamPos = camPos;
                    soxLtnNodes[soxLtnNode.meIndex - 1].Update();
                }
            }
            else
            {
                // 중간 노드인 경우
                soxLtnNode.editorCamPos = camPos;
                soxLtnNode.Update();
                soxLtnNodes[soxLtnNode.meIndex + 1].editorCamPos = camPos;
                soxLtnNodes[soxLtnNode.meIndex + 1].Update();
                soxLtnNodes[soxLtnNode.meIndex - 1].editorCamPos = camPos;
                soxLtnNodes[soxLtnNode.meIndex - 1].Update();
            }
        }
    }
}