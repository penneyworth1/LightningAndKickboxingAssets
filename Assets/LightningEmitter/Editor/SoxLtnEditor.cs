using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(SoxLtn))]
public class SoxLtnEditor : Editor
{
    // GUILayout.BeginHorizontal()을 사용하면 Indent가 제대로 안되어서 강제로 Space를 적용하는데 이때 indent 값을 상수로 정의
    private const float editorIndent = 15f;

    private Material matBefore; // 에디터의 재질 변경이 UnDo 등의 상황에서 제대로 하단의 재질 에디터에 반영되지 않으므로 매 인스펙터 업데이트마다 이전과 비교해서 강제로 내용을 반영한다.

    // 에디터 fold용 변수
    private bool SoxLtnFoldInteractionPerformance = true;
    private bool SoxLtnFoldRenderer = true;
    private bool SoxLtnFoldPlayControl = true;
    private bool SoxLtnFoldLife = false;
    private bool SoxLtnFoldColorThickness = false;
    private bool SoxLtnFoldGeometry = true;
    private bool SoxLtnFoldUV = false;
    private bool SoxLtnFoldConeShape = false;
    private bool SoxLtnFoldWave = false;
    private bool SoxLtnFoldPosOffsetAnim = false;

    private MaterialEditor matEditor;

    private SerializedProperty firstNode_sp;
    private SerializedProperty lastNode_sp;
    private SerializedProperty dotSize_sp;
    private SerializedProperty animType_sp;
    private SerializedProperty ifFollowWhenStop_sp;
    private SerializedProperty lookAtCamera_sp;
    private SerializedProperty ifUseCameraTag_sp;
    private SerializedProperty cameraTag_sp;
    private SerializedProperty shellOffsetStart_sp;
    private SerializedProperty shellOffsetEnd_sp;
    private SerializedProperty recalculateNormals_sp;
    private SerializedProperty recalculateTangents_sp;
    private SerializedProperty ltnMat_sp;
    private SerializedProperty sortinglayer_sp;
    private SerializedProperty orderInLayer_sp;
    private SerializedProperty autoActivateBirth_sp;
    private SerializedProperty loop_sp;
    private SerializedProperty autoTerminate_sp;
    private SerializedProperty autoTerminateType_sp;
    private SerializedProperty genDuration_sp;
    private SerializedProperty genRate_sp;
    private SerializedProperty playDelay_sp;
    private SerializedProperty burstRandomOffsetMax_sp;
    private SerializedProperty bursts_sp;
    private SerializedProperty appearMinMax_sp;
    private SerializedProperty lifeMinMax_sp;
    private SerializedProperty fadeMinMax_sp;
    private SerializedProperty appearColor_sp;
    private SerializedProperty appearColorType_sp;
    private SerializedProperty appearThinType_sp;
    private SerializedProperty mainColor_sp;
    private SerializedProperty fadeOutColor_sp;
    private SerializedProperty fadeOutColorType_sp;
    private SerializedProperty fadeOutThinType_sp;
    private SerializedProperty streakWidthMinMax_sp;
    private SerializedProperty nodeCount_sp;
    private SerializedProperty subStep_sp;
    private SerializedProperty subStepNoiseAmp_sp;
    private SerializedProperty subStepNoiseAmpType_sp;
    private SerializedProperty uvType_sp;
    private SerializedProperty uvFitRepeat_sp;
    private SerializedProperty uvScale_sp;
    private SerializedProperty uvOffsetMinMax_sp;
    private SerializedProperty texScrollSpdSingle_sp;
    private SerializedProperty texSheetCount_sp;
    private SerializedProperty texSheetBirthType_sp;
    private SerializedProperty texSheetAnimType_sp;
    private SerializedProperty texSheetAnimInterval_sp;
    private SerializedProperty expandAnimSpd_sp;
    private SerializedProperty particlesObjs_sp;
    private SerializedProperty oneLinkedParticleAtFrame_sp;
    private SerializedProperty ifUseCone_sp;
    private SerializedProperty coneStartBias_sp;
    private SerializedProperty coneEndBias_sp;
    private SerializedProperty coneStartAmplify_sp;
    private SerializedProperty coneEndAmplify_sp;
    private SerializedProperty ifUseWave_sp;
    private SerializedProperty ifWaveRandomTime_sp;
    private SerializedProperty ifWaveMultiply_sp;
    private SerializedProperty waveMultiplierBirth_sp;
    private SerializedProperty waveMultiplierDeath_sp;
    private SerializedProperty waves_sp;
    private SerializedProperty ifUsePosOffsetAnim_sp;
    private SerializedProperty posOffsetAnimType_sp;
    private SerializedProperty posOffsetAnimMin_sp;
    private SerializedProperty posOffsetAnimMax_sp;

    private void GetEditorPrefs()
    {
        if (EditorPrefs.HasKey("SoxLtnFoldInteractionPerformance"))
            SoxLtnFoldInteractionPerformance = EditorPrefs.GetBool("SoxLtnFoldInteractionPerformance");

        if (EditorPrefs.HasKey("SoxLtnFoldRenderer"))
            SoxLtnFoldRenderer = EditorPrefs.GetBool("SoxLtnFoldRenderer");

        if (EditorPrefs.HasKey("SoxLtnFoldPlayControl"))
            SoxLtnFoldPlayControl = EditorPrefs.GetBool("SoxLtnFoldPlayControl");

        if (EditorPrefs.HasKey("SoxLtnFoldLife"))
            SoxLtnFoldLife = EditorPrefs.GetBool("SoxLtnFoldLife");

        if (EditorPrefs.HasKey("SoxLtnFoldColorThickness"))
            SoxLtnFoldColorThickness = EditorPrefs.GetBool("SoxLtnFoldColorThickness");

        if (EditorPrefs.HasKey("SoxLtnFoldGeometry"))
            SoxLtnFoldGeometry = EditorPrefs.GetBool("SoxLtnFoldGeometry");

        if (EditorPrefs.HasKey("SoxLtnFoldUV"))
            SoxLtnFoldUV = EditorPrefs.GetBool("SoxLtnFoldUV");

        if (EditorPrefs.HasKey("SoxLtnFoldConeShape"))
            SoxLtnFoldConeShape = EditorPrefs.GetBool("SoxLtnFoldConeShape");

        if (EditorPrefs.HasKey("SoxLtnFoldWave"))
            SoxLtnFoldWave = EditorPrefs.GetBool("SoxLtnFoldWave");

        if (EditorPrefs.HasKey("SoxLtnFoldPosOffsetAnim"))
            SoxLtnFoldPosOffsetAnim = EditorPrefs.GetBool("SoxLtnFoldPosOffsetAnim");
    }

