using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _photonUsername;
    [SerializeField] private string _roomname;

    private ClientState _currentState;

    public static PhotonManager Instance { get; private set; }

    public event Action<List<RoomInfo>> OnRoomListUpdated;
    public event Action<string> OnClientStateChanged;
    public event Action OnRoomLeft;

    public string PhotonUsername { get => _photonUsername; set => _photonUsername = value; }
    public string Roomname { get => _roomname; set => _roomname = value; }
    public bool RoomIsPrivate { get; set; }

    private void Awake()
    {
        if (PhotonNetwork.IsConnected)
        {
            var player = PhotonNetwork.LocalPlayer;
            PhotonUsername = player.NickName;
        }
    }

    private void Update()
    {
        if (_currentState != PhotonNetwork.NetworkClientState)
        {
            OnClientStateChanged?.Invoke(PhotonNetwork.NetworkClientState.ToString());
            _currentState = PhotonNetwork.NetworkClientState;
        }
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
            PhotonNetwork.JoinLobby();
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
            MaxPlayers = 4,
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
        SceneManager.LoadScene("MenuScene");
        OnRoomLeft?.Invoke();
    }

    public void StartTheGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.LoadLevel("GameMapScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdated.Invoke(roomList);
        Debug.Log("Room list updated");
    }
}