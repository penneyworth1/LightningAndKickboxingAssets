using UnityEngine;
using System.Collections.Generic;

public class SoxLtnRecycle : MonoBehaviour {

    public SoxLtn soxLtn;
    public List<GameObject> onEffects = new List<GameObject>();
    public List<GameObject> offEffects = new List<GameObject>();

    [HideInInspector]
    public SoxLtn.AutoTerminateType autoTerminateType;
	
    public void DeathCheck()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (onEffects.Count == 0)
            {
                soxLtn.soxLtnRecycle = null;
                GameObject.DestroyImmediate(gameObject);
            }
            return;
        }
#endif

        // Runtime
        if (soxLtn != null)
        {
            if (!soxLtn.activate && onEffects.Count <= 0 && soxLtn.autoTerminate)
            {
				// D프로젝트 브랜치 시작
                //GameObject.Destroy(soxLtn.gameObject);
                switch (autoTerminateType)
                {
                    case SoxLtn.AutoTerminateType.DESTROY:
                        GameObject.Destroy(soxLtn.gameObject);
						GameObject.Destroy(gameObject);
                        break;
                    case SoxLtn.AutoTerminateType.DEACTIVATE:
                        soxLtn.gameObject.SetActive(false);
                        gameObject.SetActive(false);
                        break;
                }
				// D프로젝트 브랜치 끝				
            }
        }
        else
        {
            if (onEffects.Count == 0)
            {
                soxLtn.soxLtnRecycle = null;
                GameObject.Destroy(gameObject);
            }
        }
    }
}
