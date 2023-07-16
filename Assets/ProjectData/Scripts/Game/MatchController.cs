using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MatchController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField, Tooltip("Match time in seconds")] private float _matchTimeTotal;
    [SerializeField] private TMP_Text _timerText;
    private ReactiveProperty<float> _matchTime = new(1);
    private List<PlayerGameStatistics> _gameStats = new();
    private float _endTime;

    public event Action OnTimeExpired;

    public const int Kill_VC_Reward = 5;
    public const int Assist_VC_Reward = 1;
    public const int Kill_XP_Reward = 20;
    public const int Assist_XP_Reward = 10;

    public static MatchController Instance { get; private set; }
    public List<PlayerGameStatistics> GameStats { get => _gameStats; set => _gameStats = value; }

    public override void OnEnable()
    {
        Instance = this;
        _matchTime.Value = _matchTimeTotal;
        _matchTime.OnValueChanged += SetMatchTimerValue;
        _endTime = Time.time + _matchTimeTotal;
    }

    private void FixedUpdate()
    {
        _matchTime.Value = _endTime - Time.time;
    }

    private void SetMatchTimerValue(float timeLeft)
    {
        if (_matchTime.Value <= 0)
        {
            OnTimeExpired?.Invoke();
            GrantRewardsForPlayers();
            return;
        }
        var timeInMinutes = timeLeft / 60;
        var minutesLeft = (int)timeInMinutes;
        int secondsLeft = (int)((timeInMinutes - minutesLeft) * 60);
        var secondsLeftFormated = string.Format("{0:d2}", secondsLeft);
        _timerText.text = string.Format($"{minutesLeft} : {secondsLeftFormated}");
    }

    private void GrantRewardsForPlayers()
    {
        foreach (var playerStats in _gameStats)
        {
            var stats = playerStats.PlayerStatistics;
            PlayFabClientAPI.GetAccountInfo(new()
            {
                Username = stats.Name.Value,
            },
            result =>
            {
                GrantVirtualCurrencyToPlayer(result, stats);
                GrantExperienceToPlayer(result, stats);
            },
            error =>
            {
                error.GenerateErrorReport();
            });
        }
    }

    private void GrantVirtualCurrencyToPlayer(GetAccountInfoResult result, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabServerAPI.AddUserVirtualCurrency(new()
        {
            PlayFabId = result.AccountInfo.PlayFabId,
            VirtualCurrency = "Credits",
            Amount = stats.Kills.Value * Kill_VC_Reward + stats.Assists.Value * Assist_VC_Reward,
        },
        result =>
        {
            Debug.Log($"Current {stats.Name} currency: {result.Balance}");
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }

    private void GrantExperienceToPlayer(GetAccountInfoResult result, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabServerAPI.GetTitleData(new()
        {
            AuthenticationContext = result.Request.AuthenticationContext,
            Keys = { "Experience" },
        },
        result =>
        {
            result.Data.TryGetValue("Experience", out string exp);
            if (!int.TryParse(exp, out var currentPlayerXP)) Debug.LogError("Failed parsing player experience");
            SetNewXPValue(result, stats, currentPlayerXP);
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }

    private void SetNewXPValue(PlayFab.ServerModels.GetTitleDataResult result, PlayerGameStatistics.GameStatistics stats, int currentPlayerXP)
    {
        PlayFabClientAPI.UpdateUserData(new()
        {
            AuthenticationContext = result.Request.AuthenticationContext,
            Data = new Dictionary<string, string>
                    {
                        { "Experience", (currentPlayerXP + stats.Kills.Value * Kill_XP_Reward + stats.Assists.Value * Assist_XP_Reward).ToString() }
                    },
            Permission = UserDataPermission.Public,
        },
        result =>
        {
        
        },
        error =>
        {
            error.GenerateErrorReport();
        });
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (photonView.IsRoomView && stream.IsWriting)
        {
            stream.SendNext(_matchTime.Value);
        }
        else
        {
            _matchTime.Value = (float)stream.ReceiveNext();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }
}
