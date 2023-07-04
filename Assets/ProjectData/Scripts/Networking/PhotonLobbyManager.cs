using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PhotonLobbyManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks, ILobbyCallbacks
{
    [SerializeField] private ServerSettings _serverSettings;
    [SerializeField] private TMP_Text _callbackMessages;
    [SerializeField] private string _photonUsername;
    [SerializeField] private string _roomname;
    private LoadBalancingClient _loadBalancingClient = new();
    private bool _roomIsPrivate;

    public string PhotonUsername { get => _photonUsername; set => _photonUsername = value; }
    public string Roomname { get => _roomname; set => _roomname = value; }
    public bool RoomIsPrivate { get => _roomIsPrivate; set => _roomIsPrivate = value; }

    public event Action<List<RoomInfo>> OnRoomListUpdated;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _loadBalancingClient.AddCallbackTarget(this);
        _loadBalancingClient.ConnectUsingSettings(_serverSettings.AppSettings);
    }

    private void OnDestroy()
    {
        _loadBalancingClient?.RemoveCallbackTarget(this);
    }

    private void Update()
    {
        _loadBalancingClient?.Service();
        _callbackMessages.text = _loadBalancingClient?.State.ToString();
    }


    public void ConnectPhoton()
    {
        _loadBalancingClient.AuthValues = new()
        {
            UserId = _photonUsername
        };
        _loadBalancingClient.OpJoinLobby(TypedLobby.Default);
    }

    public void DisconnectPhoton() => _loadBalancingClient.Disconnect(DisconnectCause.None);

    public void CreateRoom()
    {
        _loadBalancingClient.OpCreateRoom(
        new EnterRoomParams()
        {
            RoomName = _roomname,
            RoomOptions = new()
            {
                MaxPlayers = 4,
                IsOpen = true,
                IsVisible = _roomIsPrivate,
            },
        });
    }

    public void LeaveGame()
    {
        PhotonNetwork.LoadLevel("MenuScene");
        _loadBalancingClient.OpLeaveRoom(false);
    }

    public void OnConnected()
    {
        Debug.LogWarning("Connected");
    }

    public void OnConnectedToMaster()
    {
        Debug.LogWarning("Connected to master server");
    }

    public void OnCreatedRoom()
    {
        Debug.LogWarning("Room created");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Room creation failed. Code: {returnCode}, {message}");
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {

    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected due to: {cause}");
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {

    }

    public void OnJoinedLobby()
    {
        Debug.LogWarning("Joined lobby");
    }

    public void OnJoinedRoom()
    {
        Debug.LogWarning("Joined room");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Random room join failed. Code: {returnCode}, {message}");
        _loadBalancingClient.OpCreateRoom(new EnterRoomParams());
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Room join failed. Code: {returnCode}, {message}");
    }

    public void OnLeftLobby()
    {
        Debug.LogWarning("Disconnected lobby");
    }

    public void OnLeftRoom()
    {
        Debug.LogWarning("Disconnected room");
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {

    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        OnRoomListUpdated.Invoke(roomList);
        Debug.LogWarning("Room list updated");
    }
}
