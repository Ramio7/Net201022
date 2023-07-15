using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomController : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text _playerList;
    private List<Player> _playersInRoom = new();

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void AddPlayer(Player player)
    {
        _playerList.text += $"\n{player.NickName}";
        _playersInRoom.Add(player);
    }

    private void DeletePlayer(Player player)
    {
        var lineStartingIndex = _playerList.text.IndexOf($"\n{player.NickName}");
        if (lineStartingIndex < 0) return;
        var targetString = $"\n{player.NickName}";
        var newText = _playerList.text.Remove(lineStartingIndex, targetString.Length);
        _playerList.text = newText;
        _playersInRoom.Remove(player);
    }

    public override void OnJoinedRoom()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        foreach (var player in players)
        {
            AddPlayer(player.Value);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DeletePlayer(otherPlayer);
    }
}
