using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public ulong clientId;
    public float speed = 30.0f;
    public float lifeTime = 5.0f;

    private Rigidbody _rb;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            _rb.linearVelocity = transform.forward * speed;
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Destroy the bullet after 'lifeTime' seconds
        Invoke(nameof(DestroyBullet), lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            PlayerDamage other = collision.gameObject.GetComponent<PlayerDamage>();

            if (other != null) // && collisionObjectClientId != other.OwnerClientId)
            {
                other.OnDamage();
                Debug.Log("Bullet from " + clientId + "has hit " + other.OwnerClientId);

                // Update score
                GameManager.instance.AddScore(clientId);
            }
        }

        // Handle what happens when the bullet hits something (e.g., damage, explosion)
        DestroyBullet();  // Destroy bullet on impact
    }

    private void DestroyBullet()
    {
        if (IsServer && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();  // Use Despawn instead of Destroy for networked objects
        }
    }
}
