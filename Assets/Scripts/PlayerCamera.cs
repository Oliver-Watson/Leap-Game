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
        //transform.Rotate(Vector3.up, mouseInput.x * sensitivity * Time.deltaTime);

        //pitch -= mouseInput.y * sensitivity * Time.deltaTime;
        //pitch = Mathf.Clamp(pitch, -90f, 90f);
        //transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, 0f);

        ////Debug.Log("Mouse input:" + mouseInput.x);
        ///

        rotationX += mouseInput.x * sensitivity * Time.deltaTime;
        rotationY -= mouseInput.y * sensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        transform.forward = new Vector3(rotationX, 0f, 0f);

        //transform.Rotate(transform.up, mouseX);

        //transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 0), Camera.main.transform.up);

        orientation.rotation = Quaternion.Euler(0f, rotationX, 0f);
        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);


        //transform.rotation *= Quaternion.Euler(0f, mouseInput.y * sensitivity * Time.deltaTime, 0f);

        //pitch -= mouseInput.y * sensitivity * Time.deltaTime;
        //pitch = Mathf.Clamp(pitch, -90f, 90f);


        //transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, mouseX, 0f);

        Debug.Log("Mouse input: " + mouseInput.x);
    }
}
