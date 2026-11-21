using UnityEngine;
using UnityEngine.InputSystem;

public class InputCheck : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("마우스 클릭 들어옴");
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Debug.Log("터치 들어옴");
        }
    }
}