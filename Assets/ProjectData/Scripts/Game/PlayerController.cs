using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public float Health = 1f;

    public static GameObject LocalPlayerInstance;

    public event Action<float> OnPlayerHpValueChanged;

    [SerializeField] private GameObject playerUiPrefab;

    [SerializeField] private GameObject beams;

    bool IsFiring;

    #region MonoBehaviour CallBacks

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        CameraWork _cameraWork = gameObject.GetComponent<CameraWork>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><b>Missing</b></Color> CameraWork Component on player Prefab.", this);
        }

        if (this.playerUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

#if UNITY_5_4_OR_NEWER
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
    }


    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable();

#if UNITY_5_4_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
    }

    private bool leavingRoom;

    public void Update()
    {
        if (photonView.IsMine)
        {
            this.ProcessInputs();

            if (this.Health <= 0f && !this.leavingRoom)
            {
                this.leavingRoom = GameManager.Instance.LeaveRoom();
            }
        }

        if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy)
        {
            this.beams.SetActive(this.IsFiring);
        }
    }

    public override void OnLeftRoom()
    {
        this.leavingRoom = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.TryGetComponent<Bullet>(out var bullet))
        {
            Health -= bullet.bulletDamage;
        }
    }


#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


    /// <summary>
    /// MonoBehaviour method called after a new level of index 'level' was loaded.
    /// We recreate the Player UI because it was destroy when we switched level.
    /// Also reposition the player if outside the current arena.
    /// </summary>
    /// <param name="level">Level index loaded</param>
    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }

        GameObject _uiGo = Instantiate(this.playerUiPrefab);
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    #endregion

    #region Private Methods


#if UNITY_5_4_OR_NEWER
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }
#endif

    /// <summary>
    /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
    /// </summary>
    void ProcessInputs()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // we don't want to fire when we interact with UI buttons for example. IsPointerOverGameObject really means IsPointerOver*UI*GameObject
            // notice we don't use on on GetbuttonUp() few lines down, because one can mouse down, move over a UI element and release, which would lead to not lower the isFiring Flag.
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //	return;
            }

            if (!this.IsFiring)
            {
                this.IsFiring = true;
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (this.IsFiring)
            {
                this.IsFiring = false;
            }
        }
    }
    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.IsFiring);
            stream.SendNext(this.Health);
            UpdatePlayerHp(Health);
        }
        else
        {
            // Network player, receive data
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
        }
    }

    private void UpdatePlayerHp(float health) => OnPlayerHpValueChanged?.Invoke(health);

    #endregion
}
