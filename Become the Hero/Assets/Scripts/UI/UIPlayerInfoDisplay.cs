using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerInfoDisplay : MonoBehaviour
{
    private enum EventType { Player, Enemy }

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
    [SerializeField]
    private EventType eventType;


    void Awake()
    {
        if(eventType == EventType.Player)
        {
            EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_PLAYER_INFO).
                AddListener(UpdatePlayerInfo);
        }
        else
        {
            EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_SELECTED_ENEMY_INFO).
                AddListener(UpdatePlayerInfo);
        }
    }


    public void UpdatePlayerInfo(EntityController controller)
    {
        playerName.text = controller.GetEntity().vals.GetEntityName();
        playerHPBar.fillAmount = (float)controller.GetCurrentHP() / (float)controller.maxHP;

        if(controller is PlayerController)
        {
            playerHP.text = $"<color=#FFFF00>{controller.GetCurrentHP()}</color>/{controller.maxHP}";
        }
        else
        {
            playerHP.text = "";
        }


        playerMPBar.fillAmount = (float)controller.GetCurrentMP() / (float)controller.maxMP;
        playerMP.text = $"<color=#FFFF00>{controller.GetCurrentMP()}</color>/{controller.maxMP}";
    }


    void OnDestroy()
    {
        if (eventType == EventType.Player)
        {
            EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_PLAYER_INFO).
                RemoveListener(UpdatePlayerInfo);
        }
        else
        {
            EventManager.Instance.GetEntityControllerEvent(EventConstants.INITIALIZE_SELECTED_ENEMY_INFO).
                RemoveListener(UpdatePlayerInfo);
        }
    }
}
