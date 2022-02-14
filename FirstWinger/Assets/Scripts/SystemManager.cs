using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemManager : MonoBehaviour
{
    static SystemManager instance = null;

    public static SystemManager Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField]
    Player player;

    public Player Hero
    {
        get
        {
            return player;
        }
    }

    GamePointAccumulator gamePointAccumulator = new GamePointAccumulator();

    public GamePointAccumulator GamePointAccumulator
    {
        get
        {
            return gamePointAccumulator;
        }
    }

    [SerializeField]
    EffectManager effectManager;

    public EffectManager EffectManager
    {
        get
        {
            return effectManager;
        }
    }

    [SerializeField]
    EnemyManager enemyManager;

    public EnemyManager EnemyManager
    {
        get
        {
            return enemyManager;
        }
    }

    [SerializeField]
    BulletManager bulletManager;

    public BulletManager BulletManager
    {
        get
        {
            return bulletManager;
        }
    }

    [SerializeField]
    DamageManager damageManager;

    public DamageManager DamageManager
    {
        get
        {
            return damageManager;
        }
    }

    PrefabCacheSystem enemyCacheSystem = new PrefabCacheSystem();

    public PrefabCacheSystem EnemyCacheSystem
    {
        get
        {
            return enemyCacheSystem;
        }
    }

    PrefabCacheSystem bulletCacheSystem = new PrefabCacheSystem();

    public PrefabCacheSystem BulletCacheSystem
    {
        get
        {
            return bulletCacheSystem;
        }
    }

    PrefabCacheSystem effectCacheSystem = new PrefabCacheSystem();

    public PrefabCacheSystem EffectCacheSystem
    {
        get
        {
            return effectCacheSystem;
        }
    }

    PrefabCacheSystem damageCacheSystem = new PrefabCacheSystem();

    public PrefabCacheSystem DamageCacheSystem
    {
        get
        {
            return damageCacheSystem;
        }
    }

    [SerializeField]
    EnemyTable enemyTable;

    public EnemyTable EnemyTable
    {
        get
        {
            return enemyTable;
        }
    }

    public void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("SystemManager error! Singleton error!");
            Destroy(gameObject);
            return;
        }

        instance = this;

        //Scene 이동 간에 사라지지 않도록 처리
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
