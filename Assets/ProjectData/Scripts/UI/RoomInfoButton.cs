using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomInfoButton : MonoBehaviour
{
    public RoomInfo ButtonRoomInfo;

    [SerializeField] private TMP_Text roomInfoText;

    public void GetRoomInfo(RoomInfo roomInfo)
    {
        ButtonRoomInfo = roomInfo;
        roomInfoText.text = roomInfo.ToStringFull();
    }
}
