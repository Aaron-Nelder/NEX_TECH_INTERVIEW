using UnityEngine;

public class PistolLaser : MonoBehaviour
{
    [SerializeField] LineRenderer m_lineRenderer;
    [SerializeField] Transform m_dotTransform;
    [SerializeField] MeshRenderer m_dotRenderer;
    [SerializeField] Transform m_bulletSpawnPoint;
    [SerializeField] LayerMask m_damageableLayer;

    [Header("Colors")]
    [SerializeField] Color m_damageableColor;
    [SerializeField] Color m_nonDamageableColor;

    Material m_dotMaterial;
    float m_laserRange = 100.0f;
    bool m_isInitialized = false;
    bool m_isDamageable = false;

    public void Init(float range)
    {
        m_laserRange = range;
        m_dotMaterial = m_dotRenderer.material;
        m_dotMaterial.SetColor("_DamageableColor", m_damageableColor);
        m_dotMaterial.SetColor("_NonDamageableColor", m_nonDamageableColor);
        m_isInitialized = true;

        SetState(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!m_isInitialized) return;

        // Set the start point of the line renderer
        m_lineRenderer.SetPosition(0, transform.position);

        if (Physics.Raycast(m_bulletSpawnPoint.position, m_bulletSpawnPoint.forward, out RaycastHit hit, m_laserRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            // check if the hit object is damageable
            if (!m_isDamageable && (m_damageableLayer & 1 << hit.collider.gameObject.layer) != 0)
                SetState(true);
            else if (m_isDamageable && (m_damageableLayer & 1 << hit.collider.gameObject.layer) == 0)
                SetState(false);

            m_lineRenderer.SetPosition(1, hit.point);
            m_dotTransform.position = hit.point + hit.normal * 0.01f;
            m_dotTransform.rotation = Quaternion.LookRotation(-hit.normal);
        }
        else
        {
            if (m_isDamageable)
                SetState(false, false);

            m_lineRenderer.SetPosition(1, transform.position + transform.forward * m_laserRange);
        }
    }

    void SetState(bool isDamageable, bool enableDot = true)
    {
        m_isDamageable = isDamageable;
        m_dotMaterial.SetFloat("_IsDamageable", isDamageable ? 1 : 0);
        SetLineColor(isDamageable ? m_damageableColor : m_nonDamageableColor);
        m_dotTransform.gameObject.SetActive(enableDot);
    }

    void SetLineColor(Color col)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(col, 0.0f), new GradientColorKey(col, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        m_lineRenderer.colorGradient = gradient;
    }
}