    private void SetEditorPrefs()
    {
        EditorPrefs.SetBool("SoxLtnFoldInteractionPerformance", SoxLtnFoldInteractionPerformance);
        EditorPrefs.SetBool("SoxLtnFoldRenderer", SoxLtnFoldRenderer);
        EditorPrefs.SetBool("SoxLtnFoldPlayControl", SoxLtnFoldPlayControl);
        EditorPrefs.SetBool("SoxLtnFoldLife", SoxLtnFoldLife);
        EditorPrefs.SetBool("SoxLtnFoldColorThickness", SoxLtnFoldColorThickness);
        EditorPrefs.SetBool("SoxLtnFoldGeometry", SoxLtnFoldGeometry);
        EditorPrefs.SetBool("SoxLtnFoldUV", SoxLtnFoldUV);
        EditorPrefs.SetBool("SoxLtnFoldConeShape", SoxLtnFoldConeShape);
        EditorPrefs.SetBool("SoxLtnFoldWave", SoxLtnFoldWave);
        EditorPrefs.SetBool("SoxLtnFoldPosOffsetAnim", SoxLtnFoldPosOffsetAnim);
    }

    // 에디터에서 OnDestroy 는 오브젝트 선택 해제가 될 때에도 호출된다. (삭제될 때에도 기본으로 호출됨)
    void OnDestroy()
    {
        EditorPrefs.SetBool("SoxLtnFoldInteractionPerformance", SoxLtnFoldInteractionPerformance);
        EditorPrefs.SetBool("SoxLtnFoldRenderer", SoxLtnFoldRenderer);
        EditorPrefs.SetBool("SoxLtnFoldPlayControl", SoxLtnFoldPlayControl);
        EditorPrefs.SetBool("SoxLtnFoldLife", SoxLtnFoldLife);
        EditorPrefs.SetBool("SoxLtnFoldColorThickness", SoxLtnFoldColorThickness);
        EditorPrefs.SetBool("SoxLtnFoldGeometry", SoxLtnFoldGeometry);
        EditorPrefs.SetBool("SoxLtnFoldUV", SoxLtnFoldUV);
        EditorPrefs.SetBool("SoxLtnFoldConeShape", SoxLtnFoldConeShape);
        EditorPrefs.SetBool("SoxLtnFoldWave", SoxLtnFoldWave);
        EditorPrefs.SetBool("SoxLtnFoldPosOffsetAnim", SoxLtnFoldPosOffsetAnim);
    }

#if UNITY_EDITOR
    // EditorUpdate 함수가 에디터에서도 정기적으로 호출되도록 하기 위한 코드
    void OnEnable()
    {
        firstNode_sp = serializedObject.FindProperty("firstNode");
        lastNode_sp = serializedObject.FindProperty("lastNode");
        dotSize_sp = serializedObject.FindProperty("dotSize");
        animType_sp = serializedObject.FindProperty("animType");
        ifFollowWhenStop_sp = serializedObject.FindProperty("ifFollowWhenStop");
        lookAtCamera_sp = serializedObject.FindProperty("lookAtCamera");
        ifUseCameraTag_sp = serializedObject.FindProperty("ifUseCameraTag");
        cameraTag_sp = serializedObject.FindProperty("cameraTag");
        shellOffsetStart_sp = serializedObject.FindProperty("shellOffsetStart");
        shellOffsetEnd_sp = serializedObject.FindProperty("shellOffsetEnd");
        recalculateNormals_sp = serializedObject.FindProperty("recalculateNormals");
        recalculateTangents_sp = serializedObject.FindProperty("recalculateTangents");
        ltnMat_sp = serializedObject.FindProperty("ltnMat");
        sortinglayer_sp = serializedObject.FindProperty("sortinglayer");
        orderInLayer_sp = serializedObject.FindProperty("orderInLayer");
        autoActivateBirth_sp = serializedObject.FindProperty("autoActivateBirth");
        loop_sp = serializedObject.FindProperty("loop");
        autoTerminate_sp = serializedObject.FindProperty("autoTerminate");
        autoTerminateType_sp = serializedObject.FindProperty("autoTerminateType");
        genDuration_sp = serializedObject.FindProperty("genDuration");
        genRate_sp = serializedObject.FindProperty("genRate");
        playDelay_sp = serializedObject.FindProperty("playDelay");
        burstRandomOffsetMax_sp = serializedObject.FindProperty("burstRandomOffsetMax");
        bursts_sp = serializedObject.FindProperty("bursts");
        appearMinMax_sp = serializedObject.FindProperty("appearMinMax");
        lifeMinMax_sp = serializedObject.FindProperty("lifeMinMax");
        fadeMinMax_sp = serializedObject.FindProperty("fadeMinMax");
        appearColor_sp = serializedObject.FindProperty("appearColor");
        appearColorType_sp = serializedObject.FindProperty("appearColorType");
        appearThinType_sp = serializedObject.FindProperty("appearThinType");
        mainColor_sp = serializedObject.FindProperty("mainColor");
        fadeOutColor_sp = serializedObject.FindProperty("fadeOutColor");
        fadeOutColorType_sp = serializedObject.FindProperty("fadeOutColorType");
        fadeOutThinType_sp = serializedObject.FindProperty("fadeOutThinType");
        streakWidthMinMax_sp = serializedObject.FindProperty("streakWidthMinMax");
        nodeCount_sp = serializedObject.FindProperty("nodeCount");
        subStep_sp = serializedObject.FindProperty("subStep");
        subStepNoiseAmp_sp = serializedObject.FindProperty("subStepNoiseAmp");
        subStepNoiseAmpType_sp = serializedObject.FindProperty("subStepNoiseAmpType");
        uvType_sp = serializedObject.FindProperty("uvType");
        uvFitRepeat_sp = serializedObject.FindProperty("uvFitRepeat");
        uvScale_sp = serializedObject.FindProperty("uvScale");
        uvOffsetMinMax_sp = serializedObject.FindProperty("uvOffsetMinMax");
        texScrollSpdSingle_sp = serializedObject.FindProperty("texScrollSpdSingle");
        texSheetCount_sp = serializedObject.FindProperty("texSheetCount");
        texSheetBirthType_sp = serializedObject.FindProperty("texSheetBirthType");
        texSheetAnimType_sp = serializedObject.FindProperty("texSheetAnimType");
        texSheetAnimInterval_sp = serializedObject.FindProperty("texSheetAnimInterval");
        expandAnimSpd_sp = serializedObject.FindProperty("expandAnimSpd");
        particlesObjs_sp = serializedObject.FindProperty("particleObjs");
        oneLinkedParticleAtFrame_sp = serializedObject.FindProperty("oneLinkedParticleAtFrame");
        ifUseCone_sp = serializedObject.FindProperty("ifUseCone");
        coneStartBias_sp = serializedObject.FindProperty("coneStartBias");
        coneEndBias_sp = serializedObject.FindProperty("coneEndBias");
        coneStartAmplify_sp = serializedObject.FindProperty("coneStartAmplify");
        coneEndAmplify_sp = serializedObject.FindProperty("coneEndAmplify");
        ifUseWave_sp = serializedObject.FindProperty("ifUseWave");
        ifWaveRandomTime_sp = serializedObject.FindProperty("ifWaveRandomTime");
        ifWaveMultiply_sp = serializedObject.FindProperty("ifWaveMultiply");
        waveMultiplierBirth_sp = serializedObject.FindProperty("waveMultiplierBirth");
        waveMultiplierDeath_sp = serializedObject.FindProperty("waveMultiplierDeath");
        waves_sp = serializedObject.FindProperty("waves");
        ifUsePosOffsetAnim_sp = serializedObject.FindProperty("ifUsePosOffsetAnim");
        posOffsetAnimType_sp = serializedObject.FindProperty("posOffsetAnimType");
        posOffsetAnimMin_sp = serializedObject.FindProperty("posOffsetAnimMin");
        posOffsetAnimMax_sp = serializedObject.FindProperty("posOffsetAnimMax");

        SoxLtn soxLtn = (SoxLtn)target;

#if UNITY_2018_3_OR_NEWER  // 프리팹스테이지는 2018.3 이후에 적용된 기능
        // 프리팹스테이지 검사. null 인지 체크하여 프리뷰 씬에 테스트 이펙트를 생성할지 결정.
        UnityEditor.Experimental.SceneManagement.PrefabStage prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null && Application.isPlaying == false)
        {
            soxLtn.prefabStage = true;
            soxLtn.previewScene = prefabStage.scene;
        }
        else
        {
            soxLtn.prefabStage = false;
        }
#endif

