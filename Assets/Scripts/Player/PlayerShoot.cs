using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    public GameObject bulletPrefab;   // Assign your bullet prefab here
    public Transform firePoint;       // The point from which the bullet will be fired
    public float fireForce = 30.0f;     // Speed of the fired bullet

    private Rigidbody _rb;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Get the Rigidbody component
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && IsOwner)
        {
            FireServerRpc(firePoint.position, firePoint.rotation);
        }
    }

    [ServerRpc]
    void FireServerRpc(Vector3 position, Quaternion rotation)
    {
        // Instantiate bullet at the specified position and rotation
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);

        // Spawn the bullet across the network
        bullet.GetComponent<NetworkObject>().Spawn();

        // Get the Rigidbody of the bullet
        //Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        // Get the forward velocity of the tank (projected onto the forward vector)
        //Vector3 tankForwardVelocity = Vector3.Project(_rb.linearVelocity, firePoint.forward);

        // Add the bullet's own forward speed on top of the tank's forward velocity
        //Vector3 bulletVelocity = firePoint.forward * fireForce; //+ tankForwardVelocity;

        // Apply the combined velocity as a force (impulse)
        //bulletRb.AddForce(bulletVelocity, ForceMode.Impulse);
    }
}
