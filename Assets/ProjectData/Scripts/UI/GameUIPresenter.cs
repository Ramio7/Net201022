using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class GameUIPresenter : MonoBehaviour
{
    [SerializeField] private GameObject _playerStatisticsPrefab;

    [Header("UI fields")]
    [SerializeField] private Slider _playerHp;
    [SerializeField] private TMP_Text _ammoCounter;
    [SerializeField] private TMP_Text _gameEventsBar;
    [SerializeField] private Canvas _gameStatisticsCanvas;
    [SerializeField] private Canvas _exitGameCanvas;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private Transform _playerStatisticsParent;

    private GameStatisticsPanelController _statisticsController;
    private MatchController _matchController;

    public float playerMaxHP;
    public GameStatisticsPanelController StatisticsController { get => _statisticsController; set => _statisticsController = value; }

    private void Start()
    {
        StartGameStatistics();
        HideMouseCursor();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        ShowMouseCursor();
        UnsubscribeEvents();
        _statisticsController.Dispose();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) SetExitCanvasActiveSelf();

        if (Input.GetKey(KeyCode.Tab)) SetStatisticsCanvasActiveSelf();
        else SetStatisticsCanvasUnactiveSelf();
    }

    private void StartGameStatistics()
    {
        var players = PhotonNetwork.CurrentRoom.Players;
        _statisticsController = new GameStatisticsPanelController(_playerStatisticsPrefab, _playerStatisticsParent, players);
    }

    private void SubscribeEvents()
    {
        _yesButton.onClick.AddListener(LeaveRoom);
        _noButton.onClick.AddListener(SetExitCanvasUnactiveSelf);
    }

    private void UnsubscribeEvents()
    {
        _yesButton.onClick.RemoveAllListeners();
        _noButton.onClick.RemoveAllListeners();
    }

    private void HideMouseCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ShowMouseCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetPlayerHPSlider(float hp) => _playerHp.value = hp / playerMaxHP;
    public void SetBulletsCounter(int bulletCount, int maxBullets) => _ammoCounter.text = $"Ammo: {bulletCount} / {maxBullets}";

    private void SetExitCanvasActiveSelf()
    {
        _exitGameCanvas.enabled = true;
        ShowMouseCursor();
    }

    private void SetExitCanvasUnactiveSelf()
    {
        _exitGameCanvas.enabled = false;
        HideMouseCursor();
    }

    private void SetStatisticsCanvasActiveSelf() => _gameStatisticsCanvas.enabled = true;
    private void SetStatisticsCanvasUnactiveSelf() => _gameStatisticsCanvas.enabled= false;
    private void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
