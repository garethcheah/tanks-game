using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5.0f;   // Movement speed
    public float turnSpeed = 50.0f;   // Rotation speed

    private Rigidbody _rb;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Get the Rigidbody component
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (IsOwner)
        {
            OnMove();
            OnRotate();
        }
    }

    private void OnMove()
    {
        // Get the input for movement (W/S or Up/Down)
        float moveInput = Input.GetAxis("Vertical");

        // Calculate the movement vector (forward or backward)
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.deltaTime;

        // Apply the movement to the Rigidbody (with MovePosition to handle physics)
        _rb.MovePosition(_rb.position + move);
    }

    private void OnRotate()
    {
        // Get the input for rotation (A/D or Left/Right)
        float turnInput = Input.GetAxis("Horizontal");

        // Calculate the rotation (around the Y-axis)
        float turn = turnInput * turnSpeed * Time.deltaTime;

        // Apply the rotation
        _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }
}
