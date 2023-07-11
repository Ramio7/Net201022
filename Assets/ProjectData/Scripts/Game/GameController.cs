using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviourPunCallbacks
{
    static public GameController Instance;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private LevelView _levelView;

    private GameObject _playerCharacter;

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
                _playerCharacter = PhotonNetwork.Instantiate(_playerPrefab.name, _levelView.GetSpawnPoint().position, Quaternion.identity, 0);
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }

        switch (PhotonNetwork.LocalPlayer.ActorNumber)
        {
            case 0:
                SetPlayerColor(Color.black);
                break;
            case 1:
                SetPlayerColor(Color.cyan);
                break;
            case 2:
                SetPlayerColor(Color.magenta);
                break;
            case 3:
                SetPlayerColor(Color.blue);
                break;
        }
    }

    private void SetPlayerColor(Color color)
    {
        _playerCharacter.GetComponent<MeshRenderer>().material.color = color;
    }
}
