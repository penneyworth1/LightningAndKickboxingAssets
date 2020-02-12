using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public static class SoxLtnEditorSettings
{
    // ltnMatScroll 대신 에디터용으로 사용하는 재질
    // 에디터 Preview용으로 인스턴싱된 재질을 soxLtn에 넣다보니 프리팹 Apply시에 문제됨 (존재하지 않는 재질이라서 그런 듯). 그래서 에디터 프리뷰용 스태틱 재질 사용
    public static Material ltnMatScrollEditor;
}
#endif

public class SoxLtn : MonoBehaviour {
#if UNITY_EDITOR
    [HideInInspector]
    public float versionNow = 1.403f;
#endif

    [HideInInspector]
    public float version;

    // 메인 프로그래머가 라이트닝 이펙트를 씬에 Instance할 때 사용하는 시작 위치와 끝 위치 제어용
    // Use this variable when you instantiate soxLtn in scene.
    public Transform firstNode;
    public Transform lastNode;

    [System.Serializable]  //EditorGuiLayout 에서 사용하기 위해 Serializable 처리
    public struct Burst
    {
        public float interval;
        public int streakCount;
    }

    [System.Serializable]  //EditorGuiLayout 에서 사용하기 위해 Serializable 처리
    public struct LinkedParticleObjs
    {
        public GameObject particleObj;
        public bool rotationAlign;
        public bool attachToNode;
    }

    public enum ConstantRelated
    { CONSTANT, DISTANCE_RELATED }

    public enum AnimType
    { STATIC, DYNAMIC }

    public enum LookAtCamera
    { BIRTH, ALWAYS }

    public enum UVType
    { FIT, SCALE }

    public enum TexSheetBirtyType
    { BOTTOM, RANDOM }

    public enum TexSheetAnimType
    { SERIAL, RANDOM }

    public enum AnimCurveType
    { CONSTANT, LINEAR, EASEOUT, EASEIN }

    public enum AutoTerminateType
    { DESTROY, DEACTIVATE }
    public AutoTerminateType autoTerminateType = AutoTerminateType.DESTROY;

    public SoxLtnNode[] soxLtnNodes;    // 라이트닝 노드들을 기억하는 배열

    public float shellOffsetStart = 0f;
    public float shellOffsetEnd = 0f;

    public bool recalculateNormals = true;
    public bool recalculateTangents = false;

    public float playDelay = 0f;
    // ifPlayWithChildren - 이 변수는 playDelay 를 위해서 Play() 함수를 invoke 처리해야하는데 Play 함수에 withChildren 옵션이 있는 경우도 있고 없는 경우도 있어서 이에 대응하기 위한 변수임
    public bool ifPlayWithChildren = true;

    public float dotSize = 0.2f;
    public float unityEditorTimeCorrect = 1f;
    
    // 카메라 태그 (없으면 알아서 찾도록), 주로 GUI에서 따로 카메라를 사용할 경우 태그를 직접 지정할 수 있도록 하기 위함임
    public bool ifUseCameraTag = false;
    public string cameraTag = "Untagged";

    public bool activate = false;
    private bool activateBefore = false;   // 이전 프레임의 activate 상태를 체크하여 처음 activate 되었는지 알기 위함
    public bool autoActivateBirth = true;
    public bool loop = false;
    public bool autoTerminate = false;

    // ifPlayed 대해서.
    // 오브젝트 최초 생성시에는 Enabled 와 Start가 활성화되고, 이후 오브젝트가 있는 상태에서 Enabled 에서는 Start가 작동하지 않으므로
    // Enabled 에 Play()를 넣으면 최초 생성시 두 번 Play() 된다. 그렇다고 안 넣으면 재활용시 Enabled 상황에서 Play()가 안된다.
    // 이런 이유로 Enabled 와 Start 모두 Play()를 넣고서, Start에서의 Play를 할지 말지 검사하는 변수가 필요함
    private bool ifPlayed = false;
    private bool ifEnabled = false; // OnEnabled 함수에 관련 이슈 있음

