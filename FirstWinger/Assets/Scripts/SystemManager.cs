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
    EnemyTable enemyTable;

    public EnemyTable EnemyTable
    {
        get
        {
            return enemyTable;
        }
    }

    [SerializeField]
    ItemTable itemTable;

    public ItemTable ItemTable
    {
        get
        {
            return itemTable;
        }
    }

    [SerializeField]
    ItemDropTable itemDropTable;

    public ItemDropTable ItemDropTable
    {
        get
        {
            return itemDropTable;
        }
    }

    BaseSceneMain currentSceneMain;

    public BaseSceneMain CurrentSceneMain
    {
        set
        {
            currentSceneMain = value;
        }
    }

    [SerializeField]
    NetworkConnectionInfo connectionInfo = new NetworkConnectionInfo();

    public NetworkConnectionInfo ConnectionInfo
    {
        get
        {
            return connectionInfo;
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

    void Start()
    {
        BaseSceneMain baseSceneMain = GameObject.FindObjectOfType<BaseSceneMain>();
        Debug.Log("OnSceneLoaded ! baseSceneMain.name = " + baseSceneMain.name);
        SystemManager.Instance.CurrentSceneMain = baseSceneMain;
    }

    public T GetCurrentSceneMain<T>()
        where T : BaseSceneMain
    {
        return currentSceneMain as T;
    }
}