        GetEditorPrefs();

        matBefore = soxLtn.ltnMat;
        if (soxLtn.ltnMat != null)
            SoxLtnEditorSettings.ltnMatScrollEditor = Instantiate(soxLtn.ltnMat);

        if (Application.isPlaying)
        {
            return;
        }
        else
        {
            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        if (soxLtn.ltnMat != null)
        {
            matEditor = (MaterialEditor)CreateEditor(soxLtn.ltnMat);
        }

        // 버전 체크 후 노드체크해야한다. 마우스 우클릭에서 신규 생성되는 SoxLtn은 자동으로 버전이 붙지만 인스펙터에서 Add Component로 생성된 SoxLtn은 노드 없이 생겨난다.
        // 노드 없는데 버전번호가 0이면 인스펙터에서 Add Component로 생성된 것으로 간주 (다소 불안하긴 하지만 - 예를 들어 노드를 수동으로 다 삭제한 경우)
        VersionCheck(soxLtn);

        SoxLtnUtilEditor.CheckIsolatedNode(soxLtn);

        // 프로젝트 창에서 선택한 프리팹을 버전체크하면 문제가 발생한다. Selection.transforms.Length가 0이면 Project View 라는 뜻
        if (Selection.transforms.Length > 0)
        {
            SoxLtn[] soxLtns = soxLtn.GetComponentsInChildren<SoxLtn>();
            foreach (SoxLtn tempSoxLtn in soxLtns)
            {
                if (SoxLtnUtilEditor.CheckNodes(tempSoxLtn) == SoxLtnUtilEditor.SoxLtnCheckNodesResult.NODE_DIFFERENT)
                {
                    EditorUtility.DisplayDialog("Lightning Emitter Notice", "The node count value for Lightning Emitter does not match the actual node count. Changing the actual number of nodes is only possible at the Prefab Stage. Lightning Emitter's node value has been modified to match the actual number of nodes.", "OK");
                }
            }
        }

        // ltnEffectCount 에 의해서 에디터 모드에서도 항상 UV 스크롤을 업데이트 하는 기능이 있는데,
        // 에디터 모드에서는 ltnEffectCount를 정확하게 계산하기가 어려워서 안전하게 에디터가 Enable 될 때에 ltnEffectCount 값을 초기화 한다.
        // 언제나 에디터를 선택한 순간에는 불필요한 이펙트는 없을 것이므로.
        // 플레이모드에서는 이 기능은 쓰면 안됨
        if (!Application.isPlaying)
        {
            soxLtn.ltnEffectCount = 0;
        }

        soxLtn.EditorTimeInit();
    }

    void OnDisable()
    {
        SoxLtn soxLtn = (SoxLtn)target;

#if UNITY_2018_3_OR_NEWER  // 프리팹스테이지는 2018.3 이후에 적용된 기능
        soxLtn.prefabStage = false;
#endif

        SetEditorPrefs();

        if (Application.isPlaying)
        {
            return;
        }
        else
        {
            // if Editor
            soxLtn.Deactivate();
            if (soxLtn.soxLtnRecycle != null)
                DestroyImmediate(soxLtn.soxLtnRecycle.gameObject);

            /*
            // SoxLtn 스크립트를 에디터에서 수동으로 삭제할 경우 자동으로 SoxLtnNode 들이 제거되는 기능인데
            // 삭제까지는 잘 되지만 삭제 직후 UnDo 를 하면 유니티가 강제 종료되는 문제가 있어서 이 기능 자체를 보류
            // 참고로 [ExecuteInEditMode]에 의해서 SoxLtn의 OnDestroy 이벤트에서 작동하는 방식도 마찬가지의 Undo 크래쉬 발생함
            if (soxLtn == null)
            {
                // 에디터 상태에서 SoxLtn 스크립트가 삭제된 상황
                foreach (SoxLtnNode soxLtnNode in soxLtn.soxLtnNodes)
                {
                    if (soxLtnNode != null)
                    {
                        GameObject.DestroyImmediate(soxLtnNode.gameObject);
                    }
                }
            }
            */

            UnityEditor.EditorApplication.update -= EditorUpdate;
        }
    }
    // 여기까지 EditorUpdate 함수가 에디터에서도 정기적으로 호출되도록 하기 위한 코드 끝
#endif

    void EditorUpdate()
    {
        if (Application.isPlaying)
        {
            return;
        }
        SoxLtn soxLtn = (SoxLtn)target;
        if (soxLtn)
        {
            SoxLtn[] soxLtns = soxLtn.GetComponentsInChildren<SoxLtn>();
            foreach (SoxLtn tempSoxLtn in soxLtns)
            {
                //통합 업데이트 함수 호출
                tempSoxLtn.AnyUpdate();
            }

            // 에디터상태에서 이펙트 노드의 AnyUpdate 함수를 강제로 호출하도록 함
            if (soxLtn.soxLtnRecycle != null)
            {
                for (int i = 0; i < soxLtn.soxLtnRecycle.onEffects.Count; i++)
                {
                    SoxLtnEffect soxLtnEffect = soxLtn.soxLtnRecycle.onEffects[i].GetComponent<SoxLtnEffect>();
                    soxLtnEffect.AnyUpdate();
                    if (soxLtn.soxLtnRecycle == null) // AnyUpdate 에서 Recycle 이 최종 삭제되면 for 비교를 마지막 한 번은 더한다. 그 때에 Null 에러 발생하므로 Null 검사 후 바로 break 검사.
                        break;
                }
            }
        }
    }

    private void EditorPlayButton(SoxLtn soxLtn)
    {
        if (Application.isPlaying)
        {
            soxLtn.Play();
        }
        else
        {
            soxLtn.InvokePlay();
        }
    }