    public bool isStopped
    {
        get
        {
            if (soxLtnRecycle == null)
            {
                return true;
            }

            if (soxLtnRecycle.onEffects.Count <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public float genDuration = 2f;
    public float genRate = 10f;
    private int genedCount = 0;

    public float burstRandomOffsetMax = 0f;
    private float burstRandomOffset = 0f;
    public Burst[] bursts;
    private int burstIndex;    // Burst 배열에서 어느 인덱스까지 발생시켰는지
    private float accumInterval;  // Burst의 Interval 값들을 누적하는 변수

    public Color appearColor = new Color(1f, 1f, 1f, 0f);
    public AnimCurveType appearColorType = AnimCurveType.LINEAR;
    public AnimCurveType appearThinType = AnimCurveType.CONSTANT;
    public Vector2 appearMinMax = new Vector2(0f, 0f);
    public Vector2 lifeMinMax = new Vector2(0.1f, 0.1f);
    public Color mainColor = new Color(1f, 1f, 1f, 1f);
    public Vector2 fadeMinMax = new Vector2(0.5f, 0.5f);
    public Color fadeOutColor = new Color(1f, 1f, 1f, 0f);
    public AnimCurveType fadeOutColorType = AnimCurveType.LINEAR;
    public AnimCurveType fadeOutThinType = AnimCurveType.CONSTANT;

    public Vector2 streakWidthMinMax = new Vector2(0.2f, 0.6f);

    public Material ltnMat; // 에디터용 원본 재질
    private Material ltnMatBefore; // 에디터용 원본 재질이 변경된 경우에만 ltnMatScroll 로 인스턴싱 해아한다. 이런 체크 없이 AnyStart에서 무조건 인스턴싱 해버리면 배칭이 안됨.
    public Material ltnMatScroll; // 원본 보존용 인스턴싱 재질
    public Material material
    {
        get { return ltnMatScroll; }
    }

    //public List<Renderer> effectRenderers; // 이펙트 개별 스크롤 속도 제어를 위해 개별 재질을 사용하려고 만들었으나 봉인

    public int sortinglayer = 0;
    public int orderInLayer = 0;

    public int nodeCount = 3;
    public int subStep = 2;
    public ConstantRelated subStepNoiseAmpType = ConstantRelated.CONSTANT;
    public float subStepNoiseAmp = 0f;
    public float nodeDistanceAll;

    // 웨이브 애니메이션을 위한 구조체
    [System.Serializable]  //EditorGuiLayout 에서 사용하기 위해 Serializable 처리
    public struct WaveAnim
    {
        // 주파수, 웨이브가 얼마나 빽빽하게 발생하는지
        public float frequency;
        public float frequencyRandom;
        // 웨이브가 공간상의 어떤 방향으로 발생하는지 Vector3
        public Vector3 amplify;
        public Vector3 amplifyRandom;
        // 스크롤 속도, 마이너스 속도도 가능함
        public float scrollSpeed;
        public float scrollSpeedRandom;

        public WaveAnim(float _frequency, float _frequencyRandom, Vector3 _amplify, Vector3 _amplifyRandom, float _scrollSpeed, float _scrollSpeedRandom)
        {
            frequency = _frequency;
            frequencyRandom = _frequencyRandom;
            amplify = _amplify;
            amplifyRandom = _amplifyRandom;
            scrollSpeed = _scrollSpeed;
            scrollSpeedRandom = _scrollSpeedRandom;
        }
    }

    public WaveAnim[] waves = new WaveAnim[]
    {
        new WaveAnim(1f, 0f, new Vector3(1f, 0f, 0f), Vector3.zero, 4f, 0f)
    };

    public bool ifUseWave = false;

    // 시작과 끝으로 얼마나 뾰족하게 모이는지. conStartWidth 값이 0이면 뾰족하고 1이면 평평하고 1 이상이면 오히려 벌어진다.
    public bool ifUseCone = false;
    public float coneStartBias = 0.5f;
    public float coneEndBias = 0.5f;
    public float coneStartAmplify = 0f;
    public float coneEndAmplify = 0f;

    // 웨이브가 모두 같은 time 에 의해 영향받게되면 모두 같은 웨이브가 된다. time에 랜덤값을 적용할지 여부.
    // 여기서 결정된 랜덤값은 스트럭트에 있지 않고 각 이펙트가 private 변수로 들고있게 함
    public bool ifWaveRandomTime = false;

    public bool ifWaveMultiply = false;
    public float waveMultiplierBirth = 1f;
    public float waveMultiplierDeath = 1f;

    public AnimType animType = AnimType.STATIC;
    public float expandAnimSpd = 0f;
    public bool ifFollowWhenStop = true;

    public LookAtCamera lookAtCamera = LookAtCamera.BIRTH;

    public UVType uvType = UVType.FIT;
    public int uvFitRepeat = 1;
    public float uvScale = 1f;
    public Vector2 uvOffsetMinMax = new Vector2(0f, 1f);

    //public Vector2 texScrollSpdMinMax = Vector2.zero;
    //public bool texScrollRandomDir = false;
    public float texScrollSpdSingle = 0f;
    public Vector2 uvOffsetGlobal = Vector2.zero;

    public int texSheetCount = 1;
    public TexSheetBirtyType texSheetBirthType = TexSheetBirtyType.BOTTOM;
    public TexSheetAnimType texSheetAnimType = TexSheetAnimType.SERIAL;
    public float texSheetAnimInterval = 0.1f;

    public LinkedParticleObjs[] particleObjs;
    public bool oneLinkedParticleAtFrame = false; // 한 프레임에 동시에 여러 Streak 가 발생할 경우 딱 하나만 발생시킨다. Burst에서 번개가 여러 줄기로 이루어져있으나 한 점에서 발사되는 경우에 유용함.

    // 플레이모드가 아닌 에디터에서는 Time.time 이나 deltaTime 등을 System.DateTime 으로부터 직접 계산해야한다.
    // editDateTimeStart - 에디터에서 시작하는 순간의 DateTime 을 기억하기 위한 변수
    private DateTime editDateTimeStart;
    // editBeforeFrameTime 은 에디터에서 dateTime 을 이용한 second 값을 사용할 때 바로 전 프레임에서 처리되었던 시간을 기억하기 위한 변수 float Second
    private float editBeforeFrameTime;
    // editTime 은 에디터에서 시작부터 누적된 Time 값. float Second
    private float playTimeStart;

    // 현재 작동중인 이펙트 카운터 (주로 Single UV 스크롤의 Offset 값 세팅을 위해서 갯수를 세어야함)
    public int ltnEffectCount = 0;

    // 재사용 노드
    public SoxLtnRecycle soxLtnRecycle;

    // Position Offset Animation
    public bool ifUsePosOffsetAnim;
    public AnimCurveType posOffsetAnimType = AnimCurveType.LINEAR;
    public Vector3 posOffsetAnimMin;
    public Vector3 posOffsetAnimMax;

#if (UNITY_EDITOR && UNITY_2018_3_OR_NEWER) // 프리팹스테이지는 2018.3 이후에 적용된 기능
    public bool prefabStage = false; // 프리팹스테이지인지 아닌지 플래그. 에디터 스크립트에서 세팅되며 런타임에는 필요없음. true일 경우 soxLtnRecycle 을 프리팹스테이지로 옮긴다.
    public Scene previewScene; // 프리팹스테이지일 경우 soxLtnRecycle 이 옮겨갈 씬을 지정한다. 역시 에디터 스크립트에서 세팅됨.
#endif

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject != transform.gameObject)
        {
            return;
        }

        if (soxLtnNodes == null)
        {
            return;
        }
		
        foreach (SoxLtnNode soxLtnNode in soxLtnNodes)
        {
            // 에디터에서 노드가 강제 삭제된 경우도 있으므로 null 검사
            if (soxLtnNode != null)
            {
                float scaledDotSize = (
                    Mathf.Abs(soxLtnNode.transform.lossyScale.x) +
                    Mathf.Abs(soxLtnNode.transform.lossyScale.y) +
                    Mathf.Abs(soxLtnNode.transform.lossyScale.z)
                    ) / 3f * dotSize;

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(soxLtnNode.transform.position, scaledDotSize);

                Gizmos.color = new Color(1, 1, 1, 0.5f);
                if (soxLtnNode.meIndex != 0)
                {
                    Gizmos.color = new Color(1, 1, 1, 0.5f);
                    Gizmos.DrawLine(soxLtnNodes[soxLtnNode.meIndex - 1].transform.position, soxLtnNode.transform.position);
                }
            }
        }
    }
#endif

    void OnEnable()
    {
        /*
         * OnEnabel에 관한 이슈들
         * 
         * OnEnable 은 다른 오브젝트들의 Awake 보다 먼저 발동한다.
         * Ltn 이펙트가 다른 오브젝트의 자식으로 있는데, 다른 오브젝트가 Awake에서 비활성 처리된다면
         * Ltn이 OnEnable에서 먼저 초기화 되는 문제가 생긴다.
         * 그래서 OnEnable에서 감지한 Enable은 AnyUpdate에서 처리해야한다.
         * 
         * 외부에서 오브젝트를 인스턴싱 한 뒤 firstNode 나 lastNode를 지정할 때 많은 이슈가 있다.
         * OnEnable은 인스턴싱한 순간 발동되어서 firstNode나 lastNode를 인지하지 못한다.
         * 그래서 OnEnable에서 Start 등의 초기화 관련 처리를 하면 안된다.
         * 
         * 또 다른 이슈 (OnEnable에서 Play()를 해줬을 당시)
         * Auto Activate at Birth 속성을 가진 Ltn 이펙트에 외부 스크립트로 '처음 시작할 때 켜져있으면 꺼라' 라는 기능이 함께 작동하는 특수 경우때문에 문제가 되었었음
         * OnEnable --> Play --> OnDisable --> InvokePlay(by Play) 순서로 작동하면서, 나중에 실제로 켜졌을 때 누적된 이펙트 카운트가 뭉텅이로 나오는 문제가 발생함
         */

         ifEnabled = true; // 딜레이가 없으면 ifEnabled만 켜줘서 AnyUpdate때 Enable 처리를 하도록.
    }

    void OnDisable()
    {
        Stop();
    }

    void Awake()
    {
        // 파티클 오브젝트를 외부에서 Start() 등의 상황에서 사용하려면 Ltn은 Awake에서 파티클을 초기화 해줘야함
        // particleObjs 오브젝트 체크하여 현재 soxLtn 노드 계층구조 안에 없으면 생성해 붙인다.
        for (int i = 0; i < particleObjs.Length; i++)
        {
            if (particleObjs[i].particleObj)
            {
                GameObject instancedParticle = GameObject.Instantiate(particleObjs[i].particleObj);
                // 파티클 시스템에 playOnAwake 옵션이 있으면 엉뚱한 곳에서 파티클이 한 번 생기는 문제를 예방하기 위해 파티클 사용시 active 해주도록
                instancedParticle.SetActive(false);
                instancedParticle.transform.parent = transform;
                instancedParticle.name = "Instanced Particle";
                particleObjs[i].particleObj = instancedParticle;

                if (particleObjs[i].attachToNode && soxLtnNodes[i] != null)
                {
                    particleObjs[i].particleObj.transform.parent = soxLtnNodes[i].transform;
                }
            }
        }

        // (이펙트 개별 스크롤 속도 제어를 위해 개별 재질을 사용하려고 만들었으나 봉인)
        //effectRenderers = new List<Renderer>(); // 이건 재활용되어도 초기화되면 안되어서 Awake때 한 번만 한다.

        // 재질 인스턴싱. 런타임 프로퍼티 변경이 원본에 영향을 주지 않도록.
        if (ltnMat != null)
        {
            ltnMatScroll = Instantiate(ltnMat);
            ltnMatBefore = ltnMat;
        }

        // 에디터에서 이펙트를 Play를 해서 Recycle이 존재하던 와중에 실행할 경우 리사이클의 On / Off Effect 카운트가 꼬인다. 일단 지우고 시작.
        // Recycle은 Start() 에서 생성할 예정
        if (Application.isPlaying && soxLtnRecycle != null)
        {
            Destroy(soxLtnRecycle.gameObject);
        }

#if (UNITY_EDITOR && UNITY_2018_3_OR_NEWER) // 프리팹스테이지는 2018.3 이후에 적용된 기능
        prefabStage = false; // 퍼블릭 변수인지라 에디터에서 true로 세팅되기도 하므로 Awake에서 확실히 꺼줌
#endif
    }

    void Start()
    {
        if (ifPlayed)
            return;

        // 씬 내에 자식 이펙트가 있을 수 있는 가능성이 있는데 이런건 싹 다 지워준다.
        KillMyEffectOnStart();

        // AnyStart에서 하면 안되는 것들 (loop 시에 유지되어야 하는 값들)
        ltnEffectCount = 0;

        // 플레이모드 스타트에서만 해줘야하는, SoxLtnNode들에 대해 MeshRenderer 를 삭제한다. (구버전에서 만든 메시가 존재할 수 있음)
        soxLtnNodes = GetSoxLtnNodesInChildren();
        foreach (SoxLtnNode soxLtnNode in soxLtnNodes)
        {
            MeshRenderer mr = soxLtnNode.gameObject.GetComponent<MeshRenderer>();
            if (mr)
            {
                Destroy(mr);
            }

            MeshFilter mf = soxLtnNode.gameObject.GetComponent<MeshFilter>();
            if (mf)
            {
                Destroy(mf);
            }
        }

        // Recycle 생성
        if (Application.isPlaying)
        {
            CreateRecycle();
        }
    }

    private void CreateRecycle()
    {
        if (soxLtnRecycle != null)
            return;

        GameObject recycle = new GameObject(transform.name + "_recycle");
        soxLtnRecycle = recycle.AddComponent<SoxLtnRecycle>();
        soxLtnRecycle.soxLtn = this;
        soxLtnRecycle.onEffects = new List<GameObject>();
        soxLtnRecycle.offEffects = new List<GameObject>();
        soxLtnRecycle.autoTerminateType = autoTerminateType;
        soxLtnRecycle.gameObject.hideFlags = HideFlags.HideInHierarchy; // 원래 HideAndDontSave를 하려고 했으나 HideAndDontSave에서는 Play 종료시 Destroy 관련 알 수 없는 에러가 발생하여 단순히 Hide만 함

#if (UNITY_EDITOR && UNITY_2018_3_OR_NEWER) // 프리팹스테이지는 2018.3 이후에 적용된 기능
        if (prefabStage)
        {
            SceneManager.MoveGameObjectToScene(soxLtnRecycle.gameObject, previewScene);
        }
#endif
    }

    void Update()
    {
        AnyUpdate();
    }

    // AnyStart와 Activate 는 같은 기능
    public void AnyStart()
    {
        ifPlayed = true;

        activate = true;
        activateBefore = false;

        EditorTimeInit();
        playTimeStart = Time.time;

        // 재질 인스턴싱. 런타임 프로퍼티 변경이 원본에 영향을 주지 않도록.
        // Awake에서도 인스턴싱을 해주고 있지만 재활용 등에 의햔 OnEnable 상황에서도 늘 최신 재질을 유지할 수 있도록 새로 인스턴싱한다. (변경 검사 후 인스턴싱)
        if (ltnMat != null)
        {
            // 재질이 변경된 경우에만 새로 인스턴싱
            if (ltnMat != ltnMatBefore)
            {
                ltnMatScroll = Instantiate(ltnMat);
                ltnMatBefore = ltnMat;
            }
        }

        burstIndex = 0;
        burstRandomOffset = UnityEngine.Random.value * burstRandomOffsetMax;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 에디터에서 에러 안나려면 soxLtnNodes 넣어줘야함
            soxLtnNodes = GetSoxLtnNodesInChildren();
        }
#endif

        //(Start)========================================================이 블럭의 순서 중요 시작
        if (firstNode != null)
        {
            soxLtnNodes[0].transform.position = firstNode.position;
            soxLtnNodes[0].attachNode = firstNode;
        }

        if (lastNode != null)
        {
            soxLtnNodes[soxLtnNodes.Length - 1].transform.position = lastNode.position;
            soxLtnNodes[soxLtnNodes.Length - 1].attachNode = lastNode;
        }

        if (shellOffsetStart != 0f &&
            soxLtnNodes[0].attachNode != null)
        {
            soxLtnNodes[0].ShellOffsetNode();
        }

        if (shellOffsetEnd != 0f &&
            soxLtnNodes[soxLtnNodes.Length - 1].attachNode != null)
        {
            soxLtnNodes[soxLtnNodes.Length - 1].ShellOffsetNode();
        }
        //(End)=========================================================이 블럭의 순서 중요 끝

        // Recycle 생성
        if (Application.isPlaying && !soxLtnRecycle)
        {
            CreateRecycle();
        }

        // 바로 위 검사를 거치면 soxLtnRecycle 반드시 존재함
        // soxLtnRecycle 비활성 상태면 활성상태로
        if (Application.isPlaying && !soxLtnRecycle.gameObject.activeSelf)
        {
            soxLtnRecycle.gameObject.SetActive(true);
        }

        SetNodeDistances();
        NodeDistanceSum();
        NodeFirstLookAtCam();
        AnyUpdate(); // 최초 프레임에서 업데이트가 같이 작동하도록 하기 위해
    }

