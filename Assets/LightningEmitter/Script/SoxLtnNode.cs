using UnityEngine;
using System.Collections;

public class SoxLtnNode : MonoBehaviour
{
    public enum NodeType
    {
        CIRCLE,
        SPHERE
	}

    public enum FillType
    {
        INSIDE,
        SURFACE
    }

    // 에디터에서 애니메이션 창이 Playing 혹은 Auto Recording 상태인지를 체크하는 변수. 이 상태에서 카메라를 LookAt 하면 안된다. (불필요한 Rotation 키가 자꾸 생긴다)
    // 알아낼 수 있는 AnimationMode 가 에디터 클래스라서 바깥인 에디터에서 세팅해줘야한다.
    [HideInInspector]
    public bool ifEditorAnimationPlaying = false;

    public float circleArea = 1;
    public NodeType nodeType;
    public FillType fillType;

    public float zTwistMin;
    public float zTwistMax;

    public bool positionConstraintToAttachNode = false;
    public Transform attachNode;

    // Auto Middle Position 옵션은 중간 노드일 때에만 보여야해서 Hide 해주고 OnInspectorGUI()에서 처리함
    [HideInInspector]
    public bool autoMiddlePosition = false;
    
    [HideInInspector]
    public int nodeCount;
    [HideInInspector]
    public int meIndex;
    [HideInInspector]
    public float distance;

    public SoxLtn soxLtn;
    [HideInInspector]
    public Vector3 camVec = Vector3.zero;
    [HideInInspector]
    public Vector3 editorCamPos = Vector3.zero;

    private bool initialized = false;
    [HideInInspector]
    public bool firstLookAt = true;

    private Transform cameraMain;

    private void OnEnable()
    {
        initialized = false;
    }

    public void Initialize()
    {
        firstLookAt = true;
        if (soxLtn.firstNode != null && meIndex == 0)
        {
            attachNode = soxLtn.firstNode;
        }

        if (soxLtn.lastNode != null && meIndex == (nodeCount - 1))
        {
            attachNode = soxLtn.lastNode;
        }

        cameraMain = GetMainCamera();

        initialized = true;
    }

    public void Update()
    {
        if (!soxLtn) return;

        if (initialized == false)
            Initialize();

#if UNITY_EDITOR
        //에디터 모드일 때에만 체크
        if (!Application.isPlaying)
        {
            // 에디터에서 수동으로 노드를 강제로 삭제할 경우 nodeCount 값이 순간적으로 잘못 반영될 때가 있어서 매 번 업데이트 해줘야한다.
            soxLtn.soxLtnNodes = soxLtn.GetSoxLtnNodesInChildren();
            nodeCount = soxLtn.soxLtnNodes.Length;
            for (int i = 0; i < soxLtn.soxLtnNodes.Length; i++)
            {
                if (this == soxLtn.soxLtnNodes[i]) meIndex = i;
            }
        }
#endif

        //autoMiddlePosition 은 LookAt 보다 먼저 처리되어야한다.
        if (autoMiddlePosition && soxLtn.soxLtnNodes != null)
        {
            float mePos = 1.0f / (float)(nodeCount - 1) * (float)meIndex;
            transform.position = Vector3.Lerp(soxLtn.soxLtnNodes[0].transform.position, soxLtn.soxLtnNodes[soxLtn.soxLtnNodes.Length - 1].transform.position, mePos);
        }

        if (Application.isPlaying)
        {
            if (soxLtn.soxLtnRecycle == null)
                return;

            if (soxLtn.soxLtnRecycle.onEffects.Count > 0) // 뭔가 살아있는 이펙트가 하나라도 있을 때에만 카메라 Look 처리를 한다. 그렇지 않을 경우 재생이 끝났는데 꺼지지 않은 이펙트에서 계속 Look 업데이트가 발생할 수 있음.
            {
                if (soxLtn.animType == SoxLtn.AnimType.DYNAMIC)
                {
                    // DYNAMIC 타입이고 Play중이면 NodeCameraLookAtAndDistance()을 항상 해준다. Node의 distance 와 방향을 업데이트 해줘야함.
                    NodeCameraLookAtAndDistance();
                }
                else
                {
                    // STATIC 이면 LookAtCamera가 ALWAYS인 경우에만 항상 LookAt 한다.
                    if (soxLtn.lookAtCamera == SoxLtn.LookAtCamera.ALWAYS)
                    {
                        // ALWAYS
                        NodeCameraLookAtAndDistance();
                    }
                    else
                    {
                        // BIRTH
                        if (firstLookAt)
                        {
                            NodeCameraLookAtAndDistance();
                        }
                    }
                }

                firstLookAt = false;
            }
        }
        else
        {   // 에디터 상태일 때에는 언제나 LookAt 처리를 해준다. Recycle이 있을 경우에만.
            if (soxLtn.soxLtnRecycle != null)
                NodeCameraLookAtAndDistance();
        }

        //여기까지 에디터모드를 고려함.
        if (!soxLtn.activate) // Ltn이 activate가 아니면 이후 처리할 필요가 없음.
            return;

        if (positionConstraintToAttachNode && attachNode != null)
        {
            transform.position = attachNode.position;
            ShellOffsetNode();
        }
    } // end of Update ()

