using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviourPunCallbacks
{
    static public GameController Instance;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private LevelView _levelView;

    private void Start()
    {
        Instance = this;

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
            PhotonNetwork.Instantiate(_uiPrefab.name, Vector3.zero, Quaternion.identity);

            if (PlayerManager.LocalPlayerInstance == null)
            {
                PhotonNetwork.Instantiate(_playerPrefab.name, _levelView.GetSpawnPoint().position, Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }
}
