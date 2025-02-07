using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PistolController : MonoBehaviour
{
    [SerializeField] Animator m_animator;
    [SerializeField] AudioSource m_source;
    [SerializeField] AudioClip m_fireSound;
    [SerializeField] AudioClip m_dropMagSound;
    [SerializeField] AudioClip m_equipMagSound;
    [SerializeField] AudioClip m_dryFireSound;
    [SerializeField] XRInputButtonReader m_xRInputButtonReader;
    [SerializeField] XRInputButtonReader m_magReleaseReader;
    [SerializeField] Transform m_bulletSpawnPoint;
    [SerializeField] PistolLaser m_pistolLaser;
    [SerializeField] float m_fireRate = 0.1f;
    [SerializeField] XRSocketInteractor m_socketInteractor;
    [SerializeField] PistolMag m_pistolMag;
    [SerializeField] float m_releaseMagTime = 0.5f;
    float m_releaseMagEndTime = 0;

    [Header("Bullet")]
    [SerializeField] GameObject m_bulletPrefab;
    [SerializeField] int m_pooledBullets = 10;

    [SerializeField] ProjectileData m_projectileData;

    [Header("Muzzle Flash")]
    [SerializeField] GameObject m_muzzleFlash;
    [SerializeField] float m_muzzleFlashTime = 0.1f;

    [Header("Casing")]
    [SerializeField] GameObject m_casingPrefab;
    [SerializeField] Transform m_casingSpawnPoint;
    [SerializeField] Vector3 m_maxCasingForce;
    [SerializeField] Vector3 m_minCasingForce;

    // pools
    Rigidbody[] m_casingPool;
    int m_bulletIndex = 0;

    float m_muzzleFlashEndTime = float.MaxValue;
    float m_lastFireTime = 0.0f;

    bool CanFire => Time.time >= m_lastFireTime && m_pistolMag?.CurrentAmmo > 0;
    int m_fireHash = Animator.StringToHash("Fire");
    int m_fireRateHash = Animator.StringToHash("FireRate");
    int m_isEmptyHash = Animator.StringToHash("IsEmpty");

    void Start()
    {
        InitPools();
        m_pistolLaser?.Init(m_projectileData.Velocity * m_projectileData.LifeTime);
    }

    void OnEnable()
    {
        m_xRInputButtonReader.inputActionReferencePerformed.action.performed += OnInputActionPerformed;
        m_magReleaseReader.inputActionReferencePerformed.action.performed += ReleaseMag;
        m_muzzleFlash.SetActive(false);
        m_animator.SetFloat(m_fireRateHash, 1.0f / m_fireRate);
        bool hasAmmo = m_pistolMag?.CurrentAmmo > 0;
        m_animator.SetBool(m_isEmptyHash, !hasAmmo);
        m_socketInteractor.selectEntered.AddListener(SelectEntered);
        m_socketInteractor.selectExited.AddListener(SelectExit);
    }

    void OnDisable()
    {
        m_xRInputButtonReader.inputActionReferencePerformed.action.performed -= OnInputActionPerformed;
        m_magReleaseReader.inputActionReferencePerformed.action.performed -= ReleaseMag;
        m_socketInteractor.selectEntered.RemoveListener(SelectEntered);
        m_socketInteractor.selectExited.RemoveListener(SelectExit);
    }

    void InitPools()
    {
        m_casingPool = new Rigidbody[m_pooledBullets];
        for (int i = 0; i < m_pooledBullets; i++)
        {
            GameObject casing = Instantiate(m_casingPrefab, m_casingSpawnPoint.position, Quaternion.identity);
            casing.SetActive(false);
            m_casingPool[i] = casing.GetComponent<Rigidbody>();
        }
    }

    void Fire()
    {
        EjectCasing();
        GameObject bullet = Instantiate(m_bulletPrefab);
        ProjectileController projectile = bullet.GetComponent<ProjectileController>();
        projectile.Init(m_projectileData, this);
        projectile.transform.SetPositionAndRotation(m_bulletSpawnPoint.position, m_bulletSpawnPoint.rotation);
        projectile.FireProjectile();
        m_source.PlayOneShot(m_fireSound);

        m_animator.SetTrigger(m_fireHash);
        m_lastFireTime = Time.time + m_fireRate;
        ToggleMuzzleFlash(true);

        m_pistolMag.CurrentAmmo--;

        if (m_pistolMag?.CurrentAmmo <= 0)
        {
            m_animator.SetBool(m_isEmptyHash, true);
            m_pistolMag.CurrentAmmo = 0;
        }
    }

    void EjectCasing()
    {
        Rigidbody casing = m_casingPool[m_bulletIndex];
        casing.gameObject.SetActive(true);
        casing.transform.SetPositionAndRotation(m_casingSpawnPoint.position, m_casingSpawnPoint.rotation);
        casing.linearVelocity = Vector3.zero;
        casing.angularVelocity = Vector3.zero;
        Vector3 force = new Vector3(Random.Range(m_minCasingForce.x, m_maxCasingForce.x), Random.Range(m_minCasingForce.y, m_maxCasingForce.y), Random.Range(m_minCasingForce.z, m_maxCasingForce.z));
        force = m_casingSpawnPoint.TransformDirection(force);
        casing.AddForce(force, ForceMode.Impulse);
    }

    void OnInputActionPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (CanFire)
            Fire();
        else
            m_source.PlayOneShot(m_dryFireSound);
    }

    void ToggleMuzzleFlash(bool enabled)
    {
        m_muzzleFlashEndTime = enabled ? Time.time + m_muzzleFlashTime : float.MaxValue;
        m_muzzleFlash.SetActive(enabled);
    }

    void Update()
    {
        if (Time.time >= m_muzzleFlashEndTime)
            ToggleMuzzleFlash(false);

        if (Time.time >= m_releaseMagEndTime)
            m_socketInteractor.socketActive = true;
    }

    public void ReleaseMag(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (m_pistolMag is null) return;
        m_pistolMag.transform.SetParent(null);
        m_pistolMag = null;
        m_releaseMagEndTime = Time.time + m_releaseMagTime;
        m_animator.SetBool(m_isEmptyHash, true);
        WristUI.Instance.SetAmmoText(0, false);
        m_source.PlayOneShot(m_dropMagSound);
        m_socketInteractor.socketActive = false;
    }

    public void SelectEntered(SelectEnterEventArgs args)
    {
        PistolMag mag = args.interactableObject.transform.GetComponent<PistolMag>();
        if (mag is not null)
        {
            m_pistolMag = mag;
            mag.transform.SetParent(m_socketInteractor.transform);
            if (m_pistolMag.CurrentAmmo > 0)
                m_animator.SetBool(m_isEmptyHash, false);

            WristUI.Instance.SetAmmoText(m_pistolMag.CurrentAmmo);
            m_source.PlayOneShot(m_equipMagSound);
        }
    }

    public void SelectExit(SelectExitEventArgs args)
    {
        if (args.interactableObject.transform.GetComponent<PistolMag>())
        {
            ReleaseMag(new());
        }
    }
}