    public void ShellOffsetNode()
    {
        if (soxLtn == null)
        {
            return;
        }

        if (meIndex == 0 && soxLtn.shellOffsetStart != 0.0f && attachNode != null)
        {
            // 첫 노드의 위치에서 두 번째 노드의 위치 방향으로 shellOffsetStart만큼 이동시킨다.
            transform.position = attachNode.position + (soxLtn.soxLtnNodes[1].transform.position - attachNode.position).normalized * soxLtn.shellOffsetStart;
        }

        if (meIndex == (nodeCount - 1) && soxLtn.shellOffsetEnd != 0.0f && attachNode != null)
        {
            // 마지막 노드의 위치에서 바로 전 노드의 위치 방향으로 shellOffsetEnd만큼 이동시킨다.
            transform.position = attachNode.position + (soxLtn.soxLtnNodes[soxLtn.soxLtnNodes.Length - 2].transform.position - attachNode.position).normalized * soxLtn.shellOffsetEnd;
        }
    }

    // LookAt도 하지만 노드간 distance도 세팅한다. (distance는 부산물)
    public void NodeCameraLookAtAndDistance()
    {
        if (nodeCount >= 2)
        {
            if (Application.isPlaying)
            {   // Play mode
                if (cameraMain)
                {
                    //메인 카메라가 있는 경우
                    camVec = -cameraMain.forward;
                }
                else
                {
                    //메인 카메라가 없는 경우
                    camVec = Vector3.up;
                }
            }
            else
            {   // Editor mode
                camVec = editorCamPos - transform.position;
            }

            if ((meIndex == 0) || ((meIndex + 1) == nodeCount))
            {
                // 처음 혹은 마지막 노드인 경우
                if (meIndex == 0)
                {
                    // 처음 노드인 경우
                    if (!ifEditorAnimationPlaying)
                        transform.LookAt(soxLtn.soxLtnNodes[meIndex + 1].transform, camVec);
                    distance = 0.0f;
                }
                else
                {
                    // 마지막 노드인 경우
                    Transform beforeNode = soxLtn.soxLtnNodes[meIndex - 1].transform;
                    Vector3 lookPos = transform.position - beforeNode.position;
                    distance = lookPos.magnitude;
                    if (distance != 0 && !ifEditorAnimationPlaying)
                    {
                        transform.rotation = Quaternion.LookRotation(lookPos, camVec);
                    }
                }
            }
            else
            {
                // 중간 노드인 경우
                Transform beforeNode = soxLtn.soxLtnNodes[meIndex - 1].transform;
                Vector3 lookPos = transform.position - beforeNode.position;
                distance = lookPos.magnitude;
                Quaternion rotBefore = Quaternion.identity;
                if (distance != 0)
                {
                    rotBefore = Quaternion.LookRotation(lookPos, camVec);
                }

                Vector3 lookPosAfter = soxLtn.soxLtnNodes[meIndex + 1].transform.position - transform.position;
                if (lookPosAfter.sqrMagnitude != 0 && !ifEditorAnimationPlaying)
                {
                    Quaternion rotAfter = Quaternion.LookRotation(lookPosAfter, camVec);
                    transform.rotation = Quaternion.Slerp(rotBefore, rotAfter, 0.5f);
                }
            }
        } // end of if (nodeCount >= 2)
    } // end of NodeCameraLookAtAndDistance()

    private Transform GetMainCamera()
    {
        if (soxLtn.ifUseCameraTag && soxLtn.cameraTag != "Untagged")
        {
            //카메라 태그를 사용하는 경우, 그리고 태그에 Untagged가 아닌 뭔가가 지정되어있을 때
            GameObject[] cameraTagObjs = GameObject.FindGameObjectsWithTag(soxLtn.cameraTag);
            foreach (GameObject go in cameraTagObjs)
            {
                Camera tempCamera = go.GetComponent<Camera>();
                if (tempCamera != null)
                {
                    return (tempCamera.transform);
                }
            }
        }

        // 카메라 태그를 사용하지 않는 일반 경우.
        if (Camera.main != null)
        {
            return (Camera.main.transform);
        }

        GameObject mainCameraTagObj = GameObject.FindWithTag("MainCamera");
        if (mainCameraTagObj != null)
        {
            return (mainCameraTagObj.transform);
        }

        foreach (Camera c in Camera.allCameras)
        {
            if (c.enabled)
            {
                return (c.transform);
            }
        }

        return null;
    }
}