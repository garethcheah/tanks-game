using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SimpleChat : NetworkBehaviour
{
    public TMP_Text txtChat;
    public TMP_InputField inputChat;

    public void SendMessageToServer()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        if (IsClient && inputChat.text.Length > 0)
        {
            ChatServerRPC(inputChat.text, clientId);
            inputChat.text = "";
        }

        if ((IsServer || IsHost) && inputChat.text.Length > 0)
        {
            ChatClientRPC(inputChat.text, clientId);
            inputChat.text = "";
        }
    }

    // Called on the client, but runs on the server
    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC(string message, ulong clientId)
    {
        // Shows a message from a client on the server
        txtChat.text = "[" + clientId.ToString() + "] " + message;
        ChatClientRPC(message, clientId);
    }

    // Called on the server, but runs on the client
    [ClientRpc]
    public void ChatClientRPC(string message, ulong clientId)
    {
        // Show a message from the server
        txtChat.text = "[" + clientId.ToString() + "] " + message;
    }
}
