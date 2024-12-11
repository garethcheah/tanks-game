using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    [SerializeField] private int _maxConnections = 4;

    [Header("Connection UI")]
    [SerializeField] private GameObject _pnlJoinGame;
    [SerializeField] private GameObject _pnlChat;
    [SerializeField] private GameObject _pnlColorChange;
    [SerializeField] private TMP_Text _txtStatus;
    [SerializeField] private TMP_Text _txtPlayerId;
    [SerializeField] private TMP_InputField _txtJoinCode;

    private string _playerId;
    private bool _clientAuthenticated = false;
    private string _joinCode;

    public void JoinServer()
    {
        NetworkManager.Singleton.StartServer();
        Debug.Log("Joining Server.");
    }

    public void JoinHost()
    {
        if (!_clientAuthenticated)
        {
            Debug.Log("Client is not authenticated. Please try again.");
            return;
        }

        StartCoroutine(ConfigureGetCodeAndJoinHost());
    }

    public void JoinClient()
    {
        if (!_clientAuthenticated)
        {
            Debug.Log("Client is not authenticated. Please try again.");
            return;
        }

        if (_txtJoinCode.text.Length <= 0)
        {
            Debug.Log("No join code entered.");
            _txtStatus.text = "Please enter a valid join code.";
            return;
        }

        Debug.Log(_txtJoinCode.text);
        StartCoroutine(ConfigureUseCodeJoinClient(_txtJoinCode.text));
    }

    public async Task<RelayServerData> AllocateRelayServerAndGetCode(int maxConnections, string region = null)
    {
        Allocation allocation;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception ex)
        {
            Debug.Log($"Relay allocation request failed. - {ex.ToString()}");
            throw;
        }

        try
        {
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception ex)
        {
            Debug.Log($"Unable to create a join code - {ex.ToString()}");
            throw;
        }

        return new RelayServerData(allocation, "dtls"); // ???
    }

    public async Task<RelayServerData> JoinRelayServerWithCode(string joinCode)
    {
        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception ex)
        {
            Debug.Log($"Relay allocation join failed. - {ex.ToString()}");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    private async void Start()
    {
        await AuthenticatePlayer();
    }

    private async Task AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            _playerId = AuthenticationService.Instance.PlayerId;
            _clientAuthenticated = true;
            _txtPlayerId.text = $"Player ID: {_playerId}";
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error occurred while authenticating player. - {ex.ToString()}");
        }
    }

    IEnumerator ConfigureGetCodeAndJoinHost()
    {
        var allocateAndGetCode = AllocateRelayServerAndGetCode(_maxConnections);

        while (!allocateAndGetCode.IsCompleted)
        {
            // Wait until we create the allocation and get the code
            yield return null;
        }

        // Show an error if the allocation and code fails
        if (allocateAndGetCode.IsFaulted)
        {
            Debug.LogError($"Cannot start the server due to an exception.");
            yield break;
        }

        var relayServerData = allocateAndGetCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        _txtJoinCode.gameObject.SetActive(true);
        _txtJoinCode.text = _joinCode;
        _txtStatus.text = $"Joined as host.";
        _txtPlayerId.text = $"Player ID: {_playerId} (HOST) Join Code: {_joinCode}";
        _pnlJoinGame.SetActive(false);
        _pnlChat.SetActive(true);
        _pnlColorChange.SetActive(true);
    }

    IEnumerator ConfigureUseCodeJoinClient(string joinCode)
    {
        var joinAllocationFromCode = JoinRelayServerWithCode(joinCode);

        while (!joinAllocationFromCode.IsCompleted)
        {
            yield return null;
        }

        if (joinAllocationFromCode.IsFaulted)
        {
            Debug.Log("Cannot join relay due to an exception.");
            _txtStatus.text = "The code you entered was invalid. Please try again.";
            yield break;
        }

        var relayServerData = joinAllocationFromCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();

        _txtStatus.text = "Joined as client.";
        _pnlJoinGame.SetActive(false);
        _pnlChat.SetActive(true);
        _pnlColorChange.SetActive(true);
    }
}
