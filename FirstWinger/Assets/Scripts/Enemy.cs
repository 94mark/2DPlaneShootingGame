using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : Actor
{
    public enum State : int
    {
        None = -1,
        Ready = 0,
        Appear,
        Battle,
        Dead,
        Disappear
    }

    [SerializeField]
    [SyncVar]
    State CurrentState = State.None;

    const float MaxSpeed = 10.0f;

    const float MaxSpeedTime = 0.5f;

    [SerializeField]
    [SyncVar]
    Vector3 TargetPosition;

    [SerializeField]
    [SyncVar]
    float CurrentSpeed;

    [SyncVar]
    Vector3 CurrentVelocity;

    [SyncVar]
    float MoveStartTime = 0.0f;    

    [SerializeField]
    Transform FireTransform;

    [SerializeField]
    [SyncVar]
    float BulletSpeed = 1;

    [SyncVar]
    float LastActionUpdateTime = 0.0f;

    [SerializeField]
    [SyncVar]
    int FireRemainCount = 1;

    [SerializeField]
    [SyncVar]
    int GamePoint = 10;

    [SyncVar]
    [SerializeField]
    string filePath;

    public string FilePath
    {
        get
        {
            return filePath;
        }
        set
        {
            filePath = value;
        }
    }

    [SyncVar]
    Vector3 AppearPoint;
    [SyncVar]
    Vector3 DisappearPoint;

    [SerializeField]
    [SyncVar]
    float ItemDropRate;

    [SerializeField]
    [SyncVar]
    int ItemDropID;

    protected override void Initialize()
    {
        base.Initialize();
        //Debug.Log("Enemy : Initialize");

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        if(!((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {            
            transform.SetParent(inGameSceneMain.EnemyManager.transform);
            inGameSceneMain.EnemyCacheSystem.Add(FilePath, gameObject);
            gameObject.SetActive(false);
        }

        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);
    }

    protected override void UpdateActor()
    {      
        switch(CurrentState)
        {
            case State.None:
                break;
            case State.Ready:
                UpdateReady();
                break;
            case State.Dead:
                break;
            case State.Appear:
            case State.Disappear:
                UpdateSpeed();
                UpdateMove();
                break;
            case State.Battle:
                UpdateBattle();
                break;
            default:
                Debug.LogError("Undefined State!");
                break;
        }        
    }

    void UpdateSpeed()
    {
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, MaxSpeed, (Time.time - MoveStartTime) / MaxSpeedTime);
    }

    void UpdateMove()
    {
        float distance = Vector3.Distance(TargetPosition, transform.position);
        if(distance == 0)
        {
            Arrived();
            return;
        }
        CurrentVelocity = (TargetPosition - transform.position).normalized * CurrentSpeed;

        //속도 = 거리 / 시간 이므로 시간 = 거리 / 속도
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref CurrentVelocity, distance / CurrentSpeed, MaxSpeed);
    }

    void Arrived()
    {
        CurrentSpeed = 0.0f;
        if(CurrentState == State.Appear)
        {
            CurrentState = State.Battle;
            LastActionUpdateTime = Time.time;
        }
        else //if (CurrentState = State.Disappear)
        {
            CurrentState = State.None;
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveEnemy(this);
        }
    }

    public void Reset(SquadronMemberStruct data)
    {
        if(isServer)
        {
            RpcReset(data);
        }
        else
        {
            CmdReset(data);
            if (isLocalPlayer)
                ResetData(data);
        }
    }

    void ResetData(SquadronMemberStruct data)
    {
        EnemyStruct enemyStruct = SystemManager.Instance.EnemyTable.GetEnemy(data.EnemyID);

        CurrentHp = MaxHP = enemyStruct.MaxHP;
        Damage = enemyStruct.Damage;
        crashDamage = enemyStruct.CrashDamage;
        BulletSpeed = enemyStruct.BulletSpeed;
        FireRemainCount = enemyStruct.FireRemainCount;
        GamePoint = enemyStruct.GamePoint;

        AppearPoint = new Vector3(data.AppearPointX, data.AppearPointY, 0);
        DisappearPoint = new Vector3(data.DisappearPointX, data.DisappearPointY, 0);

        ItemDropRate = enemyStruct.ItemDropRate;
        ItemDropID = enemyStruct.ItemDropID;

        CurrentState = State.Ready;
        LastActionUpdateTime = Time.time;

        isDead = false; //Enemy는 재사용되므로 초기화
    }

    public void Appear(Vector3 targetPos)
    {
        TargetPosition = targetPos;
        CurrentSpeed = MaxSpeed;

        CurrentState = State.Appear;
        MoveStartTime = Time.time;
    }

    void Disappear(Vector3 targetPos)
    {
        TargetPosition = targetPos;
        CurrentSpeed = 0.0f;

        CurrentState = State.Disappear;
        MoveStartTime = Time.time;
    }

    void UpdateReady()
    {
        if(Time.time - LastActionUpdateTime > 1.0f)
        {
            Appear(AppearPoint);
        }
    }

    void UpdateBattle()
    {
        if(Time.time - LastActionUpdateTime > 1.0f)
        {
            if(FireRemainCount > 0)
            {
                Fire();
                FireRemainCount--;
            }
            else
            {
                Disappear(DisappearPoint);
            }          

            LastActionUpdateTime = Time.time;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player)
        {
            if(!player.IsDead)
            {
                BoxCollider box = ((BoxCollider)other);
                Vector3 crashPos = player.transform.position + box.center;
                crashPos.x += box.size.x * 0.5f;

                player.OnCrash(CrashDamage, crashPos);
            }                
        }            
    }

    public void Fire()
    {
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.EnemyBulletIndex);
        bullet.Fire(actorInstanceID, FireTransform.position, -FireTransform.right, BulletSpeed, Damage);        
    }

    protected override void OnDead()
    {
        base.OnDead();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        inGameSceneMain.GamePointAccumulator.Accumulate(GamePoint);
        inGameSceneMain.EnemyManager.RemoveEnemy(this);

        GenerateItem();

        CurrentState = State.Dead;
    }

    protected override void DecreaseHP(int value, Vector3 damagePos)
    {
        base.DecreaseHP(value, damagePos);

        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Generate(DamageManager.EnemyDamageIndex, damagePoint, value, Color.magenta);
    }

    [Command]
    public void CmdReset(SquadronMemberStruct data)
    {
        ResetData(data);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcReset(SquadronMemberStruct data)
    {
        ResetData(data);
        base.SetDirtyBit(1);
    }

    void GenerateItem()
    {
        if (!isServer)
            return;

        // 아이템 생성 확율을 검사
        float ItemGen = Random.Range(0.0f, 100.0f);
        if (ItemGen > ItemDropRate)
            return;

        ItemDropTable itemDropTable = SystemManager.Instance.ItemDropTable;
        ItemDropStruct dropStruct = itemDropTable.GetDropData(ItemDropID);

        // 어느 아이템을 생성할 것인지 확율 검사
        ItemGen = Random.Range(0, dropStruct.Rate1 + dropStruct.Rate2 + dropStruct.Rate3);
        int ItemIndex = -1;

        if (ItemGen <= dropStruct.Rate1)     // 1번 아이템 비율보다 작은 경우
            ItemIndex = dropStruct.ItemID1;
        else if (ItemGen <= (dropStruct.Rate1 + dropStruct.Rate2))   // 2번 아이템 비율보다 작은 경우
            ItemIndex = dropStruct.ItemID2;
        else //if (ItemGen <= (dropStruct.Rate1 + dropStruct.Rate2 + dropStruct.Rate3)) // 3번 아이템 비율인 경우
            ItemIndex = dropStruct.ItemID3;

        Debug.Log("GenerateItem ItemIndex = " + ItemIndex);

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        inGameSceneMain.ItemBoxManager.Generate(ItemIndex, transform.position);
    }

    public void AddList()
    {
        if (isServer)
            RpcAddList();
    }

    [ClientRpc]
    public void RpcAddList()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.AddList(this);
        base.SetDirtyBit(1);
    }

    public void RemoveList()
    {
        if (isServer)
            RpcRemoveList();
    }

    [ClientRpc]
    public void RpcRemoveList()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveList(this);
        base.SetDirtyBit(1);
    }
}
