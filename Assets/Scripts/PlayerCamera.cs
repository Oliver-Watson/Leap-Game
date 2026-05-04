using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity;
    [SerializeField] private Transform orientation;
    private Vector2 mouseInput;
    private float rotationX;
    private float rotationY;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseInput();
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }

    private void HandleMouseInput()
    {
        rotationX += mouseInput.x * sensitivity * Time.deltaTime;
        rotationY -= mouseInput.y * sensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        orientation.rotation = Quaternion.Euler(0f, rotationX, 0f);
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        Debug.Log("Mouse input: " + mouseInput.x);
    }
}
