﻿using UnityEngine;
using System.Collections;

public class Uv_s : MonoBehaviour
{
    [SerializeField]
    private float scrollSpeedX = 0.1f;
    [SerializeField]
    private float scrollSpeedY = 0.1f;

    private Renderer ren;
    private Vector2 defaultOffset = Vector2.zero;

    void Update()
    {
        float x = Mathf.Repeat(Time.time * scrollSpeedX, 1);
        float y = Mathf.Repeat(Time.time * scrollSpeedY, 1);
        SetOffset(new Vector2(x, y));
    }

    private void SetOffset(Vector2 offset)
    {
        if (ren == null) return;
        ren.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }

    private void OnEnable()
    {
        ren = GetComponent<Renderer>();
        //defaultOffset = ren.sharedMaterial.GetTextureOffset("_MainTex");
    }

    private void OnDisable()
    {
        SetOffset(defaultOffset);
    }
}