    // AnyUpdate 에서 가장 처음 호출된다는 전제로 작동
    private void CheckEnable()
    {
        if (!ifEnabled || !autoActivateBirth)
            return;

        ifEnabled = false;
        Play();
    }

    // 에디터와 런타임 공용 업데이트 함수
    public void AnyUpdate()
    {
        CheckEnable(); // OnEnable 에서 Start()를 사용하지 못하기때문에 Enable을 감지하여 강제로 AnyStart 해준다.

        // anyTime 은 Time.time용, anyDeltaTime 은 Time.smoothDeltaTime 용
        float anyTime;
        float anyDeltaTime;
        if (Application.isPlaying)
        {
            // 플레이 모드에서
            anyTime = Time.time - playTimeStart;
            anyDeltaTime = Time.smoothDeltaTime;
        }
        else
        {
            // 에디트 모드에서
            anyTime = (float)(DateTime.Now - editDateTimeStart).TotalSeconds;
            anyDeltaTime = (anyTime - editBeforeFrameTime) * unityEditorTimeCorrect;
            editBeforeFrameTime = anyTime;
        }

        // UV 스크롤, Test에 의해 발생하는 라이트닝도 스크롤 적용을 받기 위해서 activate 체크 전에 일다 UV 스크롤부터 한다.
        // 이 방식은 모든 재질이 같은 스크롤값을 가진다. (재질을 인스턴싱 해야 서로 다른 스크롤값을 가지게 되는데 그러면 드로우콜이 증가함)
        if (texScrollSpdSingle != 0 && ltnEffectCount > 0)
        {
            uvOffsetGlobal.x += anyDeltaTime * -texScrollSpdSingle;

            // effectRenderers : 이펙트 개별 스크롤 속도 제어를 위해 개별 재질을 사용하려고 만들었으나 봉인
            /*
            for (int i = 0; i < effectRenderers.Count; i++)
            {
                if (effectRenderers[i].gameObject.activeInHierarchy)
                {
                    float offsetBefore = effectRenderers[i].sharedMaterial.GetTextureOffset("_MainTex").x;

                    // newOffset에서 1로 나눈 나머지를 쓰는건 offset 값이 지나치게 커지는 것을 피하기위함인데
                    // offset Before를 빼주고 다시 더해줘야 스크롤 속도가 점점 빨라지지 않음
                    float newOffset = (uvOffsetGlobal.x - offsetBefore) % 1f + offsetBefore;
                    effectRenderers[i].sharedMaterial.SetTextureOffset("_MainTex", new Vector2(newOffset, 0f));
                }
            }
            */

            // 이펙트 각자는 버택스가 UV 좌표를 랜덤하게 들고있음.
            uvOffsetGlobal.x %= 1f;
            ltnMatScroll.SetTextureOffset("_MainTex", uvOffsetGlobal);
        }

        if (!activate)
        {
            activateBefore = activate;
            return;
        }

        // 여기까지 오면 activate 가 true임
        if (!activateBefore)
        {   // 이전 프레임에서 activateBefore 가 false 였다면 이번 프레임에서 처음으로 activate 진입했다는..
            // Activate 토글을 누르는 순간의 최초 진입시 해야할 처리들을 이곳에서 해준다
            genedCount = 0;
            editDateTimeStart = DateTime.Now;
            playTimeStart = Time.time;
            if (bursts != null)
            {
                burstIndex = 0;
                accumInterval = burstRandomOffset;
            }

            activateBefore = activate;
        }

        int nowSpawnCount = 0;
        // Generation Rate에 의한 라이트닝 계산
        if (genRate != 0f)
        {
            // play 시작순간부터 현재 시간까지 전부 몇 개 스폰되었어야하는지 총 량 계산
            int totalGenCount = Mathf.FloorToInt(anyTime / (1f / genRate)) + 1;
            // 총 스폰되었어야 할 카운트에서 그동안 스폰된 카운트를 빼면 현재 스폰해야할 카운트가 계산됨
            nowSpawnCount = totalGenCount - genedCount;
            if (nowSpawnCount != 0)
            {
                genedCount = totalGenCount;
            }
        }

        // Burst에 의한 라이트닝 계산
        if (bursts != null)
        {
            // accumInterval 값은 최초 진입시 burstRandomOffset이 더해져서 들어온다.
            if (anyTime >= accumInterval)
            {
                for (int i = burstIndex; i < bursts.Length; i++)
                {
                    float tempAccumInterval = accumInterval + bursts[i].interval;
                    if (anyTime >= tempAccumInterval)
                    {
                        // Burst 배열의 Interval 값을 더해봐도 현재 시간이 더크다면 Burst 해줘야 할 타이밍이라는 뜻
                        accumInterval += bursts[i].interval;
                        nowSpawnCount += bursts[i].streakCount;
                        burstIndex = i + 1;
                    }
                    else
                    {
                        // Burst 배열의 Interval 값을 더해봤더니 현재 시간이 더 작다면 for 루프를 break 해줘야함
                        break;
                    }
                }
            }
        }

        // 실제 라이트닝 생성
        for (int i = 0; i < nowSpawnCount; i++)
        {
            if (i == 0)
            {
                // 최초 파티클은 ps.Play로
                GenStreak(true);
            }
            else
            {
                // 나머지 파티클은 ps.Emit()로
                GenStreak(false);
            }
        }

        if (anyTime > genDuration)
        {   // 제한시간을 넘어가면
            if (!loop)
            {   // 루프가 아니라면 일단 비활성화 하고서 Play모드에서는 시한부 사망 선고
                // UV 스크롤 애니메이션을 SoxLtn에서 하기때문에 SoxLtn을 바로 삭제하지 않고 계속 비교하다가 Recycle과 함께 제거된다.
                Deactivate();
				
				// D프로젝트 브랜치 시작
                // Recycle 의 DeathCheck() 에서 soxLtn을 제거한다.
				// D프로젝트 브랜치 끝
            }
            else
            {   // 루프면
                AnyStart();
            }
        }
    }//end of AnyUpdate()

