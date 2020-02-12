using UnityEngine;
using System;
using System.Collections;

public class SoxLtnEffect : MonoBehaviour {

    // soxLtn 이 삭제된 상황에서도 자기 자신의 수명을 다하고 죽도록 하기 위해 대부분의 이펙트 변수들을 private 변수로 기억해둔다
    // soxLtn 에서 랜덤 범위로 지정된 것들은 최종 랜덤값 하나만 기억한다.

    private bool ifActivate = false;
    private bool ifRecycleEnabled = false;

    public SoxLtn soxLtn;
    public SoxLtnNode[] soxLtnNodes;
    private Matrix4x4[] nodeMatrixs;
    private Matrix4x4 meMatrix;
    private Matrix4x4 meMatrixInverse;
    private float nodeDistanceAll;
    public int subStep;
    private Vector3[] subStepNoises;
    private float subStepNoiseAmp;
    private SoxLtn.ConstantRelated subStepNoiseAmpType;

    public bool recalculateNormals;
    public bool recalculateTangents;

    // soxLtn이 지워지더라도 soxLtn의 위치를 기억해두는 벡터
    private Vector3 soxLtnPosBackup;

    public SoxLtn.AnimType animType;
    public SoxLtn.UVType uvType;

    //Twist 배열 (노드 수만큼)
    public float[] zTwists;

    public int uvFitRepeat;
    public float uvScale;
    public float uvOffset;
    public int texSheetCount;

    // Texture UV Sheet 변수
    private SoxLtn.TexSheetBirtyType texSheetBirthType;
    private SoxLtn.TexSheetAnimType texSheetAnimType;
    public float texSheetAnimInterval;
    private int texSheetNow = 0;    // 현재 어느 텍스쳐 쉬트를 가리키고 있는지 기억하는 변수. 최초 생성시에는 0이고 1base 로 증가한다. 예를 들어 4개 쉬트면 1~4 로 증가
    private float texSheetBeforeTime;

    public float streakWidth;
    private float streakWidthHalf;

    // 이하 시간 계산을 위한 변수들
    private float anyTime;
    private float anyDeltaTime;
    private DateTime birthDateTime;
    private float birthTime;
    private float appearTime;
    private Color appearColor;
    private SoxLtn.AnimCurveType appearColorType;
    private SoxLtn.AnimCurveType appearThinType;
    private float lifeTime;
    // lifeDelta - 수명이 얼마나 지났는지 알려주는 0~1 값.
    private float lifeDelta;
    private Color mainColor;
    private float fadeTime;
    private float fadeDelta = 0f;
    // allLifeTime 은 appearTime, lifeTime, fadeTime 을 미리 계산해둔다. 최적화 목적
    private float allLifeTime;
    // fadeTimeRest 는 appearTime + lifeTime 연산을 미리 해두는 변수
    private float fadeTimeRest;
    private Color fadeOutColor;
    private SoxLtn.AnimCurveType fadeOutColorType;
    private SoxLtn.AnimCurveType fadeOutThinType;

    private float editBeforeFrameTime;

    private enum FadeState
    { FADEIN, MAIN, FADEOUT }

    private FadeState fadeState = FadeState.FADEIN;
    // CONSTANT 스테이트에서 아무 액션도 안하려면 스테이트가 변하는 순간 딱 한번 세팅해줄 필요가 있다. 그러려면 바로 이전 스테이트를 기억해서 비교해줘야한다.
    private FadeState fadeStateBefore = FadeState.FADEOUT;

    // 지오메트리 변수들
    private MeshFilter mf;
    private Mesh mesh;
    private MeshRenderer mr;
    private Vector3[] vertices;
    private Color[] vColors;
    private int[] tris;
    private Vector3[] normals;
    private Vector2[] uvs;
    private float[] dists;

    private Vector3[] nodeVerts;

    private Vector3 halfWidth = Vector3.zero;

    // Expand 확장되는 랜덤 방향벡터. 초기 위치 선정에도 사용됨. 
    private Vector3 expDirCircle;
    private Vector3 expDirSphere;
    private float expDist;  // 0 ~ 1 사이 값에서부터 확장되는 위치 값. 누적된다.
    private float expDistSurf; // 표면에서부터 확장되는 위치 값, 누적된다.
    private float expandAnimSpd;

    // Linked Particle
    // 모든 라이트닝 이펙트들이 파티클 하나를 사용하기때문에 지속적으로 파티클이 따라가는 방식은 불가능하고 태어날 때 한 번 터뜨려주는 것만 가능함
    private bool ifPlayParticle;
    // firstStreakAtFrame - 하나의 프레임에 동시에 여러 개의 링크드 파티클이 플레이 될 경우 여려 개중 마지막에 처리되는 이펙트 하나만 싱글 플레이여야한다. 싱글플레이는 ps.Play(), 아닌 것은 ps.Emit()
    // firstStreakAtFrame 변수는 외부에서(soxLtn) 이펙트 생성 혹은 재활용시 세팅된다.
    public bool firstStreakAtFrame; // firstStreakAtFrame
    private bool ifDisableParticle;

    // Wave 관련 변수들
    private struct WaveAnimEffect
    {
        // 주파수, 웨이브가 얼마나 빽빽하게 발생하는지
        public float frequency;
        // 웨이브가 공간상의 어떤 방향으로 발생하는지 Vector3
        public Vector3 amplify;
        // 스크롤 속도, 마이너스 속도도 가능함
        public float scrollSpeed;
    }
    private WaveAnimEffect[] effectWaves;
    private bool ifUseWave;
    // 웨이브가 모두 같은 time 에 의해 영향받게되면 모두 같은 웨이브가 된다. time에 랜덤값을 적용할지 여부.
    // 여기서 결정된 랜덤값은 스트럭트에 있지 않고 각 이펙트가 private 변수로 들고있게 함
    private bool ifUseCone;
    private float coneStartBias;
    private float coneEndBias;
    private float coneStartAmplify;
    private float coneEndAmplify;

    private float waveRandomTime;
    // 시작과 끝으로 얼마나 뾰족하게 모이는지. conStartWidth 값이 0이면 뾰족하고 1이면 평평하고 1 이상이면 오히려 벌어진다.

    private bool ifWaveMultiply;
    private float waveMultiplierBirth;
    private float waveMultiplierDeath;

    private Transform meTrans;

    private bool isRecycle = false;
    public SoxLtnRecycle soxLtnRecycle;

    private float scaleVar;

    // Position Offste Anim을 사용하는지
    private bool ifUsePosOffsetAnim;
    private SoxLtn.AnimCurveType posOffsetAnimType;
    // posOffsetAnim 이동 목표지점. 변수 초기화 함수에서 soxLtn 방향으로 정렬된다.
    private Vector3 posOffsetAnim;

    // Use this for initialization
    void Start()
    {
        AnyStart();
    }

