using Photon.Pun;
using Photon.Realtime;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private LevelView _levelView;

    private GameObject _playerCharacter;
    private PlayerController _playerController;
    private GameUIPresenter _gameUIPresenter;
    private Vector3 _playerSpawnPosition;

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;

        _levelView.OnSpawnPointGranted += SetPlayerSpawnPosition;
        _levelView.GetSpawnPoint();
    }

    public void Start()
    {
        if (!photonView.IsMine) return;

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

        //ChangePlayerColors();
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
        _playerController = null;
    }

    private PlayerController InstantiatePlayerCharacter(GameUIPresenter gameUIPresenter)
    {
        _playerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, _playerSpawnPosition, Quaternion.identity, 0);
        var playerController = _playerCharacter.GetComponentInChildren<PlayerController>();
        gameUIPresenter.playerMaxHP = playerController.Max_Health;
        playerController.OnPlayerHpValueChanged += gameUIPresenter.SetPlayerHPSlider;
        playerController.OnPlayerAmmoChanged += gameUIPresenter.SetBulletsCounter;
        playerController.OnPlayerIsDead += RevivePlayer;
        return playerController;
    }

    //private void ChangePlayerColors()
    //{
    //    foreach (var playerCharacter in PhotonNetwork.CurrentRoom.Players.Values)
    //    {
    //        playerCharacter
    //        switch (playerCharacter.ActorNumber)
    //        {
    //            case 1:
    //                SetPlayerColor(Color.black, playerController);
    //                break;
    //            case 2:
    //                SetPlayerColor(Color.cyan, playerController);
    //                break;
    //            case 3:
    //                SetPlayerColor(Color.magenta, playerController);
    //                break;
    //            case 4:
    //                SetPlayerColor(Color.blue, playerController);
    //                break;
    //            case 5:
    //                SetPlayerColor(Color.gray, playerController);
    //                break;
    //            case 6:
    //                SetPlayerColor(Color.green, playerController);
    //                break;
    //            case 7:
    //                SetPlayerColor(Color.yellow, playerController);
    //                break;
    //            case 8:
    //                SetPlayerColor(Color.white, playerController);
    //                break;
    //            case 9:
    //                SetPlayerColor(Color.red, playerController);
    //                break;
    //            default:
    //                float r = Random.Range(0.0f, 1.0f);
    //                float g = Random.Range(0.0f, 1.0f);
    //                float b = Random.Range(0.0f, 1.0f);
    //                Color color = new(r, g, b);
    //                SetPlayerColor(color, playerController);
    //                break;
    //        }
    //    }
    //}

    //private void SetPlayerColor(Color color, PlayerController playerController)
    //{
    //    foreach (var mesh in playerController.MeshList) mesh.material.color = color;
    //}

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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //ChangePlayerColors();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        /*ChangePlayerColors()*/;
    }
}