    public void EditorTimeInit()
    {
        editDateTimeStart = DateTime.Now;
        editBeforeFrameTime = 0f;
    }

    private void NodeDistanceSum()
    {
        nodeDistanceAll = 0f;
        foreach (SoxLtnNode tempLtnNode in soxLtnNodes)
        {
            nodeDistanceAll += tempLtnNode.distance;
        }
    }

    void OnDestroy()
    {
        if (soxLtnRecycle != null)
        {
            if (soxLtnRecycle.onEffects.Count == 0)
            {
                GameObject.Destroy(soxLtnRecycle.gameObject);
                foreach (SoxLtnNode soxLtnNode in soxLtnNodes)
                {
                    if (soxLtnNode != null)
                    {
                        GameObject.Destroy(soxLtnNode.gameObject);
                    }
                }
            }
        }
    }

    // AnyStart와 Activate 는 같은 기능
    public void Activate()
    {
        AnyStart();
    }

    public void Deactivate()
    {
        activate = false;
        activateBefore = false;
        ifPlayed = false;

        // ifFollowWhenStop기능을 위해 Deactivate 상황에서 이펙트 각자들에게 애니메이션 타입을 STATIC 혹은 DYNAMIC 으로 세팅한다.
        // 이를 위해서 이펙트의 재활용 혹은 최초 생성시 soxLtn의 STATIC / DYNAMIC 여부를 얻어와야함
        // ifFollowWhenStop이 켜져있는 것이 기본 작동이고 꺼져있는 경우 예외처리임
        if (ifFollowWhenStop == false &&
            soxLtnRecycle != null)
        {
            foreach (GameObject onEffect in soxLtnRecycle.onEffects)
            {
                onEffect.GetComponent<SoxLtnEffect>().animType = AnimType.STATIC;
            }
        }
    }

