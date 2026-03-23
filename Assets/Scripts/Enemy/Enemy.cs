using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public EnemyData Data => data;
    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWaypoint;
    private float _lives;
    private float _maxLives;

    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;

    // Lưu scale gốc để flip sprite đúng
    private Vector3 _originalLocalScale;

    private void Awake()
    {
        _currentPath = GameObject.Find("Path").GetComponent<Path>();
        _healthBarOriginalScale = healthBar.localScale;
        _originalLocalScale = transform.localScale;
    }

    // Reset về waypoint đầu mỗi khi quái được tái sử dụng từ pool
    private void OnEnable()
    {
        _currentWaypoint = 0;
        // Đặt z = 0 rõ ràng để tránh Z-fighting gây chớp nháy
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        _targetPosition = _currentPath.GetPosition(_currentWaypoint);

        // Reset lại scale về gốc (tránh inherit scale sai từ lần trước)
        transform.localScale = _originalLocalScale;
    }

    void Update()
    {
        Vector3 moveDir = _targetPosition - transform.position;

        // --- Fix moonwalk: flip sprite theo hướng di chuyển ---
        if (moveDir.x > 0.01f)
        {
            // Di chuyển sang phải → scale X dương
            transform.localScale = new Vector3(
                Mathf.Abs(_originalLocalScale.x),
                _originalLocalScale.y,
                _originalLocalScale.z);
        }
        else if (moveDir.x < -0.01f)
        {
            // Di chuyển sang trái → flip scale X âm
            transform.localScale = new Vector3(
                -Mathf.Abs(_originalLocalScale.x),
                _originalLocalScale.y,
                _originalLocalScale.z);
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.speed * Time.deltaTime);

        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if (relativeDistance < 0.1f)
        {
            if (_currentWaypoint < _currentPath.Waypoints.Length - 1)
            {
                _currentWaypoint++;
                _targetPosition = _currentPath.GetPosition(_currentWaypoint);
            }
            else
            {
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        _lives -= damage;
        _lives = Math.Max(_lives, 0);
        UpdateHealthBar();

        if (_lives <= 0)
        {
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        float healthPercent = _lives / _maxLives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }

    public void Initialize(float healthMultiplication)
    {
        _maxLives = data.lives * healthMultiplication;
        _lives = _maxLives;
        UpdateHealthBar();
    }
}
