using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ToUI;
using DG.Tweening;


public class EntityControllerUI : MonoBehaviour
{
    private const float FADE_IN_SPEED = 0.25f;
    private const float BAR_SUBTRACT_SPEED = 0.4f;

    [SerializeField]
    private Image background;
    [SerializeField]
    private UIBar barHP;
    [SerializeField]
    private UIBar barMP;

    protected float entityHP;
    protected float entityMP;
    public bool visible { get => barMP.visible || barHP.visible; }

    private IEnumerator fadeOut;
    private IEnumerator currentAnimHP;
    private IEnumerator currentAnimMP;

    private EntityController current;


    void Awake()
    {
        EventManager.Instance.GetGameEvent(EventConstants.HIDE_UI).AddListener(HideUI);
    }


    public void ResetUI(EntityController ec, int hp, int mp)
    {
        if (barHP == null) return;
        current = ec;

        barHP.maxValue = hp;
        barMP.maxValue = mp;
        entityHP = hp;
        entityMP = mp;

        barHP.SetValueImmediate(hp);
        barMP.SetValueImmediate(mp);
    }


    public void ShowUI()
    {
        barHP?.ShowUI();
        barMP?.ShowUI();
    }


    public void HideUI()
    {
        barHP.HideUI();
        barMP.HideUI();
    }


    public void ChangeHP(int dif)
    {
        entityHP -= dif;
        entityHP = Mathf.Clamp(entityHP, 0, barHP.maxValue);

        currentAnimHP = SetHPBarFillAmount(entityHP);
        StartCoroutine(currentAnimHP);
    }


    private IEnumerator SetHPBarFillAmount(float val)
    {
        barHP?.ShowUI();

        while (!barHP.visible)
            yield return null;
        
        barHP.SetValue(val);
    }


    public void ChangeMP(int dif)
    {
        entityMP += dif;
        entityMP = Mathf.Clamp(entityMP, 0, barMP.maxValue);

        currentAnimMP = SetMPBarFillAmount(entityMP);
        StartCoroutine(currentAnimMP);
    }


    private IEnumerator SetMPBarFillAmount(float val)
    {
        barMP?.ShowUI();

        while (!barMP.visible)
            yield return null;

        barMP.SetValue(val);
    }


    void OnDestroy()
    {
        EventManager.Instance.GetGameEvent(EventConstants.HIDE_UI).RemoveListener(HideUI);
    }
}
