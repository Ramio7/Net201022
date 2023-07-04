using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _photonUsername;
    [SerializeField] private string _roomname;

    public Action<List<RoomInfo>> OnRoomListUpdated;

    public string PhotonUsername { get => _photonUsername; set => _photonUsername = value; }
    public string Roomname { get => _roomname; set => _roomname = value; }
    public bool RoomIsPrivate { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void ConnectPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log($"{_photonUsername} already connected");
        }
        else
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.NickName = _photonUsername;
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = PhotonNetwork.AppVersion;
        }
    }
    public void DisconnectPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(_roomname, new()
        {
            IsOpen = true,
            IsVisible = RoomIsPrivate,
            MaxPlayers = 4
        });
    }

    public void JoinRoom(RoomInfo roomInfo) => PhotonNetwork.JoinRoom(roomInfo.Name);

    public void JoinRoom(string roomName) => PhotonNetwork.JoinRoom(roomName);

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveCurrentRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartTheGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel($"PunBasics-Room for {PhotonNetwork.CurrentRoom.PlayerCount}-edited");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdated.Invoke(roomList);
    }
}