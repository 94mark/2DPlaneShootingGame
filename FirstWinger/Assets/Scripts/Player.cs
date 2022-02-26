using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : Actor
{
    const string PlayerHUDPath = "Prefabs/PlayerHUD";

    [SerializeField]
    [SyncVar]
    Vector3 MoveVector = Vector3.zero;

    [SerializeField]
    NetworkIdentity NetworkIdentity = null;

    [SerializeField]
    float Speed;

    [SerializeField]
    BoxCollider boxCollider;

    [SerializeField]    
    Transform FireTransform;    

    [SerializeField]
    float BulletSpeed = 1;

    InputController inputController = new InputController();

    [SerializeField]
    [SyncVar]
    bool Host = false; //Host 플레이어인지 여부

    [SerializeField]
    Material ClientPlayerMaterial;

    [SerializeField]
    [SyncVar]
    int UsableItemCount = 0;

    public int ItemCount
    {
        get
        {
            return UsableItemCount;
        }
    }

    protected override void Initialize()
    {
        base.Initialize();

        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();

        if (isLocalPlayer)
            inGameSceneMain.Hero = this;

        if(isServer && isLocalPlayer)
        {
            Host = true;
            RpcSetHost();
        }

        if (Host)
        {     
            MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            meshRenderer.material = ClientPlayerMaterial;
        }

        if (actorInstanceID != 0)
            inGameSceneMain.ActorManager.Regist(actorInstanceID, this);

        InitializePlayerHUD();
    }

    void InitializePlayerHUD()
    {
        InGameSceneMain inGameSceneMain = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>();
        GameObject go = Resources.Load<GameObject>(PlayerHUDPath);
        GameObject goInstance = Instantiate<GameObject>(go, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, inGameSceneMain.DamageManager.CanvasTransform);
        PlayerHUD playerHUD = goInstance.GetComponent<PlayerHUD>();
        playerHUD.Initialize(this);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("OnStartClient");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("OnStartLocalPlayer");
    }

    protected override void UpdateActor()
    {
        if (!isLocalPlayer)
            return;

        UpdateInput();
        UpdateMove();
    }

    [ClientCallback]
    public void UpdateInput()
    {
        inputController.UpdateInput();
    }

    void UpdateMove()
    {
        if (MoveVector.sqrMagnitude == 0)
            return;

        //transform.position += MoveVector;
        //CmdMove(MoveVector);
        if (isServer)
        {
            RpcMove(MoveVector); //Host 플레이어인 경우 RPC로 보내고
        }
        else
        {
            CmdMove(MoveVector); //Client 플레이어인 경우 Cmd로 호스트로 보낸 후 자신을 Self 동작
            if (isLocalPlayer)
                transform.position += AdjustMoveVector(MoveVector);
        }
    }

    [Command]
    public void CmdMove(Vector3 moveVector)
    {
        this.MoveVector = moveVector;
        transform.position += AdjustMoveVector(this.MoveVector);
        base.SetDirtyBit(1);
        this.MoveVector = Vector3.zero; //타 플레이어가 보낸 경우 Update를 통해 초기화 되지 않으므로 사용 후 바로 초기화
    }

    [ClientRpc]
    public void RpcMove(Vector3 moveVector)
    {
        this.MoveVector = moveVector;
        transform.position += AdjustMoveVector(this.MoveVector);
        base.SetDirtyBit(1);
        this.MoveVector = Vector3.zero; //타 플레이어가 보낸 경우 Update를 통해 초기화 되지 않으므로 사용 후 바로 초기화
    }


    public void ProcessInput(Vector3 moveDirection)
    {
        if (!isLocalPlayer)
            return;

        MoveVector = moveDirection * Speed * Time.deltaTime;
    }

    Vector3 AdjustMoveVector(Vector3 moveVector)
    {
        Transform mainBGQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;
        Vector3 result = Vector3.zero;

        result = boxCollider.transform.position + boxCollider.center + moveVector;

        if (result.x - boxCollider.size.x * 0.5f < -mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;
        if (result.x + boxCollider.size.x * 0.5f > mainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;
        if (result.y - boxCollider.size.y * 0.5f < -mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;
        if (result.y + boxCollider.size.y * 0.5f > mainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        return moveVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy)
        {
            if(!enemy.IsDead)
            {
                BoxCollider box = ((BoxCollider)other);
                Vector3 crashPos = enemy.transform.position + box.center;
                crashPos.x += box.size.x * 0.5f;

                enemy.OnCrash(CrashDamage, crashPos);
            }                
        }
    }

    public void Fire()
    {
        if (Host)
        {
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBulletIndex, FireTransform.position);
            bullet.Fire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
        else
        {
            CmdFire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
    }

    [Command]
    public void CmdFire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBulletIndex, firePosition);
        bullet.Fire(ownerInstanceID, firePosition, direction, speed, damage);
        base.SetDirtyBit(1);
    }

    public void FireBomb()
    {
        if (UsableItemCount <= 0)
            return;

        if(Host)
        {
            Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBombIndex, FireTransform.position);
            bullet.Fire(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
        else
        {
            CmdFireBomb(actorInstanceID, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
        }
        DecreaseUsableItemCount();
    }

    [Command]
    public void CmdFireBomb(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        Bullet bullet = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().BulletManager.Generate(BulletManager.PlayerBombIndex, firePosition);
        bullet.Fire(ownerInstanceID, firePosition, direction, speed, damage);
        base.SetDirtyBit(1);
    }

    void DecreaseUsableItemCount()
    {
        if(isServer)
        {
            RpcDecreaseUsableItemCount();
        }
        else
        {
            CmdDecreaseUsableItemCount();
            if (isLocalPlayer)
                UsableItemCount--;
        }
    }

    [Command]
    public void CmdDecreaseUsableItemCount()
    {
        UsableItemCount--;
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcDecreaseUsableItemCount()
    {
        UsableItemCount--;
        base.SetDirtyBit(1);
    }

    protected override void DecreaseHP(int value, Vector3 damagePos)
    {
        base.DecreaseHP(value, damagePos);

        Vector3 damagePoint = damagePos + Random.insideUnitSphere * 0.5f;
        SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().DamageManager.Generate(DamageManager.PlayerDamageIndex, damagePoint, value, Color.red);
    }

    protected override void OnDead()
    {
        base.OnDead();
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetHost()
    {
        Host = true;
        base.SetDirtyBit(1);
    }

    protected virtual void InternalIncreaseHP(int value)
    {
        if (isDead)
            return;

        CurrentHp += value;

        if (CurrentHp > MaxHP)
            CurrentHp = MaxHP;
    }

    public virtual void IncreaseHP(int value)
    {
        if (isDead)
            return;

        CmdIncreaseHP(value);
    }

    [Command]
    public void CmdIncreaseHP(int value)
    {
        InternalIncreaseHP(value);
        base.SetDirtyBit(1);
    }

    public virtual void IncreaseUsableItem(int value = 1)
    {
        if (isDead)
            return;

        CmdIncreaseUsableItem(value);
    }

    [Command]
    public void CmdIncreaseUsableItem(int value)
    {
        UsableItemCount += value;
        base.SetDirtyBit(1);
    }
}
