﻿using UnityEngine;
using System.Collections;

public class LookingCameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private Transform cameraTran;
    //private Quaternion defaultRotation;
    //private bool isInCamera;

    void Start ()
    {
        if (target == null) target = transform;
        cameraTran = Camera.main.transform;
        //defaultRotation = target.rotation;
    }

    //void Update ()
    //{
    //    if (!isInCamera) return;
    //    target.rotation = cameraTran.rotation * defaultRotation;
    //}

    //void OnBecameInvisible()
    //{
    //    Debug.Log("Invisible");
    //    isInCamera = false;
    //}
    //void OnBecameVisible()
    //{
    //    Debug.Log("Visible");
    //    isInCamera = true;
    //}
    void OnWillRenderObject()
    {
        target.rotation = cameraTran.rotation;
    }

}
