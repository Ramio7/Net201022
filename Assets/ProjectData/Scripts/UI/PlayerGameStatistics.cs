using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerGameStatistics: MonoBehaviour, IPunObservable
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
    }

    private void OnDestroy()
    {
        PlayerStatistics.Name.OnValueChanged -= SetPlayerName;
        PlayerStatistics.Kills.OnValueChanged -= SetPlayerKills;
        PlayerStatistics.Assists.OnValueChanged -= SetPlayerAssists;
        PlayerStatistics.Deaths.OnValueChanged -= SetPlayerDeaths;
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

    public void StartPlayerStatistics(Player player)
    {
        PlayerStatistics.Name.Value = player.NickName;
        PlayerStatistics.Kills.Value = 0;
        PlayerStatistics.Assists.Value = 0;
        PlayerStatistics.Deaths.Value = 0;
    }

    private void SetPlayerName(string playerName) => _playerName.text = playerName;
    private void SetPlayerKills(int kills) => _playerKills.text = kills.ToString();
    private void SetPlayerAssists(int assists) => _playerAssists.text = assists.ToString();
    private void SetPlayerDeaths(int deaths) => _playerDeaths.text = deaths.ToString();

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(PlayerStatistics.Kills.Value);
            stream.SendNext(PlayerStatistics.Assists.Value);
            stream.SendNext(PlayerStatistics.Deaths.Value);
        }
        else
        {
            PlayerStatistics.Kills.Value = (int)stream.ReceiveNext();
            PlayerStatistics.Assists.Value = (int)stream.ReceiveNext();
            PlayerStatistics.Deaths.Value = (int)stream.ReceiveNext();
        }
    }
}
