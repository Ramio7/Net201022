using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private GameObject _levelViewPrefab;
    
    private LevelView _levelView;
    private GameObject _playerCharacter;
    private PlayerController _playerController;
    private GameUIPresenter _gameUIPresenter;
    private Vector3 _playerSpawnPosition;

    private void Awake()
    {
        PhotonNetwork.Instantiate(_levelViewPrefab.name, Vector3.zero, Quaternion.identity).TryGetComponent(out LevelView levelView);
        _levelView = levelView;
        _levelView.OnSpawnPointGranted += SetPlayerSpawnPosition;
        _levelView.GetSpawnPoint();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("MenuScene");

            return;
        }

        if (_playerPrefab == null)
        {
            Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (!PhotonNetwork.Instantiate(_uiPrefab.name, Vector3.zero, Quaternion.identity).TryGetComponent(out GameUIPresenter gameUIPresenter))
                Debug.LogError("GameUIPresenter not attached to UI");
            _gameUIPresenter = gameUIPresenter;

            if (_playerCharacter == null)
            {
                _playerController = InstantiatePlayerCharacter(_gameUIPresenter);
                SetPlayerBulletsMaximum(gameUIPresenter, _playerController);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        if (_playerCharacter.TryGetComponent(out PhotonView photonView) && photonView.IsMine)
        {
            switch (PhotonNetwork.LocalPlayer.ActorNumber)
            {
                case 1:
                    SetPlayerColor(Color.black);
                    break;
                case 2:
                    SetPlayerColor(Color.cyan);
                    break;
                case 3:
                    SetPlayerColor(Color.magenta);
                    break;
                case 4:
                    SetPlayerColor(Color.blue);
                    break;
                case 5: 
                    SetPlayerColor(Color.gray);
                    break;
                case 6:
                    SetPlayerColor(Color.green);
                    break;
                case 7:
                    SetPlayerColor(Color.yellow);
                    break;
                case 8:
                    SetPlayerColor(Color.white);
                    break;
                case 9:
                    SetPlayerColor(Color.red);
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        _levelView.OnSpawnPointGranted -= SetPlayerSpawnPosition;
        if (_playerController != null)
        {
            _playerController.OnPlayerHpValueChanged -= _gameUIPresenter.SetPlayerHPSlider;
            _playerController.OnPlayerAmmoChanged -= _gameUIPresenter.SetBulletsCounter;
            _playerController.OnPlayerIsDead -= RevivePlayer;
        }
        _playerController = null;
    }

    private PlayerController InstantiatePlayerCharacter(GameUIPresenter gameUIPresenter)
    {
        _playerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, _playerSpawnPosition, Quaternion.identity, 0);
        _playerCharacter.TryGetComponent(out PlayerController playerController);
        gameUIPresenter.playerMaxHP = playerController.Max_Health;
        playerController.OnPlayerHpValueChanged += gameUIPresenter.SetPlayerHPSlider;
        playerController.OnPlayerAmmoChanged += gameUIPresenter.SetBulletsCounter;
        playerController.OnPlayerIsDead += RevivePlayer;
        playerController.StartCameraController();
        return playerController;
    }

    private void SetPlayerColor(Color color) => _playerCharacter.GetComponent<MeshRenderer>().material.color = color;

    private void SetPlayerSpawnPosition(Vector3 position) => _playerSpawnPosition = position;

    private async void RevivePlayer()
    {
        await Task.Delay(5000);
        _levelView.GetSpawnPoint();
        _playerCharacter.transform.SetPositionAndRotation(_playerSpawnPosition, Quaternion.identity);

        _playerCharacter.SetActive(true);
    }

    private static void SetPlayerBulletsMaximum(GameUIPresenter _gameUIPresenter, PlayerController _playerController)
    {
        _gameUIPresenter.SetBulletsCounter(_playerController.Max_Bullets, _playerController.Max_Bullets);
    }
}
