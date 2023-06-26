using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string _photonUsername;
    [SerializeField] private string _roomname;

    public string PhotonUsername { get => _photonUsername; set => _photonUsername = value; }
    public string Roomname { get => _roomname; set => _roomname = value; }

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
        //Debug.Log("Disconnected from Photon Network");
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(_roomname);
        //Debug.Log($"{_roomname} succesfully created");
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(_roomname);
        //Debug.Log($"{_roomname} entered");
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveCurrentRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnConnectedToMaster()
    {
        //Debug.Log("OnConnectedToMaster");
    }

    public override void OnCreatedRoom()
    {
        //Debug.Log("OnCreatedRoom");
    }

    public override void OnJoinedRoom()
    {
       //Debug.Log($"OnJoinedRoom {PhotonNetwork.CurrentRoom.Name}");
    }
}
