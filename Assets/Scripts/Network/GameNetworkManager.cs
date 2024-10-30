using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public void JoinServer()
    {
        NetworkManager.Singleton.StartServer();
        Debug.Log("Joining Server.");
    }

    public void JoinHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Starting Host.");
    }

    public void JoinClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Starting Client.");
    }
}
