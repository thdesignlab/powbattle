using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : BaseMoveController
{
    protected bool isMine;
    protected NavMeshAgent agent;
    protected string targetTag;

    protected Transform targetTran;

    [SerializeField]
    protected int maxHP = 1000;
    protected int nowHP;
    [SerializeField]
    protected int speed = 30;
    [SerializeField]
    protected int defence = 0;
    [SerializeField]
    protected int attackRange = 10;


    protected void Start()
    {
        isMine = (tag == Common.CO.TAG_UNIT) ? true : false;

        targetTag = (isMine) ? Common.CO.TAG_ENEMY : Common.CO.TAG_UNIT;
        agent = GetComponent<NavMeshAgent>();

        Search();
    }

    protected void Update()
    {
        if (targetTran == null)
        {
            Search();
            return;
        }

        if (Vector3.Distance(myTran.position, targetTran.position) <= attackRange)
        {
            //攻撃
            ObjectController targetObjCon = targetTran.GetComponent<ObjectController>();
            targetObjCon.DestoryObject();
        }
        else
        {
            //移動
            agent.destination = targetTran.position;
        }

    }

    //ターゲットサーチ
    protected void Search()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        if (targets.Length == 0) return;

        int index = 0;
        float distance = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            float tmpDistance = Vector3.Distance(myTran.position, targets[i].transform.position);
            if (i == 0 || tmpDistance < distance)
            {
                distance = tmpDistance;
                index = i;
            }
        }
        targetTran = targets[index].transform;
    }

    //移動

    //攻撃



}
