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

    // Used to offset position and height
    private float heightBase = 0.55f;
    private float heightDifference = 0.075f;
    private float textHPPos = 0.06f;
    private float noTextHPPos = 0.07f;
    private float textMPPos = -0.15f;
    private float noTextMPPos = -0.08f;

    protected float entityHP;
    protected float entityMP;
    public bool visible { get; private set; }
    private bool hpAnimating;
    private bool mpAnimating;
    private bool showText;

    private IEnumerator fadeOut;
    private IEnumerator currentAnimHP;
    private IEnumerator currentAnimMP;

    private EntityController current;


    void Awake()
    {
        visible = false;

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

        showText = false;
        RectTransform rt = background.GetComponent<RectTransform>();
        rt.DOAnchorPosY(heightDifference, 0);
        rt.DOSizeDelta(new Vector2(rt.sizeDelta.x, heightBase - (heightDifference * 2.0f)), 0);

        barHP.transform.parent.GetComponent<RectTransform>().DOAnchorPosY(noTextHPPos, 0);
        barMP.transform.parent.GetComponent<RectTransform>().DOAnchorPosY(noTextMPPos, 0);
    }


    public void ShowUI()
    {
        if (visible)
            return;

        visible = true;
        barHP?.ShowUI();
        barMP?.ShowUI();
    }


    public void HideUI()
    {
        if (!visible || fadeOut != null)
            return;

        if (fadeOut != null)
            StopCoroutine(fadeOut);

        fadeOut = HideUIAfterAnim();
        StartCoroutine(fadeOut);
    }


    private IEnumerator HideUIAfterAnim()
    {
        barHP?.HideUI();
        barMP?.HideUI();

        while (hpAnimating || mpAnimating)
            yield return null;

        visible = false;
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
