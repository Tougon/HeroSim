using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class EntityControllerUI : MonoBehaviour
{
    private const float FADE_IN_SPEED = 0.25f;
    private const float BAR_SUBTRACT_SPEED = 0.4f;

    [SerializeField]
    private Image background;
    [SerializeField]
    private Image barHP;
    [SerializeField]
    private TextMeshProUGUI textHP;
    [SerializeField]
    private Image barMP;
    [SerializeField]
    private TextMeshProUGUI textMP;

    // Used to offset position and height
    private float heightBase = 0.5f;
    private float heightDifference = 0.05f;
    private float textHPPos = 0.06f;
    private float noTextHPPos = 0.07f;
    private float textMPPos = -0.15f;
    private float noTextMPPos = -0.08f;

    protected float maxHP;
    protected float entityHP;
    protected float maxMP;
    protected float entityMP;

    private CanvasGroup group;
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
        group = GetComponent<CanvasGroup>();
        group.alpha = 0;
        visible = false;

        EventManager.Instance.GetGameEvent(EventConstants.ON_TURN_BEGIN).AddListener(ShowUI);
    }


    public void ResetUI(EntityController ec, int hp, int mp)
    {
        current = ec;

        maxHP = hp;
        maxMP = mp;
        entityHP = hp;
        entityMP = mp;

        barHP.fillAmount = 1;
        barMP.fillAmount = 1;

        textHP.text = "" + maxHP + "/" + maxHP;
        textMP.text = "" + maxMP + "/" + maxMP;

        showText = false;
        RectTransform rt = background.GetComponent<RectTransform>();
        rt.DOAnchorPosY(heightDifference, 0);
        rt.DOSizeDelta(new Vector2(rt.sizeDelta.x, heightBase - (heightDifference * 2.0f)), 0);

        barHP.GetComponent<RectTransform>().DOAnchorPosY(noTextHPPos, 0);
        textHP.color = Color.clear;
        barMP.GetComponent<RectTransform>().DOAnchorPosY(noTextMPPos, 0);
        textMP.color = Color.clear;
    }


    public void ShowText()
    {
        showText = true;
        RectTransform rt = background.GetComponent<RectTransform>();
        rt.DOAnchorPosY(0, 0.3f);
        rt.DOSizeDelta(new Vector2(rt.sizeDelta.x, heightBase), 0.3f);

        barHP.GetComponent<RectTransform>().DOAnchorPosY(textHPPos, 0.3f);
        textHP.DOColor(Color.white, 0.3f);
        barMP.GetComponent<RectTransform>().DOAnchorPosY(textMPPos, 0.3f);
        textMP.DOColor(Color.white, 0.3f);
    }


    public void ShowUI()
    {
        if (visible)
            return;

        group.DOFade(1, FADE_IN_SPEED).OnComplete(SetUIVisible);
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
        while (hpAnimating || mpAnimating)
            yield return null;


        group.DOFade(0, FADE_IN_SPEED).OnComplete(SetUIInvisible);
    }


    public void ChangeHP(int dif)
    {
        entityHP -= dif;
        entityHP = Mathf.Clamp(entityHP, 0, maxHP);
        float val = ((float)entityHP) / ((float)maxHP);

        currentAnimHP = SetHPBarFillAmount(val);
        StartCoroutine(currentAnimHP);
    }


    private IEnumerator SetHPBarFillAmount(float val)
    {
        ShowUI();

        while (!visible)
            yield return null;

        hpAnimating = true;
        barHP.DOFillAmount(val, BAR_SUBTRACT_SPEED).OnComplete(OnHPAnimateFinish);

        while (hpAnimating)
        {
            int percent = Mathf.RoundToInt(barHP.fillAmount * (float)maxHP);
            textHP.text = "" + percent + "/" + maxHP;
            yield return null;
        }
        
        textHP.text = "" + current.GetCurrentHP() + "/" + maxHP;
    }


    public void ChangeMP(int dif)
    {
        entityMP += dif;
        entityMP = Mathf.Clamp(entityMP, 0, maxMP);
        float val = ((float)entityMP) / ((float)maxMP);

        currentAnimMP = SetMPBarFillAmount(val);
        StartCoroutine(currentAnimMP);
    }


    private IEnumerator SetMPBarFillAmount(float val)
    {
        ShowUI();

        while (!visible)
            yield return null;

        mpAnimating = true;
        barMP.DOFillAmount(val, BAR_SUBTRACT_SPEED).OnComplete(OnMPAnimateFinish);

        while (mpAnimating)
        {
            int percent = Mathf.RoundToInt(barMP.fillAmount * (float)maxMP);
            textMP.text = "" + percent + "/" + maxMP;
            yield return null;
        }

        textMP.text = "" + current.GetCurrentMP() + "/" + maxMP;
    }


    private void SetUIVisible() { visible = true; }
    private void SetUIInvisible() { visible = false; fadeOut = null; }
    private void OnHPAnimateFinish() { hpAnimating = false; }
    private void OnMPAnimateFinish() { mpAnimating = false; }
}