    // 오버로딩, withChildren 매개변수가 없으면 디폴트로 true, 즉 자식노드 모두 Activate 해줘야함
    public void Play()
    {
        ifPlayWithChildren = true; // Play 함수의 withChildren 디폴트는 true임
        if (playDelay > 0f)
        {
            Invoke("InvokePlay", playDelay);
        }
        else
        {
            InvokePlay();
        }
    }

    public void InvokePlay()
    {
        // GetComponentsInChildren<SoxLtn> 은 자기 자신의 SoxLtn 도 포함해서 리턴.
        SoxLtn[] childrenSoxLtns = GetComponentsInChildren<SoxLtn>();
        foreach (SoxLtn tempSoxLtn in childrenSoxLtns)
        {
            tempSoxLtn.Activate();
        }
    }

    // 오버로딩
    public void Play(bool withChildren)
    {
        ifPlayWithChildren = withChildren;
        if (playDelay > 0f)
        {
            Invoke("InvokePlayWithChildren", playDelay);
        }
        else
        {
            InvokePlayWithChildren();
        }
    }

    public void InvokePlayWithChildren()
    {
        if (!ifPlayWithChildren)
        {
            Activate();
            return;
        }

        // GetComponentsInChildren<SoxLtn> 은 자기 자신의 SoxLtn 도 포함해서 리턴듯.
        SoxLtn[] childrenSoxLtns = GetComponentsInChildren<SoxLtn>();
        foreach(SoxLtn tempSoxLtn in childrenSoxLtns)
        {
            tempSoxLtn.Activate();
        }
    }

