using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WristUI : MonoBehaviour
{
    public static WristUI Instance { get; private set; }

    [Header("Health")]
    [SerializeField] HealthController m_healthController;
    [SerializeField] Image m_healthBar;
    [SerializeField] Color m_maxHealthColor;
    [SerializeField] Color m_minHealthColor;

    [SerializeField] TMP_Text m_ammoText;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        m_healthController.OnHealthChanged += OnHealthChanged;
        OnHealthChanged(m_healthController.GetHealth());
    }

    void OnDisable()
    {
        m_healthController.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float health)
    {
        float fillAmount = health / m_healthController.GetMaxHealth();
        m_healthBar.color = Color.Lerp(m_minHealthColor, m_maxHealthColor, fillAmount);
        m_healthBar.fillAmount = fillAmount;
    }

    public void SetAmmoText(int ammo, bool display = true)
    {
        m_ammoText.text = ammo.ToString();
        m_ammoText.gameObject.SetActive(display);
    }

}
