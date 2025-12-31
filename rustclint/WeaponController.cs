using UnityEngine;
using System.Collections;
using RustlikeClient.Network;

namespace RustlikeClient.Combat
{
    /// <summary>
    /// Controla disparo, mira e reload de armas no cliente
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform weaponHolder;
        public GameObject currentWeaponModel;

        [Header("Weapon Settings")]
        public int currentWeaponItemId = -1;
        public WeaponData currentWeaponData;

        [Header("Shooting")]
        public float shootCooldown = 0.5f;
        public LayerMask hitLayers;
        public float maxShootDistance = 100f;

        [Header("Recoil")]
        public float recoilAmount = 2f;
        public float recoilSpeed = 5f;
        public float recoilRecoverySpeed = 3f;

        [Header("Effects")]
        public GameObject muzzleFlashPrefab;
        public GameObject hitEffectPrefab;
        public GameObject bloodEffectPrefab;

        [Header("UI")]
        public UnityEngine.UI.Image crosshair;
        public Color normalCrosshairColor = Color.white;
        public Color hitCrosshairColor = Color.red;

        [Header("Audio")]
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;

        private AudioSource _audioSource;
        private float _lastShootTime;
        private bool _isReloading;
        private Vector3 _currentRecoil;
        private Coroutine _recoilCoroutine;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
        }

        private void Update()
        {
            // Não atira se UI estiver aberta
            bool uiOpen = UI.InventoryManager.Instance != null && 
                          UI.InventoryManager.Instance.IsInventoryOpen();
            
            if (uiOpen) return;

            HandleInput();
            ApplyRecoilRecovery();
        }

        private void HandleInput()
        {
            if (currentWeaponData == null) return;

            // Disparo
            if (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && currentWeaponData.isAutomatic))
            {
                TryShoot();
            }

            // Reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload();
            }
        }

        /// <summary>
        /// Tenta disparar a arma
        /// </summary>
        public void TryShoot()
        {
            if (_isReloading)
            {
                Debug.Log("[WeaponController] Recarregando...");
                return;
            }

            if (Time.time - _lastShootTime < shootCooldown)
            {
                return;
            }

            if (currentWeaponData == null)
            {
                Debug.LogWarning("[WeaponController] Nenhuma arma equipada");
                return;
            }

            // Verifica munição (se necessário)
            if (currentWeaponData.requiresAmmo)
            {
                if (!HasAmmo())
                {
                    PlaySound(emptySound);
                    Debug.Log("[WeaponController] Sem munição!");
                    
                    if (UI.NotificationManager.Instance != null)
                    {
                        UI.NotificationManager.Instance.ShowWarning("Sem munição!");
                    }
                    
                    return;
                }
            }

            _lastShootTime = Time.time;

            // Raycast para detectar hit
            PerformRaycast();

            // Efeitos visuais/sonoros
            PlayShootEffects();

            // Recoil
            ApplyRecoil();
        }

        /// <summary>
        /// Raycast para detectar acerto
        /// </summary>
        private void PerformRaycast()
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxShootDistance, hitLayers))
            {
                Debug.Log($"[WeaponController] 🎯 Hit: {hit.collider.name} @ {hit.point}");

                // Verifica se acertou um jogador
                var otherPlayer = hit.collider.GetComponent<NetworkPlayerSync>();

                if (otherPlayer != null)
                {
                    // Verifica se foi headshot
                    bool isHeadshot = IsHeadshot(hit.point, hit.collider.transform);

                    // Envia ataque para servidor
                    SendAttackToServer(otherPlayer, hit.point, isHeadshot);

                    // Efeito de sangue
                    SpawnEffect(bloodEffectPrefab, hit.point, hit.normal);

                    // Feedback visual
                    ShowHitMarker(isHeadshot);
                }
                else
                {
                    // Acertou ambiente
                    SpawnEffect(hitEffectPrefab, hit.point, hit.normal);
                }
            }
            else
            {
                Debug.Log("[WeaponController] Miss!");
            }
        }

        /// <summary>
        /// Verifica se o hit foi headshot
        /// </summary>
        private bool IsHeadshot(Vector3 hitPoint, Transform target)
        {
            // Procura por collider de head no target
            var headCollider = target.GetComponentInChildren<Collider>();
            if (headCollider != null && headCollider.CompareTag("Head"))
            {
                return true;
            }

            // Fallback: verifica altura (acima de 1.6m do chão)
            float heightFromGround = hitPoint.y - target.position.y;
            return heightFromGround > 1.6f;
        }

        /// <summary>
        /// Envia ataque para servidor
        /// </summary>
        private async void SendAttackToServer(Network.NetworkPlayerSync victim, Vector3 hitPosition, bool isHeadshot)
        {
            // Pega ID do outro jogador (assumindo que está armazenado no NetworkPlayerSync)
            int victimId = victim.GetPlayerId();

            var packet = new Network.AttackRequestPacket
            {
                VictimId = victimId,
                WeaponItemId = currentWeaponItemId,
                HitPosX = hitPosition.x,
                HitPosY = hitPosition.y,
                HitPosZ = hitPosition.z,
                IsHeadshot = isHeadshot
            };

            Debug.Log($"[WeaponController] 📤 Enviando ataque: Vítima={victimId}, Arma={currentWeaponItemId}, Headshot={isHeadshot}");

            await Network.NetworkManager.Instance.SendPacketAsync(
                Network.PacketType.AttackRequest,
                packet.Serialize(),
                LiteNetLib.DeliveryMethod.ReliableOrdered
            );
        }

        /// <summary>
        /// Tenta recarregar a arma
        /// </summary>
        public async void TryReload()
        {
            if (_isReloading) return;

            if (currentWeaponData == null || !currentWeaponData.requiresAmmo)
            {
                Debug.Log("[WeaponController] Arma não precisa de reload");
                return;
            }

            if (!HasAmmo())
            {
                Debug.Log("[WeaponController] Sem munição para recarregar");
                
                if (UI.NotificationManager.Instance != null)
                {
                    UI.NotificationManager.Instance.ShowWarning("Sem munição!");
                }
                
                return;
            }

            Debug.Log($"[WeaponController] 🔄 Recarregando {currentWeaponData.weaponName}...");

            _isReloading = true;

            // Envia pedido de reload ao servidor
            var packet = new Network.ReloadRequestPacket
            {
                WeaponItemId = currentWeaponItemId
            };

            await Network.NetworkManager.Instance.SendPacketAsync(
                Network.PacketType.ReloadRequest,
                packet.Serialize(),
                LiteNetLib.DeliveryMethod.ReliableOrdered
            );

            // Animação/som de reload
            PlaySound(reloadSound);

            // Aguarda tempo de reload
            if (currentWeaponData.reloadTime > 0)
            {
                await System.Threading.Tasks.Task.Delay((int)(currentWeaponData.reloadTime * 1000));
            }

            _isReloading = false;

            Debug.Log("[WeaponController] ✅ Reload completo!");
        }

        /// <summary>
        /// Verifica se tem munição
        /// </summary>
        private bool HasAmmo()
        {
            if (UI.InventoryManager.Instance == null) return false;
            return UI.InventoryManager.Instance.HasItem(currentWeaponData.ammoItemId);
        }

        /// <summary>
        /// Aplica recoil
        /// </summary>
        private void ApplyRecoil()
        {
            if (_recoilCoroutine != null)
            {
                StopCoroutine(_recoilCoroutine);
            }

            _recoilCoroutine = StartCoroutine(RecoilCoroutine());
        }

        private IEnumerator RecoilCoroutine()
        {
            float recoilX = Random.Range(-recoilAmount, recoilAmount) * 0.3f;
            float recoilY = Random.Range(recoilAmount * 0.5f, recoilAmount);

            _currentRecoil += new Vector3(-recoilY, recoilX, 0);

            float elapsed = 0f;
            float duration = 0.1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (playerCamera != null)
                {
                    playerCamera.transform.localRotation *= Quaternion.Euler(
                        Mathf.Lerp(0, -recoilY, t),
                        Mathf.Lerp(0, recoilX, t),
                        0
                    );
                }

                yield return null;
            }
        }

        private void ApplyRecoilRecovery()
        {
            if (_currentRecoil.magnitude > 0.01f)
            {
                _currentRecoil = Vector3.Lerp(_currentRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
            }
        }

        /// <summary>
        /// Efeitos de disparo
        /// </summary>
        private void PlayShootEffects()
        {
            PlaySound(shootSound);

            if (muzzleFlashPrefab != null && weaponHolder != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, weaponHolder.position, weaponHolder.rotation);
                Destroy(flash, 0.1f);
            }

            // Animação de disparo (se tiver)
            if (currentWeaponModel != null)
            {
                var animator = currentWeaponModel.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("Shoot");
                }
            }
        }

        /// <summary>
        /// Spawna efeito no mundo
        /// </summary>
        private void SpawnEffect(GameObject effectPrefab, Vector3 position, Vector3 normal)
        {
            if (effectPrefab == null) return;

            GameObject effect = Instantiate(effectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(effect, 2f);
        }

        /// <summary>
        /// Mostra indicador de acerto
        /// </summary>
        private void ShowHitMarker(bool isHeadshot)
        {
            if (crosshair != null)
            {
                StopAllCoroutines();
                StartCoroutine(HitMarkerCoroutine(isHeadshot));
            }
        }

        private IEnumerator HitMarkerCoroutine(bool isHeadshot)
        {
            Color originalColor = crosshair.color;
            Color hitColor = isHeadshot ? Color.yellow : hitCrosshairColor;

            crosshair.color = hitColor;

            if (isHeadshot)
            {
                // Aumenta tamanho para headshot
                Vector3 originalScale = crosshair.transform.localScale;
                crosshair.transform.localScale = originalScale * 1.5f;

                yield return new WaitForSeconds(0.1f);

                crosshair.transform.localScale = originalScale;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }

            crosshair.color = originalColor;
        }

        /// <summary>
        /// Toca som
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

public void EquipWeapon(WeaponData weapon)
{
    currentWeaponData = weapon;
    currentWeaponItemId = weapon.itemId; // ⭐ IMPORTANTE
    
    Debug.Log($"[WeaponController] ⚔️ Equipou: {weapon.weaponName} (ID: {weapon.itemId})");

    // Spawna modelo 3D se tiver
    if (weapon.modelPrefab != null && weaponHolder != null)
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        currentWeaponModel = Instantiate(weapon.modelPrefab, weaponHolder);
        currentWeaponModel.transform.localPosition = Vector3.zero;
        currentWeaponModel.transform.localRotation = Quaternion.identity;
    }
}

