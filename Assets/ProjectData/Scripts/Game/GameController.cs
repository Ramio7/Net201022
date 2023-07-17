using Photon.Pun;
using Photon.Realtime;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private GameObject _gameStatisticsPrefab;
    [SerializeField] private LevelView _levelView;
    [SerializeField] private int _playerReviveTime;

    private GameObject _playerCharacter;
    private PlayerController _playerController;
    private GameUIPresenter _gameUIPresenter;
    private Vector3 _playerSpawnPosition;
    private GameStatisticsPanelController _gameStatisticsPanelController;

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;

        _levelView = FindFirstObjectByType<LevelView>();
        _levelView.OnSpawnPointGranted += SetPlayerSpawnPosition;
        _levelView.GetSpawnPoint();
    }

    public void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("MenuScene");

            return;
        }

        if (_playerPrefab == null)
        {
            Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Controller'", this);
        }
        else
        {
            if (_playerCharacter == null)
            {
                _playerController = InstantiatePlayerCharacter();
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }

            if (!Instantiate(_uiPrefab, Vector3.zero, Quaternion.identity).TryGetComponent(out GameUIPresenter gameUIPresenter))
                Debug.LogError("GameUIPresenter not attached to UI");
            InitGameUI(gameUIPresenter);

            _gameStatisticsPanelController = FindFirstObjectByType<GameStatisticsPanelController>();

            if (_gameStatisticsPanelController == null)
            {
                var uiContainerTransform = FindFirstObjectByType<UIContainer>().transform;
                PhotonNetwork.Instantiate(_gameStatisticsPrefab.name, Vector3.zero, Quaternion.identity).
                    TryGetComponent(out GameStatisticsPanelController gameStatisticsPanelController);
                _gameStatisticsPanelController = gameStatisticsPanelController;
                gameStatisticsPanelController.gameObject.transform.SetParent(uiContainerTransform, false);
            }
        }
    }

    private void InitGameUI(GameUIPresenter gameUIPresenter)
    {
        _gameUIPresenter = gameUIPresenter;
        gameUIPresenter.playerMaxHP = _playerController.Max_Health;
        _playerController.Health.OnValueChanged += gameUIPresenter.SetPlayerHPSlider;
        _playerController.OnPlayerAmmoChanged += gameUIPresenter.SetBulletsCounter;
        SetPlayerBulletsMaximum(gameUIPresenter, _playerController);
    }

    private void OnDestroy()
    {
        if (!PhotonNetwork.IsConnected) return;

        if (_playerController != null)
        {
            _playerController.OnPlayerHpValueChanged -= _gameUIPresenter.SetPlayerHPSlider;
            _playerController.OnPlayerAmmoChanged -= _gameUIPresenter.SetBulletsCounter;
            _playerController.OnPlayerIsDead -= RevivePlayer;
        }
        PhotonNetwork.Destroy(_playerController.gameObject);
        _playerController = null;
    }

    private PlayerController InstantiatePlayerCharacter()
    {
        _playerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, _playerSpawnPosition, Quaternion.identity, 0);
        _playerCharacter.TryGetComponent<PlayerController>(out var playerController);
        playerController.OnPlayerIsDead += RevivePlayer;
        return playerController;
    }

    private void SetPlayerSpawnPosition(Vector3 position) => _playerSpawnPosition = position;

    private async void RevivePlayer()
    {
        await Task.Delay(_playerReviveTime);
        _levelView.GetSpawnPoint();
        _playerCharacter.transform.SetPositionAndRotation(_playerSpawnPosition, Quaternion.identity);
        _playerCharacter.SetActive(true);
        _playerController.Health.Value = _playerController.Max_Health;
        _playerController.BulletsCount.Value = _playerController.Max_Bullets;
    }

    private static void SetPlayerBulletsMaximum(GameUIPresenter gameUIPresenter, PlayerController playerController)
    {
        gameUIPresenter.SetBulletsCounter(playerController.Max_Bullets, playerController.Max_Bullets);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //ChangePlayerColors();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        /*ChangePlayerColors()*/
    }
}
