using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private BulletDiploma _bulletPrefab;
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField, Range(5f, 15f)] private float _playerThrottle;
    [SerializeField, Range(0.1f, 5f)] private float _mouseSensitivity;
    [SerializeField, Min(200), Tooltip("Fire delay in milliseconds")] private int _fireRate;
    [SerializeField, Min(1000), Tooltip("Reload time in milleseconds")] private int _reloadTime;
    [SerializeField] private List<MeshRenderer> _meshes;

    private Rigidbody _rigidbody;
    private float _axisVertical;
    private float _axisHorizontal;
    private float _scroll;
    private bool _isFiring;
    private float _health;
    private Player _killer;
    private Player _assistant;

    public ReactiveProperty<bool> IsFiring = new(false);
    public ReactiveProperty<bool> IsDead = new(false);
    public ReactiveProperty<float> Health = new(100);
    public ReactiveProperty<int> BulletsCount = new(30);
    public List<MeshRenderer> MeshList { get; private set; }

    public static GameObject LocalPlayerInstance;
    public static Player Player;
    public static PlayerController Instance;

    public event Action<float> OnPlayerHpValueChanged;
    public event Action<int, int> OnPlayerAmmoChanged;
    public event Action OnPlayerIsDead;
    public event Action<Player, Player, Player> OnPlayerIsDeadForStats;

    public float Max_Health { get; } = 100f;
    public int Max_Bullets { get; } = 30;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
            Player = photonView.Owner;
            Instance = this;
        }
    }

    private void Start()
    {
        if (!photonView.IsMine && !PhotonNetwork.IsConnected) return;

        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _rigidbody.inertiaTensor = Vector3.zero;
        IsFiring.OnValueChanged += Fire;
        IsFiring.OnValueChanged += FireFieldChange;
        Health.OnValueChanged += HealthFieldChange;
    }

    private void OnDestroy()
    {
        IsFiring.OnValueChanged -= Fire;
        IsFiring.OnValueChanged -= FireFieldChange;
        Health.OnValueChanged += HealthFieldChange;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButton(0)) IsFiring.Value =true;
        else IsFiring.Value = false;

        if (Input.GetKey(KeyCode.W)) _axisVertical = 1;
        else if (Input.GetKey(KeyCode.S)) _axisVertical = -1;
        else _axisVertical = 0;

        if (Input.GetKey(KeyCode.A)) _axisHorizontal = -1;
        else if (Input.GetKey(KeyCode.D)) _axisHorizontal = 1;
        else _axisHorizontal = 0;

        _scroll = Input.GetAxis("Mouse X");
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (_scroll != 0.0f) RotateCharacter(_scroll);

        if (_axisVertical != 0) Move(_axisVertical);
        if (_axisHorizontal != 0) Strafe(_axisHorizontal);

        _rigidbody.inertiaTensor = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.gameObject.TryGetComponent(out BulletDiploma bullet)) HealthCheck(bullet);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_isFiring);
            stream.SendNext(_health);
        }
        else
        {
            IsFiring.Value = (bool)stream.ReceiveNext();
            Health.Value = (float)stream.ReceiveNext();
        }
    }

    private void Move(float axisVertical) => _rigidbody.AddForce(axisVertical * _playerThrottle * Time.deltaTime * transform.forward, ForceMode.VelocityChange);

    private void Strafe(float axisHorizontal) => _rigidbody.AddForce(axisHorizontal * _playerThrottle * Time.deltaTime * transform.right, ForceMode.VelocityChange);

    private void RotateCharacter(float scroll) => transform.Rotate(Vector3.up, scroll * _mouseSensitivity);

    private async void Fire(bool isFiring)
    {
        if (!isFiring) return;

        if (BulletsCount.Value == 0)
        {
            await Reload();
            return;
        }
        await StartBullet();
    }

    private void HealthCheck(BulletDiploma bullet)
    {
        Health.Value -= bullet.BulletDamage;
        OnPlayerHpValueChanged?.Invoke(Health.Value);
        _assistant = bullet.GetComponent<PhotonView>().Owner;
        if (Health.Value <= 0)
        {
            _killer = bullet.GetComponent<PhotonView>().Owner;
            if (_killer == _assistant) _assistant = null;
            gameObject.SetActive(false);
            OnPlayerIsDeadForStats?.Invoke(_killer, _assistant, Player);
            OnPlayerIsDead?.Invoke();
        }
    }

    private async Task Reload()
    {
        await Task.Run(() => WaitReload());
        BulletsCount.Value = Max_Bullets;
        OnPlayerAmmoChanged?.Invoke(BulletsCount.Value, Max_Bullets);
        IsFiring.Value = false;
    }

    private async Task StartBullet()
    {
        var bullet = PhotonNetwork.Instantiate(_bulletPrefab.name, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation);
        BulletsCount.Value -= 1;
        OnPlayerAmmoChanged?.Invoke(BulletsCount.Value, Max_Bullets);
        await Task.Run(() => FireRateWaiting());
        IsFiring.Value = false;
    }

    private Task FireRateWaiting() => Task.Delay(_fireRate);

    private Task WaitReload() => Task.Delay(_reloadTime);

    private void FireFieldChange(bool value) => _isFiring = value;
    private void HealthFieldChange(float value) => _health = value;
}
