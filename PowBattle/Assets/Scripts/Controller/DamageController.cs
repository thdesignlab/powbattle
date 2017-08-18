using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DamageController : MonoBehaviour
{
    [SerializeField]
    protected bool isSpawnDamageEffect;

    const float FORWARD_RATE = 0.8f;
    const float SIDE_RATE = 1.2f;
    const float BACK_RATE = 1.5f;

    private GameObject _damageEffect;
    protected GameObject damageEffect
    {
        get { return (_damageEffect) ? _damageEffect : _damageEffect = Common.Func.GetEffectResource("DamageEffect"); }
    }

    public bool Damage(int damage, float impact, Transform attackTran, Transform hitTran, Transform ownerTran)
    {
        if (hitTran == null || attackTran == null) return false;

        bool isHit = false;
        int hitDamage = 0;
        switch (hitTran.tag)
        {
            case Common.CO.TAG_UNIT:
            case Common.CO.TAG_ENEMY:
                //ユニット
                //被弾方向補正
                Vector3 hitVector = attackTran.position - hitTran.position;
                float damageRate = GetHitAngleRate(hitTran, hitVector);
                damage = (int)(damage * damageRate);
                hitDamage = hitTran.GetComponent<UnitController>().Hit(damage, impact * -hitVector.normalized, ownerTran);
                if (hitDamage > 0) SpawnDamageEffect(attackTran, hitTran);
                isHit = true;
                break;

            case Common.CO.TAG_HQ:
            case Common.CO.TAG_ENEMY_HQ:
                //HQ
                hitDamage = hitTran.GetComponent<UnitController>().Hit(damage / 10, ownerTran);
                isHit = true;
                break;

            case Common.CO.TAG_BREAK_OBSTACLE:
                //破壊可能障害物
                ObstacleController obstacleCtrl = hitTran.GetComponent<ObstacleController>();
                if (ownerTran != null)
                {
                    int obstacleSide = obstacleCtrl.GetSide();
                    if (obstacleSide != Common.CO.SIDE_UNKNOWN && obstacleSide == Common.Func.GetMySide(ownerTran.tag)) damage = 0;
                }
                if (damage > 0) hitDamage = obstacleCtrl.Hit(damage, ownerTran);
                if (hitDamage > 0) SpawnDamageEffect(attackTran, hitTran);
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

    private void SpawnDamageEffect(Transform attackTran, Transform hitTran)
    {
        if (damageEffect == null) return;
        Vector3 hitPos = (attackTran.position + hitTran.position) / 2.0f;
        //hitPos = (hitPos + hitTran.position) / 2.0f;
        Instantiate(damageEffect, hitPos, Quaternion.identity);
    }
}
