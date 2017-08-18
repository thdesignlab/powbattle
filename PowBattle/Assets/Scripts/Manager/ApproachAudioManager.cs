using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class ApproachAudioManager : BattleAudioManager
{
    [SerializeField]
    protected float distance;
    protected Transform targetTran;

    protected bool isApproach = false;

    private void Update()
    {
        if (audioSource == null || targetTran == null) return;

        if (distance >= Vector3.Distance(transform.position, targetTran.position))
        {
            if (!isApproach)
            {
                Play();
                isApproach = true;
            }
        }
        else if (audioSource.isPlaying)
        {
            Stop();
        }
    }

    public void SetTarget(Transform target)
    {
        targetTran = target;
    }
}
