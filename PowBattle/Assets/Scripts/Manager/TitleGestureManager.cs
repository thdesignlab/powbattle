using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class TitleGestureManager : GestureManager
{

    protected override void Tap(Vector2 screenPoint)
    {
        GetComponent<TitleManager>().StartGame();
    }
}
