using ToUI;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputProcessor : MonoBehaviour
{
    private Vector2 MovementValue;

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
        MovementValue = MovementAction.ReadValue<Vector2>();
    }
}
