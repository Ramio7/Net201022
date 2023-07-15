using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStatisticsPanelController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerGameStatisticsPrefab;
    [SerializeField] private Transform _gameStatisticsParent;

    private List<PlayerGameStatistics> _playerLinesList = new();

    public static event Action<List<PlayerGameStatistics>> OnGameEnd;

    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        var playerList = PhotonNetwork.CurrentRoom.Players;
        foreach (var player in playerList)
        {
            AddGameStatistics(player.Value);
        }
    }

    private void AddGameStatistics(Player player)
    {
        var lineGameObject = PhotonNetwork.Instantiate(_playerGameStatisticsPrefab.name, _gameStatisticsParent.position, Quaternion.identity);
        lineGameObject.transform.SetParent(_gameStatisticsParent, false);
        lineGameObject.TryGetComponent<PlayerGameStatistics>(out var playerGameStatistics);
        playerGameStatistics.StartPlayerStatistics(player);
        _playerLinesList.Add(playerGameStatistics);
    }

    private void ClearGameStatistics(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        PhotonNetwork.Destroy(findedLine.gameObject);
    }

    private static bool PlayerStatisticsFinder(PlayerGameStatistics playerGameStatistics, Player player)
    {
        return playerGameStatistics.PlayerStatistics.Name.GetValue() == player.NickName;
    }

    public void AddKillToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        var kills = findedLine.PlayerStatistics.Kills.Value + 1;
        findedLine.PlayerStatistics.Kills.SetValue(kills);
    }

    public void AddAssistToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        var assists = findedLine.PlayerStatistics.Assists.GetValue() + 1;
        findedLine.PlayerStatistics.Assists.SetValue(assists);
    }

    public void AddDeathToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        var deaths = findedLine.PlayerStatistics.Deaths.GetValue() + 1;
        findedLine.PlayerStatistics.Deaths.SetValue(deaths);
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
