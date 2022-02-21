using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Actor : NetworkBehaviour
{
    [SerializeField]
    [SyncVar]
    protected int MaxHP = 100;

    [SerializeField]
    [SyncVar]
    protected int CurrentHp;

    [SerializeField]
    [SyncVar]
    protected int Damage = 1;

    [SerializeField]
    [SyncVar]
    protected int crashDamage = 100;

    [SerializeField]
    [SyncVar]
    bool isDead = false;

    public bool IsDead
    {
        get
        {
            return isDead;
        }
    }

    protected int CrashDamage
    {
        get
        {
            return crashDamage;
        }
    }

    [SyncVar]
    protected int actorInstanceID = 0;

    public int ActorInstanceID
    {
        get
        {
            return actorInstanceID;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        CurrentHp = MaxHP;

        if(isServer)
        {
            actorInstanceID = GetInstanceID();
            RpcSetActorInstanceID(actorInstanceID);
        }
    }
    // Update is called once per frame
    void Update()
    {
        UpdateActor();
    }

    protected virtual void UpdateActor()
    {

    }

    public virtual void OnBulletHited(Actor attacker, int damage, Vector3 hitPos)
    {
        Debug.Log("OnBulletHited damage = " + damage);
        DecreaseHP(attacker, damage, hitPos);
    }

    public virtual void OnCrash(Actor attacker, int damage, Vector3 crashPos)
    {
        Debug.Log("OnCrash attacker = " + attacker.name + ", damage = " + damage);
        DecreaseHP(attacker, damage, crashPos);
    }

    protected virtual void DecreaseHP(Actor attacker, int value, Vector3 damagePos)
    {
        if (isDead)
            return;

        CurrentHp -= value;

        if (CurrentHp < 0)
            CurrentHp = 0;

        if (CurrentHp == 0)
        {
            OnDead(attacker);
        }
    }

    protected virtual void OnDead(Actor killer)
    {
        Debug.Log(name + " OnDead");
        isDead = true;

        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.ActorDeadFxIndex, transform.position);
    }

    public void SetPosition(Vector3 position)
    {
        // ���������� NetworkBehaviour �ν��Ͻ��� Update�� ȣ��Ǿ� ����ǰ� ������
        //CmdSetPosition(position);

        // MonoBehaviour �ν��Ͻ��� Update�� ȣ��Ǿ� ����ǰ� �������� �ļ�
        if (isServer)
        {
            RpcSetPosition(position);        // Host �÷��̾��ΰ�� RPC�� ������
        }
        else
        {
            CmdSetPosition(position);        // Client �÷��̾��ΰ�� Cmd�� ȣ��Ʈ�� ������ �ڽ��� Self ����
            if (isLocalPlayer)
                transform.position = position;
        }
    }

    [Command]
    public void CmdSetPosition(Vector3 position)
    {
        this.transform.position = position;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetPosition(Vector3 position)
    {
        this.transform.position = position;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        this.gameObject.SetActive(value);
        base.SetDirtyBit(1);
    }

    public void UpdateNetworkActor()
    {
        if(isServer)
        {
            RpcUpdateNetworkActor();
        }
        else
        {
            CmdUpdateNetworkActor();
        }
    }

    [Command]
    public void CmdUpdateNetworkActor()
    {
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcUpdateNetworkActor()
    {
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcSetActorInstanceID(int instID)
    {
        if (this.actorInstanceID != 0)
            SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.Regist(this.actorInstanceID, this);

        base.SetDirtyBit(1);
    }
}
