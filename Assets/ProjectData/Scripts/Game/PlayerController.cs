using Photon.Pun;
using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(/*typeof(CharacterController),*/ typeof(Rigidbody))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private BulletDiploma _bulletPrefab;
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private float _playerSpeed;
    private Rigidbody _rigidbody;
    private float _axisVertical;
    private float _axisHorizontal;
    private float _scroll;

    public ReactiveProperty<bool> IsFiring = new();
    public ReactiveProperty<bool> IsDead = new();
    public ReactiveProperty<float> Health = new();
    public ReactiveProperty<int> BulletsCount = new();

    public static GameObject LocalPlayerInstance;

    public event Action<float> OnPlayerHpValueChanged;
    public event Action<int, int> OnPlayerAmmoChanged;
    public event Action OnPlayerIsDead;

    public float Max_Health { get; } = 100f;
    public int Max_Bullets { get; } = 30;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!photonView.IsMine && !PhotonNetwork.IsConnected) return;

        IsFiring.OnValueChanged += Fire;

        Camera.main.transform.SetPositionAndRotation(_cameraTransform.position, _cameraTransform.rotation);
    }

    private void OnDestroy()
    {
        IsFiring.OnValueChanged -= Fire;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) IsFiring.SetValue(true);
        else IsFiring.SetValue(false);

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
        if (_axisVertical != 0) Move(_axisVertical);
        if (_axisHorizontal != 0) Strafe(_axisHorizontal);
        if (_scroll != 0.0f) RotateCharacter(_scroll);
    }

    private void Move(float axisVertical)
    {
        _rigidbody.AddForce(axisVertical * _playerSpeed * Time.deltaTime * Vector3.forward, ForceMode.VelocityChange);
    }

    private void Strafe(float axisHorizontal)
    {
        _rigidbody.AddForce(axisHorizontal * _playerSpeed * Time.deltaTime * Vector3.right, ForceMode.VelocityChange);
    }

    private void RotateCharacter(float scroll)
    {
        transform.Rotate(Vector3.up, scroll);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (other.TryGetComponent<BulletDiploma>(out var bullet))
        {
            HealthCheck(bullet);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(IsFiring.GetValue());
            stream.SendNext(Health.GetValue());
            stream.SendNext(IsDead.GetValue());
        }
        else
        {
            IsFiring.SetValue((bool)stream.ReceiveNext());
            IsDead.SetValue((bool)stream.ReceiveNext());
            var playerHealth = (float)stream.ReceiveNext();
            Health.SetValue(playerHealth);
        }
    }

    private async void Fire(bool isFiring)
    {
        if (!isFiring) return;

        if (BulletsCount.GetValue() == 0)
        {
            await Reload();
            return;
        }
        await StartBullet();
    }

    private void HealthCheck(BulletDiploma bullet)
    {
        var health = Health.GetValue() - bullet.bulletDamage;
        OnPlayerHpValueChanged?.Invoke(health);
        Health.SetValue(health);
        if (Health.GetValue() <= 0) gameObject.SetActive(false);
        OnPlayerIsDead?.Invoke();
    }

    private async Task Reload()
    {
        await Task.Run(() => WaitReload());
        BulletsCount.SetValue(Max_Bullets);
        IsFiring.SetValue(false);
    }

    private async Task StartBullet()
    {
        PhotonNetwork.Instantiate(_bulletPrefab.name, _bulletSpawnPoint.position, Quaternion.identity);
        var bulletsLeft = BulletsCount.GetValue() - 1;
        BulletsCount.SetValue(bulletsLeft);
        OnPlayerAmmoChanged?.Invoke(bulletsLeft, Max_Bullets);
        await Task.Run(() => FireRateWaiting());
        IsFiring.SetValue(false);
    }

    private Task FireRateWaiting() => Task.Delay(200);

    private Task WaitReload() => Task.Delay(1000);
}
