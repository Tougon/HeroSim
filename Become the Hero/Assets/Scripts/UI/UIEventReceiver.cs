using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using ToUI;
using System;

public enum ScreenConstants
{
    TextDisplay,
    ActionMenu,
    SpellMenu,
    TargetMenu,
}


/// <summary>
/// Class representing an arbitrary open or close call to a screen including a callback
/// This is used due to a restriction with the scriptable object architecture
/// If I started this in the year 2023, I probably would have just used delegates
/// </summary>
public struct UIOpenCloseCall
{
    public string MenuName;
    public UIScreen.ScreenDelegateSignature Callback;
}


public class UIEventReceiver : MonoBehaviour
{
    [Serializable]
    public struct UIScreenData
    {
        public ScreenConstants ScreenName;
        public UIScreen Screen;
    }

    [SerializeField]
    private List<UIScreenData> _data;
    private Dictionary<string, UIScreen> ScreenMap;


    // Start is called before the first frame update
    void Awake()
    {
        ScreenMap = new Dictionary<string, UIScreen>(_data.Count);

        foreach(var data in _data)
        {
            if (!ScreenMap.ContainsKey(data.ScreenName.ToString()))
                ScreenMap.Add(data.ScreenName.ToString(), data.Screen);
        }

        // Assign listeners here.
        EventManager.Instance.GetUIGameEvent(EventConstants.SHOW_SCREEN).AddListener(OnShowScreen);
        EventManager.Instance.GetUIGameEvent(EventConstants.HIDE_SCREEN).AddListener(OnHideScreen);
        EventManager.Instance.GetUIGameEvent(EventConstants.HIDE_ALL_SCREENS).AddListener(OnHideAllScreens);
    }


    // Update is called once per frame
    void Update()
    {
        var movementValue = 
            VariableManager.Instance.GetVector2VariableValue(VariableConstants.UI_INPUT_VALUE);

        UIScreenQueue.Instance?.CurrentScreen?.OnMovementUpdate(movementValue);
    }


    private void OnShowScreen(UIOpenCloseCall screenCall)
    {
        if (ScreenMap.TryGetValue(screenCall.MenuName, out var screen))
        {
            screen.OnScreenShowDelegate += screenCall.Callback;
            screen.Show();
        }
    }


    private void OnHideScreen(UIOpenCloseCall screenCall)
    {
        if (ScreenMap.TryGetValue(screenCall.MenuName, out var screen))
        {
            screen.OnScreenHideDelegate += screenCall.Callback;
            screen.Hide();
        }
    }


    private void OnHideAllScreens(UIOpenCloseCall screenCall)
    {
        bool bRanDelegate = false;

        foreach(var screen in ScreenMap.Values)
        {
            if (screen.Showing && !bRanDelegate)
            {
                screen.OnScreenHideDelegate += screenCall.Callback;
                bRanDelegate = true;

                screen.Hide();
            }
        }

        if(!bRanDelegate)
        {
            screenCall.Callback.Invoke();
        }
    }


    public void OpenMenu(ScreenConstants NewScreen)
    {
        if (ScreenMap.TryGetValue(NewScreen.ToString(), out var screen))
        {
            screen.Show();
        }
    }


    public void SwitchMenus(ScreenConstants OldScreen, ScreenConstants NewScreen)
    {
        if (ScreenMap.TryGetValue(OldScreen.ToString(), out var screen))
        {
            screen.OnScreenHideDelegate += () => OpenMenu(NewScreen);
            screen.Hide();
        }
    }


    private void OnDestroy()
    {
        EventManager.Instance.GetUIGameEvent(EventConstants.SHOW_SCREEN).RemoveListener(OnShowScreen);
        EventManager.Instance.GetUIGameEvent(EventConstants.HIDE_SCREEN).RemoveListener(OnHideScreen);
        EventManager.Instance.GetUIGameEvent(EventConstants.HIDE_ALL_SCREENS).RemoveListener(OnHideAllScreens);
    }
}
