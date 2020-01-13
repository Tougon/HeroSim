using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITransition : MonoBehaviour
{
    RawImage image;

    public float inScale = 1;
    public float outScale = 2500;

    // Start is called before the first frame update
    void Start()
    {
        Rect rect = new Rect(0, 0, 1, 1);

        image = GetComponent<RawImage>();

        float amt = (-outScale / 2.0f) + 0.5f;
        float amt2 = (-inScale / 2.0f) + 0.5f;
        DOTween.To(()=> image.uvRect, x => image.uvRect = x, new Rect(amt, amt, outScale, outScale), 0.75f).SetEase(Ease.InQuint);
        //image.uvRect = new Rect(amt, amt, outScale, outScale);
        //DOTween.To(() => image.uvRect, x => image.uvRect = x, new Rect(amt2, amt2, inScale, inScale), 1.5f).SetEase(Ease.OutQuint);

        StartCoroutine(Teeeeetas(amt2));
    }

    public IEnumerator Teeeeetas(float amt2)
    {
        yield return new WaitForSeconds(0.9f);
        DOTween.To(() => image.uvRect, x => image.uvRect = x, new Rect(amt2, amt2, inScale, inScale), 0.75f).SetEase(Ease.OutQuint);
    }
}
