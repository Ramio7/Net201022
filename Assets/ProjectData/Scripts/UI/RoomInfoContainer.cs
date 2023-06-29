using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomInfoContainer : MonoBehaviour
{
    public RoomInfo ButtonRoomInfo { get; private set; }

    [SerializeField] private TMP_Text _roomInfoText;

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        ButtonRoomInfo = roomInfo;
        _roomInfoText.text = roomInfo.ToStringFull();
    }

    public void ClearRoomInfo()
    {
        ButtonRoomInfo = null;
        _roomInfoText.text = string.Empty;
    }
}
