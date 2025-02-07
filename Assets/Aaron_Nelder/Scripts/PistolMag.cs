using UnityEngine;

public class PistolMag : MonoBehaviour
{
    int m_currentAmmo = 15;
    public int CurrentAmmo 
    { 
        get => m_currentAmmo; 
        set 
        { 
            m_currentAmmo = value; 
            WristUI.Instance.SetAmmoText(m_currentAmmo);
        } 
    }
}