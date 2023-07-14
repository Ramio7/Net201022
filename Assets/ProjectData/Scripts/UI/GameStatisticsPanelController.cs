using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStatisticsPanelController : IDisposable
{
    private Dictionary<string, PlayerGameStatistics> _gameStatisticsList = new();
    private List<PlayerGameStatistics> _playerLinesList = new();
    private readonly GameObject _playerGameStatisticsPrefab;
    private readonly Transform _gameStatisticsParent;

    public static GameStatisticsPanelController Instance;

    public event Action<PlayerGameStatistics> OnPlayerLeftRoom;
    public event Action<List<PlayerGameStatistics>> OnGameEnd;

    public GameStatisticsPanelController(GameObject playerStatisticsPrefab, Transform gameStatisticsParent, Dictionary<int, Player> playerList)
    {
        Instance = this;
        _playerGameStatisticsPrefab = playerStatisticsPrefab;
        _gameStatisticsParent = gameStatisticsParent;
        foreach (var player in playerList)
        {
            AddGameStatistics(player.Value);
        }
        PhotonManager.Instance.OnPlayerJoined += AddGameStatistics;
        PhotonManager.Instance.OnPlayerLeft += ClearGameStatistics;
    }

    public void Dispose()
    {
        PhotonManager.Instance.OnPlayerJoined -= AddGameStatistics;
        PhotonManager.Instance.OnPlayerLeft -= ClearGameStatistics;
        OnGameEnd?.Invoke(_playerLinesList);
    }

    private void AddGameStatistics(Player player)
    {
        var lineGameObject = PhotonNetwork.Instantiate(_playerGameStatisticsPrefab.name, _gameStatisticsParent.position, Quaternion.identity);
        lineGameObject.transform.SetParent(_gameStatisticsParent, false);
        lineGameObject.TryGetComponent<PlayerGameStatistics>(out var playerGameStatistics);
        _gameStatisticsList.Add(player.NickName, playerGameStatistics);
        _playerLinesList.Add(playerGameStatistics);
    }

    private void ClearGameStatistics(Player player)
    {
        bool playerStatisticsFinder(PlayerGameStatistics playerGameStatistics)
        {
            return playerGameStatistics.PlayerStatistics.Name.GetValue() == player.NickName;
        }
        var findedLine = _playerLinesList.Find(playerStatisticsFinder);
        OnPlayerLeftRoom?.Invoke(findedLine);
        PhotonNetwork.Destroy(findedLine.gameObject);
    }

    public void AddKillToPlayer(string playerName)
    {
        if (_gameStatisticsList.ContainsKey(playerName))
        {
            var playerKills = _gameStatisticsList[playerName].PlayerStatistics.Kills.GetValue() + 1;
            _gameStatisticsList[playerName].PlayerStatistics.Kills.SetValue(playerKills);
        }
    }

    public void AddAssistToPlayer(string playerName)
    {
        if (_gameStatisticsList.ContainsKey(playerName))
        {
            var playerKills = _gameStatisticsList[playerName].PlayerStatistics.Assists.GetValue() + 1;
            _gameStatisticsList[playerName].PlayerStatistics.Assists.SetValue(playerKills);
        }
    }

    public void AddDeathToPlayer(string playerName)
    {
        if (_gameStatisticsList.ContainsKey(playerName))
        {
            var playerKills = _gameStatisticsList[playerName].PlayerStatistics.Assists.GetValue() + 1;
            _gameStatisticsList[playerName].PlayerStatistics.Assists.SetValue(playerKills);
        }
    }
}
