using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _photonUsername;
    [SerializeField] private string _roomname = "game";
    [SerializeField] private int _roomListUpdateDelay;

    private ClientState _currentState;

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
    }

    private void Update()
    {
        if (_currentState != PhotonNetwork.NetworkClientState)
        {
            OnClientStateChanged?.Invoke(PhotonNetwork.NetworkClientState.ToString());
            _currentState = PhotonNetwork.NetworkClientState;
        }
    }

    public async void ConnectPhoton()
    {
        await Task.Run(() => WaitPlayFabLogin());

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
            PhotonNetwork.AddCallbackTarget(this);
        }
    }

    private Task WaitPlayFabLogin()
    {
        while (!PlayFabClientAPI.IsClientLoggedIn()) Task.Delay(100);
        return Task.FromResult(0);
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
        });
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
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveCurrentRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartTheGame()
    {
        PhotonNetwork.LoadLevel("GameMapScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"{returnCode}: {message}");
        JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdated?.Invoke(roomList);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient) StartTheGame();
        OnPlayerJoined?.Invoke(PhotonNetwork.LocalPlayer);
    }

    public override void OnLeftRoom()
    {
        OnPlayerLeft?.Invoke(PhotonNetwork.LocalPlayer);
        DisconnectPhoton();
        if (SceneManager.GetActiveScene().name != "MenuScene") SceneManager.LoadScene("MenuScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerJoined?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        JoinRoom(_roomname);
    }
}