    public override void OnInspectorGUI()
    {
        float savedLabelWidth = EditorGUIUtility.labelWidth;
        float savedFieldWidth = EditorGUIUtility.fieldWidth;
        SoxLtn soxLtn = (SoxLtn)target;

        // GUI레이아웃 시작=======================================================
        EditorGUILayout.LabelField(new GUIContent("Connect start and end to external objects (for programmers)", "For example, if you fire a lightning toward an enemy that is created in real time in the game, this is a variable for specifying the launch object and enemy object of the lightning."));
        EditorGUI.indentLevel++;
        GUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox("soxLtn.firstNode", MessageType.None);
        if (soxLtn.firstNode == null) GUI.enabled = false;
        if (GUILayout.Button("Clear"))
        {
            soxLtn.firstNode = null;
        }
        GUI.enabled = true;
        EditorGUILayout.HelpBox("soxLtn.lastNode", MessageType.None);
        if (soxLtn.lastNode == null) GUI.enabled = false;
        if (GUILayout.Button("Clear"))
        {
            soxLtn.lastNode = null;
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(firstNode_sp, new GUIContent(""), true);
        EditorGUILayout.PropertyField(lastNode_sp, new GUIContent(""), true);
        GUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

        GUILayout.BeginHorizontal();
        if (!soxLtn.gameObject.activeSelf) GUI.enabled = false;
        if (!soxLtn.activate)
        {
            // CheckDeadEffect(soxLtn); // 이걸 켜면 Test 버튼이 막혀서 다시 꺼줌
            if (GUILayout.Button("Play"))
            {
                EditorPlayButton(soxLtn);
            }
        }
        else
        {
            // activate 인 경우
            if (GUILayout.Button("Stop"))
            {
                soxLtn.Stop();
            }
        }

        if (GUILayout.Button("Restart"))
        {
            if (!soxLtn.activate)
            {
                // 이펙트가 재생중이 아니면 Play 버튼과 동일하게 작동.
                EditorPlayButton(soxLtn);
            }
            else
            {
                // 이펙트가 재생중이면 일단 Stop 후 다시 Play
                soxLtn.Stop();
                EditorPlayButton(soxLtn);
            }
        }

        if (GUILayout.Button("Single Test"))
        {
            if (Application.isPlaying)
            {
                // Play 중일 때에는 노드가 카메라를 바라보지 않을 수 있다.
                foreach (SoxLtnNode soxLtnNode in soxLtn.soxLtnNodes)
                {
                    soxLtnNode.NodeCameraLookAtAndDistance();
                }
                soxLtn.GenStreak(true);
            }
            else
            {
                SoxLtnEffect tempLtnEffect = soxLtn.GenStreak(true);
                tempLtnEffect.AnyStart();
            }
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        Undo.RecordObject(target, "SoxLtn changed settings");

        // Info Text
        if (soxLtn.soxLtnRecycle != null)
        {
            EditorGUILayout.HelpBox(String.Format("On effects - {0}    Off effects - {1}    Total - {2}", soxLtn.soxLtnRecycle.onEffects.Count, soxLtn.soxLtnRecycle.offEffects.Count, soxLtn.soxLtnRecycle.onEffects.Count + soxLtn.soxLtnRecycle.offEffects.Count), MessageType.None);
        }
        else
        {
            EditorGUILayout.HelpBox(String.Format("On effects - {0}    Off effects - {1}    Total - {2}", 0, 0, 0), MessageType.None);
        }

        EditorGUILayout.PropertyField(dotSize_sp, new GUIContent("Helper dot size"), true);

        // 에디터 타임 보정용 변수인데 사용 보류
        //soxLtn.unityEditorTimeCorrect = EditorGUILayout.Slider(new GUIContent("Editor preview speed correction"), soxLtn.unityEditorTimeCorrect, 0f, 10f);
        //EditorGUILayout.HelpBox("( Playback speed of the Edit-Mode can be different from the speed of play Play-Mode. )", MessageType.None);

        SoxLtnFoldInteractionPerformance = EditorGUILayout.Foldout(SoxLtnFoldInteractionPerformance, "Interaction, Performance");
        if (SoxLtnFoldInteractionPerformance)
        {
            EditorGUI.indentLevel++;
            if (animType_sp.enumValueIndex == (int)SoxLtn.AnimType.STATIC)
            { EditorGUILayout.HelpBox("( STATIC is fast, but some functions are limited. )", MessageType.None); }
            else { EditorGUILayout.HelpBox("( You can animate SoxLtnNode by assign 'Attach Node' at SoxLtnNode. )", MessageType.None); }
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 110f;
            EditorGUIUtility.fieldWidth = 80f;
            EditorGUILayout.PropertyField(animType_sp, new GUIContent("Node animation"), true, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUIUtility.fieldWidth = savedFieldWidth;
            if (animType_sp.enumValueIndex == (int)SoxLtn.AnimType.STATIC)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(ifFollowWhenStop_sp, new GUIContent("Follow when stop", "If this option is turned off, the effect will not follow when playback is stopped."), true);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 110f;
            EditorGUIUtility.fieldWidth = 80f;
            // DYNAMIC 에서는 LookAt camera 처리를 언제나 하므로 LookAt camera를 비활성화 처리
            if (animType_sp.enumValueIndex == (int)SoxLtn.AnimType.STATIC)
            {
                EditorGUILayout.PropertyField(lookAtCamera_sp, new GUIContent("LookAt camera", "BIRTH operation is fast when the camera is fixed. ALWAYS updates the effect so that it looks at the camera every frame."), true, GUILayout.ExpandWidth(false));
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(lookAtCamera_sp, new GUIContent("LookAt camera", "If 'Node animation' is DYNAMIC, 'LookAt camera' will always work."), true);
                GUI.enabled = true;
            }
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUIUtility.fieldWidth = savedFieldWidth;

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(ifUseCameraTag_sp, new GUIContent("Use camera tag", "Use this feature when you want to use a particular camera, for example, the camera used in the UI"), true);
            if (!ifUseCameraTag_sp.boolValue) GUI.enabled = false;
            EditorGUILayout.PropertyField(cameraTag_sp, new GUIContent(""), true, GUILayout.ExpandWidth(false));
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(shellOffsetStart_sp, new GUIContent("Shell offset Start", "For example, if a lightning occurs in a translucent sphere, the starting point of the lightning must move by the radius of the sphere in order for the lightning to start at the surface of the sphere. This function works if the 'Attach Node' of the first SoxLtnNode is specified or if 'soxLtn.firstNode' is specified."), true);
            EditorGUILayout.PropertyField(shellOffsetEnd_sp, new GUIContent("Shell offset End", "For example, if the target of a lightning is a translucent sphere, in order for the end of the lightning to match the surface of the target sphere, the endpoint of the lightning must move by the radius of the target sphere. This function works if the 'Attach Node' of the last SoxLtnNode is specified or 'soxLtn.lastNode' is specified."), true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 140f;
            EditorGUIUtility.fieldWidth = 20f;
            EditorGUILayout.PropertyField(recalculateNormals_sp, new GUIContent("Recalculate Normals", "Most effects do not require normal or tangent calculations."), true);
            EditorGUIUtility.labelWidth = 145f;
            EditorGUILayout.PropertyField(recalculateTangents_sp, new GUIContent("Recalculate Tangents", "Most effects do not require normal or tangent calculations."), true);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUIUtility.fieldWidth = savedFieldWidth;
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        SoxLtnFoldRenderer = EditorGUILayout.Foldout(SoxLtnFoldRenderer, "Renderer");
        if (SoxLtnFoldRenderer)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.fieldWidth = 120f; // 재질을 고르는 조그만 버튼이 우측 스크롤바에 자꾸 가려서 재질만 특별히 필드 폭을 좁게함.
            EditorGUILayout.PropertyField(ltnMat_sp, new GUIContent("Material"), true, GUILayout.ExpandWidth(false));
            EditorGUIUtility.fieldWidth = savedFieldWidth;
            if (EditorGUI.EndChangeCheck() || matBefore != soxLtn.ltnMat) // 단순 EndChangeCheck 만으로는 UnDo 등 에디터상의 다양한 상황을 모두 해결해주지 않음.
            {
                // 이전 재질에디터를 일단 한번 지워준다.
                if (matEditor != null)
                {
                    DestroyImmediate(matEditor);
                }

                // 현재 재질을 다시 등록
                if (soxLtn.ltnMat != null)
                {
                    matEditor = (MaterialEditor)CreateEditor(soxLtn.ltnMat);
                }

                if (soxLtn.ltnMat != null)
                    SoxLtnEditorSettings.ltnMatScrollEditor = Instantiate(soxLtn.ltnMat);
            }
            matBefore = soxLtn.ltnMat;

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sortinglayer_sp, new GUIContent("Sorting Layer", "Sorting Layers and Order in Layer are used to control the order in which effects are drawn with the Sprite Renderer. However, you can still use it without the Sprite Renderer. This takes precedence over the Render Queue. Set it to 0 if it is not a special case."), true);
            EditorGUILayout.PropertyField(orderInLayer_sp, new GUIContent("Order in Layer", "Sorting Layers and Order in Layer are used to control the order in which effects are drawn with the Sprite Renderer. However, you can still use it without the Sprite Renderer. This takes precedence over the Render Queue. Set it to 0 if it is not a special case."), true);
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        SoxLtnFoldPlayControl = EditorGUILayout.Foldout(SoxLtnFoldPlayControl, "Play control");
        if (SoxLtnFoldPlayControl)
        {
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 150f;
            EditorGUIUtility.fieldWidth = 30f;
            EditorGUILayout.PropertyField(autoActivateBirth_sp, new GUIContent("Auto Activate at Birth"), true, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = 70f;
            EditorGUILayout.PropertyField(loop_sp, new GUIContent("Looping"), true, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUIUtility.fieldWidth = savedFieldWidth;
            GUILayout.EndHorizontal();

            if (loop_sp.boolValue) GUI.enabled = false;
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 110f;
            EditorGUIUtility.fieldWidth = 30f;
            EditorGUILayout.PropertyField(autoTerminate_sp, new GUIContent("Auto terminate", "Play mode only"), true, GUILayout.ExpandWidth(false));
            if (!autoTerminate_sp.boolValue || Application.isPlaying) GUI.enabled = false;
            EditorGUIUtility.labelWidth = 120f;
            EditorGUIUtility.fieldWidth = 80f;
            EditorGUILayout.PropertyField(autoTerminateType_sp, new GUIContent("Termination type"), true, GUILayout.ExpandWidth(false));
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUIUtility.fieldWidth = savedFieldWidth;
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 80f;
            EditorGUILayout.PropertyField(genDuration_sp, new GUIContent("Duration"), true);
            EditorGUIUtility.labelWidth = 55f;
            EditorGUILayout.PropertyField(genRate_sp, new GUIContent("Rate"), true);
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 80f;
            EditorGUILayout.PropertyField(playDelay_sp, new GUIContent("Play delay", "Play delay works only at runtime."), true);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUI.indentLevel--;
        }

        EditorGUIUtility.labelWidth = 150f;
        EditorGUILayout.PropertyField(burstRandomOffsetMax_sp, new GUIContent("Burst random offset max"), true);
        EditorGUIUtility.labelWidth = savedLabelWidth;
        EditorGUILayout.PropertyField(bursts_sp, new GUIContent("Burst events"), true);

        // Burst의 인터벌 합계를 계산
        float sumBurstIntervals = 0f;
        if (soxLtn.bursts != null)
        {
            if (soxLtn.bursts.Length > 1)
            {
                for (int i = 0; i < soxLtn.bursts.Length; i++)
                {
                    sumBurstIntervals += soxLtn.bursts[i].interval;
                }
            }
        }
        if (sumBurstIntervals > 0f)
        {
            GUI.enabled = false;
            if (sumBurstIntervals > soxLtn.genDuration)
            {
                EditorGUILayout.LabelField(String.Format("The sum of the Intervals is {0} and greater than the Duration.", sumBurstIntervals));
            }
            else
            {
                EditorGUILayout.LabelField(String.Format("The sum of the Intervals is {0}.", sumBurstIntervals));
            }
            GUI.enabled = true;
        }

        SoxLtnFoldLife = EditorGUILayout.Foldout(SoxLtnFoldLife, "Life");
        if (SoxLtnFoldLife)
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 140f;
            EditorGUILayout.PropertyField(appearMinMax_sp, new GUIContent("Fadein   [Min - Max]"), true);
            appearMinMax_sp.vector2Value = AbsClampMax(appearMinMax_sp.vector2Value);

            EditorGUILayout.PropertyField(lifeMinMax_sp, new GUIContent("Main     [Min - Max]"), true);
            lifeMinMax_sp.vector2Value = AbsClampMax(lifeMinMax_sp.vector2Value);

            EditorGUILayout.PropertyField(fadeMinMax_sp, new GUIContent("Fadeout [Min - Max]"), true);
            fadeMinMax_sp.vector2Value = AbsClampMax(fadeMinMax_sp.vector2Value);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUI.indentLevel--;
        }

        SoxLtnFoldColorThickness = EditorGUILayout.Foldout(SoxLtnFoldColorThickness, "Color, Thickness");
        if (SoxLtnFoldColorThickness)
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 150f;
            EditorGUIUtility.fieldWidth = 80f;
            EditorGUILayout.PropertyField(appearColor_sp, new GUIContent("Fadein color"), true, GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(appearColorType_sp, new GUIContent("Fadein color curve"), true, GUILayout.ExpandWidth(false));
            if (animType_sp.enumValueIndex == (int)SoxLtn.AnimType.STATIC)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(appearThinType_sp, new GUIContent("Fadein thin curve", "Thickness animation at Fadein is possible when 'Node animation' is DYNAMIC type. (Note that thickness animation at Fadeout is also possible with STATIC)"), true, GUILayout.ExpandWidth(false));
            }
            else
            {
                EditorGUILayout.PropertyField(appearThinType_sp, new GUIContent("Fadein thin curve"), true, GUILayout.ExpandWidth(false));
            }
            GUI.enabled = true;

            EditorGUILayout.PropertyField(mainColor_sp, new GUIContent("Main color"), true, GUILayout.ExpandWidth(false));

            EditorGUILayout.PropertyField(fadeOutColor_sp, new GUIContent("Fadeout color"), true, GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fadeOutColorType_sp, new GUIContent("Fadeout color curve"), true, GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fadeOutThinType_sp, new GUIContent("Fadeout thin curve"), true, GUILayout.ExpandWidth(false));
            EditorGUIUtility.fieldWidth = savedFieldWidth;

            //soxLtn.streakWidthMinMax = EditorGUILayout.Vector2Field(new GUIContent("Thickness [Min - Max]"), soxLtn.streakWidthMinMax);
            EditorGUILayout.PropertyField(streakWidthMinMax_sp, new GUIContent("Thickness [Min - Max]"), true);
            streakWidthMinMax_sp.vector2Value = AbsClampMax(streakWidthMinMax_sp.vector2Value);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUI.indentLevel--;
        }

        SoxLtnFoldGeometry = EditorGUILayout.Foldout(SoxLtnFoldGeometry, "Geometry");
        if (SoxLtnFoldGeometry)
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 150f;
            // 프로젝트 창에서 선택한 프리팹은 노드 수를 변경하면 안된다. Selection.transforms.Length가 0이면 Project View 라는 뜻
            if (Selection.transforms.Length == 0)
                GUI.enabled = false;
#if UNITY_2018_3_OR_NEWER  // 프리팹스테이지는 2018.3 이후에 적용된 기능
            // 노드 숫자의 변경은 계층구조를 변경하는 것이므로 프리팹모드에서만 허용된다.
            // 하지만 프리팹인 경우에만 에디터에서 편집 불가. 일반 게임오브젝트는 그대로 둔다.
            PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(soxLtn);
            if (prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant)
            {
                if (soxLtn.prefabStage == false)
                    GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(nodeCount_sp, new GUIContent("Node count", "Changing the Node hierarchy is only possible in Prefab Mode."), true);
            nodeCount_sp.intValue = Mathf.Clamp(nodeCount_sp.intValue, 2, 20);
            GUI.enabled = true;
#else
            EditorGUILayout.PropertyField(nodeCount_sp, new GUIContent("Node count"), true);
            nodeCount_sp.intValue = (int)Mathf.Clamp(nodeCount_sp.intValue, 2, 20);
#endif
            GUI.enabled = true;

            EditorGUILayout.PropertyField(subStep_sp, new GUIContent("Sub-step", "Sub-step between nodes. If the Sub-step value is too large, the performance is degraded."), true);
            subStep_sp.intValue = Mathf.Clamp(subStep_sp.intValue, 0, 99);
            EditorGUILayout.PropertyField(subStepNoiseAmp_sp, new GUIContent("Sub-step noise", "Applies a zigzag-shaped noise to the sub-step vertex. It does not apply to vertices at node locations."), true);
            EditorGUILayout.PropertyField(subStepNoiseAmpType_sp, new GUIContent("Noise distance type", "In the DISTANCE_RELATED type, the 'Sub-step noise' increases when the distance of 'LtnNode' increases."), true);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUI.indentLevel--;
        }

        SoxLtnFoldUV = EditorGUILayout.Foldout(SoxLtnFoldUV, "UV");
        if (SoxLtnFoldUV)
        {
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 90f;
            EditorGUILayout.PropertyField(uvType_sp, new GUIContent("UV Type"), true);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            if (uvType_sp.enumValueIndex == (int)SoxLtn.UVType.FIT)
            {
                // FIT 방식
                EditorGUILayout.PropertyField(uvFitRepeat_sp, new GUIContent("Repeat count"), true);
                uvFitRepeat_sp.intValue = Mathf.Max(1, uvFitRepeat_sp.intValue);
            }
            else
            {
                // SCALE 방식
                EditorGUILayout.PropertyField(uvScale_sp, new GUIContent("UV scale"), true);
            }
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 150f;
            EditorGUILayout.PropertyField(uvOffsetMinMax_sp, new GUIContent("UV offset [Min - Max]", "If you specify different values for Min and Max, the starting position of the texture will be determined randomly."), true);
            Vector2 tMinMax = uvOffsetMinMax_sp.vector2Value;
            tMinMax.y = Mathf.Max(tMinMax.x, tMinMax.y);
            tMinMax.x = Mathf.Clamp(tMinMax.x, 0f, 1f);
            tMinMax.y = Mathf.Clamp(tMinMax.y, 0f, 1f);
            uvOffsetMinMax_sp.vector2Value = tMinMax;

            EditorGUILayout.PropertyField(texScrollSpdSingle_sp, new GUIContent("UV scroll speed", "For optimization, all effects have the same UV scrolling speed."), true);

            EditorGUILayout.PropertyField(texSheetCount_sp, new GUIContent("Texture sheet count", "The Texture sheet function only supports vertical sheets for horizontal scrolling."), true);
            texSheetCount_sp.intValue = Mathf.Max(1, texSheetCount_sp.intValue);
            EditorGUIUtility.labelWidth = savedLabelWidth;

            if (texSheetCount_sp.intValue <= 1)
                GUI.enabled = false;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(texSheetBirthType_sp, new GUIContent("Birth type"), true);
            EditorGUILayout.PropertyField(texSheetAnimType_sp, new GUIContent("Anim. type"), true);
            EditorGUILayout.PropertyField(texSheetAnimInterval_sp, new GUIContent("Anim. Interval"), true);
            texSheetAnimInterval_sp.floatValue = Mathf.Max(0f, texSheetAnimInterval_sp.floatValue);
            EditorGUI.indentLevel--;
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }

        if (animType_sp.enumValueIndex == (int)SoxLtn.AnimType.STATIC) GUI.enabled = false;
        EditorGUILayout.PropertyField(expandAnimSpd_sp, new GUIContent("Expand anim. speed", "'Expand animation' animates the effect based on the range set in 'Circle Area' of 'SoxLtnNode'. This function is activated when 'Node animation' is DYNAMIC."), true);
        GUI.enabled = true;

        EditorGUILayout.PropertyField(particlesObjs_sp, new GUIContent("Linked particle objects", "The order of Linked particles is the same as the order of the Nodes. This function works only in Play mode. If multiple effects occur simultaneously by a Burst, the Particle's simulation space must be World."), true);
        EditorGUIUtility.labelWidth = 172f;
        EditorGUILayout.PropertyField(oneLinkedParticleAtFrame_sp, new GUIContent("One Linked Particle at Frame", "In the same frame, only one Linked Particle is played. It is useful when there are several effects at the same time by the Burst, but the location of the effect is one point."), true);
        EditorGUIUtility.labelWidth = savedLabelWidth;

        SoxLtnFoldConeShape = EditorGUILayout.Foldout(SoxLtnFoldConeShape, new GUIContent("Cone shape", "Narrow the ends of the lightning to create a pointed shape. If the sub-step is insufficient, the texture may be distorted."));
        if (SoxLtnFoldConeShape)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ifUseCone_sp, new GUIContent("Use cone shape"), true);
            if (ifUseCone_sp.boolValue == false)
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 95f;
            EditorGUILayout.PropertyField(coneStartBias_sp, new GUIContent("Bias Start"), true);
            coneStartBias_sp.floatValue = Mathf.Clamp(coneStartBias_sp.floatValue, 0f, 1f);
            EditorGUILayout.PropertyField(coneEndBias_sp, new GUIContent("Bias End"), true);
            coneEndBias_sp.floatValue = Mathf.Clamp(coneEndBias_sp.floatValue, 0f, 1f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(coneStartAmplify_sp, new GUIContent("Amplify Start"), true);
            coneStartAmplify_sp.floatValue = Mathf.Max(coneStartAmplify_sp.floatValue, 0f);
            EditorGUILayout.PropertyField(coneEndAmplify_sp, new GUIContent("Amplify End"), true);
            coneEndAmplify_sp.floatValue = Mathf.Max(coneEndAmplify_sp.floatValue, 0f);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }

        SoxLtnFoldWave = EditorGUILayout.Foldout(SoxLtnFoldWave, new GUIContent("Wave", "Apply wave to Lightning. A certain number of sub-steps are needed."));
        if (SoxLtnFoldWave)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ifUseWave_sp, new GUIContent("Use wave", "Some functions of Wave work when 'Node animation' is DYNAMIC type (eg Scroll Speed)."), true);
            if (ifUseWave_sp.boolValue == false)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(ifWaveRandomTime_sp, new GUIContent("Random time", "The starting position of the waveform is determined at random."), true);

            if (animType_sp.enumValueIndex == (int)SoxLtn.AnimType.STATIC)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(ifWaveMultiply_sp, new GUIContent("Use multiplier", "This check box determines whether to apply a multiplier for a Lifetime. This function is activated when 'Node animation' is DYNAMIC type."), true);
            if (ifWaveMultiply_sp.boolValue == false)
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 70f;
            EditorGUILayout.PropertyField(waveMultiplierBirth_sp, new GUIContent("Birth", "Amplify multiplier Birth"), true);
            EditorGUILayout.PropertyField(waveMultiplierDeath_sp, new GUIContent("Death", "Amplify multiplier Death"), true);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            EditorGUI.indentLevel--;
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            if (ifUseWave_sp.boolValue == false)
                GUI.enabled =false;
            EditorGUILayout.PropertyField(waves_sp, new GUIContent("Waves", "Multiple waves can be mixed."), true);
            EditorGUI.indentLevel--;
            GUI.enabled = true;
        }

        SoxLtnFoldPosOffsetAnim = EditorGUILayout.Foldout(SoxLtnFoldPosOffsetAnim, new GUIContent("Position offset animation", "Moves the effect relative to the local axis of the SoxLtn object."));
        if (SoxLtnFoldPosOffsetAnim)
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 195f;
            EditorGUILayout.PropertyField(ifUsePosOffsetAnim_sp, new GUIContent("Use Position offset animation"), true);
            EditorGUIUtility.labelWidth = savedLabelWidth;
            if (ifUsePosOffsetAnim_sp.boolValue == false)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(posOffsetAnimType_sp, new GUIContent("Curve type"), true);
            if (posOffsetAnimType_sp.enumValueIndex == (int)SoxLtn.AnimCurveType.CONSTANT)
            {
                posOffsetAnimType_sp.enumValueIndex = (int)SoxLtn.AnimCurveType.LINEAR;
            }
            EditorGUILayout.PropertyField(posOffsetAnimMin_sp, new GUIContent("Distance Min."), true);
            EditorGUILayout.PropertyField(posOffsetAnimMax_sp, new GUIContent("Distance Max."), true);
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }

        if (matEditor != null)
        {
            // Draw the material's foldout and the material shader field
            // Required to call matEditor.OnInspectorGUI ();
            matEditor.DrawHeader();

            //  We need to prevent the user to edit Unity default materials
            bool isDefaultMaterial = !AssetDatabase.GetAssetPath(soxLtn.ltnMat).StartsWith("Assets");

            using (new EditorGUI.DisabledGroupScope(isDefaultMaterial))
            {
                // Draw the material properties
                // Works only if the foldout of matEditor.DrawHeader () is open
                matEditor.OnInspectorGUI();
            }
        }

        serializedObject.ApplyModifiedProperties();    // 이건 에디터GUI의 변화를 실제 오브젝트에 반영하는 것
        serializedObject.Update();                     // 이건 오브젝트의 변화를 에디터에 반영하는 것. 예를 들어 Undo 등의 변화라던가 Reset 버튼에 의한 변화가 있으면 업데이트 해줘야한다.

        Undo.FlushUndoRecordObjects();

        //DrawDefaultInspector();
        // GUI레이아웃 끝========================================================

        // 프로젝트 창에서 선택한 프리팹을 버전체크하면 문제가 발생한다. Selection.transforms.Length가 0이면 Project View 라는 뜻
        if (Selection.transforms.Length > 0)
        {
            if (SoxLtnUtilEditor.CheckNodes(soxLtn) == SoxLtnUtilEditor.SoxLtnCheckNodesResult.NODE_DIFFERENT)
            {
                EditorUtility.DisplayDialog("Lightning Emitter Notice", "The node count value for Lightning Emitter does not match the actual node count. Changing the actual number of nodes is only possible at the Prefab Stage. Lightning Emitter's node value has been modified to match the actual number of nodes.", "OK");
            }
        }
    }

