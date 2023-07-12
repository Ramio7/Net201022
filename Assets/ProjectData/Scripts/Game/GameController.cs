using Photon.Pun;
using Photon.Realtime;
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
            Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        else
        {
            if (!PhotonNetwork.Instantiate(_uiPrefab.name, Vector3.zero, Quaternion.identity).TryGetComponent(out GameUIPresenter _gameUIPresenter))
                Debug.LogError("GameUIPresenter not attached to UI");

            if (_playerCharacter == null)
            {
                _playerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, _playerSpawnPosition, Quaternion.identity, 0);
                _playerCharacter.TryGetComponent(out PlayerController _playerController);
                _gameUIPresenter.PlayerMaxHP = _playerController.Max_Health;
                _playerController.OnPlayerHpValueChanged += _gameUIPresenter.SetPlayerHPSlider;
                _playerController.OnPlayerAmmoChanged += _gameUIPresenter.SetBulletsCounter;
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

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
        }
    }

    public override void OnDisable()
    {
        _levelView.OnSpawnPointGranted -= SetPlayerSpawnPosition;
        _playerController.OnPlayerHpValueChanged -= _gameUIPresenter.SetPlayerHPSlider;
        _playerController.OnPlayerAmmoChanged -= _gameUIPresenter.SetBulletsCounter;
        base.OnDisable();
    }

    private void SetPlayerColor(Color color)
    {
        _playerCharacter.GetComponent<MeshRenderer>().material.color = color;
    }

    private void SetPlayerSpawnPosition(Vector3 position) => _playerSpawnPosition = position;
}
