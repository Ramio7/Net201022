using TMPro;
using UnityEngine;

public class PlayerGameStatistics: MonoBehaviour
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

    private void OnEnable()
    {
        PlayerStatistics.Kills.OnValueChanged += SetPlayerKills;
        PlayerStatistics.Assists.OnValueChanged += SetPlayerAssists;
        PlayerStatistics.Deaths.OnValueChanged += SetPlayerDeaths;
        PlayerStartGameStatistics(PhotonManager.Instance.PhotonUsername);
    }

    public void PlayerStartGameStatistics(string playerName)
    {
        PlayerStatistics.Name.SetValue(playerName);
        PlayerStatistics.Kills.SetValue(0);
        PlayerStatistics.Assists.SetValue(0);
        PlayerStatistics.Deaths.SetValue(0);
    }

    private void SetPlayerKills(int kills) => _playerKills.text = kills.ToString();
    private void SetPlayerAssists(int assists) => _playerAssists.text = assists.ToString();
    private void SetPlayerDeaths(int deaths) => _playerDeaths.text = deaths.ToString();
}
