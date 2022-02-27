using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : Actor
{
    public enum State : int
    {
        None = -1,  // �����
        Ready = 0,  // �غ� �Ϸ�
        Appear,     // ����
        Battle,     // ������
        Dead,       // ���
        Disappear,  // ����
    }

    /// <summary>
    /// ���� ���°�
    /// </summary>
    [SerializeField]
    [SyncVar]
    State CurrentState = State.None;

    /// <summary>
    /// �ְ� �ӵ�
    /// </summary>
    protected const float MaxSpeed = 10.0f;

    /// <summary>
    /// �ְ� �ӵ��� �̸��� �ð�
    /// </summary>
    const float MaxSpeedTime = 0.5f;


    /// <summary>
    /// ��ǥ��
    /// </summary>
    [SerializeField]
    [SyncVar]
    protected Vector3 TargetPosition;

    [SerializeField]
    [SyncVar]
    protected float CurrentSpeed;

    /// <summary>
    /// ������ ����� �ӵ� ����
    /// </summary>
    [SyncVar]
    protected Vector3 CurrentVelocity;

    [SyncVar]
    protected float MoveStartTime = 0.0f; // �̵����� �ð�

    [SerializeField]
    protected Transform FireTransform;

    [SerializeField]
    [SyncVar]
    float BulletSpeed = 1;

    [SyncVar]
    protected float LastActionUpdateTime = 0.0f;

    [SerializeField]
    [SyncVar]
    protected int FireRemainCount = 1;

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
    Vector3 AppearPoint;      // ����� ���� ��ġ
    [SyncVar]
    Vector3 DisappearPoint;      // ����� ��ǥ ��ġ

    [SerializeField]
    [SyncVar]
    float ItemDropRate;     // ������ ���� Ȯ��

    [SerializeField]
    [SyncVar]
    int ItemDropID;         // ������ ������ ������ ItemDrop ���̺��� �ε���

    protected virtual int BulletIndex
    {
        get
        {
            return BulletManager.EnemyBulletIndex;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            transform.SetParent(inGameSceneMain.EnemyManager.transform);
            inGameSceneMain.EnemyCacheSystem.Add(FilePath, gameObject);
            gameObject.SetActive(false);
        }

        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);
    }

    // Update is called once per frame
    protected override void UpdateActor()
    {
        //
        switch (CurrentState)
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

    protected void UpdateSpeed()
    {
        // CurrentSpeed ���� MaxSpeed �� �����ϴ� ������ �帥 �ð���ŭ ���
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, MaxSpeed, (Time.time - MoveStartTime) / MaxSpeedTime);
    }

    void UpdateMove()
    {
        float distance = Vector3.Distance(TargetPosition, transform.position);
        if (distance == 0)
        {
            Arrived();
            return;
        }

        // �̵����� ���. �� ������ ���� ���� �̵����͸� ������ nomalized �� �������͸� ���Ѵ�. �ӵ��� ���� ���� �̵��� ���͸� ���
        CurrentVelocity = (TargetPosition - transform.position).normalized * CurrentSpeed;

        // �ڿ������� �������� ��ǥ������ ������ �� �ֵ��� ���
        // �ӵ� = �Ÿ� / �ð� �̹Ƿ� �ð� = �Ÿ�/�ӵ�
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref CurrentVelocity, distance / CurrentSpeed, MaxSpeed);
    }

    void Arrived()
    {
        CurrentSpeed = 0.0f;    // ���������Ƿ� �ӵ��� 0
        if (CurrentState == State.Appear)
        {
            SetBattleState();
        }
        else // if (CurrentState == State.Disappear)
        {
            CurrentState = State.None;
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveEnemy(this);
        }
    }

    protected virtual void SetBattleState()
    {
        CurrentState = State.Battle;
        LastActionUpdateTime = Time.time;
    }

    public void Reset(SquadronMemberStruct data)
    {
        // ���������� NetworkBehaviour �ν��Ͻ��� Update�� ȣ��Ǿ� ����ǰ� ������
        //CmdReset(data);

        // MonoBehaviour �ν��Ͻ��� Update�� ȣ��Ǿ� ����ǰ� �������� �ļ�
        if (isServer)
        {
            RpcReset(data);        // Host �÷��̾��ΰ�� RPC�� ������
        }
        else
        {
            CmdReset(data);        // Client �÷��̾��ΰ�� Cmd�� ȣ��Ʈ�� ������ �ڽ��� Self ����
            if (isLocalPlayer)
                ResetData(data);
        }
    }

    void ResetData(SquadronMemberStruct data)
    {
        EnemyStruct enemyStruct = SystemManager.Instance.EnemyTable.GetEnemy(data.EnemyID);

        CurrentHp = MaxHP = enemyStruct.MaxHP;             // CurrentHP���� �ٽ� �Է�
        Damage = enemyStruct.Damage;                       // �Ѿ� ������
        crashDamage = enemyStruct.CrashDamage;             // �浹 ������
        BulletSpeed = enemyStruct.BulletSpeed;             // �Ѿ� �ӵ�
        FireRemainCount = enemyStruct.FireRemainCount;     // �߻��� �Ѿ� ����
        GamePoint = enemyStruct.GamePoint;                 // �ı��� ���� ����

        AppearPoint = new Vector3(data.AppearPointX, data.AppearPointY, 0);             // ����� ���� ��ġ 
        DisappearPoint = new Vector3(data.DisappearPointX, data.DisappearPointY, 0);    // ����� ��ǥ ��ġ

        ItemDropRate = enemyStruct.ItemDropRate;    // ������ ���� Ȯ��
        ItemDropID = enemyStruct.ItemDropID;        // ������ Drop ���̺� ���� �ε���

        CurrentState = State.Ready;
        LastActionUpdateTime = Time.time;
        //
        isDead = false;      // Enemy�� ����ǹǷ� �ʱ�ȭ������� ��
    }

    public void Appear(Vector3 targetPos)
    {
        TargetPosition = targetPos;
        CurrentSpeed = MaxSpeed;    // ��Ÿ������ �ְ� ���ǵ�� ����

        CurrentState = State.Appear;
        MoveStartTime = Time.time;
    }

    void Disappear(Vector3 targetPos)
    {
        TargetPosition = targetPos;
        CurrentSpeed = 0.0f;           // ��������� 0���� �ӵ� ����

        CurrentState = State.Disappear;
        MoveStartTime = Time.time;
    }

    void UpdateReady()
    {
        if (Time.time - LastActionUpdateTime > 1.0f)
        {
            Appear(AppearPoint);
        }
    }

    protected virtual void UpdateBattle()
    {
        if (Time.time - LastActionUpdateTime > 1.0f)
        {
            if (FireRemainCount > 0)
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
            if (!player.IsDead)
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
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletIndex, FireTransform.position);
        if (bullet)
            bullet.Fire(actorInstanceID, -FireTransform.right, BulletSpeed, Damage);
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

        // ������ ���� Ȯ���� �˻�
        float ItemGen = Random.Range(0.0f, 100.0f);
        if (ItemGen > ItemDropRate)
            return;

        ItemDropTable itemDropTable = SystemManager.Instance.ItemDropTable;
        ItemDropStruct dropStruct = itemDropTable.GetDropData(ItemDropID);

        // ��� �������� ������ ������ Ȯ�� �˻�
        ItemGen = Random.Range(0, dropStruct.Rate1 + dropStruct.Rate2 + dropStruct.Rate3);
        int ItemIndex = -1;

        if (ItemGen <= dropStruct.Rate1)     // 1�� ������ �������� ���� ���
            ItemIndex = dropStruct.ItemID1;
        else if (ItemGen <= (dropStruct.Rate1 + dropStruct.Rate2))   // 2�� ������ �������� ���� ���
            ItemIndex = dropStruct.ItemID2;
        else //if (ItemGen <= (dropStruct.Rate1 + dropStruct.Rate2 + dropStruct.Rate3)) // 3�� ������ ������ ���
            ItemIndex = dropStruct.ItemID3;

        Debug.Log("GenerateItem ItemIndex = " + ItemIndex);

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        inGameSceneMain.ItemBoxManager.Generate(ItemIndex, transform.position);
    }
    //
    public void AddList()
    {
        if (isServer)
            RpcAddList();        // Host �÷��̾��ΰ�� RPC�� ������
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
            RpcRemoveList();        // Host �÷��̾��ΰ�� RPC�� ������
    }

    [ClientRpc]
    public void RpcRemoveList()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EnemyManager.RemoveList(this);
        base.SetDirtyBit(1);
    }

}
