using System.Collections.Generic;

public class GameStatisticsPanelController
{
    private Dictionary<string, PlayerGameStatistics> _gameStatisticsList;

    public GameStatisticsPanelController(List<PlayerGameStatistics> gameStatisticsList)
    {
        foreach (var gameStatistics in gameStatisticsList) _gameStatisticsList.Add(gameStatistics.PlayerStatistics.Name.GetValue(), gameStatistics);
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
