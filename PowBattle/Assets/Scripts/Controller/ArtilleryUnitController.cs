using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ArtilleryUnitController : UnitController
{
    [SerializeField]
    protected bool isNotNeedSight;

    //目視チェック
    protected override bool IsDiscoveryTarget(Transform target, float range = 0)
    {
        if (isNotNeedSight) return true;
        return base.IsDiscoveryTarget(target, range);
    }
}
