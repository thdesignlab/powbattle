using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamageController : MonoBehaviour
{
    public bool Damage(Transform attackTran, Transform hitTran, int damage, Transform ownerTran)
    {
        bool isHit = false;
        Debug.Log(hitTran.tag);
        switch (hitTran.tag)
        {
            case Common.CO.TAG_UNIT:
            case Common.CO.TAG_ENEMY:
            case Common.CO.TAG_HQ:
            case Common.CO.TAG_ENEMY_HQ:
                //ユニット
                hitTran.GetComponent<UnitController>().Hit(damage, ownerTran);
                isHit = true;
                break;

            case Common.CO.TAG_OBSTACLE:
                //障害物
                isHit = true;
                break;
        }
        return isHit;
    }
}
