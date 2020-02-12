using UnityEngine;
using UnityEditor;
using System.Collections;

public class SoxLtnOnWillSaveCallback : UnityEditor.AssetModificationProcessor
{
    static string[] OnWillSaveAssets(string[] paths)
    {
        // 실행중인 모든 SoxLtn을 일단 스톱한다.
        SoxLtn[] soxLtns = Object.FindObjectsOfType<SoxLtn>();
        foreach (SoxLtn soxLtn in soxLtns)
        {
            soxLtn.Stop();
        }

        // 에디터에서 테스트 플레이 도중에 씬을 저장할 경우 Recycle 이나 Effect 오브젝트가 같이 저장되지 않도록 한다.
        SoxLtnRecycle[] soxLtnRecycles = Object.FindObjectsOfType<SoxLtnRecycle>();
        foreach (SoxLtnRecycle soxLtnRecycle in soxLtnRecycles)
        {
            GameObject.DestroyImmediate(soxLtnRecycle.gameObject);
        }

        SoxLtnEffect[] soxLtnEffects = Object.FindObjectsOfType<SoxLtnEffect>();
        foreach (SoxLtnEffect soxLtnEffect in soxLtnEffects)
        {
            GameObject.DestroyImmediate(soxLtnEffect.gameObject);
        }

        /*
        if (soxLtnRecycles.Length > 0 || soxLtnEffects.Length > 0)
        {
            Debug.Log("Temporary objects related to SoxLtn have been automatically deleted to prevent them from being stored in the scene file.");
        }
        */

        return paths;
    }
}