    public void Stop()
    {
        // GetComponentsInChildren<SoxLtn> 은 자기 자신의 SoxLtn 도 포함해서 리턴.
        SoxLtn[] childrenSoxLtns = GetComponentsInChildren<SoxLtn>();
        foreach (SoxLtn tempSoxLtn in childrenSoxLtns)
        {
            tempSoxLtn.Deactivate();
        }
    }

    public void Stop(bool withChildren)
    {
        if (!withChildren)
        {
            Deactivate();
            return;
        }

        // GetComponentsInChildren<SoxLtn> 은 자기 자신의 SoxLtn 도 포함해서 리턴.
        SoxLtn[] childrenSoxLtns = GetComponentsInChildren<SoxLtn>();
        foreach (SoxLtn tempSoxLtn in childrenSoxLtns)
        {
            tempSoxLtn.Deactivate();
        }
    }



    public SoxLtnEffect GenStreak(bool firstStreakAtFrame) // firstAtFrame 은, 동일 프레임 내에서 여러 라이트닝이 동시에 생성될 수 있는데 그 중 첫 번째 라이트닝인지. 파티클의 Play 혹은 Emit 구분에 필요하다.
    {
        NodeDistanceSum();
        if (ltnEffectCount <= 0)
            uvOffsetGlobal = Vector2.zero;

        if (soxLtnRecycle == null)
            CreateRecycle();

        GameObject newEffect;

        if (soxLtnRecycle.offEffects != null &&
            soxLtnRecycle.offEffects.Count > 0)
        {   // 재사용 가능한 것이 있으면 재사용
            newEffect = soxLtnRecycle.offEffects[0];
            soxLtnRecycle.offEffects.RemoveAt(0);
            soxLtnRecycle.onEffects.Add(newEffect);
            newEffect.SetActive(true);
            newEffect.layer = gameObject.layer; // 재활용이 복잡해지는 상황에서는 레이어 소속이 수시로 바뀔 수 있다.
        }
        else
        {   // 재사용 가능한 것이 없으면 새로 생성
            newEffect = new GameObject();
            newEffect.name = "SoxLtnEffect";
            newEffect.layer = gameObject.layer;
            newEffect.tag = gameObject.tag;
            soxLtnRecycle.onEffects.Add(newEffect);
            newEffect.transform.parent = soxLtnRecycle.transform;
        }

        SoxLtnEffect soxLtnEffect = newEffect.GetComponent<SoxLtnEffect>();
        if (!soxLtnEffect)
        {   // SoxLtnEffect 신규 생성시
            soxLtnEffect = newEffect.AddComponent<SoxLtnEffect>();
            soxLtnEffect.soxLtn = GetComponent<SoxLtn>();
            soxLtnEffect.soxLtnNodes = soxLtnNodes;
            soxLtnEffect.subStep = subStep;
            soxLtnEffect.recalculateNormals = recalculateNormals;
            soxLtnEffect.recalculateTangents = recalculateTangents;
            soxLtnEffect.uvFitRepeat = uvFitRepeat;
            soxLtnEffect.uvScale = uvScale;
            soxLtnEffect.texSheetCount = texSheetCount;
            soxLtnEffect.texSheetAnimInterval = texSheetAnimInterval;
            soxLtnEffect.animType = animType;
            soxLtnEffect.uvType = uvType;
            soxLtnEffect.soxLtnRecycle = soxLtnRecycle;
        }

        soxLtnEffect.firstStreakAtFrame = firstStreakAtFrame;

        ltnEffectCount++;

        soxLtnEffect.AnyStart();
        if (!Application.isPlaying)
        {
            soxLtnEffect.InitLocalVars();
        }

        return soxLtnEffect;
    }

