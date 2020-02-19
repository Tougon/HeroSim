using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStatusScreenStatDisplay : MonoBehaviour
{
    [SerializeField]
    private Image barHP;
    [SerializeField]
    private Image barMP;
    [SerializeField][Tooltip("Parent object for ATK/DEF/SPD display")]
    private RectTransform statBase;

    [SerializeField]
    private TextMeshProUGUI hp;
    [SerializeField]
    private TextMeshProUGUI mp;
    [SerializeField]
    private TextMeshProUGUI atk;
    [SerializeField]
    private TextMeshProUGUI def;
    [SerializeField]
    private TextMeshProUGUI spd;

    private bool displayingStats = false;


    public void SetStatDisplay(bool val)
    {
        displayingStats = val;

        hp.enabled = val;
        mp.enabled = val;
        statBase.gameObject.SetActive(val);
    }


    public void UpdateStats(EntityController ec)
    {
        barHP.fillAmount = (float)ec.GetCurrentHP() / (float)ec.maxHP;
        barMP.fillAmount = (float)ec.GetCurrentMP() / (float)ec.maxMP;

        if (displayingStats)
        {
            hp.text = ec.GetCurrentHP() + "/" + ec.maxHP;
            mp.text = ec.GetCurrentMP() + "/" + ec.maxMP;

            atk.text = ((int)((float)ec.GetAttack() * ec.GetAttackModifier())).ToString();
            def.text = ((int)((float)ec.GetDefense() * ec.GetDefenseModifier())).ToString();
            spd.text = ((int)((float)ec.GetSpeed() * ec.GetSpeedModifier())).ToString();
        }
    }
}
