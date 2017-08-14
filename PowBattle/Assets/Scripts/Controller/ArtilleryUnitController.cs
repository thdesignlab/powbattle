using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ArtilleryUnitController : UnitController
{

    //目視チェック
    protected override bool IsDiscoveryTarget(Transform target, float range = 0)
    {
        return true;
    }
}
