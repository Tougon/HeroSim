using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerInfoDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerName;
    [SerializeField]
    private TextMeshProUGUI playerHP;
    [SerializeField]
    private Image playerHPBar;
    [SerializeField]
    private TextMeshProUGUI playerMP;
    [SerializeField]
    private Image playerMPBar;


    void Awake()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_PLAYER_INFO).
            AddListener(UpdatePlayerInfo);
    }


    private void UpdatePlayerInfo(EntityController controller)
    {
        playerName.text = controller.GetEntity().vals.GetEntityName();
        playerHP.text = $"<color=#FFFF00>{controller.GetCurrentHP()}</color>/{controller.maxHP}";
        playerMP.text = $"<color=#FFFF00>{controller.GetCurrentMP()}</color>/{controller.maxMP}";
        playerHPBar.fillAmount = (float)controller.GetCurrentHP() / (float)controller.maxHP;
        playerMPBar.fillAmount = (float)controller.GetCurrentMP() / (float)controller.maxMP;
    }


    void OnDestroy()
    {
        EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_PLAYER_INFO).
            RemoveListener(UpdatePlayerInfo);
    }
}
