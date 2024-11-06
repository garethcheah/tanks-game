using Unity.Netcode;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireForce = 30.0f;

    private Rigidbody _rb;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _rb = GetComponent<Rigidbody>();
    }

    [ServerRpc]
    public void OnFireServerRpc(ServerRpcParams serverRpcParams = default)
    {
        OnFire(serverRpcParams.Receive.SenderClientId);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (IsServer && IsLocalPlayer)
            {
                OnFire(OwnerClientId);
            }
            else if (IsClient && IsLocalPlayer)
            {
                OnFireServerRpc();
            }
        }
    }

    private void OnFire(ulong clientId)
    {
        // Instantiate bullet at the specified position and rotation
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Spawn the bullet across the network
        bullet.GetComponent<NetworkObject>().Spawn();

        // Grab client ID for bullet
        bullet.GetComponent<Bullet>().clientId = clientId;

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
