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
    [SerializeField] private int _roomListUpdateDelay;

    private ClientState _currentState;

    private readonly TypedLobby _sqlLobby = new("DiplomaLoppy", LobbyType.SqlLobby);
    private const string MAP_KEY = "MAP";
    private const string GAME_MODE_KEY = "GAME_MODE";

    public static PhotonManager Instance { get; private set; }

    public event Action<List<RoomInfo>> OnRoomListUpdated;
    public event Action<string> OnClientStateChanged;
    public event Action<Player> OnPlayerJoined;
    public event Action<Player> OnPlayerLeft;

    public string PhotonUsername { get => _photonUsername; set => _photonUsername = value; }
    public string Roomname { get => _roomname; set => _roomname = value; }
    public bool RoomIsPrivate { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
        if (PhotonNetwork.IsConnectedAndReady)
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
            PhotonNetwork.JoinLobby(_sqlLobby);
            PhotonNetwork.AddCallbackTarget(this);
        }
    }

    public void RoomListUpdate() => PhotonNetwork.GetCustomRoomList(_sqlLobby, "MAP BETWEEN 0 AND 10 AND GAME_MODE BETWEEN 0 AND 10");

    public void DisconnectPhoton()
    {
        PhotonNetwork.Disconnect();
    }

    public void CreateRoom()
    {
        var roomOptions = new RoomOptions
        {
            IsOpen = true,
            IsVisible = RoomIsPrivate,
            MaxPlayers = 9,
            CustomRoomPropertiesForLobby = new string[] { MAP_KEY, GAME_MODE_KEY },
            CustomRoomProperties = new() { { MAP_KEY, 1 }, { GAME_MODE_KEY, 1 } }
        };
        PhotonNetwork.CreateRoom(_roomname, roomOptions, _sqlLobby);
    }

    public void JoinRoom(RoomInfo roomInfo)
    {
        PhotonNetwork.JoinRoom(roomInfo.Name);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, _sqlLobby, "MAP BETWEEN 0 AND 10 AND GAME_MODE BETWEEN 0 AND 10");
    }

    public void LeaveCurrentRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartTheGame()
    {
        PhotonNetwork.LoadLevel("GameMapScene");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdated.Invoke(roomList);
    }

    public override void OnLeftRoom()
    {
        if (SceneManager.GetActiveScene().name != "MenuScene") SceneManager.LoadScene("MenuScene");
    }

    public override void OnConnectedToMaster()
    {
        RoomListUpdate();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerJoined?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        OnPlayerLeft?.Invoke(otherPlayer);
    }
}