using Photon.Pun;
using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CameraController), typeof(CharacterController))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private BulletDiploma _bulletPrefab;
    [SerializeField] private Transform _bulletSpawnPoint;


    public ReactiveProperty<bool> IsFiring = new();
    public ReactiveProperty<bool> IsDead = new();
    public ReactiveProperty<float> Health = new();
    public ReactiveProperty<int> BulletsCount = new();
    private const int Max_Bullets = 30;

    public static GameObject LocalPlayerInstance;

    public event Action<float> OnPlayerHpValueChanged;
    public event Action<int, int> OnPlayerAmmoChanged;

    public float Max_Health { get; } = 100f;

    public void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        IsFiring.OnValueChanged += Fire;
        CameraController _cameraController = gameObject.GetComponent<CameraController>();

        if (_cameraController != null)
        {
            if (photonView.IsMine)
            {
                _cameraController.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("Missing CameraController Component on player Prefab.", this);
        }
    }

    private void OnDestroy()
    {
        IsFiring.OnValueChanged -= Fire;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0)) IsFiring.SetValue(true);
        else IsFiring.SetValue(false);
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

    private void HealthCheck(BulletDiploma bullet)
    {
        var health = Health.GetValue() - bullet.bulletDamage;
        Health.SetValue(health);
        if (Health.GetValue() <= 0) gameObject.SetActive(false);

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
