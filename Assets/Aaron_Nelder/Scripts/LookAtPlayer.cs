using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] Transform m_playerTransform;
    [SerializeField] float m_rotationSpeed = 1.0f;
    [SerializeField] float m_lookAtRange = 5.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_playerTransform == null) return;

        Vector3 direction = m_playerTransform.position - transform.position;

        if (direction.magnitude <= m_lookAtRange)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);
        }
    }
}
