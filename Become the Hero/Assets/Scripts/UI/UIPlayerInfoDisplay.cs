using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.XR;

public class UIPlayerInfoDisplay : MonoBehaviour
{
    private enum EventType { Player, Enemy }

    [SerializeField]
    private TextMeshProUGUI playerName;
    [SerializeField]
    private TextMeshProUGUI playerHP;
    [SerializeField]
    private TextMeshProUGUI playerHPLabel;
    [SerializeField]
    private Image playerHPBar;
    [SerializeField]
    private TextMeshProUGUI playerMP;
    [SerializeField]
    private TextMeshProUGUI playerMPLabel;
    [SerializeField]
    private Image playerMPBar;
    [SerializeField]
    private EventType eventType;
    [Header("Highlight Properties")]
    [SerializeField]
    private Color highlightColor = new Color(1, 1, 1, 1);


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
            EventManager.Instance.GetGameEvent(EventConstants.INITIALIZE_ALL_ENEMY_INFO).
                AddListener(ShowAll);
        }
    }


    public void ShowAll()
    {
        // TODO: Localize
        playerName.text = "All";

        playerHPBar.transform.parent.gameObject.SetActive(false);
        playerMPBar.transform.parent.gameObject.SetActive(false);
    }


    public void UpdatePlayerInfo(EntityController controller)
    {
        playerName.text = controller.GetEntity().vals.GetEntityName();

        playerHPBar.transform.parent.gameObject.SetActive(true);
        playerHPBar.fillAmount = (float)controller.GetCurrentHP() / (float)controller.maxHP;
        playerHP.text = (controller is PlayerController) ? 
            $"<color=#FFFF00>{controller.GetCurrentHP()}</color>/{controller.maxHP}" : "";
        playerHPLabel.enabled = (controller is PlayerController);

        if(controller is PlayerController)
        {
            playerMPBar.transform.parent.gameObject.SetActive(true);
            playerMPBar.fillAmount = (float)controller.GetCurrentMP() / (float)controller.maxMP;
            playerMP.text = $"<color=#FFFF00>{controller.GetCurrentMP()}</color>/{controller.maxMP}";
        }
        else
        {
            playerMPBar.transform.parent.gameObject.SetActive(false);
        }
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
            EventManager.Instance.GetGameEvent(EventConstants.INITIALIZE_ALL_ENEMY_INFO).
                RemoveListener(ShowAll);
        }
    }
}
