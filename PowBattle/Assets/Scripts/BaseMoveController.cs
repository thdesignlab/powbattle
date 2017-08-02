using UnityEngine;
using System.Collections;

public abstract class BaseMoveController : MonoBehaviour
{
    protected Transform myTran;

    protected virtual void Awake()
    {
        myTran = transform;
    }

    //移動

    //回転
}
