using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using ToUI;

public class UIEventReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // Assign listeners here.
        // TODO: Dictionary of menus so we can set up proper listening (event => show UI action)
    }


    // Update is called once per frame
    void Update()
    {
        var movementValue = 
            VariableManager.Instance.GetVector2VariableValue(VariableConstants.UI_INPUT_VALUE);

        UIScreenQueue.Instance.CurrentScreen?.OnMovementUpdate(movementValue);
    }


    private void OnDestroy()
    {
        
    }
}
