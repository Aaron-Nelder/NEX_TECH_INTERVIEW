using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] Transform m_playerSpawn;
    [SerializeField] Transform m_player;

    [SerializeField] GameObject m_winObj;
    [SerializeField] GameObject m_loseObj;
    [SerializeField] float m_messageTime = 5.0f;

    [SerializeField] List<IDamageable> m_activeHostiles = new List<IDamageable>();

    float m_messageDur = 0;

    public void RegisterHostile(IDamageable hostile)
    {
        m_activeHostiles.Add(hostile);
    }

    public void UnregisterHostile(IDamageable hostile)
    {
        m_activeHostiles.Remove(hostile);

        if (m_activeHostiles.Count == 0)
        {
            m_winObj.SetActive(true);
            m_messageDur = 0;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        if (m_messageDur < m_messageTime)
            m_messageDur += Time.fixedDeltaTime;

        if (m_messageDur >= m_messageTime)
        {
            m_winObj.SetActive(false);
            m_loseObj.SetActive(false);
        }

    }

    public void RespawnPlayer()
    {
        m_player.SetPositionAndRotation(m_playerSpawn.position, m_playerSpawn.rotation);
        HealthController.PlayerHealthController.SetHealth(HealthController.PlayerHealthController.GetMaxHealth());
        m_loseObj.SetActive(true);
        m_messageDur = 0;
    }
}
