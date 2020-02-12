using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 다른 위치에 있는 SoxLtn을 등록해서 원격으로 Play() 혹은 Stop() 신호를 주기 위한 목적
// 애니메이션 이벤트로 SoxLtn에 Play, Stop 을 수행해야할 일이 종종 있는데, 애니메이션 이벤트는 애니메이터가 있는 노드만 인식한다. SoxLtn은 다른 위치에 있는 경우가 많다.
public class SoxLtnPlayer : MonoBehaviour {

    public SoxLtn[] soxLtns;

    public void PlaySoxLtn(int ltnIndex)
    {
        if (soxLtns.Length <= 0 || soxLtns.Length < ltnIndex)
        {
            return;
        }

        if (soxLtns[ltnIndex] != null)
        {
            soxLtns[ltnIndex].Play();
        }
    }

    public void StopSoxLtn(int ltnIndex)
    {
        if (soxLtns.Length <= 0 || soxLtns.Length < ltnIndex)
        {
            return;
        }

        if (soxLtns[ltnIndex] != null)
        {
            soxLtns[ltnIndex].Stop();
        }
    }
}
