using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    /// <summary>
    /// 0 - offline, 1 - inGame, 2 - endGame
    /// </summary>
    public NetworkVariable<short> state = new NetworkVariable<short>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private Transform[] _startPositions;

    [Header("UI Elements")]
    [SerializeField] private GameObject _endGameScreen;
    [SerializeField] private TMP_Text _endGameText;
    [SerializeField] private TMP_Text _scoreText;

    private NetworkObject _localPlayer;
    private Dictionary<ulong, string> _playerNames = new Dictionary<ulong, string>();
    private Dictionary<ulong, int> _playerScores = new Dictionary<ulong, int>();

    public void OnPlayerJoin(NetworkObject playerObject)
    {
        // Assign the player position
        playerObject.transform.position = _startPositions[(int)playerObject.OwnerClientId].position;

        //Initialize player score
        _playerScores.Add(playerObject.OwnerClientId, 0);
    }

    public void SetLocalPlayer(NetworkObject localPlayer)
    {
        _localPlayer = localPlayer;

        if (_playerNameInput.text.Trim().Length > 0 )
        {
            _localPlayer.GetComponent<PlayerInfo>().SetName(_playerNameInput.text.Trim());
        }
        else
        {
            _localPlayer.GetComponent<PlayerInfo>().SetName($"Player-{localPlayer.OwnerClientId}");
        }

        _playerNameInput.gameObject.SetActive(false);
    }

    public void SetPlayerName(NetworkObject playerObject, string playerName)
    {
        if (_playerNames.ContainsKey(playerObject.OwnerClientId))
        {
            _playerNames[playerObject.OwnerClientId] = playerName;
        }
        else
        {
            _playerNames.Add(playerObject.OwnerClientId, playerName);
        }
    }

    public void AddScore(ulong playerId)
    {
        if (IsServer)
        {
            _playerScores[playerId]++;
            ShowScoreUI();
            CheckWinner(playerId);
        }
    }

    public void ShowScoreUI()
    {
        _scoreText.text = "";
        PlayerScores _scores = new PlayerScores();
        _scores.scores = new List<ScoreInfo>();

        foreach (var item in _playerScores)
        {
            ScoreInfo scoreInfo = new ScoreInfo();
            scoreInfo.score = item.Value;
            scoreInfo.id = item.Key;
            scoreInfo.name = _playerNames[item.Key];
            _scores.scores.Add(scoreInfo);

            _scoreText.text += $"[{item.Key}] {_playerNames[item.Key]} : {item.Value}/10\n";
        }

        UpdateScoreClientRpc(JsonUtility.ToJson(_scores));
    }

    public void StartGame()
    {
        state.Value = 1;
        ShowScoreUI();
    }

    public void ResetPlayerPosition(NetworkObject playerObject, ulong playerId)
    {
        playerObject.transform.position = _startPositions[(int)playerId].position;
    }

    public void CheckWinner(ulong playerId)
    {
        if (_playerScores[playerId] >= 10)
        {
            EndGame(playerId);
        }
    }

    public void EndGame(ulong winnerId)
    {
        if (IsServer)
        {
            _endGameScreen.SetActive(true);

            if (winnerId == NetworkManager.LocalClientId)
            {
                _endGameText.text = "You win!";
            }
            else
            {
                _endGameText.text = $"You lose!\nThe winner is {_playerNames[winnerId]}";
            }
        }

        ScoreInfo tempScoreInfo = new ScoreInfo();
        tempScoreInfo.score = _playerScores[winnerId];
        tempScoreInfo.id = winnerId;
        tempScoreInfo.name = _playerNames[winnerId];

        ShowGameEndUIClientRPC(JsonUtility.ToJson(tempScoreInfo));
    }

    [ClientRpc]
    public void UpdateScoreClientRpc(string playerScores)
    {
        PlayerScores _scores = JsonUtility.FromJson<PlayerScores>(playerScores);
        _scoreText.text = "";

        foreach (var item in _scores.scores)
        {
            _scoreText.text += $"[{item.id}] {item.name} : {item.score}/10\n";
        }
    }

    [ClientRpc]
    public void ShowGameEndUIClientRPC(string winnerInfo)
    {
        _endGameScreen.SetActive(true);
        ScoreInfo scoreInfo = JsonUtility.FromJson<ScoreInfo>(winnerInfo);

        if (scoreInfo.id == NetworkManager.LocalClientId)
        {
            _endGameText.text = "You win!";
        }
        else
        {
            _endGameText.text = $"You lose!\nThe winner is {scoreInfo.name}";
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (IsServer)
        {
            state.Value = 0;
        }
    }
}

[System.Serializable]
public class PlayerScores
{
    public List<ScoreInfo> scores;
}

[System.Serializable]
public class ScoreInfo
{
    public ulong id;
    public string name;
    public int score;
}
