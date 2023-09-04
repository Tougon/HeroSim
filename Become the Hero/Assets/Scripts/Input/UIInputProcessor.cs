using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIInputProcessor : MonoBehaviour
{
    private PlayerInput PlayerInput;
    private InputAction MovementAction;
    private InputAction ConfirmAction;
    private InputAction CancelAction;
    private InputAction Aux1Action;
    private InputAction Aux2Action;

    void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        BindInputActions();
    }


    private void BindInputActions()
    {
        MovementAction = PlayerInput.actions["Movement"];
        ConfirmAction = PlayerInput.actions["Confirm"];
        CancelAction = PlayerInput.actions["Cancel"];
        Aux1Action = PlayerInput.actions["Aux 1"];
        Aux2Action = PlayerInput.actions["Aux 2"];
    }


    private void Update()
    {
        UpdateInputValues();
    }


    private void UpdateInputValues()
    {
        VariableManager.Instance.SetVector2VariableValue(VariableConstants.UI_INPUT_VALUE, 
            MovementAction.ReadValue<Vector2>());

        if (ConfirmAction.WasPressedThisFrame())
        {
            ToUI.UIScreenQueue.Instance?.CurrentScreen?.OnConfirmPressed();
        }
        else if (CancelAction.WasPressedThisFrame())
        {
            ToUI.UIScreenQueue.Instance?.CurrentScreen?.OnCancelPressed();
        }
        else if (Aux1Action.WasPressedThisFrame())
        {
            ToUI.UIScreenQueue.Instance?.CurrentScreen?.OnAux1Pressed();
        }
        else if (Aux2Action.WasPressedThisFrame())
        {
            ToUI.UIScreenQueue.Instance?.CurrentScreen?.OnAux2Pressed();
        }
    }
}
