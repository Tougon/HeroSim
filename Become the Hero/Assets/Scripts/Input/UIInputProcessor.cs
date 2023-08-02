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
    }
}
