using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;

public class PlayerInRoomListController : IDisposable
{
    private TMP_Text _playerList;

    public PlayerInRoomListController(Dictionary<int, Player> playerList, TMP_Text playerListText)
    {
        _playerList = playerListText;
        foreach (var player in playerList)
        {
            AddPlayer(player.Value.NickName);
        }
        PhotonManager.Instance.OnPlayerJoined += AddPlayer;
        PhotonManager.Instance.OnPlayerLeft += DeletePlayer;
    }

    private void AddPlayer(string playerName)
    {
        _playerList.text += $"\n{playerName}";
    }

    private void AddPlayer(Player player)
    {
        _playerList.text += $"\n{player.NickName}";
    }

    private void DeletePlayer(Player player)
    {
        var lineStartingIndex = _playerList.text.IndexOf($"\n{player.NickName}");
        var targetString = $"\n{player.NickName}";
        _playerList.text.Remove(lineStartingIndex, targetString.Length);
    } 

    public void Dispose()
    {
        PhotonManager.Instance.OnPlayerJoined -= AddPlayer;
        PhotonManager.Instance.OnPlayerLeft -= DeletePlayer;
    }
}
