using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputProcessor : MonoBehaviour
{
    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>());
    }
}
