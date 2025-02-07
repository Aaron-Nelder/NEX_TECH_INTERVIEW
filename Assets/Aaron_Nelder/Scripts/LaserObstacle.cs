using System.Collections.Generic;
using UnityEngine;

public class LaserObstacle : MonoBehaviour
{
    [SerializeField] float m_damage = 10.0f;
    [SerializeField] float m_damageInterval = 0.5f;
    [SerializeField] float m_range = 10.0f;
    [SerializeField] LayerMask m_damagableLayers;
    [SerializeField] LineRenderer m_lineRenderer;
    [SerializeField] LayerMask m_ReflectiveLayer;
    [SerializeField] int m_maxReflections = 3;
    float m_lastDamageTime = 0.0f;

    // Update is called once per frame
    void Update()
    {
        m_lineRenderer.positionCount = 0;

        List<Vector3> positions = new List<Vector3>();
        positions.Add(transform.position);

        Vector3 reflection = Vector3.zero;
        for (int i = 1; i < m_maxReflections; i++)
        {
            Vector3 startPos = positions[i - 1];
            Vector3 direction = i == 1 ? transform.forward : startPos - positions[i - 2];
            direction = reflection == Vector3.zero ? direction : reflection;
            direction.Normalize();
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, m_range, m_ReflectiveLayer, QueryTriggerInteraction.Ignore))
            {
                positions.Add(hit.point);
                reflection = Vector3.Reflect(direction, hit.normal);
            }
            else if (Physics.Raycast(startPos, direction, out hit, m_range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                positions.Add(hit.point);
                break;
            }
            else
            {
                positions.Add(startPos + (direction * m_range));
                break;
            }
        }

        m_lineRenderer.positionCount = positions.Count;
        m_lineRenderer.SetPositions(positions.ToArray());

        // performs a linecast to check for damageable objects
        if (Time.time >= m_lastDamageTime + m_damageInterval)
            for (int i = 1; i < positions.Count; i++)
                if (Physics.Linecast(positions[i - 1], positions[i], out RaycastHit hit, m_damagableLayers, QueryTriggerInteraction.Ignore))
                    TryDealDamage(hit.transform);
    }

    void TryDealDamage(Transform hitTransform)
    {
        hitTransform.GetComponent<IDamageable>()?.TakeDamage(m_damage);
        m_lastDamageTime = Time.time;
    }
}