    public void AnyStart()
    {
        // SoxLtn에서 LtnEffect를 생성만 하고 방치하면 다음 프레임에서 이펙트가 생겨난다.
        // Gen 되자마자 해당 프레임에 이펙트가 등장하려면 생성과 동시에 AnyStart()를 호출해야하는데
        // 그러면 최초 한 번은 AnyStart()가 두 번 불리는 부작용이 있어서 ifActivate 를 검사해서 한 번만 작동하게 한다
        if (ifActivate)
            return;

        meTrans = transform;

        soxLtnPosBackup = soxLtn.transform.position;

        // 내 위치를 soxLtn과 정렬시키는건 InitLocalVars()보다 먼저 선행되어야한다.
        // InitLocalVars() 안에서 Position Offset Animation 을 위해서 이펙트 오브젝트의 방향으로 회전된 벡터를 세팅하기때문
        meTrans.position = soxLtn.transform.position;
        meTrans.rotation = soxLtn.transform.rotation;

        InitLocalVars();

        ifPlayParticle = false;
        ifDisableParticle = true;

        int substepCount = (soxLtn.nodeCount - 1) * subStep;

        // 서브스텝 노이즈는 재활용때라도 리셋해야함
        subStepNoiseAmp = soxLtn.subStepNoiseAmp;
        subStepNoiseAmpType = soxLtn.subStepNoiseAmpType;

        // 재활용시 스타트 해야할 것들은 이것까지 하고 중단
        if (isRecycle)
        {   // 재활용이라면?
            // UV 세팅
            SetUV();    // 가로방향 세팅
            texSheetNow = 0;
            SetTexSheet();  // 세로방향 세팅
            if (Application.isPlaying)
            {
                texSheetBeforeTime = Time.time;
            }
            else
            {
                texSheetBeforeTime = (float)(DateTime.Now - birthDateTime).TotalSeconds;
            }

            // 매모리 재생성을 막기 위해서 재활용에서는 new 없이 기존 배열 내용 업데이트
            for (int i = 0; i < substepCount; i++)
            {
                subStepNoises[i] = UnityEngine.Random.insideUnitSphere;
            }

            ColorStreakSimple(appearColor);
            SetVertsPos();
            InitFadeState();

            return;
        }
        // 이하 최초 스타트에서 해야할 것들

        // 서브스텝 노이즈를 위해 벡터 배열 생성
        subStepNoises = new Vector3[substepCount];
        for (int i = 0; i < substepCount; i++)
        {
            subStepNoises[i] = UnityEngine.Random.insideUnitSphere;
        }

        nodeVerts = new Vector3[soxLtnNodes.Length];

        GenMesh();

        // UV 세팅
        SetUV();    // 가로방향 세팅
        texSheetNow = 0;
        SetTexSheet();  // 세로방향 세팅
        if (Application.isPlaying)
        {
            texSheetBeforeTime = Time.time;
        }
        else
        {
            texSheetBeforeTime = (float)(DateTime.Now - birthDateTime).TotalSeconds;
        }

        nodeMatrixs = new Matrix4x4[soxLtnNodes.Length];

        SetVertsPos();
        InitFadeState();
        ColorStreakSimple(appearColor);

        // 일단 start 한 번 거치고 난 뒤에는 재활용이다
        isRecycle = true;
        ifActivate = true;
    }

    // 재활용에 의해 Activate 상태가 바뀌면 새로운 변수들과 새로운 지오메트리가 적용되기도 전에 화면에 잠깐 그전 쓰레기 데이터가 보이는 문제가 있어서
    // Enable 이벤트에서 처리해줘야하는데, 최초 오브젝트 생성시 enable 에서도 OnEnable 이 작동되는 문제가 있음
    // 그래서 ifRecycleEnabled 변수로 재활용 Enable 인지를 체크
    void OnEnable()
    {
        if (ifRecycleEnabled)
        {
            if (Application.isPlaying)
            {
                // 플레이 모드에서
                anyTime = Time.time;
                anyDeltaTime = Time.smoothDeltaTime;
            }
            InitLocalVars();
            InitFadeState();
            // AnyStart 는 SoxLtn 에서 재활용 활성화 하면서 호출
        }
        else
        {
            ifRecycleEnabled = true;
        }
    }

    void Update()
    {
        AnyUpdate();
    }

    public void AnyUpdate()
    {
        if (allLifeTime <= 0f)
        {
            KillMe();
            return;
        }

        // (주로 에디터 환경에서) 최초 한 번은 AnyStart 보다 AnyUpdate가 먼저 호출되기때문에 AnyStart 에서 세팅되는 nodeVerts 변수가 null 인지를 검사해서 AnyStart 후 그냥 리턴함
        if (nodeVerts == null)
        {
            AnyStart();
            return;
        }

        // anyTime 은 Time.time용, anyDeltaTime 은 Time.smoothDeltaTime 용
        if (Application.isPlaying)
        {
            // 플레이 모드에서
            anyTime = Time.time;
            anyDeltaTime = Time.smoothDeltaTime;
        }
        else
        {
            // 에디트 모드에서
            anyTime = (float)(DateTime.Now - birthDateTime).TotalSeconds;
            anyDeltaTime = (anyTime - editBeforeFrameTime) * soxLtn.unityEditorTimeCorrect;
            editBeforeFrameTime = anyTime;
        }
        // 에디터와 Play 공용 타임 세팅 끝

        float timeFromBirth = anyTime - birthTime;
        lifeDelta = timeFromBirth / allLifeTime;

        fadeState = FadeState.MAIN;

        // Fadein 처리
        if (appearTime != 0 && timeFromBirth < appearTime)
        {
            // fadeStateBefore 는 SetVertsPos() 에서 처리, FadeIn 에서는 fadeStateBefore 없이도 CONSTANT 상태를 유지할 수 있다.
            fadeState = FadeState.FADEIN;
            FadeIn(timeFromBirth);
        }

        // Fadeout 처리
        if (fadeTime != 0 && timeFromBirth > fadeTimeRest)
        {
            // fadeStateBefore 는 FadeOut 의 경우 처리할 필요 없다. 그냥 유지하면 되므로
            fadeState = FadeState.FADEOUT;
            FadeOut(timeFromBirth);
        }

        // MAIN 처리
        if (fadeState == FadeState.MAIN)
        {
            // 컬러 세팅은 딱 한번만 하기 위함
            if (fadeState != fadeStateBefore)
            {
                ColorStreakSimple(mainColor);
                fadeStateBefore = fadeState;
            }
        }

        // Position Offset Animation
        if (ifUsePosOffsetAnim)
        {
            // 원점에서 posOffsetAnim으로 보간하는건 Lerp 대신에 lifeDelta를 곱하는 것으로 대체
            meTrans.position = GetPosOffsetAnimPos() + soxLtnPosBackup;
        }

        // 다이나믹 타입이면 SetVertsPos() 함수를 호출한다.
        if (animType == SoxLtn.AnimType.DYNAMIC)
        {
            SetVertsPos();
        }

        // 텍스쳐 쉬트 애니메이션 세팅
        if ((anyTime - texSheetBeforeTime) > texSheetAnimInterval && texSheetAnimInterval != 0 && texSheetCount > 1)
        {
            SetTexSheet();
            texSheetBeforeTime = anyTime;
        }

        // 수명이 다하면 오브젝트 삭제. 오브젝트 삭제는 맨 마지막에 해야 에러를 피한다.
        // birthTime 은 Play 모드에서는 특정 float 값을 가지고있을 수 있으나 Edit 모드에서는 무조건 0이다.
        if (timeFromBirth > allLifeTime)
        {
            KillMe();
        }
    }

