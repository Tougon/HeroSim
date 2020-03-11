using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Background : MonoBehaviour
{
    private List<SpriteRenderer> bgLayers = new List<SpriteRenderer>();
    private Color bgDefault;
    private Color target;


    // Awake is called before the first frame update
    void Awake()
    {
        bgLayers.AddRange(GetComponentsInChildren<SpriteRenderer>());
        bgDefault = bgLayers[0].color;

        EventManager.Instance.GetVector2Event(EventConstants.START_BACKGROUND_FADE).AddListener(SetBGFade);
        EventManager.Instance.GetVector3Event(EventConstants.SET_BACKGROUND_COLOR).AddListener(SetTargetColor);
        EventManager.Instance.GetGameEvent(EventConstants.RESET_BACKGROUND_COLOR).AddListener(ResetTargetColor);
    }


    public void ResetTargetColor()
    {
        SetTargetColor(new Vector3(bgDefault.r, bgDefault.g, bgDefault.b));
    }
    

    public void SetTargetColor(Vector3 color)
    {
        foreach (SpriteRenderer sr in bgLayers)
        {
            target = new Color(color.x, color.y, color.z, sr.color.a);
            sr.color = target;
        }
    }

    
    public void SetBGFade(Vector2 param)
    {
        foreach (SpriteRenderer sr in bgLayers)
            sr.DOFade(param.x, param.y);
    }


    private void OnDestroy()
    {
        EventManager.Instance.GetVector2Event(EventConstants.START_BACKGROUND_FADE).RemoveListener(SetBGFade);
        EventManager.Instance.GetVector3Event(EventConstants.SET_BACKGROUND_COLOR).RemoveListener(SetTargetColor);
        EventManager.Instance.GetGameEvent(EventConstants.RESET_BACKGROUND_COLOR).RemoveListener(ResetTargetColor);
    }
}
