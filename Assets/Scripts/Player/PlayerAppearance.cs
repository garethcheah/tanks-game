using Unity.Netcode;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    public MeshRenderer tankRenderer;
    public Material[] tankMaterials;

    // NetworkVariable to store the tank's color
    public NetworkVariable<int> playerColor = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Apply default material when tank is spawned
        ApplyTankMaterial(playerColor.Value);

        // Subscribe to color changes
        playerColor.OnValueChanged += OnColorIndexChanged;

        if (IsOwner)
        {
            GameManager.instance.SetActivePlayerColorDropdown(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // Unsubscribe from color changes
        playerColor.OnValueChanged -= OnColorIndexChanged;
    }

    [ServerRpc(RequireOwnership = true)]
    public void SetTankColorServerRpc(int colorIndex)
    {
        playerColor.Value = colorIndex;
    }

    public void OnColorDropdownValueChanged(int colorIndex)
    {
        if (IsOwner)
        {
            SetTankColorServerRpc(colorIndex);
        }
    }

    private void OnColorIndexChanged(int prevVal, int newVal)
    {
        if (newVal != prevVal)
        {
            // Update the tank's material
            if (tankRenderer != null)
            {
                ApplyTankMaterial(newVal);
            }
        }
    }

    private void ApplyTankMaterial(int colorIndex)
    {
        tankRenderer.material = tankMaterials[colorIndex];
    }
}
