using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour, IDisposable
{
    const float IMPACT_LIFETIME = 5.0f;

    [SerializeField] float m_bulletLength = 0.005f;
    [SerializeField] TrailRenderer m_trailRenderer;

    [Header("Layers")]
    [SerializeField] LayerMask m_interactableLayers;
    [SerializeField] LayerMask m_damagableLayers;
    [SerializeField] LayerMask m_enemyLayers;

    [Header("Impacts")]
    [SerializeField] GameObject m_normalImpactPrefab;
    [SerializeField] GameObject m_enemyImpactPrefab;
    GameObject m_currentImpact;

    ProjectileData m_projectileData;
    PistolController m_pistolController;

    bool m_isFired, m_isImpacted;
    float m_lifeTime, m_impactTime;

    public void Dispose()
    {
        Destroy(gameObject);
    }

    public void Init(ProjectileData data, PistolController pistol)
    {
        m_pistolController = pistol;
        m_projectileData = data;
    }

    public void FireProjectile()
    {
        m_lifeTime = Time.time + m_projectileData.LifeTime;
        m_impactTime = Time.time + IMPACT_LIFETIME;
        m_trailRenderer.emitting = true;
        for (int i = 0; i < m_trailRenderer.positionCount; i++)
            m_trailRenderer.SetPosition(i, transform.position);
        transform.SetParent(null);
        m_isFired = true;
    }

    void EnableImpact(RaycastHit hit, bool hitEnemy)
    {
        m_currentImpact = hitEnemy ? m_enemyImpactPrefab : m_normalImpactPrefab;
        m_currentImpact.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
        m_currentImpact.SetActive(true);
    }

    void Update()
    {
        if (!m_isFired)
            return;

        if (m_isImpacted)
        {
            if (Time.time >= m_impactTime)
                Dispose();
            return;
        }

        if (Time.time >= m_lifeTime)
        {
            Dispose();
            return;
        }

        m_isImpacted = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_projectileData.Velocity * Time.deltaTime, m_interactableLayers);

        if (m_isImpacted)
        {
            transform.position = hit.point + (transform.forward * m_bulletLength);
            transform.SetParent(hit.transform);

            bool hitEnemy = (m_enemyLayers & 1 << hit.collider.gameObject.layer) != 0;

            EnableImpact(hit, hitEnemy);

            if ((m_damagableLayers & 1 << hit.collider.gameObject.layer) != 0)
                OnDamagableHit(hit.transform, hitEnemy);

            hit.rigidbody?.AddForceAtPosition(-hit.normal * m_projectileData.Force, hit.point, ForceMode.Impulse);
            m_trailRenderer.emitting = false;
        }
        else
            transform.position += transform.forward * m_projectileData.Velocity * Time.deltaTime;
    }

    void OnDamagableHit(Transform hitTransform, bool isEnemy)
    {
        hitTransform.GetComponent<IDamageable>()?.TakeDamage(m_projectileData.Damage);
        // TODO:: Add enemy hit logic
    }
}

[Serializable]
public struct ProjectileData
{
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float LifeTime { get; private set; }
    [field: SerializeField] public float Velocity { get; private set; }
    [field: SerializeField] public float Force { get; private set; }
}
