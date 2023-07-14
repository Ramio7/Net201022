using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerGameStatistics: MonoBehaviourPunCallbacks, IPunObservable
{
    public struct GameStatistics
    {
        public ReactiveProperty<string> Name;
        public ReactiveProperty<int> Kills;
        public ReactiveProperty<int> Assists;
        public ReactiveProperty<int> Deaths;
    }

    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private TMP_Text _playerKills;
    [SerializeField] private TMP_Text _playerAssists;
    [SerializeField] private TMP_Text _playerDeaths;

    public GameStatistics PlayerStatistics { get; private set; } = new();

    private void Awake()
    {
        InitPLayerStatistics();
        PlayerStatistics.Name.OnValueChanged += SetPlayerName;
        PlayerStatistics.Kills.OnValueChanged += SetPlayerKills;
        PlayerStatistics.Assists.OnValueChanged += SetPlayerAssists;
        PlayerStatistics.Deaths.OnValueChanged += SetPlayerDeaths;
        StartPlayerStatistics();
    }

    private void InitPLayerStatistics()
    {
        PlayerStatistics = new()
        {
            Name = new ReactiveProperty<string>(string.Empty),
            Kills = new ReactiveProperty<int>(-1),
            Assists = new ReactiveProperty<int>(-1),
            Deaths = new ReactiveProperty<int>(-1),
        };
    }

    private void StartPlayerStatistics()
    {
        PlayerStatistics.Name.SetValue(PhotonNetwork.LocalPlayer.NickName);
        PlayerStatistics.Kills.SetValue(0);
        PlayerStatistics.Assists.SetValue(0);
        PlayerStatistics.Deaths.SetValue(0);
    }

    private void SetPlayerName(string playerName) => _playerName.text = playerName;
    private void SetPlayerKills(int kills) => _playerKills.text = kills.ToString();
    private void SetPlayerAssists(int assists) => _playerAssists.text = assists.ToString();
    private void SetPlayerDeaths(int deaths) => _playerDeaths.text = deaths.ToString();

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            var kills = PlayerStatistics.Kills.GetValue();
            var assisits = PlayerStatistics.Assists.GetValue();
            var deaths = PlayerStatistics.Deaths.GetValue();
            stream.SendNext(kills);
            stream.SendNext(assisits);
            stream.SendNext(deaths);
        }
        else
        {
            var kills = (int)stream.ReceiveNext();
            var assists = (int)stream.ReceiveNext();
            var deaths = (int)stream.ReceiveNext();
            PlayerStatistics.Kills.SetValue(kills);
            PlayerStatistics.Assists.SetValue(assists);
            PlayerStatistics.Deaths.SetValue(deaths);
        }
    }
}
