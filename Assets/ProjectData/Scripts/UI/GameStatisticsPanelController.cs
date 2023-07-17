using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameStatisticsPanelController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject _playerGameStatisticsPrefab;
    [SerializeField] private Transform _gameStatisticsParent;
    [SerializeField] private Canvas _gameStatisticsCanvas;
    [SerializeField] private TMP_Text _gameEventsText;
    [SerializeField, Tooltip("In milliseconds")] private int _gameEventMessageLifeTime;

    private List<PlayerGameStatistics> _playerLinesList = new();

    public static GameStatisticsPanelController Instance;

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AddCallbackTarget(this);
        
        if (photonView.IsMine)
        {
            var playerList = PhotonNetwork.CurrentRoom.Players;
            foreach (var player in playerList)
            {
                AddGameStatistics(player.Value);
            }
            MatchController.Instance.OnTimeExpired += SendPlayerStatistics;
            MatchController.Instance.OnMatchEnd += SetGameStatisticsCanvasActive;
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Tab)) SetGameStatisticsCanvasActive(true);
        else SetGameStatisticsCanvasActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (photonView.IsRoomView && stream.IsWriting)
        //{
        //    stream.SendNext(_gameEventsText.text);
        //}
        //else
        //{
        //    _gameEventsText.text = (string)stream.ReceiveNext();
        //}
    }

    private void SetGameStatisticsCanvasActive(bool isActive) => _gameStatisticsCanvas.enabled = isActive;

    private void AddGameStatistics(Player player)
    {
        var lineGameObject = PhotonNetwork.Instantiate(_playerGameStatisticsPrefab.name, _gameStatisticsParent.position, Quaternion.identity);
        lineGameObject.transform.SetParent(_gameStatisticsParent, false);
        lineGameObject.TryGetComponent<PlayerGameStatistics>(out var playerGameStatistics);
        playerGameStatistics.StartPlayerStatistics(player);
        _playerLinesList.Add(playerGameStatistics);
        if (PhotonNetwork.LocalPlayer == player)
        {
            PlayerController.Instance.OnPlayerIsDeadForStats += SetPLayerStatisticsOnKill;
            PlayerController.Instance.OnPlayerIsDeadForStats += SendGameEventMessange;
        }
    }

    private void ClearGameStatistics(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        PhotonNetwork.Destroy(findedLine.gameObject);
    }

    private static bool PlayerStatisticsFinder(PlayerGameStatistics playerGameStatistics, Player player)
    {
        return playerGameStatistics.PlayerStatistics.Name.Value == player.NickName;
    }

    private void SetPLayerStatisticsOnKill(Player killer, Player assistant, Player victim)
    {
        if (photonView.IsMine)
        {
            if (killer != victim) AddKillToPlayer(killer);
            AddAssistToPlayer(assistant);
            AddDeathToPlayer(victim);
        }
    }

    private async void SendGameEventMessange(Player killer, Player assistant, Player victim)
    {
        if (photonView.IsMine)
        {
            if (killer == victim && assistant == null) _gameEventsText.text += $"{victim.NickName} commited suicide\n";
            else if (killer == victim && assistant != null) _gameEventsText.text += $"{victim.NickName} commited suicide with {assistant.NickName} help\n";
            else if (killer != null && assistant != null) _gameEventsText.text += $"{killer.NickName} killed {victim.NickName} with {assistant.NickName} support\n";
            else if (assistant == null) _gameEventsText.text += $"{killer.NickName} killed {victim.NickName}\n";
            await Task.Run(() => Task.Delay(_gameEventMessageLifeTime));
            DeleteFirstMessange();
        }
    }

    private void DeleteFirstMessange()
    {
        if (_gameEventsText.text == string.Empty) return;
        var gameEventsMessages = _gameEventsText.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        _gameEventsText.text = _gameEventsText.text.Remove(0, gameEventsMessages[0].Length);
    }

    public void AddKillToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        findedLine.PlayerStatistics.Kills.Value += 1;
    }

    public void AddAssistToPlayer(Player player)
    {
        if (player == null) return;
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        findedLine.PlayerStatistics.Assists.Value += 1;
    }

    public void AddDeathToPlayer(Player player)
    {
        var findedLine = _playerLinesList.Find(playerGameStatistics => PlayerStatisticsFinder(playerGameStatistics, player));
        findedLine.PlayerStatistics.Deaths.Value += 1;
    }

    private void SendPlayerStatistics()
    {
        foreach (var line in _playerLinesList) MatchController.Instance.GameStats.Add(line);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddGameStatistics(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ClearGameStatistics(otherPlayer);
    }
}
