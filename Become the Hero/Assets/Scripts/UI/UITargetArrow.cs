using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITargetArrow : MonoBehaviour
{
    public Image image;
    public TweenSystem tweenSystem;

    private void Awake()
    {
        tweenSystem = GetComponentInChildren<TweenSystem>();
    }
}
