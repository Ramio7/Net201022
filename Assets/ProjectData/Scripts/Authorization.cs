using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Authorization : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _playFabTitleId;
    [SerializeField] private string _userName;
    [SerializeField] private string _password;
    [SerializeField] private string _email;
    private LoginWithPlayFabRequest _request;

    public string UserName { get => _userName; set => _userName = value; }

    public string PassWord { get => _password; set => _password = value; }

    public string EMail { get => _email; set => _email = value; }

    public static event Action<string, Color> PlayFabMessage;

    private void Start()
    {
        
    }

    public void ConnectPlayFab()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = _playFabTitleId;


        _request = new LoginWithPlayFabRequest
        {
            Username = UserName,
            Password = PassWord,
        };

        PlayFabClientAPI.LoginWithPlayFab(
            _request,
            result =>
            {
                Debug.Log(result.PlayFabId);
                PhotonNetwork.AuthValues = new AuthenticationValues(result.PlayFabId);
                PhotonNetwork.NickName = result.PlayFabId;
                PlayFabMessage.Invoke($"{result.PlayFabId} connected to PlayFab", Color.green);
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                PlayFabMessage.Invoke(error.GenerateErrorReport(), Color.red);
            });
    }

    public void DisconnectPlayFab()
    {
        DisconnectPhoton();
        Application.Quit();
    }

    public void ConnectPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomOrCreateRoom(roomName: $"Room N{Random.Range(0, 9999)}");
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = PhotonNetwork.AppVersion;
        }
    }

    public void DisconnectPhoton()
    {
        PhotonNetwork.Disconnect();
        Debug.Log("Disconnected from Photon Network");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        if (!PhotonNetwork.InRoom)
            PhotonNetwork.JoinRandomOrCreateRoom(roomName: $"Room N{Random.Range(0, 9999)}");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"OnJoinedRoom {PhotonNetwork.CurrentRoom.Name}");
    }
}
