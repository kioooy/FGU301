using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Platform : MonoBehaviour
{
    public static event Action<Platform> OnPlatformClicked;
    [SerializeField] private LayerMask platformLayerMask;
    public static bool towerPanelOpen { get; set; } = false;

    private void Update()
    {
        if (towerPanelOpen || Time.timeScale == 0f)
            return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D raycastHit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, platformLayerMask);

            if (raycastHit.collider != null)
            {
                Platform platform = raycastHit.collider.GetComponent<Platform>();
                if (platform != null)
                {
                    OnPlatformClicked?.Invoke(platform);
                }
            }
        }
    }
    public void PlaceTower(TowerData data)
    {
        Vector3 platformPosition = transform.position;
        GameObject towerInstance = Instantiate(data.prefab, platformPosition, Quaternion.identity);

        Transform towerBase = towerInstance.transform.Find("Tower");
        if (towerBase != null)
        {
            Vector3 towerBaseOffset = towerBase.localPosition;
            towerInstance.transform.position = platformPosition - towerBaseOffset;
        }

        // Link platform to the tower and hide platform
        Tower tower = towerInstance.GetComponent<Tower>();
        if (tower != null)
        {
            tower.SetPlatform(this);
        }
        
        gameObject.SetActive(false);
    }
}