    // Position Offset Animation을 위해 lifeDelta, scaleVar, 보간 애니메이션 타입을 고려한 벡터를 리턴
    private Vector3 GetPosOffsetAnimPos()
    {
        Vector3 returnVec = Vector3.zero;
        switch (posOffsetAnimType)
        {
            case SoxLtn.AnimCurveType.EASEOUT:
                returnVec = posOffsetAnim * SoxLtn.EaseOut(0f, 1f, lifeDelta) * scaleVar;
                break;
            case SoxLtn.AnimCurveType.EASEIN:
                returnVec = posOffsetAnim * SoxLtn.EaseIn(0f, 1f, lifeDelta) * scaleVar;
                break;
            default:
                //CONSTANT, LINEAR
                returnVec = posOffsetAnim * lifeDelta * scaleVar;
                break;
        }

        return returnVec;
    }

    private void FadeIn(float timeFromBirth)
    {
        // fadeDelta 변수 - 페이드아웃이 몇 % 진행되었는지 델타값, SetVertsPos()에서도 사용하기때문에 private 처리.
        fadeDelta = timeFromBirth / appearTime;

        //Color newColor = Color.Lerp(Color.white, soxLtn.fadeOutColor, delta);
        switch (appearColorType)
        {
            case SoxLtn.AnimCurveType.CONSTANT:
                break;
            case SoxLtn.AnimCurveType.LINEAR:
                ColorStreak(appearColor, mainColor, fadeDelta);
                break;
            case SoxLtn.AnimCurveType.EASEIN:
                ColorStreak(appearColor, mainColor, SoxLtn.EaseIn(0f, 1f, fadeDelta));
                break;
            case SoxLtn.AnimCurveType.EASEOUT:
                ColorStreak(appearColor, mainColor, SoxLtn.EaseOut(0f, 1f, fadeDelta));
                break;
            default:
                break;
        }
    }

    private void FadeOut(float timeFromBirth)
    {
        // fadeDelta 변수 - 페이드아웃이 몇 % 진행되었는지 델타값, SetVertsPos()에서도 사용하기때문에 private 처리.
        fadeDelta = 1f - (timeFromBirth - fadeTimeRest) / fadeTime;

        // 다이나믹 타입은 SetVertsPos() 에서 폭을 조절한다.
        if (animType == SoxLtn.AnimType.STATIC)
        {
            switch (fadeOutThinType)
            {
                case SoxLtn.AnimCurveType.CONSTANT:
                    break;
                case SoxLtn.AnimCurveType.LINEAR:
                    ThinStreak(fadeDelta);
                    break;
                case SoxLtn.AnimCurveType.EASEIN:
                    ThinStreak(SoxLtn.EaseIn(0f, 1f, fadeDelta));
                    break;
                case SoxLtn.AnimCurveType.EASEOUT:
                    ThinStreak(SoxLtn.EaseOut(0f, 1f, fadeDelta));
                    break;
                default:
                    break;
            }
        }

        switch (fadeOutColorType)
        {
            case SoxLtn.AnimCurveType.CONSTANT:
                if (fadeState != fadeStateBefore)
                {
                    // CONSTANT 라도 한 번은 세팅해준다.
                    ColorStreak(fadeOutColor, mainColor, fadeDelta);
                    fadeStateBefore = fadeState;
                }
                break;
            case SoxLtn.AnimCurveType.LINEAR:
                ColorStreak(fadeOutColor, mainColor, fadeDelta);
                break;
            case SoxLtn.AnimCurveType.EASEIN:
                ColorStreak(fadeOutColor, mainColor, SoxLtn.EaseOut(0f, 1f, fadeDelta));
                break;
            case SoxLtn.AnimCurveType.EASEOUT:
                ColorStreak(fadeOutColor, mainColor, SoxLtn.EaseIn(0f, 1f, fadeDelta));
                break;
            default:
                break;
        }
    }

