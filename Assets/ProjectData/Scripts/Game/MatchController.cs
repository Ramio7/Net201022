using Photon.Pun;
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
    public event Action<bool> OnMatchEnd;

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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (photonView.IsRoomView && stream.IsWriting)
        {
            stream.SendNext(_timerText.text);
        }
        else
        {
            _timerText.text = (string)stream.ReceiveNext();
        }
    }

    private void SetMatchTimerValue(float timeLeft)
    {
        if (_matchTime.Value <= 0)
        {
            OnTimeExpired?.Invoke();
            OnMatchEnd?.Invoke(true);
            Task.Run(() => GrantRewardsForPlayers());
            return;
        }
        var timeInMinutes = timeLeft / 60;
        var minutesLeft = (int)timeInMinutes;
        int secondsLeft = (int)((timeInMinutes - minutesLeft) * 60);
        var secondsLeftFormated = string.Format("{0:d2}", secondsLeft);
        _timerText.text = string.Format($"{minutesLeft} : {secondsLeftFormated}");
    }

    private Task GrantRewardsForPlayers()
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
        return Task.CompletedTask;
    }

    private void GrantVirtualCurrencyToPlayer(GetAccountInfoResult result, PlayerGameStatistics.GameStatistics stats)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new()
        {
            AuthenticationContext = result.Request.AuthenticationContext,
            VirtualCurrency = "CR",
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
        PlayFabClientAPI.GetUserData(new()
        {
            AuthenticationContext = result.Request.AuthenticationContext,
            PlayFabId = result.AccountInfo.PlayFabId,
            Keys = { "Experience" }
        },
        result =>
        {
            PlayFabClientAPI.UpdateUserData(new()
            {
                AuthenticationContext = result.Request.AuthenticationContext,
                Data = new()
            {
                { "Experience", (result.Data["Experience"].Value + stats.Kills.Value * Kill_XP_Reward + stats.Assists.Value * Assist_XP_Reward).ToString() }
            }
            },
            result =>
            {

            },
            error =>
            {
                error.GenerateErrorReport();
            });
        },
        error => { });

    }
}