    void OnSceneGUI()
    {
        SoxLtn soxLtn = (SoxLtn)target;

        foreach (SoxLtnNode soxLtnNode in soxLtn.soxLtnNodes)
        {
            soxLtnNode.editorCamPos = SceneView.lastActiveSceneView.camera.transform.position;
            soxLtnNode.Update();
        }
    }

    // Undo나 Play 도중 Save 등의 예외상황에 의해 씬에 불필요한 이펙트가 남아있는 경우 삭제하는 함수
    private void CheckDeadEffect(SoxLtn soxLtn)
    {
        if (!soxLtn.activate)
        {
            SoxLtnEffect[] tempLtnEffects = FindObjectsOfType<SoxLtnEffect>();
            foreach (SoxLtnEffect tempLtnEffect in tempLtnEffects)
            {
                if (tempLtnEffect.soxLtn == soxLtn)
                {
                    GameObject.DestroyImmediate(tempLtnEffect.gameObject);
                }
            }
        }
    }

    //MinMax 값에 대해서 Min이 Max보다 크지 않고, Min Max 모두 0보다 크게 해준다.
    private Vector2 AbsClampMax(Vector2 minMax)
    {
        Vector2 tMinMax = minMax;
        tMinMax.y = Mathf.Max(tMinMax.x, tMinMax.y);
        if (tMinMax.x < 0) tMinMax.x = 0;
        if (tMinMax.y < 0) tMinMax.y = 0;
        return tMinMax;
    }