    private void KillMe()
    {
        if (soxLtnRecycle == null) // 이펙트가 있다면 Recycle도 당연히 있어야하므로 발생할 일이 없으나 안전을 위해 로그 처리 후 자기 삭제
        {
            Debug.Log("Something went wrong with SoxLtn's Recycle.");
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        if (Application.isPlaying)
        {
            soxLtnRecycle.onEffects.Remove(gameObject);
            soxLtnRecycle.offEffects.Add(gameObject);
            gameObject.SetActive(false);
            soxLtnRecycle.DeathCheck();
        }
        else
        {
            soxLtnRecycle.onEffects.Remove(gameObject);
            UnityEngine.Object.DestroyImmediate(gameObject);
            soxLtnRecycle.DeathCheck();
        }

        if (soxLtn)
        {
            soxLtn.ltnEffectCount--;
        }

        ifActivate = false;
    }

    // Start() 혹은 OnEnable 등에서 SetVertsPos() 를 어쩔 수 없이 해야하는데, 이 때 SetVertsPos() 직후 FadeState를 다시 초기화해줘야함
    private void InitFadeState()
    {
        fadeState = FadeState.FADEIN;
        // fadeStateBefore 는 최초 상태가 fadeState와 다른 값으로 초기화되어야한다.
        fadeStateBefore = FadeState.FADEOUT;
    }

    public void InitLocalVars()
    {
        SetEffectMaterial();

        birthDateTime = DateTime.Now;
        editBeforeFrameTime = 0f;
        if (Application.isPlaying)
        {
            anyTime = Time.time;
            anyDeltaTime = Time.smoothDeltaTime;
            birthTime = Time.time;
        }
        else
        {
            anyTime = (float)(DateTime.Now - birthDateTime).TotalSeconds;
            anyDeltaTime = (anyTime - editBeforeFrameTime) * soxLtn.unityEditorTimeCorrect;
            birthTime = 0f;
        }

        uvOffset = UnityEngine.Random.Range(soxLtn.uvOffsetMinMax.x, soxLtn.uvOffsetMinMax.y);
        appearTime = UnityEngine.Random.Range(soxLtn.appearMinMax.x, soxLtn.appearMinMax.y);
        appearColor = soxLtn.appearColor;
        lifeTime = UnityEngine.Random.Range(soxLtn.lifeMinMax.x, soxLtn.lifeMinMax.y);
        mainColor = soxLtn.mainColor;
        fadeTime = UnityEngine.Random.Range(soxLtn.fadeMinMax.x, soxLtn.fadeMinMax.y);
        allLifeTime = appearTime + lifeTime + fadeTime;
        fadeTimeRest = appearTime + lifeTime;
        fadeOutColor = soxLtn.fadeOutColor;

        float timeFromBirth = anyTime - birthTime;
        lifeDelta = timeFromBirth / allLifeTime;
        // 최초 초기화때에는 allLifeTime 이 0으로 들어오는 경우가 있어서 lifeDelta 에 NaN 혹은 무한대가 될 때가 있음
        if (allLifeTime == 0f)
        {
            lifeDelta = 0f;
        }

        nodeDistanceAll = soxLtn.nodeDistanceAll;

        fadeDelta = 0f;

        zTwists = new float[soxLtnNodes.Length];
        //zTwists 노드별 Z트위스트값 지정
        for (int i = 0; i < soxLtnNodes.Length; i++)
        {
            zTwists[i] = UnityEngine.Random.Range(soxLtnNodes[i].zTwistMin, soxLtnNodes[i].zTwistMax);
        }

        texSheetBirthType = soxLtn.texSheetBirthType;
        texSheetAnimType = soxLtn.texSheetAnimType;

        appearColorType = soxLtn.appearColorType;

        animType = soxLtn.animType;
        // animType이 STATIC 인 경우 Fadein thin 기능이 작동하지 않기때문에 강제로 appearThinType 을 CONSTANT 로 해줘야한다. 안그러면 비활성화된 LINEAR 등 유저가 지정해둔 타입으로 작동하려고 하면서 문제가 생긴다.
        if (animType == SoxLtn.AnimType.STATIC)
        {
            appearThinType = SoxLtn.AnimCurveType.CONSTANT;
        }
        else
        {
            appearThinType = soxLtn.appearThinType;
        }
        fadeOutColorType = soxLtn.fadeOutColorType;
        fadeOutThinType = soxLtn.fadeOutThinType;

        streakWidth = UnityEngine.Random.Range(soxLtn.streakWidthMinMax.x, soxLtn.streakWidthMinMax.y);
        streakWidthHalf = streakWidth * 0.5f;

        Vector2 tVec = UnityEngine.Random.insideUnitCircle;
        tVec = tVec.normalized;
        expDirCircle = new Vector3(tVec.x, tVec.y, 0);
        // expDirSphere 은 expDirCircle 과 뒤섞여서 사용될 수 있으므로 expDirCircle 의 방향을 기반으로 한 랜덤 백터여야 확장 애니메이션시 엉뚱한 방향으로 안간다.
        expDirSphere = new Vector3(tVec.x, tVec.y, UnityEngine.Random.Range(-1f, 1f));
        expDirSphere = expDirSphere.normalized;
        expDist = UnityEngine.Random.value; // 일단 0 ~ 1 사이의 값으로 랜덤 초기화한 뒤에 실제 사용할 때 반경 변수와 연계한다.
        expDistSurf = 1f; // 일단 1인 표면에서 시작
        expandAnimSpd = soxLtn.expandAnimSpd;

        if (soxLtn.waves != null)
        {
            effectWaves = new WaveAnimEffect[soxLtn.waves.Length];

            float tempRandom;
            for (int i = 0; i < effectWaves.Length; i++)
            {
                tempRandom = UnityEngine.Random.value - 0.5f;
                effectWaves[i].frequency = soxLtn.waves[i].frequency + soxLtn.waves[i].frequencyRandom * tempRandom;
                tempRandom = UnityEngine.Random.value - 0.5f;
                effectWaves[i].amplify.x = soxLtn.waves[i].amplify.x + (soxLtn.waves[i].amplifyRandom.x * tempRandom);
                tempRandom = UnityEngine.Random.value - 0.5f;
                effectWaves[i].amplify.y = soxLtn.waves[i].amplify.y + (soxLtn.waves[i].amplifyRandom.y * tempRandom);
                tempRandom = UnityEngine.Random.value - 0.5f;
                effectWaves[i].amplify.z = soxLtn.waves[i].amplify.z + (soxLtn.waves[i].amplifyRandom.z * tempRandom);
                tempRandom = UnityEngine.Random.value - 0.5f;
                effectWaves[i].scrollSpeed = soxLtn.waves[i].scrollSpeed + soxLtn.waves[i].scrollSpeedRandom * tempRandom;
            }
        }

        ifUseCone = soxLtn.ifUseCone;
        // 엣지 총합이 둘밖에 없는 상황에서는 ifUseCone을 꺼야함
        if ((soxLtnNodes.Length - 1) * (subStep + 1) + 1 <= 2)
        {
            ifUseCone = false;
        }

        ifUseWave = soxLtn.ifUseWave;

        coneStartBias = soxLtn.coneStartBias;
        coneEndBias = soxLtn.coneEndBias;
        coneStartAmplify = soxLtn.coneStartAmplify;
        coneEndAmplify = soxLtn.coneEndAmplify;

        if (soxLtn.ifWaveRandomTime)
        {
            // Sine 결과값에 랜덤을 주려면 PI 만큼의 랜덤값이 필요하다.
            waveRandomTime = (UnityEngine.Random.value - 0.5f) * Mathf.PI;
        }
        else
        {
            waveRandomTime = 0f;
        }

        ifWaveMultiply = soxLtn.ifWaveMultiply;
        waveMultiplierBirth = soxLtn.waveMultiplierBirth;
        waveMultiplierDeath = soxLtn.waveMultiplierDeath;

        ifUsePosOffsetAnim = soxLtn.ifUsePosOffsetAnim;
        if (ifUsePosOffsetAnim)
        {
            posOffsetAnimType = soxLtn.posOffsetAnimType;
            posOffsetAnim = Vector3.zero;
            posOffsetAnim.x = UnityEngine.Random.Range(soxLtn.posOffsetAnimMin.x, soxLtn.posOffsetAnimMax.x);
            posOffsetAnim.y = UnityEngine.Random.Range(soxLtn.posOffsetAnimMin.y, soxLtn.posOffsetAnimMax.y);
            posOffsetAnim.z = UnityEngine.Random.Range(soxLtn.posOffsetAnimMin.z, soxLtn.posOffsetAnimMax.z);

            // vec 벡터를 soxLtn 로컬 방향으로 회전시켜야한다.
            // TRS 매트릭스 변수 준비
            Matrix4x4 tempRotMatrix = Matrix4x4.identity;
            // 매트릭스에 soxLtn의 로컬 방향 반영
            tempRotMatrix.SetTRS(Vector3.zero, soxLtn.transform.rotation, Vector3.one);

            // soxLtn 로컬 방향으로 posOffsetAnim 벡터를 회전
            posOffsetAnim = tempRotMatrix.MultiplyPoint3x4(posOffsetAnim);
        }
    }

    private void SetEffectMaterial()
    {
        if (mr == null)
        {
            return;
        }

        if (soxLtn.ltnMatScroll)
        {   // 지정된 Material이 있을 경우
            mr.sharedMaterial = soxLtn.ltnMatScroll;
        }
        else
        {   // 지정된 Material이 없을 경우 그냥 디퓨즈 재질을 지정한다.
            mr.sharedMaterial = new Material(Shader.Find("Diffuse"));
        }
        
        #if UNITY_EDITOR
        if (!Application.isPlaying && SoxLtnEditorSettings.ltnMatScrollEditor != null)
            mr.sharedMaterial = SoxLtnEditorSettings.ltnMatScrollEditor; // 에디터 프리뷰용 인스턴싱 재질은 에디터용 스태틱 변수를 사용
        #endif
    }

    private void GenMesh ()
    {
        mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = gameObject.AddComponent<MeshFilter>();
        }

        mesh = new Mesh();
        mesh.MarkDynamic();
        mf.mesh = mesh;

        mr = GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }

