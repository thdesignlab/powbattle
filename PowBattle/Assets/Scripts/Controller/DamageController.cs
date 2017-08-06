using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamageController : MonoBehaviour
{
    const float FORWARD_RATE = 1.0f;
    const float SIDE_RATE = 1.2f;
    const float BACK_RATE = 1.5f;

    public bool Damage(int damage, float impact, Transform attackTran, Transform hitTran, Transform ownerTran)
    {
        if (hitTran == null) return false;

        bool isHit = false;
        //Debug.Log(hitTran.tag);
        switch (hitTran.tag)
        {
            case Common.CO.TAG_UNIT:
            case Common.CO.TAG_ENEMY:
                //ユニット
                //被弾方向補正
                if (attackTran == null) return false;
                Vector3 hitVector = attackTran.position - hitTran.position;

                float damageRate = GetHitAngleRate(hitTran, hitVector);
                damage = (int)(damage * damageRate);
                hitTran.GetComponent<UnitController>().Hit(damage, impact * -hitVector.normalized, ownerTran);
                isHit = true;
                break;

            case Common.CO.TAG_HQ:
            case Common.CO.TAG_ENEMY_HQ:
                //HQ
                hitTran.GetComponent<UnitController>().Hit(damage / 5, ownerTran);
                isHit = true;
                break;

            case Common.CO.TAG_BREAK_OBSTACLE:
                //破壊可能障害物
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

    public float GetHitAngleRate(Transform hitTran, Vector3 hitVector3)
    {
        Vector2 forwardVector2 = new Vector2(hitTran.forward.z, hitTran.forward.x);
        Vector2 hitVector2 = new Vector2(hitVector3.z, hitVector3.x);
        float angleDiff = Vector2.Angle(forwardVector2, hitVector2);
        float damageRate = BACK_RATE;
        if (angleDiff <= 45)
        {
            damageRate = FORWARD_RATE;
        }
        else if (angleDiff <= 135)
        {
            damageRate = SIDE_RATE;
        }
        //Debug.Log(angleDiff + ">> " + damageRate);
        
        return damageRate;
    }
}
