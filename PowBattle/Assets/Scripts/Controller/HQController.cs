using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HQController : UnitController
{

    protected override void Update()
    {
    }
    public override int Hit(int damage, Transform enemyTran)
    {
        Debug.Log("HQ HIT");
        return base.Hit(damage, enemyTran);
    }
}
