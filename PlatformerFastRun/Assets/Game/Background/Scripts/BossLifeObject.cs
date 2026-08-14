using UnityEngine;
public class BossLifeObject : MonoBehaviour
{
    [Header("Destroy Effect")]
    [SerializeField] GameObject destroyEffectPrefab;
    [SerializeField] Transform effectSpawnPoint; // if empty, uses this object's position

    public BossHealth BossHealth { get; private set; }
    void Awake()
    {
        BossHealth = GetComponentInParent<BossHealth>();
    }

    public void PlayDestroyEffect()
    {
        if (destroyEffectPrefab == null) return;
        Vector3 pos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
        Instantiate(destroyEffectPrefab, pos, Quaternion.identity);
    }
}