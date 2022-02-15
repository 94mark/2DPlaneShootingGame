using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSceneMain : BaseSceneMain
{
    [SerializeField]
    Player player;

    public Player Hero
    {
        get
        {
            if(!player)
            {
                Debug.LogError("Main Player is not setted!");
            }
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
}