public void UnequipWeapon()
{
    currentWeaponData = null;
    currentWeaponItemId = -1; // ⭐ IMPORTANTE

    if (currentWeaponModel != null)
    {
        Destroy(currentWeaponModel);
        currentWeaponModel = null;
    }

    Debug.Log("[WeaponController] Arma desequipada");
}

        /// <summary>
        /// Para debug
        /// </summary>
        private void OnGUI()
        {
            if (Input.GetKey(KeyCode.F10))
            {
                GUI.Box(new Rect(10, 950, 300, 120), "Weapon Controller (F10)");
                
                string weaponName = currentWeaponData != null ? currentWeaponData.weaponName : "None";
                GUI.Label(new Rect(20, 975, 280, 20), $"Weapon: {weaponName}");
                GUI.Label(new Rect(20, 995, 280, 20), $"Reloading: {_isReloading}");
                GUI.Label(new Rect(20, 1015, 280, 20), $"Cooldown: {Mathf.Max(0, shootCooldown - (Time.time - _lastShootTime)):F2}s");
                
                if (currentWeaponData != null && currentWeaponData.requiresAmmo)
                {
                    int ammoCount = UI.InventoryManager.Instance?.CountItem(currentWeaponData.ammoItemId) ?? 0;
                    GUI.Label(new Rect(20, 1035, 280, 20), $"Ammo: {ammoCount}");
                }
            }
        }
    }
}