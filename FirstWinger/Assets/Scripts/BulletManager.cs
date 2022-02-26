using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public const int PlayerBulletIndex = 0;
    public const int EnemyBulletIndex = 1;
    public const int PlayerBombIndex = 2;
    public const int BossBulletIndex = 3;
    public const int GuidedMissileIndex = 4;

    [SerializeField]
    PrefabCacheData[] bulletFiles;

    Dictionary<string, GameObject> FileCache = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //Prepare();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject Load(string resourcePath)
    {
        GameObject go = null;

        if(FileCache.ContainsKey(resourcePath)) //ĳ�� Ȯ��
        {
            go = FileCache[resourcePath];
        }
        else
        {
            //ĳ�ÿ� �����Ƿ� �ε�
            go = Resources.Load<GameObject>(resourcePath);
            if(!go)
            {
                Debug.LogError("Load error! path = " + resourcePath);
                return null;
            }
            //�ε� �� ĳ�ÿ� ����
            FileCache.Add(resourcePath, go);
        }
        return go;
    }

    public void Prepare()
    {
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return;

        for(int i = 0; i < bulletFiles.Length; i++)
        {
            GameObject go = Load(bulletFiles[i].filePath);
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.GenerateCache(bulletFiles[i].filePath, go, bulletFiles[i].cacheCount, this.transform);
        }
    }

    public Bullet Generate(int index)
    {
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return null;

        string filePath = bulletFiles[index].filePath;
        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.Archive(filePath);

        Bullet bullet = go.GetComponent<Bullet>();

        return bullet;
    }

    public bool Remove(Bullet bullet)
    {
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
            return true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletCacheSystem.Restore(bullet.FilePath, bullet.gameObject);
        return true;
    }
}