    // 버전 변화에 따른 자동 처리 (On Enable에서 처리함)
    private void VersionCheck(SoxLtn soxLtn)
    {
        // 이 버전 체크 함수 이후 노드체크 됨을 전제로 함. 마우스 우클릭에서 신규 생성되는 SoxLtn은 자동으로 버전이 붙지만 인스펙터에서 Add Component로 생성된 SoxLtn은 노드 없이 생겨난다.
        // 노드 없는데 버전번호가 0이면 인스펙터에서 Add Component로 생성된 것으로 간주 (다소 불안하긴 하지만 - 예를 들어 노드를 수동으로 다 삭제한 경우)
        if (soxLtn.GetComponentsInChildren<SoxLtnNode>().Length == 0 && soxLtn.version < soxLtn.versionNow)
        {
            // 인스펙터에서 컴포넌트가 방금 추가된 상황 (아직 노드가 추가되기 직전), 버전 기록
            soxLtn.version = soxLtn.versionNow;
        }

        if (soxLtn.soxLtnNodes != null)
        {
            // 1.378부터 노드의 스케일이 이펙트에 반영되는 변화가 있은 이후 기존 버전의 이펙트들의 노드의 스케일을 1로 리셋할 필요가 있음
            // 기존 노드의 스케일은 기즈모와 연동되던 것들이라 스케일이 의미가 없음
            if (soxLtn.version < 1.378f)
            {
                foreach (SoxLtnNode node in soxLtn.soxLtnNodes)
                {
                    node.transform.localScale = Vector3.one;
                }
                //수정한 뒤 최신 버전으로 표기
                soxLtn.version = soxLtn.versionNow;
                //씬이 수정되었다는 표시
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

                EditorUtility.DisplayDialog("Lightning Emitter Notice", "As the Lightning Emitter was upgraded, the node's Scale was automatically initialized.", "OK");
            }
        }
    }
}

