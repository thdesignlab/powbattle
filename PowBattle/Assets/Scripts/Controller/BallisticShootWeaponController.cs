﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BallisticShootWeaponController : ShootWeaponController
{
    [SerializeField]
    protected float shootAngle;
    private float speed;

    protected override void LockOn(Transform target)
    {
        if (shootAngle <= 0 || 90 < shootAngle) shootAngle = 45;
        Vector3 pos = target.position;
        pos += GetShootDiff(pos);
        myTran.LookAt(new Vector3(pos.x, myTran.position.y, pos.z));
        myTran.Rotate(Vector3.right, -shootAngle);

        //初速計算
        CalcShootSpeed(myTran.position, pos, shootAngle);
    }

    protected override GameObject Shoot(int muzzleNo = 0)
    {
        if (speed <= 0)
        {
            //攻撃不可の場合
            myTran.parent.GetComponent<UnitController>().SetTarget(null);
            return null;
        }
        GameObject obj = base.Shoot(muzzleNo);
        if (obj == null) return null;

        //初速付与
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        Vector3 force = obj.transform.forward * speed * rigidbody.mass;
        rigidbody.AddForce(force, ForceMode.Impulse);
        return obj;
    }

    private void CalcShootSpeed(Vector3 shootPos, Vector3 targetPos, float angle)
    {
        speed = ComputeVectorFromAngle(shootPos, targetPos, angle);
        if (speed <= 0.0f)
        {
            //Debug.LogWarning("!!");
            return;
        }

        //Vector3 vec = ConvertVectorToVector3(speed, angle, shootPos, targetPos);
        //InstantiateShootObject(vec);
    }

    private float ComputeVectorFromAngle(Vector3 shootPos, Vector3 targetPos, float angle)
    {
        // xz平面の距離を計算。
        Vector2 pos0 = new Vector2(shootPos.x, shootPos.z);
        Vector2 pos1 = new Vector2(targetPos.x, targetPos.z);
        float distance = Vector2.Distance(pos0, pos1);

        float x = distance;
        float g = Physics.gravity.y;
        float y0 = shootPos.y;
        float y = targetPos.y;
        float v0 = 0;

        for (float a = angle; a < 90; a += 5)
        {
            float rad = a * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float tan = Mathf.Tan(rad);
            float v0Square = g * x * x / (2 * cos * cos * (y - y0 - x * tan));

            if (v0Square <= 0.0f) continue;

            v0 = Mathf.Sqrt(v0Square);
            break;
        }
        
        return v0;
    }

    private Vector3 ConvertVectorToVector3(float i_v0, float i_angle, Vector3 shootPos, Vector3 targetPos)
    {
        //Vector3 startPos = m_shootPoint.transform.position;
        //Vector3 targetPos = i_targetPosition;
        shootPos.y = 0.0f;
        targetPos.y = 0.0f;

        Vector3 dir = (targetPos - shootPos).normalized;
        Quaternion yawRot = Quaternion.FromToRotation(Vector3.right, dir);
        Vector3 vec = i_v0 * Vector3.right;

        vec = yawRot * Quaternion.AngleAxis(i_angle, Vector3.forward) * vec;

        return vec;
    }

    //private void InstantiateShootObject(Vector3 i_shootVector)
    //{
    //    if (m_shootObject == null)
    //    {
    //        throw new System.NullReferenceException("m_shootObject");
    //    }

    //    if (m_shootPoint == null)
    //    {
    //        throw new System.NullReferenceException("m_shootPoint");
    //    }

    //    var obj = Instantiate<GameObject>(m_shootObject, m_shootPoint.position, Quaternion.identity);
    //    var rigidbody = obj.AddComponent<Rigidbody>();

    //    // 速さベクトルのままAddForce()を渡してはいけないぞ。力(速さ×重さ)に変換するんだ
    //    Vector3 force = i_shootVector * rigidbody.mass;

    //    rigidbody.AddForce(force, ForceMode.Impulse);
    //}
}
