using UnityEngine;
using UnityEngine.InputSystem;

public class InputTester : MonoBehaviour
{
    [SerializeField] bool m_enableLogging = false;
    [SerializeField] bool m_joinActions = false;
    [SerializeField] InputActionAsset m_inputActionMap;
    [SerializeField] string[] m_actionNames;

    void FixedUpdate()
    {
        LogInputs();
    }

    void LogInputs()
    {
        if (m_inputActionMap == null || !m_enableLogging) return;
        string log = "";
        foreach (var name in m_actionNames)
        {
            InputAction action = m_inputActionMap.FindAction(name);

            if (action == null) continue;

            if(m_joinActions)
                log += $"Action: {name} - ({action.ReadValueAsObject()}) | ";
            else
                Debug.Log($"Action: {name} | Value: {action.ReadValueAsObject()}");
        }
    }
}