#if UNITY_EDITOR
public static class SoxLtnUtilEditor
{
    public enum SoxLtnCheckNodesResult
    { DO_NOTHING, NODE_REDUCED, NODE_INCREASED, NODE_DIFFERENT }

    // 노드 상태가 정상적인지 체크하여 적절한 조치를 취한다.
    public static SoxLtnCheckNodesResult CheckNodes(SoxLtn soxLtn)
    {
        // 실행중에는 체크노드를 하지 않는다.
        if (Application.isPlaying)
        {
            return SoxLtnCheckNodesResult.DO_NOTHING;
        }

        SoxLtnCheckNodesResult result = SoxLtnCheckNodesResult.DO_NOTHING;

        // 자식노드 카운트와 SoxLtnNode 카운트가 일치하는지 검사하여 처리함
        SoxLtnNode[] tempSoxLtnNodes = soxLtn.GetSoxLtnNodesInChildren();

#if UNITY_2018_3_OR_NEWER  // 프리팹스테이지는 2018.3 이후에 적용된 기능
        if (soxLtn.prefabStage == false && soxLtn.nodeCount != tempSoxLtnNodes.Length) // 프리팹 스테이지가 아닌데 노드 수가 다르면
        {
            // 프리팹인지 검사 필요. 프리팹이면 아무 작동 안하고 NODE_DIFFERENT 리턴 (리턴 받아서 오류 다이얼로그 박스 출력)
            PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(soxLtn);
            if (prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant)
            {
                // 프리팹 스테이지가 아닌데 프리팹이고 숫자가 다르다?
                // 예를들어 paste component value 등으로 인해 노드 숫자가 변경되면 이런 상황 발생.
                // 노드 계층구조 변경 없이 soxLtn의 노드 값을 실제 노드 개수에 맞춘다.
                soxLtn.nodeCount = tempSoxLtnNodes.Length;
                return SoxLtnCheckNodesResult.NODE_DIFFERENT; 
            }
        }
#endif

        // 줄여야 하면 그냥 노드 삭제
        if ((soxLtn.nodeCount < tempSoxLtnNodes.Length) && (soxLtn.nodeCount != 0))
        {
            for (int i = tempSoxLtnNodes.Length; i > soxLtn.nodeCount; i--)
            {
                // nodeCount와 meIndex 적용
                //t.nodeCount = t.transform.parent.childCount;
                for (int nc = 0; nc < soxLtn.nodeCount; nc++)
                {
                    tempSoxLtnNodes[nc].GetComponent<SoxLtnNode>().nodeCount = soxLtn.nodeCount;
                    tempSoxLtnNodes[nc].GetComponent<SoxLtnNode>().meIndex = nc;
                }
                GameObject.DestroyImmediate(tempSoxLtnNodes[i - 1].gameObject, true);
            }
            result = SoxLtnCheckNodesResult.NODE_REDUCED;
        }

        // 늘려야 하면 노드 추가 후 SoxLtnNode 컴포넌트 추가
        if (soxLtn.nodeCount > tempSoxLtnNodes.Length)
        {
            Transform lastNode;
            Vector3 addDirVector;
            if (tempSoxLtnNodes.Length != 0)
            {
                lastNode = tempSoxLtnNodes[tempSoxLtnNodes.Length - 1].transform;
                addDirVector = lastNode.forward * 4f;
            }
            else
            {
                lastNode = soxLtn.transform;
                addDirVector = new Vector3(0, 0, 0);
            }

            for (int i = 0; i < (soxLtn.nodeCount - tempSoxLtnNodes.Length); i++)
            {
                GameObject newNode = new GameObject();
                newNode.transform.position = lastNode.position + (addDirVector * soxLtn.dotSize * 4f);
                newNode.AddComponent<SoxLtnNode>();
                newNode.transform.parent = soxLtn.transform;
                newNode.GetComponent<SoxLtnNode>().soxLtn = soxLtn;
                lastNode = newNode.transform;
                addDirVector = lastNode.forward * 4f;
            }

            //노드를 모두 늘린 뒤 늘어난 노드들의 nodeCount 변수를 갱신해야함
            tempSoxLtnNodes = soxLtn.GetSoxLtnNodesInChildren();
            for (int i = 0; i < tempSoxLtnNodes.Length; i++)
            {
                tempSoxLtnNodes[i].nodeCount = tempSoxLtnNodes.Length;
                tempSoxLtnNodes[i].meIndex = i;
                if (tempSoxLtnNodes[i].name == "New Game Object")
                {
                    tempSoxLtnNodes[i].name = ("SoxLtnNode" + i.ToString());
                }
            }
            result = SoxLtnCheckNodesResult.NODE_INCREASED;
        }

        // 모든 SoxLtnNode 에 대해서 처리하는 부분
        tempSoxLtnNodes = soxLtn.GetSoxLtnNodesInChildren();

        foreach (SoxLtnNode soxLtnNode in tempSoxLtnNodes)
        {
            Transform nodeTransform = soxLtnNode.transform;
            // 구 버전에저 만들어진 노드에 달려있는 메시필터와 메시렌더러를 삭제한다.
            MeshFilter mf = nodeTransform.GetComponent<MeshFilter>();
            if (mf)
            {
                UnityEngine.Object.DestroyImmediate(mf);
            }

            MeshRenderer mr = nodeTransform.GetComponent<MeshRenderer>();
            if (mr)
            {
                UnityEngine.Object.DestroyImmediate(mr);
            }

            if (!soxLtnNode.soxLtn)
                soxLtnNode.soxLtn = soxLtn;

            // 시작이나 끝에 있는 노드는 autoMiddlePosition 옵션을 자동으로 꺼준다
            if (soxLtnNode.meIndex == tempSoxLtnNodes.Length || soxLtnNode.meIndex == 0)
            {
                soxLtnNode.autoMiddlePosition = false;
            }

            // firstNode 혹은 lastNode 는 자기 자신 노드를 가지고있으면 안된다.
            if (nodeTransform == soxLtn.firstNode)
            {
                soxLtn.firstNode = null;
            }

            // firstNode 혹은 lastNode 는 자기 자신 노드를 가지고있으면 안된다.
            if (nodeTransform == soxLtn.lastNode)
            {
                soxLtn.lastNode = null;
            }
        }
        soxLtn.soxLtnNodes = tempSoxLtnNodes;

        return result;
    }

    // 자식 노드들 중 연결이 끊긴 노드가 있으면 채워넣는다.
    public static void CheckIsolatedNode(SoxLtn soxLtn)
    {
        SoxLtnNode[] nodes = soxLtn.GetComponentsInChildren<SoxLtnNode>();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].soxLtn == null)
            {
                nodes[i].soxLtn = soxLtn;
            }
        }
    }
}
#endif
