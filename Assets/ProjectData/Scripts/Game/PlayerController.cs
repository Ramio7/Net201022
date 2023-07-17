using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _minePrefab;
    [SerializeField] private GameObject _gun;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _bulletSpawnPosition;
    [SerializeField, Range(5f, 15f)] private float _playerThrottle;
    [SerializeField, Range(1f, 10f)] private float _jumpForce;
    [SerializeField, Range(0.1f, 5f)] private float _mouseSensitivity;
    [SerializeField, Min(200), Tooltip("Fire delay in milliseconds")] private int _fireRate;
    [SerializeField, Min(1000), Tooltip("Reload time in milliseconds")] private int _reloadTime;
    [SerializeField, Min(20000), Tooltip("Mine cooldown in milliseconds")] private int _mineCooldown;
    [SerializeField] private List<MeshRenderer> _meshes;

    private Rigidbody _rigidbody;
    private float _axisVertical;
    private float _axisHorizontal;
    private float _scroll;
    private float _verticalScroll;
    private bool _gunIsFiring;
    private bool _gunIsReloading;
    private bool _mineIsOnCooldown;
    private Player _killer;
    private Player _assistant;

    public ReactiveProperty<bool> IsFiring = new(false);
    public ReactiveProperty<bool> IsDead = new(false);
    public ReactiveProperty<float> Health = new(100);
    public ReactiveProperty<int> BulletsCount = new(30);
    public ReactiveProperty<bool> IsJumping = new(false);
    public ReactiveProperty <bool> IsMineCharging = new(false);
    public ReactiveProperty<bool> IsReloading = new(false);

    public List<MeshRenderer> MeshList { get; private set; }

    public static GameObject LocalPlayerInstance;
    private static Player PlayerInstance;
    public static PlayerController Instance;

    public event Action<float> OnPlayerHpValueChanged;
    public event Action<int, int> OnPlayerAmmoChanged;
    public event Action OnPlayerIsDead;
    public event Action<Player, Player, Player> OnPlayerIsDeadForStats;

    public float Max_Health { get; } = 100f;
    public int Max_Bullets { get; } = 30;
    public Transform CameraTransform { get => _cameraTransform; private set => _cameraTransform = value; }
    public Player Player { get => PlayerInstance; private set => PlayerInstance = value; }

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
        IsJumping.OnValueChanged += Jump;
        IsMineCharging.OnValueChanged += ThrowMine;
        IsReloading.OnValueChanged += Reload;
    }

    private void OnDestroy()
    {
        IsFiring.OnValueChanged -= Fire;
        IsJumping.OnValueChanged -= Jump;
        IsMineCharging.OnValueChanged += ThrowMine;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetMouseButton(0)) IsFiring.Value =true;
        else IsFiring.Value = false;

        if (Input.GetMouseButtonUp(1)) IsMineCharging.Value = true;
        else IsMineCharging.Value = false;

        if (Input.GetKey(KeyCode.W)) _axisVertical = 1;
        else if (Input.GetKey(KeyCode.S)) _axisVertical = -1;
        else _axisVertical = 0;

        if (Input.GetKey(KeyCode.A)) _axisHorizontal = -1;
        else if (Input.GetKey(KeyCode.D)) _axisHorizontal = 1;
        else _axisHorizontal = 0;

        _scroll = Input.GetAxis("Mouse X");
        _verticalScroll = Input.GetAxis("Mouse Y");

        if (Input.GetKeyUp(KeyCode.Space)) IsJumping.Value = true;

        if (Input.GetKeyUp(KeyCode.R)) IsReloading.Value = true;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (_scroll != 0.0f) RotateCharacter(_scroll);
        if (_verticalScroll != 0.0f) MoveCameraVertical(_verticalScroll);

        if (_axisVertical != 0) Move(_axisVertical);
        if (_axisHorizontal != 0) Strafe(_axisHorizontal);

        _rigidbody.inertiaTensor = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        if (collision.gameObject.TryGetComponent(out IDealDamage damager) && damager.IsCharged) WriteTheAttacker(damager);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (photonView.IsMine && stream.IsWriting)
        {
            stream.SendNext(IsFiring.Value);
            stream.SendNext(Health.Value);
            stream.SendNext(IsDead.Value);
            stream.SendNext(BulletsCount.Value);
        }
        else
        {
            IsFiring.Value = (bool)stream.ReceiveNext();
            Health.Value = (float)stream.ReceiveNext();
            IsDead.Value = (bool)stream.ReceiveNext();
            BulletsCount.Value = (int)stream.ReceiveNext();
        }
    }

    private void Move(float axisVertical) => _rigidbody.AddForce(axisVertical * _playerThrottle * Time.deltaTime * transform.forward, ForceMode.VelocityChange);

    private void Strafe(float axisHorizontal) => _rigidbody.AddForce(axisHorizontal * _playerThrottle * Time.deltaTime * transform.right, ForceMode.VelocityChange);

    private void RotateCharacter(float scroll) => transform.Rotate(Vector3.up, scroll * _mouseSensitivity);

    private void MoveCameraVertical(float scroll)
    {
        _cameraTransform.Rotate(Vector3.right, -scroll * _mouseSensitivity);
        _gun.transform.Rotate(Vector3.right, -scroll * _mouseSensitivity);
    }

    private void Jump(bool isJumping)
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        IsJumping.Value = false;
    }

    private async void Fire(bool isFiring)
    {
        if (!isFiring || !photonView.IsMine || _gunIsFiring || _gunIsReloading) return;

        if (BulletsCount.Value == 0)
        {
            await Task.Run(() => Reload(true));
            return;
        }
        await CreateBullet();
    }

    private void ThrowMine(bool isMineCharging)
    {
        if (!isMineCharging || !photonView.IsMine || _mineIsOnCooldown) return;

        CreateMine();
    }

    private void WriteTheAttacker(IDealDamage damager)
    {
        OnPlayerHpValueChanged?.Invoke(Health.Value);
        _assistant = damager.GameObject.GetComponent<PhotonView>().Owner;
        if (Health.Value <= 0)
        {
            _assistant = _killer;
            _killer = damager.GameObject.GetComponent<PhotonView>().Owner;
            if (_killer == _assistant) _assistant = null;
            gameObject.SetActive(false);
            OnPlayerIsDeadForStats?.Invoke(_killer, _assistant, Player);
            OnPlayerIsDead?.Invoke();
        }
    }

    public void TakeDamage(float damage)
    {
        Health.Value -= damage;
    }

    private async void Reload(bool isReloading)
    {
        if (!isReloading) return;
        await Task.Run(() => WaitReload(_reloadTime));
        BulletsCount.Value = Max_Bullets;
        OnPlayerAmmoChanged?.Invoke(BulletsCount.Value, Max_Bullets);
        IsReloading.Value = false;
    }

    private async Task CreateBullet()
    {
        PhotonNetwork.Instantiate(_bulletPrefab.name, _bulletSpawnPosition.position, _bulletSpawnPosition.rotation);
        BulletsCount.Value -= 1;
        OnPlayerAmmoChanged?.Invoke(BulletsCount.Value, Max_Bullets);
        await Task.Run(() => FireRateWaiting(_fireRate));
        IsFiring.Value = false;
    }

    private async void CreateMine()
    {
        PhotonNetwork.Instantiate(_minePrefab.name, _bulletSpawnPosition.position, _bulletSpawnPosition.rotation).TryGetComponent(out Rigidbody rigidbody);
        var mineThrowDirection = _bulletSpawnPosition.position - gameObject.transform.position;
        rigidbody.AddForce(mineThrowDirection, ForceMode.Impulse);
        await Task.Run(() => WaitCooldown(_mineCooldown));
        IsMineCharging.Value = false;
    }

    private async Task FireRateWaiting(int fireRate)
    {
        _gunIsFiring = true;
        await Task.Run(() => Task.Delay(fireRate));
        _gunIsFiring = false;
    }

    private async Task WaitReload(int reloadTime)
    {
        _gunIsReloading = true;
        await Task.Run(() => Task.Delay(reloadTime));
        _gunIsReloading = false;
    }

    private async Task WaitCooldown(int cooldownTime)
    {
        _mineIsOnCooldown = true;
        await Task.Run(() => Task.Delay(cooldownTime));
        _mineIsOnCooldown = false;
    }
}
