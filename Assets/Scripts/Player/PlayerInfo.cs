using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player Name"), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private TMP_Text _txtPlayerName;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerName.OnValueChanged += OnNameChanged;

        // Set player name to game object
        _txtPlayerName.SetText(playerName.Value.ToString());
        gameObject.name = "Player_" + playerName.Value.ToString();

        if (IsLocalPlayer)
        {
            GameManager.instance.SetLocalPlayer(NetworkObject);
        }

        // Game Manager OnPlayerJoined
        GameManager.instance.OnPlayerJoin(NetworkObject);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        playerName.OnValueChanged -= OnNameChanged;
    }

    public void SetName(string name)
    {
        playerName.Value = name;
    }

    private void OnNameChanged(FixedString64Bytes prevVal, FixedString64Bytes newVal)
    {
        if (newVal != prevVal)
        {
            _txtPlayerName.SetText(newVal.Value);
            GameManager.instance.SetPlayerName(NetworkObject, newVal.Value);
        }
    }
}
