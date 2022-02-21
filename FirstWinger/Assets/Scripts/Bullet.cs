using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    const float LifeTime = 15.0f; // �Ѿ� ���� �ð�

    [SyncVar]
    [SerializeField]
    Vector3 MoveDirection = Vector3.zero;

    [SyncVar]
    [SerializeField]
    float Speed = 0.0f;

    [SyncVar]
    bool NeedMove = false; //�̵� �÷���

    [SyncVar]
    float FiredTime;

    [SyncVar]
    bool Hited = false; //�ε������� �÷���

    [SyncVar]
    [SerializeField]
    int Damage = 1;

    [SerializeField]
    Actor Owner;  //NetworkBehaviour ��� Ŭ������ [SyncVar]�� �ȵ�

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

    // Start is called before the first frame update
    void Start()
    {
        if (!((FWNetworkManager)FWNetworkManager.singleton).isServer)
        {
            InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
            transform.SetParent(inGameSceneMain.BulletManager.transform);
            inGameSceneMain.BulletCacheSystem.Add(FilePath, gameObject);
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ProcessDisappearCondition())
            return;

        UpdateMove();
    }

    void UpdateMove()
    {
        if (!NeedMove)
            return;

        Vector3 moveVector = MoveDirection.normalized * Speed * Time.deltaTime;
        moveVector = AdjustMove(moveVector);
        transform.position += moveVector;
    }

    public void Fire(Actor owner, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Owner = owner;
        SetPosition(firePosition);
        MoveDirection = direction;
        Speed = speed;
        Damage = damage;

        NeedMove = true;
        FiredTime = Time.time;

        UpdateNetworkBullet();
    }

    Vector3 AdjustMove(Vector3 moveVector)
    {
        RaycastHit hitInfo;
        if(Physics.Linecast(transform.position, transform.position + moveVector, out hitInfo))
        {
            Actor actor = hitInfo.collider.GetComponentInParent<Actor>();
            if (actor && actor.IsDead)
                return moveVector;

            moveVector = hitInfo.point - transform.position;
            OnBulletCollision(hitInfo.collider);
        }

        return moveVector;
    }

    void OnBulletCollision(Collider collider)
    {
        if (Hited)
            return;

        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyBullet")
            || collider.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
        {
            return;
        }

        Actor actor = collider.GetComponentInParent<Actor>();
        if (actor && actor.IsDead || actor.gameObject.layer == Owner.gameObject.layer)
            return;

        actor.OnBulletHited(Owner, Damage, transform.position);

        Collider myCollider = GetComponentInChildren<Collider>();
        myCollider.enabled = false;

        Hited = true;
        NeedMove = false;

        GameObject go = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().EffectManager.GenerateEffect(EffectManager.BulletDisappearFxIndex, transform.position);
        go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Disappear();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnBulletCollision(other);
    }

    bool ProcessDisappearCondition()
    {
        if(transform.position.x > 15.0f || transform.position.x < -15.0f
            || transform.position.y > 15.0f || transform.position.y < -15.0f)
        {
            Disappear();
            return true;
        }

        else if(Time.time - FiredTime > LifeTime)
        {
            Disappear();
            return true;
        }

        return false;
    }

    void Disappear()
    {
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Remove(this);
    }

    [ClientRpc]
    public void RpcSetActive(bool value)
    {
        this.gameObject.SetActive(value);
        base.SetDirtyBit(1);
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

    public void UpdateNetworkBullet()
    {
        //���������� NetworkBehaviour �ν��Ͻ��� Update�� ȣ��Ǿ� ����ǰ� ���� ��
        //CmdUpdateNetworkActor();

        //MonoBehaviour �ν��Ͻ��� Update�� ȣ��Ǿ� ����ǰ� ���� ���� �ļ�
        if(isServer)
        {
            RpcUpdateNetworkBullet(); //Host �÷��̾��� ��� RPC�� ����
        }
        else
        {
            CmdUpdateNetworkBullet(); //Client �÷��̾��� ��� cmd�� ȣ��Ʈ�� ���� �� �ڽ��� self ����
        }
    }

    [Command]
    public void CmdUpdateNetworkBullet()
    {
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcUpdateNetworkBullet()
    {
        base.SetDirtyBit(1);
    }
}