    public static float EaseIn(float start, float end, float delta)
    {
        return Mathf.Lerp(start, end, delta * delta);
    }

    public static Vector3 EaseInV3(Vector3 start, Vector3 end, float delta)
    {
        return Vector3.Lerp(start, end, delta * delta);
    }

    public static float EaseOut(float start, float end, float delta)
    {
        return Mathf.Lerp(start, end, (delta - (delta * delta)) + delta);
    }

    public static Vector3 EaseOutV3(Vector3 start, Vector3 end, float delta)
    {
        return Vector3.Lerp(start, end, (delta - (delta * delta)) + delta);
    }

    //  soxLtn 의 자식들 중에서 자기 자신과 관련있는 soxLtnNode 를 골라내어서 배열로 리턴함
    public SoxLtnNode[] GetSoxLtnNodesInChildren()
    {
        SoxLtnNode[] tempNodes = this.transform.GetComponentsInChildren<SoxLtnNode>();
        List<SoxLtnNode> returnNodes = new List<SoxLtnNode>();
        foreach (SoxLtnNode tempNode in tempNodes)
        {
            if (tempNode.soxLtn == this)
            {
                returnNodes.Add(tempNode);
            }
        }
        return returnNodes.ToArray();
    }

    // 에디터에서 작업하다가 Play()를 누를 경우 씬에 이펙트가 남아있는 채로 Play가 되는데 이러면 문제가 발생하므로 Start()에서 모두 찾아서 지워준다.
    private void KillMyEffectOnStart()
    {
        SoxLtnEffect[] soxLtnEffects = FindObjectsOfType<SoxLtnEffect>();
        foreach (SoxLtnEffect soxLtnEffect in soxLtnEffects)
        {
            if (soxLtnEffect.soxLtn == this)
            {
                GameObject.DestroyImmediate(soxLtnEffect.gameObject);
            }
        }
    }

    // 노드간 거리 세팅, SoxLtn의 비활성/활성 재활용시 노드들 거리 초기화에 사용됨
    private void SetNodeDistances()
    {
        if (soxLtnNodes == null)
            return;

        if (soxLtnNodes.Length < 2)
            return;

        soxLtnNodes[0].distance = 0f;
        Vector3 beforePos = soxLtnNodes[0].transform.position;
        for (int i = 1; i < soxLtnNodes.Length; i++)
        {
            Vector3 nowPos = soxLtnNodes[i].transform.position;
            soxLtnNodes[i].distance = (nowPos - beforePos).magnitude;
            beforePos = nowPos;
        }
	}

    // 노드들의 최초 LookAt (AnyStart 에서 사용됨)
    private void NodeFirstLookAtCam()
    {
        if (soxLtnNodes == null)
            return;

        for (int i = 0; i < soxLtnNodes.Length; i++)
        {
            soxLtnNodes[i].NodeCameraLookAtAndDistance();
            soxLtnNodes[i].firstLookAt = false;
        }
    }

	
	// D 프로젝트 브랜치
} // end of SoxLtn