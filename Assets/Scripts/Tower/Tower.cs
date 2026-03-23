using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    private CircleCollider2D _circleCollider;

    private List<Enemy> _enemiesInRange;
    private ObjectPooler _projectilePool;
    private float _shootTimer;
    [SerializeField] private CircleCollider2D rangeCollider;

    public int Level { get; private set; } = 1;
    public int MaxLevel { get; private set; } = 5;
    private Platform _platform;
    public TowerData Data => data;
    public static event System.Action<Tower> OnTowerClicked;

    // Range ring drawn at runtime via LineRenderer
    private LineRenderer _rangeRing;

    public float CurrentDamage => data.damage * (1 + (Level - 1) * 0.5f);
    public int UpgradeCost => Mathf.CeilToInt(data.cost * Mathf.Pow(1.5f, Level - 1));
    public int SellValue => Mathf.CeilToInt(data.cost * 0.6f + (Level - 1) * 0.4f * data.cost);

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        if (rangeCollider == null) rangeCollider = GetComponent<CircleCollider2D>();
        
        if (rangeCollider != null)
        {
            rangeCollider.radius = data.range;
            rangeCollider.isTrigger = true;
        }
        
        _enemiesInRange = new List<Enemy>();
        _projectilePool = GetComponent<ObjectPooler>();
        _shootTimer = data.shootinterval;

        // Create range ring using LineRenderer
        GameObject ringGO = new GameObject("RangeRing");
        ringGO.transform.SetParent(transform);
        ringGO.transform.localPosition = Vector3.zero;
        ringGO.transform.localRotation = Quaternion.identity;
        ringGO.transform.localScale = Vector3.one;

        _rangeRing = ringGO.AddComponent<LineRenderer>();
        _rangeRing.useWorldSpace = false;
        _rangeRing.loop = true;
        _rangeRing.widthMultiplier = 0.05f;
        _rangeRing.sortingLayerName = "Default";
        _rangeRing.sortingOrder = 10;
        _rangeRing.material = new Material(Shader.Find("Sprites/Default"));
        _rangeRing.startColor = new Color(1f, 1f, 0f, 0.6f);
        _rangeRing.endColor   = new Color(1f, 1f, 0f, 0.6f);

        int segments = 48;
        _rangeRing.positionCount = segments;
        float radius = data.range;
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            _rangeRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
        }
        _rangeRing.gameObject.SetActive(false);

        // Tạo BoxCollider2D cho thân trụ (non-trigger) để UIController dùng Physics2D phát hiện click
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        BoxCollider2D bodyCollider = gameObject.AddComponent<BoxCollider2D>();
        bodyCollider.isTrigger = false;
        if (sr != null)
        {
            bodyCollider.size   = sr.bounds.size;
            bodyCollider.offset = (Vector2)(sr.bounds.center - transform.position);
        }
    }

    private void Update()
    {
        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0)
        {
            _shootTimer = data.shootinterval;
            Shoot();
        }
    }

    // Click được xử lý từ UIController qua Physics2D.OverlapPoint
    public void NotifyClicked()
    {
        OnTowerClicked?.Invoke(this);
    }

    public void Select()
    {
        if (_rangeRing != null) _rangeRing.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        if (_rangeRing != null) _rangeRing.gameObject.SetActive(false);
    }

    public void Upgrade()
    {
        if (Level >= MaxLevel) return;
        Level++;
        // Range scale update if needed
        Debug.Log($"Tower Upgraded to Level {Level}! Damage: {CurrentDamage}");
    }

    public void Sell()
    {
        if (_platform != null)
        {
            _platform.gameObject.SetActive(true);
        }
        Destroy(gameObject);
    }

    public void SetPlatform(Platform platform)
    {
        _platform = platform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            _enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }
        _enemiesInRange.RemoveAll(e => e == null);
    }

    private void Shoot()
    {
        _enemiesInRange.RemoveAll(e => e == null);

        while (_enemiesInRange.Count > 0 && (_enemiesInRange[0] == null || !_enemiesInRange[0].gameObject.activeInHierarchy))
        {
            _enemiesInRange.RemoveAt(0);
        }

        if (_enemiesInRange.Count > 0)
        {
            GameObject projectile = _projectilePool.GetPooledObject();
            if (projectile == null)
            {
                Debug.LogWarning("Projectile Pool is empty or null!");
                return;
            }
            
            projectile.transform.position = transform.position;
            projectile.SetActive(true);
            
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                Vector2 _shootDirection = (_enemiesInRange[0].transform.position - transform.position).normalized;
                projectileScript.Shoot(data, _shootDirection, CurrentDamage);
            }
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesInRange.Remove(enemy);
    }
}
