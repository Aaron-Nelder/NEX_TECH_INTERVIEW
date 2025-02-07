using System;
using UnityEngine;

public class HealthController : MonoBehaviour, IDamageable
{
    public static HealthController PlayerHealthController { get; private set; }
    [SerializeField] float m_maxHealth = 100.0f;
    [SerializeField] float m_health = 100.0f;
    [SerializeField] bool m_destroyOnDeath = false;
    [SerializeField] bool m_isPlayer = false;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetHealth(m_maxHealth);

        if (!m_isPlayer)
            GameManager.Instance.RegisterHostile(this);
        else if (PlayerHealthController == null)
            PlayerHealthController = this;
    }

    public float GetHealth() => m_health;
    public float GetMaxHealth() => m_maxHealth;

    public void SetHealth(float health)
    {
        m_health = Mathf.Clamp(health, 0, m_maxHealth);
        OnHealthChanged?.Invoke(m_health);
    }

    public void TakeDamage(float damage)
    {
        m_health -= damage;
        m_health = Mathf.Clamp(m_health, 0, m_maxHealth);
        OnHealthChanged?.Invoke(m_health);
        if (m_health <= 0)
        {
            OnDeath?.Invoke();

            if (m_isPlayer)
            {
                GameManager.Instance.RespawnPlayer();
                return;
            }

            if (m_destroyOnDeath)
            {
                GameManager.Instance.UnregisterHostile(this);
                Destroy(transform.gameObject);
            }
        }
    }
}

public interface IDamageable
{
    public void TakeDamage(float damage);
    public void SetHealth(float health);
    public float GetHealth();
    public float GetMaxHealth();
    public event Action OnDeath;
}
