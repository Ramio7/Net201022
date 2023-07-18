using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryPoint : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _gameControllerPrefab;

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("MenuScene");

            return;
        }
        PhotonNetwork.Instantiate(_gameControllerPrefab.name, gameObject.transform.position, Quaternion.identity);
    }
}
