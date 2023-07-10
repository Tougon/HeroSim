using ScriptableObjectArchitecture;
using ToUI;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputProcessor : MonoBehaviour
{
    private PlayerInput PlayerInput;
    private InputAction MovementAction;


    void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        BindInputActions();
    }


    private void BindInputActions()
    {
        MovementAction = PlayerInput.actions["Movement"];
    }


    private void Update()
    {
        UpdateInputValues();
    }


    private void UpdateInputValues()
    {
        VariableManager.Instance.SetVector2VariableValue(VariableConstants.UI_INPUT_VALUE, 
            MovementAction.ReadValue<Vector2>());
    }
}