        SetEffectMaterial();

        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mr.sortingLayerID = soxLtn.sortinglayer;
        mr.sortingOrder = soxLtn.orderInLayer;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

        int vertCount = 2 + (2 * (soxLtnNodes.Length - 1) * (subStep + 1));
        vertices = new Vector3[vertCount];
        // 버텍스 위치는 나중에 정하더라도 일단 배열 값을 넣어준다.
        mesh.vertices = vertices;

        vColors = new Color[vertCount];
        for (int i = 0; i < vColors.Length; i++)
        {
            vColors[i] = Color.white;
        }
        mesh.colors = vColors;

        int triCount = (soxLtnNodes.Length - 1) * (subStep + 1) * 6;
        tris = new int[triCount];
        for (int i = 0; i < (vertCount - 2); i = i + 2)
        {
            int triStart = i * 3;
            tris[triStart] = i;
            tris[triStart + 1] = i + 2;
            tris[triStart + 2] = i + 1;

            tris[triStart + 3] = i + 1;
            tris[triStart + 4] = i + 2;
            tris[triStart + 5] = i + 3;
        }

        mesh.triangles = tris;

        normals = new Vector3[vertCount];
        for (int i = 0; i < vertCount; i++)
        {
            normals[i] = -Vector3.forward;
        }

