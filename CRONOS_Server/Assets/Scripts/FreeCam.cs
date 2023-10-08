using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCam : MonoBehaviour
{
    public GameObject camParent;
    public CharacterController controller;
    public float speed = 10f;
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private bool canRotate = false;

    private void Start()
    {
        xRotation = transform.eulerAngles.x;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = Input.GetAxis("Height");
        
        Vector3 direction = camParent.transform.right * x + camParent.transform.forward * z;
        controller.Move(direction * speed * Time.deltaTime);
        camParent.transform.position = new Vector3(camParent.transform.position.x, camParent.transform.position.y + y * (speed * 10) * Time.deltaTime, camParent.transform.position.z );

        if (Input.GetMouseButtonDown(1))
            SetLockState(true);
        if (Input.GetMouseButtonUp(1))
            SetLockState(false);

        if (canRotate)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            camParent.transform.Rotate(Vector3.up * mouseX);
        }
    }

    private void SetLockState(bool cursorLock)
    {
        if (cursorLock)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !cursorLock;
        canRotate = cursorLock;
    }
}
