using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviourPunCallbacks
{
    static public GameController Instance;

    private GameObject instance;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private LevelView _levelView;
    [SerializeField] private Button _exitGameButton;

    private void Start()
    {
        Instance = this;

        _exitGameButton.onClick.AddListener(ExitGame);

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
            if (PlayerManager.LocalPlayerInstance == null)
            {
                PhotonNetwork.Instantiate(this._playerPrefab.name, _levelView.GetSpawnPoint().position, Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

    private void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MenuScene");
        UIPresenter.MoveToPhotonLoginCanvas();
    }
}