        mesh.normals = normals;
    }

    // 버택스 위치 선정, 애니메이션 등이 모두 이 함수에서 수행된다.
    // 버택스 위치가 달라질 때 UV도 연동되려면 UV 값도 이곳에서 처리되어야 한다. 그러나 퍼포먼스를 위해 UV 연동은 생략
    public void SetVertsPos()
    {
        // soxLtn이 삭제되면 SetVertsPos() 작동을 멈춘다.
        // soxLtn이 삭제되어도 이펙트가 살아있도록 하려면 노드 트랜스폼에 대한 매트릭스 연산방식으로 로직을 변경해야하는데 현재는 노드 트랜스폼으로부터 직접 포인트 연산을 하는 방식이라서 이펙트가 살아있는 처리는 나중에...
        if (!soxLtn || !soxLtnNodes[0])
        {
            return;
        }
        // SetVertsPos() 함수는 DYNAMIC 타입에서 계속 불리기때문에 distance all을 계속 참조해야한다.
        nodeDistanceAll = soxLtn.nodeDistanceAll;

        // 스케일 애니메이션이 되는 경우도 있으므로 실시간으로 스케일값을 업데이트
        scaleVar = (
            Mathf.Abs(soxLtn.transform.lossyScale.x) +
            Mathf.Abs(soxLtn.transform.lossyScale.y) +
            Mathf.Abs(soxLtn.transform.lossyScale.z)
            ) / 3f;

        if (scaleVar == 0f)
        {
            // scaleVar 로 나누는 경우가 종종 있어여 0이 되는 상황을 강제로 막아준다.
            scaleVar = 1f;
        }

        // Position Offset Animation 을 사용할 경우 임시로 Node 위치를 이동시켜둔다. (soxLtn을 옮기면 자식도 한방에 옮길 수 있지만 여러 계층구조가 얽힌 라이트닝도 고려해야해서 node들을 하나씩 옮김)
        // for 루프가 끝날 때 다시 원위치 해줘야한다.
        Vector3 tempPosOffsetAnim = Vector3.one;
        if (ifUsePosOffsetAnim)
        {
            tempPosOffsetAnim = GetPosOffsetAnimPos();
            for (int i = 0; i < soxLtnNodes.Length; i++)
            {
                soxLtnNodes[i].transform.position += tempPosOffsetAnim;
            }
        }

        Matrix4x4 tempZRotMatrix = Matrix4x4.identity;
        float nodeDistanceSum = 0f;
        Vector3 waveVector;

        meMatrix = meTrans.localToWorldMatrix;
        meMatrixInverse = meMatrix.inverse;

        // 노드 매트릭스를 미리 계산해둔다. (for 문 안에서 nodeMatrixs[i], nodeMatrixs[i -1] 등으로 쓰일 일이 있음)
        for (int i = 0; i < soxLtnNodes.Length; i++)
        {
            nodeMatrixs[i] = soxLtnNodes[i].transform.localToWorldMatrix;
        }

        for (int i = 0; i < soxLtnNodes.Length; i++)
        {   // i 카운트는 노드를 의미함
            // 노드 위치 버택스들 먼저 처리

            nodeVerts[i] = new Vector3(0, 0, 0);

            // Expand용 tDirVec 방향 벡터 세팅
            Vector3 tDirVec;
            if (soxLtnNodes[i].nodeType == SoxLtnNode.NodeType.CIRCLE)
            {
                tDirVec = expDirCircle;
            }
            else
            {
                tDirVec = expDirSphere;
            }

            // zTwists 만큼 회전 매트릭스 세팅 (tempRotMatrix) (zTwists 는 각 노드마다 비틀림 값을 지정할 수 있는 기능)
            tempZRotMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0, 0, zTwists[i]), Vector3.one);

            tDirVec = tempZRotMatrix.MultiplyPoint3x4(tDirVec);

            // circle 혹은 shpere 방향으로 확장 벡터를 더해준다.
            if (soxLtnNodes[i].fillType == SoxLtnNode.FillType.INSIDE)
            {   // INSIDE
                nodeVerts[i] += tDirVec * expDist * soxLtnNodes[i].circleArea;
                // 확장 거리 변수에 시간 변화에 따른 누적 값 반영
                expDist += expandAnimSpd * anyDeltaTime;
            }
            else
            {   //SURFACE
                nodeVerts[i] += tDirVec * expDistSurf * soxLtnNodes[i].circleArea;
                // 확장 거리 변수에 시간 변화에 따른 누적 값 반영
                expDistSurf += expandAnimSpd * anyDeltaTime;
            }

            // halfWidth 이펙트 두께 설정 (두께의 절반)
            switch (fadeState)
            {
                case FadeState.FADEIN:
                    switch (appearThinType)
                    {
                        case SoxLtn.AnimCurveType.CONSTANT:
                            if (fadeState != fadeStateBefore)
                            {
                                halfWidth = new Vector3(-streakWidthHalf, 0, 0);
                                fadeStateBefore = fadeState;
                            }
                            break;
                        case SoxLtn.AnimCurveType.LINEAR:
                            halfWidth = new Vector3(-streakWidthHalf * fadeDelta, 0, 0);
                            break;
                        case SoxLtn.AnimCurveType.EASEIN:
                            halfWidth = new Vector3(-streakWidthHalf * SoxLtn.EaseIn(0f, 1f, fadeDelta), 0, 0);
                            break;
                        case SoxLtn.AnimCurveType.EASEOUT:
                            halfWidth = new Vector3(-streakWidthHalf * SoxLtn.EaseOut(0f, 1f, fadeDelta), 0, 0);
                            break;
                        default:
                            break;
                    }
                    break;
                case FadeState.MAIN:
                    halfWidth = new Vector3(-streakWidthHalf, 0, 0);
                    break;
                case FadeState.FADEOUT:
                    switch (fadeOutThinType)
                    {
                        case SoxLtn.AnimCurveType.CONSTANT:
                            halfWidth = new Vector3(-streakWidthHalf, 0, 0);
                            break;
                        case SoxLtn.AnimCurveType.LINEAR:
                            halfWidth = new Vector3(-streakWidthHalf * fadeDelta, 0, 0);
                            break;
                        case SoxLtn.AnimCurveType.EASEIN:
                            halfWidth = new Vector3(-streakWidthHalf * SoxLtn.EaseOut(0f, 1f, fadeDelta), 0, 0);
                            break;
                        case SoxLtn.AnimCurveType.EASEOUT:
                            halfWidth = new Vector3(-streakWidthHalf * SoxLtn.EaseIn(0f, 1f, fadeDelta), 0, 0);
                            break;
                        default:
                            break;
                    }
                    break;
            }

            // 현재 노드까지의 누적 길이 합산
            nodeDistanceSum += soxLtnNodes[i].distance;

            // Cone 두께 halfWidthCone 설정
            float tempCone = 1f;
            Vector3 halfWidthCone;
            if (ifUseCone)
            {
                tempCone = GetCone(nodeDistanceSum);
                halfWidthCone = halfWidth * tempCone;
            }
            else
            {
                halfWidthCone = halfWidth;
            }

            int nodeVertIndex = (subStep + 1) * (i * 2);
            Vector3 finalWorldPos1;
            Vector3 finalWorldPos2;
            if (ifUseWave && soxLtn.waves != null)
            {
                // Wave를 사용하는 경우
                // waveVector는 노드의 로컬방향이 반영되도록 하기 위해서 노드 트랜스폼을 기준으로 적용한다. (이후 서브 버택스의 웨이브는 매트릭스와 함께 연산)
                waveVector = GetWave(nodeDistanceSum) * tempCone;
                finalWorldPos1 = nodeMatrixs[i].MultiplyPoint3x4(nodeVerts[i] + halfWidthCone + waveVector);
                finalWorldPos2 = nodeMatrixs[i].MultiplyPoint3x4(nodeVerts[i] - halfWidthCone + waveVector);
            }
            else
            {   // Wave를 사용하지 않는 경우
                finalWorldPos1 = nodeMatrixs[i].MultiplyPoint3x4(nodeVerts[i] + halfWidthCone);
                finalWorldPos2 = nodeMatrixs[i].MultiplyPoint3x4(nodeVerts[i] - halfWidthCone);
            }
            vertices[nodeVertIndex] = meMatrixInverse.MultiplyPoint3x4(finalWorldPos1); //노드의 로컬을 월드로 변환한 뒤 월드를 이펙트의 로컬로 다시 변환
            vertices[nodeVertIndex + 1] = meMatrixInverse.MultiplyPoint3x4(finalWorldPos2); //노드의 로컬을 월드로 변환한 뒤 월드를 이펙트의 로컬로 다시 변환

            // PlayLinkedParticles()을 SetVertsPos()에서 해주는 이유는, 노드를 범위로 지정할 경우 라이트닝이 생겨날 위치 계산이 복잡해서 버택스 처리하면서 같이 처리해주기 위함
            if (Application.isPlaying && !ifPlayParticle)
            {
                // ifPlayParticle에 의해 딱 한번만 플레이 한다.
                PlayLinkedParticles(meMatrix.MultiplyPoint3x4((vertices[nodeVertIndex] + vertices[nodeVertIndex + 1]) * 0.5f), i);
            }

            // 여기부터 서브스텝 버택스들 처리
            if (i != 0)
            {   // 첫 번째 노드는 서브스텝이 없어서 i가 0일 때에는 처리하지 않는다
                // 서브스텝 버택스들 처리

                float tempNoiseType = 1f;
                if (subStepNoiseAmpType == SoxLtn.ConstantRelated.DISTANCE_RELATED)
                {
                    tempNoiseType = soxLtnNodes[i].distance;
                }

                for (int j = 1; j <= subStep; j++)
                {   // j 카운트는 서브스텝 버택스 순서를 의미함
                    // bias 변수는 두 노드 사이에 서브스텝에 의해 간격이 어느 위치에 있어야하는지 비율 값이다. 주로 Lerp 의 세 번째 매개변수로 사용
                    float bias = ((1f / (float)(subStep + 1)) * j);
                    // biasForWave? bias 값은 부드러운 서브스텝을 위해 중간에 변형되므로 wave의 distance 기준이 될 수 있는 순수한 bias 값이 필요함
                    float biasForWave = bias;

                    // shiftStart와 shiftEnd 값은 노드 로컬 방향을 기준으로 z 방향으로 이동한 서브스텝 포인트의 위치. 이 쉬프트 덕분에 substep이 부드러운 곡선을 그린다.
                    Vector3 shiftStart = new Vector3(0, 0, soxLtnNodes[i].distance * bias);
                    Vector3 shiftEnd = new Vector3(0, 0, soxLtnNodes[i].distance * (1f - bias) * -1f);

                    // lossy 스케일 반영 (마이너스 스케일이 있을 수 있음) shiftStart와 shiftEnd는 z값만 존재
                    // distance 는 distance 값인데, 스케일이 커지면 역으로 작아져야한다. 그 이유는, TransformPoint (매트릭스) 연산은 동일한 입력 값에 대해서 로컬 스케일이 커지면 더 큰 값을 도출하기때문에 distance는 작아져야 같은 형상을 유지한다.
                    shiftStart.z /= soxLtnNodes[i].transform.lossyScale.z;
                    shiftEnd.z /= soxLtnNodes[i].transform.lossyScale.z;

                    // !! 순서 중요
                    // 부드러운 곡선을 위해 bias에 약간의 특수 처리. shiftStart와 shiftEnd 값을 먼저 세팅한 뒤에 bias에 변화를 주는 순서가 중요함
                    if (i == 1 || i == (soxLtnNodes.Length - 1))
                    {   // 서브스텝이 시작이거나 끝 노드쪽일 경우
                        if (i == 1) bias = SoxLtn.EaseOut(0f, 1f, bias);
                        if (i == (soxLtnNodes.Length - 1)) bias = SoxLtn.EaseIn(0f, 1f, bias);
                    }
                    else
                    {   // 서브스텝이 중간 노드쪽일 경우
                        if (bias < 0.5f) { bias -= ((0.5f - bias) * (bias / 0.5f)); } else { bias += ((bias - 0.5f) * (1f - bias) / 0.5f); }
                    }

                    int substepIndex = (subStep + 1) * (i - 1) * 2 + (j * 2);
                    int substepNoiseIndex = (i - 1) * subStep + j - 1;

                    // shiftLerp 위치는 두께 halfWidth를 고려하지 않은 서브스텝 위치
                    Vector3 shiftLerp = Vector3.Lerp
                        (       // 라이트닝 노드 기준으로 로컬 포지션을 월드 포지션으로 변환한 뒤 Lerp
                                nodeMatrixs[i - 1].MultiplyPoint3x4(nodeVerts[i - 1] + shiftStart),
                                nodeMatrixs[i].MultiplyPoint3x4(nodeVerts[i] + shiftEnd),
                                bias
                        );
                    Vector3 noise = subStepNoises[substepNoiseIndex] * subStepNoiseAmp * tempNoiseType * scaleVar;
                    // 서브스텝 위치의 zTwist 로테이션 매트릭스 (두께를 적용하려면 두께 벡터를 노드 방향으로 이동시켜야함, 노드는 카메라 룩앳)
                    tempZRotMatrix.SetTRS(Vector3.zero, Quaternion.Slerp(soxLtnNodes[i - 1].transform.rotation, soxLtnNodes[i].transform.rotation, bias), Vector3.one);

                    float subDistanceSum = 0f;
                    if (ifUseWave || ifUseCone)
                    {
                        // 서브스텝 버택스의 distance
                        subDistanceSum = nodeDistanceSum + soxLtnNodes[i].distance * biasForWave - soxLtnNodes[i].distance;
                    }

                    if (ifUseCone)
                    {
                        tempCone = GetCone(subDistanceSum);
                        halfWidthCone = halfWidth * tempCone;
                    }
                    else
                    {
                        halfWidthCone = halfWidth;
                    }

                    // lossy 스케일 반영, 마이너스 스케일이 있을 수 있음
                    halfWidthCone.x *= soxLtnNodes[i].transform.lossyScale.x;

                    // 월드 포지션을 다시 meTrans 의 로컬 포지션으로 변환
                    if (ifUseWave && soxLtn.waves != null)
                    {
                        // waveVector는 노드의 로컬방향이 반영되도록 하기 위해서 매트릭스 연산 안에 넣어준다.
                        waveVector = GetWave(subDistanceSum) * tempCone;
                        vertices[substepIndex] = meMatrixInverse.MultiplyPoint3x4(shiftLerp + tempZRotMatrix.MultiplyPoint3x4(halfWidthCone + waveVector)) + noise;
                        vertices[substepIndex + 1] = meMatrixInverse.MultiplyPoint3x4(shiftLerp + tempZRotMatrix.MultiplyPoint3x4(halfWidthCone * -1f + waveVector)) + noise;
                    }
                    else
                    {
                        vertices[substepIndex] = meMatrixInverse.MultiplyPoint3x4(shiftLerp + tempZRotMatrix.MultiplyPoint3x4(halfWidthCone)) + noise;
                        vertices[substepIndex + 1] = meMatrixInverse.MultiplyPoint3x4(shiftLerp + tempZRotMatrix.MultiplyPoint3x4(halfWidthCone * -1f)) + noise;
                    }
                } // for j end
            } // if end
        } // for i end
        ifPlayParticle = true; // for문 밖에서 처리해야함.

        // Position Offset Animation 을 사용할 경우 임시로 Node 위치를 이동시켜두었던 것을
        // for 루프가 끝났으니 때 다시 원위치.
        if (ifUsePosOffsetAnim)
        {
            for (int i = 0; i < soxLtnNodes.Length; i++)
            {
                soxLtnNodes[i].transform.position -= tempPosOffsetAnim;
            }
        }

        mesh.vertices = vertices;

        mesh.RecalculateBounds();
        if (recalculateNormals)
            mesh.RecalculateNormals();
        if (recalculateTangents)
            mesh.RecalculateTangents();

    } // end of SetVertsPos()

    private float GetCone(float distance)
    {
        float returnValue = 1f;

        // bias 전체 노드 길이중에 distance가 현재 어느 위치에 있는가, 0~1
        float bias = distance / nodeDistanceAll;
        float biasInverse = 1f - bias;

        // distance 가 Start cone 에 속해있으면
        if (bias < coneStartBias)
        {
            // returnValue 는 0~1
            returnValue = bias / coneStartBias;

            // returnValue 는 coneStartAmplify ~ 1
            returnValue = Mathf.Lerp(coneStartAmplify, 1f, returnValue);
            return returnValue;
        }

        // distance 가 End cone 에 속해있으면
        if (biasInverse < coneEndBias)
        {
            // returnValue 는 0~1
            returnValue = biasInverse / coneEndBias;

            // returnValue 는 coneEndAmplify ~ 1
            returnValue = Mathf.Lerp(coneEndAmplify, 1f, returnValue);
            return returnValue;
        }

        return returnValue;
    }

    private Vector3 GetWave(float distance)
    {
        // scaleVar 는 frequency 에만 영향을 주고 amplify 는 SetVertsPos() 에서 곱해준다. 노드 버택스와 서브스텝 버택스의 스케일 연산이 다소 복잡하게 얽혀서 이렇게 했음

        Vector3 returnVector = Vector3.zero;
        float wave;
        for (int i = 0; i < effectWaves.Length; i++)
        {
            wave = Mathf.Sin(distance * effectWaves[i].frequency / scaleVar + waveRandomTime + (anyTime * -effectWaves[i].scrollSpeed));
            returnVector += effectWaves[i].amplify * wave;
        }

        if (ifWaveMultiply && animType == SoxLtn.AnimType.DYNAMIC)
        {
            returnVector *= (waveMultiplierDeath - waveMultiplierBirth) * lifeDelta + waveMultiplierBirth;
        }

        return returnVector;
    }

    // 다이나믹 타입이 아니더라도 SetVertsPos() 함수 구동 없이 vertices[], streakWidthHalf, streakWidth 변수들만으로 버택스 폭을 좁힌다.
    private void ThinStreak(float delta)
    {
        //streakWidthHalf, streakWidth 둘다 절반 값인 점을 주의
        for (int i = 0; i < vertices.Length; i += 2)
        {
            float dist = Vector3.Distance(vertices[i], vertices[i + 1]) * 0.5f; // dist 역시 절반 값으로 계산
            float targetDist = streakWidth * 0.5f * delta;
            float targetDelta = 1f - (targetDist / dist);
            Vector3 backupPos1 = vertices[i];
            Vector3 backupPos2 = vertices[i + 1];
            vertices[i] = Vector3.Lerp(backupPos1, backupPos2, targetDelta * 0.5f);
            vertices[i + 1] = Vector3.Lerp(backupPos1, backupPos2, 1f - (targetDelta * 0.5f));
        }

        mesh.vertices = vertices;

        // 바운딩박스와 노말은 굳이 재계산 안해도 될듯
    }

    // 투명도는 Particle-Additive 쉐이더와의 호환성을 위해 0.5 가 기본이다.
    // 셰이더 코드에서 _TintColor 가 사용되어야한다.
    private void ColorStreak(Color fromColor, Color toColor, float delta)
    {
        //Color newColor = Color.Lerp(Color.white, soxLtn.fadeOutColor, delta);
        Color newColor = Color.Lerp(fromColor, toColor, delta);

        for (int i = 0; i < vColors.Length; i++)
        {
            vColors[i] = newColor;
        }

        mesh.colors = vColors;
    }

    private void ColorStreakSimple(Color color)
    {
        for (int i = 0; i < vColors.Length; i++)
        {
            vColors[i] = color;
        }

        mesh.colors = vColors;
    }

    // 현재 상황에 맞게 전체 버택스 UV를 재설정한다.
    public void SetUV()
    {
        float totalDist = 0f;

        if (!isRecycle)
        {
            dists = new float[soxLtnNodes.Length];
        }

        for (int i = 0; i < soxLtnNodes.Length; i++)
        {
            dists[i] = soxLtnNodes[i].distance;
            totalDist += soxLtnNodes[i].distance;
        }

        if (!isRecycle)
        {
            uvs = new Vector2[vertices.Length];
        }

        // 첫 칸은 for 루프 밖에서 처리한다.
        uvs[0] = new Vector2(0 + uvOffset, 1);
        uvs[1] = new Vector2(0 + uvOffset, 0);

        int pt = 2;
        float ptUV = 0f + uvOffset;  // 계산 과정에서 순차적으로 증가하는 UV값
        float stepUV = 0f; // 서브 한 단계의 UV 폭
        for (int nodeCount = 1; nodeCount < soxLtnNodes.Length; nodeCount++)
        {
            if (uvType == SoxLtn.UVType.FIT)
            {   // FIT
                stepUV = dists[nodeCount] / totalDist / (subStep + 1) * uvFitRepeat;
            }
            else
            {   // SCALE
                stepUV = dists[nodeCount] / uvScale / (subStep + 1);
            }
            for (int subCount = 1; subCount <= subStep; subCount++)
            {
                ptUV += stepUV;
                uvs[pt] = new Vector2(ptUV, 1);
                uvs[pt + 1] = new Vector2(ptUV, 0);
                pt += 2;//uv 처리용 pt 인덱스 증가
            }
            // 어쨋든 노드 uv는 무조건 입력해야한다.
            ptUV += stepUV;
            uvs[pt] = new Vector2(ptUV, 1);
            uvs[pt + 1] = new Vector2(ptUV, 0);
            pt += 2;//uv 처리용 pt 인덱스 증가
        }

        mesh.uv = uvs;
    }

    public void SetTexSheet()
    {
        float vScale = 1f / texSheetCount;
        float vPos = 0f;
        if (texSheetNow == 0)
        {   // 첫 진입
            if (texSheetBirthType == SoxLtn.TexSheetBirtyType.BOTTOM || texSheetCount == 1)
            {
                vPos = 0f;
                texSheetNow = 1;
            }
            else
            {   // RANDOM
                texSheetNow = (int)(UnityEngine.Random.value * (texSheetCount - 1));
                vPos = texSheetNow * vScale;
            }
        }
        else
        {   // 첫 진입이 아님. Anim type 만 고려하면 됨
            if (texSheetAnimType == SoxLtn.TexSheetAnimType.SERIAL)
            {   //SERIAL
                if (texSheetNow != texSheetCount)
                {
                    texSheetNow++;
                }
                else
                {
                    texSheetNow = 1;
                }
            }
            else
            {   // RANDOM
                int tRandom = (int)(UnityEngine.Random.value * (texSheetCount - 1)) + 1;
                if (tRandom == texSheetNow) tRandom++;
                if (tRandom > texSheetCount) tRandom = 1;
                texSheetNow = tRandom;
            }
            vPos = (texSheetNow - 1) * vScale;
        }

        float vPosPlus = vPos + vScale;
        for (int i = 0; i < uvs.Length; i += 2)
        {
            uvs[i].y = vPosPlus;
            uvs[i + 1].y = vPos;
        }

        mesh.uv = uvs;
    }

    // Linked Particle Objects 들을 노드 위치에 정렬하여 Play 해준다.
    private void PlayLinkedParticles(Vector3 particlePos, int nodeIndex)
    {
        if (soxLtn.particleObjs == null) return;
        if (soxLtn.particleObjs.Length == 0) return;

        // Linked particle의 개수가 노드 개수보다 작을 수 있어서 파티클 배열 개수보다 높은 인덱스가 들어오면 그냥 리턴
        if (nodeIndex > (soxLtn.particleObjs.Length - 1)) return;

        if (soxLtn.particleObjs[nodeIndex].particleObj == null) return; // 중간노드 등에서 링크 파티클이 없을 경우 재생 없이 그냥 리턴

        if (ifDisableParticle)
        {
            for (int i = 0; i < soxLtn.particleObjs.Length; i++)
            {
                if (soxLtn.particleObjs[i].particleObj)
                {
                    soxLtn.particleObjs[i].particleObj.SetActive(true);
                }
            }
            ifDisableParticle = false;
        }

        //플레이 위치로 이동
        soxLtn.particleObjs[nodeIndex].particleObj.transform.position = particlePos;
        //조건부 회전
        if (soxLtn.particleObjs[nodeIndex].rotationAlign)
        {
            soxLtn.particleObjs[nodeIndex].particleObj.transform.rotation = soxLtnNodes[nodeIndex].transform.rotation;
        }
        soxLtn.particleObjs[nodeIndex].particleObj.transform.localScale = Vector3.one;

        if (firstStreakAtFrame)
        {
            // 현재 프레임에서 첫 파티클 플레이인 경우
            ParticleSystem[] tParticles = soxLtn.particleObjs[nodeIndex].particleObj.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem tParticle in tParticles)
            {
                tParticle.Play();
            }
        }
        else
        {
            if (soxLtn.oneLinkedParticleAtFrame == false)
            {
                // 현재 프레임에서 첫 Streak이 아닌 나머지 것들은 Emit()으로 처리해야함
                // 몇 개가 될지 모르는 파티클들에게 Emit 신호 뿌려주기
                ParticleSystem[] tParticles = soxLtn.particleObjs[nodeIndex].particleObj.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in tParticles)
                {
                    int emitCount = 0;
                    ParticleSystem.EmissionModule e = ps.emission;

                    if (e.burstCount > 0)
                    {
                        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[e.burstCount];

                        e.GetBursts(bursts);
                        emitCount = (int)bursts[0].maxCount;
                    }
                    ps.Emit(emitCount);
                }
            }
        }
        
    } // end of PlayLinkedParticles()
}
