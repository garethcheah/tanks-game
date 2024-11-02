using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5.0f;   // Movement speed
    public float turnSpeed = 50.0f;   // Rotation speed

    private Rigidbody _rb;
    private float _horizontal;
    private float _vertical;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Get the Rigidbody component
        _rb = GetComponent<Rigidbody>();
    }

    [ServerRpc]
    public void OnMoveServerRpc(float horizontal, float vertical)
    {
        _horizontal = horizontal;
        _vertical = vertical;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (IsServer && IsLocalPlayer)
        {
            _horizontal = Input.GetAxis("Horizontal");
            _vertical = Input.GetAxis("Vertical");
        }
        else if (IsClient && IsLocalPlayer)
        {
            //Client request to the server to move their player game object
            OnMoveServerRpc(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }

    private void FixedUpdate()
    {
        OnMove();
        OnRotate();
    }

    private void OnMove()
    {
        // Calculate the movement vector (forward or backward)
        Vector3 move = transform.forward * _vertical * moveSpeed * Time.deltaTime;

        // Apply the movement to the Rigidbody (with MovePosition to handle physics)
        _rb.MovePosition(_rb.position + move);
    }

    private void OnRotate()
    {
        // Calculate the rotation (around the Y-axis)
        Quaternion rotation = Quaternion.Euler(0.0f, _horizontal * turnSpeed * Time.deltaTime, 0.0f);

        // Apply the rotation
        _rb.MoveRotation(_rb.rotation * rotation);
    }
}
