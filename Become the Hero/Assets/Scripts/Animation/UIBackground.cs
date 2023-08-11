using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using DG.Tweening;

public class UIBackground : MonoBehaviour
{
    [SerializeField] RawImage backgroundImage;
    Material backgroundMat;

    void Awake()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).AddListener(OnBattleBegin);

        EventManager.Instance.GetVector3Event(EventConstants.SET_BACKGROUND_COLOR).AddListener(SetTargetColor);
        EventManager.Instance.GetGameEvent(EventConstants.RESET_BACKGROUND_COLOR).AddListener(ResetTargetColor);
    }


    void OnBattleBegin()
    {
        VariableManager.Instance.SetFloatVariableValue(VariableConstants.BACKGROUND_FADE_TIME, 0.5f);
        backgroundMat = backgroundImage.material;
        backgroundImage.material = Instantiate(backgroundMat);
    }


    public void ResetTargetColor()
    {
        SetTargetColor(Vector3.one);
    }


    public void SetTargetColor(Vector3 color)
    {
        float time = VariableManager.Instance.GetFloatVariableValue(VariableConstants.BACKGROUND_FADE_TIME);
        backgroundImage.material.DOColor(new Color(color.x, color.y, color.z, 1), "_Color", time);
    }


    void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.ON_BATTLE_BEGIN).RemoveListener(OnBattleBegin);

        EventManager.Instance.GetVector3Event(EventConstants.SET_BACKGROUND_COLOR).RemoveListener(SetTargetColor);
        EventManager.Instance.GetGameEvent(EventConstants.RESET_BACKGROUND_COLOR).RemoveListener(ResetTargetColor);
    }
}
