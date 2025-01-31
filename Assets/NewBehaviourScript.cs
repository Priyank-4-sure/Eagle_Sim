using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f; // Mouse sensitivity
    public Transform playerBody;         // Reference to the player's body

    private float xRotation = 0f;        // To keep track of vertical rotation

    void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust vertical rotation and clamp to prevent flipping
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -20f, 50f);

        // Apply vertical rotation to the camera (local to the player)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player horizontally based on mouseX
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
