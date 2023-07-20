using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITransition : MonoBehaviour
{
    RawImage image;
    Image hole;

    public float inScale = 1;
    public float outScale = 2500;

    public float speed = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        Rect rect = new Rect(0, 0, 1, 1);

        image = GetComponent<RawImage>();
        hole = GetComponentInChildren<Image>();
        hole.enabled = false;

        EventManager.Instance.GetGameEvent(EventConstants.BEGIN_TRANSITION_IN).AddListener(IrisOut);
        EventManager.Instance.GetGameEvent(EventConstants.BEGIN_TRANSITION_OUT).AddListener(IrisIn);
    }


    void Start()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.BEGIN_TRANSITION_IN);
    }


    public void IrisOut()
    {
        hole.enabled = false;
        image.enabled = true;

        float old = (-outScale / 2.0f) + 0.5f;
        image.uvRect = new Rect(old, old, outScale, outScale);

        float direction = (-inScale / 2.0f) + 0.5f;
        DOTween.To(() => image.uvRect, x => image.uvRect = x, new Rect(direction, direction, inScale, inScale), speed).SetEase(Ease.OutQuint)
            .OnComplete(IrisOutEnd);
    }


    public void IrisOutEnd()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.ON_TRANSITION_IN_COMPLETE);
        AllowInput();
    }


    public void IrisIn()
    {
        hole.enabled = false;
        image.enabled = true;

        float old = (-inScale / 2.0f) + 0.5f;
        image.uvRect = new Rect(old, old, inScale, inScale);

        float direction = (-outScale / 2.0f) + 0.5f;
        DOTween.To(() => image.uvRect, x => image.uvRect = x, new Rect(direction, direction, outScale, outScale), speed).SetEase(Ease.InQuint)
            .OnComplete(IrisInEnd);
    }


    public void IrisInEnd()
    {
        EventManager.Instance.RaiseGameEvent(EventConstants.ON_TRANSITION_OUT_COMPLETE);
        FillHole();
    }


    public void AllowInput()
    {
        image.enabled = false;
    }


    public void FillHole()
    {
        hole.enabled = true;
    }


    private void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.BEGIN_TRANSITION_IN).RemoveListener(IrisOut);
        EventManager.Instance.GetGameEvent(EventConstants.BEGIN_TRANSITION_OUT).RemoveListener(IrisIn);
    }
}
