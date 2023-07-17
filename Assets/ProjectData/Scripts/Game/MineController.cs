using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;

public class MineController : MonoBehaviourPunCallbacks, IDealDamage
{
    [SerializeField] private float _mineDamage;
    [SerializeField] private int _mineChargeDelay;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private float _explosionForce;
    [SerializeField] private AudioSource _audioSource;

    private bool _isActive = false;

    public float Damage { get => _mineDamage; }
    public bool IsActive { get => _isActive; set => _isActive = value; }

    public GameObject GameObject { get => gameObject; }
    public bool IsCharged { get => _isActive; }

    private void Start()
    {
        WaitMineIsReadyAsync();
    }

    private async void WaitMineIsReadyAsync()
    {
        await Task.Run(() => Task.Delay(_mineChargeDelay));
        _isActive = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out IDealDamage damager) && photonView.IsMine)
        {
            _audioSource.Play();
            PLayerAwaiter();
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController player)) player.TakeDamage(Damage);
        if (photonView.IsMine && player != null && _isActive)
        {
            _audioSource.Play();
            PLayerAwaiter();
            other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private async void PLayerAwaiter() => await Task.Run(() => Task.Delay(1000));
}
