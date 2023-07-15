using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;

public class PlayerInRoomListController : IDisposable
{
    private TMP_Text _playerList;
    private List<Player> _playersInRoom = new();

    public PlayerInRoomListController(TMP_Text playerListText)
    {
        _playerList = playerListText;
        var players = PhotonNetwork.CurrentRoom.Players;
        foreach (var p in players)
        {
            _playersInRoom.Add(p.Value);
            AddPlayer(p.Value);
        }
        PhotonManager.Instance.OnPlayerJoined += AddPlayer;
        PhotonManager.Instance.OnPlayerLeft += DeletePlayer;
    }

    private void AddPlayer(Player player)
    {
        _playerList.text += $"\n{player.NickName}";
        _playersInRoom.Add(player);
    }

    private void DeletePlayer(Player player)
    {
        var lineStartingIndex = _playerList.text.IndexOf($"\n{player.NickName}");
        var targetString = $"\n{player.NickName}";
        var newText = _playerList.text.Remove(lineStartingIndex, targetString.Length);
        _playerList.text = newText;
        _playersInRoom.Remove(player);
    }

    public void Dispose()
    {
        PhotonManager.Instance.OnPlayerJoined -= AddPlayer;
        PhotonManager.Instance.OnPlayerLeft -= DeletePlayer;
        foreach (var player in _playersInRoom)
        {
            DeletePlayer(player);
        }
    }
}
