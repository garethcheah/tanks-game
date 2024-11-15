using Unity.Netcode;
using UnityEngine;

public class PlayerDamage : NetworkBehaviour
{
    public void OnDamage()
    {
        Debug.Log($"{OwnerClientId} took damage.");
        GameManager.instance.ResetPlayerPosition(NetworkObject, OwnerClientId);
    }
}
