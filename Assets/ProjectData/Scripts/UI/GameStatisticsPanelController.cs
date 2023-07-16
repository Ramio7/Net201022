using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class GameStatisticsPanelController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerGameStatisticsPrefab;
    [SerializeField] private Transform _gameStatisticsParent;

    private List<PlayerGameStatistics> _playerLinesList = new();

    public static GameStatisticsPanelController Instance;

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AddCallbackTarget(this);
        var playerList = PhotonNetwork.CurrentRoom.Players;
        foreach (var player in playerList)
        {
            AddGameStatistics(player.Value);
            if (PhotonNetwork.LocalPlayer == player.Value)
            {
                PlayerController.Instance.OnPlayerIsDeadForStats += SetPLayerStatisticsOnKill;
            }
        }
        MatchController.Instance.OnTimeExpired += SendPlayerStatistics;
    }

    private void AddGameStatistics(Player player)
    {
        var lineGameObject = PhotonNetwork.Instantiate(_playerGameStatisticsPrefab.name, _gameStatisticsParent.position, Quaternion.identity);
        lineGameObject.transform.SetParent(_gameStatisticsParent, false);
        lineGameObject.TryGetComponent<PlayerGameStatistics>(out var playerGameStatistics);
        playerGameStatistics.StartPlayerStatistics(player);
        _playerLinesList.Add(playerGameStatistics);
        if (PhotonNetwork.LocalPlayer == player)
        {
            PlayerController.Instance.OnPlayerIsDeadForStats += SetPLayerStatisticsOnKill;
        }
    }

    private void ClearGameStatistics(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        PhotonNetwork.Destroy(findedLine.gameObject);
    }

    private static bool PlayerStatisticsFinder(PlayerGameStatistics playerGameStatistics, Player player)
    {
        return playerGameStatistics.PlayerStatistics.Name.Value == player.NickName;
    }

    private void SetPLayerStatisticsOnKill(Player killer, Player asistant, Player victim)
    {
        AddKillToPlayer(killer);
        AddAssistToPlayer(asistant);
        AddDeathToPlayer(victim);
    }

    public void AddKillToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        findedLine.PlayerStatistics.Kills.Value += 1;
    }

    public void AddAssistToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        findedLine.PlayerStatistics.Assists.Value += 1;
    }

    public void AddDeathToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        findedLine.PlayerStatistics.Deaths.Value += 1;
    }

    private void SendPlayerStatistics()
    {
        foreach (var line in _playerLinesList) MatchController.Instance.GameStats.Add(line);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddGameStatistics(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ClearGameStatistics(otherPlayer);
    }
}
