using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Actor
{
    [SerializeField]
    Vector3 MoveVector = Vector3.zero;

    [SerializeField]
    float Speed;

    [SerializeField]
    BoxCollider boxCollider;

    [SerializeField]
    Transform MainBGQuadTransform;

    [SerializeField]
    Transform FireTransform;

    [SerializeField]
    GameObject Bullet;

    [SerializeField]
    float BulletSpeed = 1;

    protected override void UpdateActor()
    {
        UpdateMove();
    }

    void UpdateMove()
    {
        if (MoveVector.sqrMagnitude == 0)
            return;

        MoveVector = AdjustMoveVector(MoveVector);

        transform.position += MoveVector;
    }

    public void ProcessInput(Vector3 moveDirection)
    {
        MoveVector = moveDirection * Speed * Time.deltaTime;
    }

    Vector3 AdjustMoveVector(Vector3 moveVector)
    {
        Vector3 result = Vector3.zero;

        result = boxCollider.transform.position + boxCollider.center + moveVector;

        if (result.x - boxCollider.size.x * 0.5f < -MainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;
        if (result.x + boxCollider.size.x * 0.5f > MainBGQuadTransform.localScale.x * 0.5f)
            moveVector.x = 0;
        if (result.y - boxCollider.size.y * 0.5f < -MainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;
        if (result.y + boxCollider.size.y * 0.5f > MainBGQuadTransform.localScale.y * 0.5f)
            moveVector.y = 0;

        return moveVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy)
        {
            if(!enemy.IsDead)
                enemy.OnCrash(this, CrashDamage);
        }
    }

    public void OnCrash(Enemy enemy, int damage)
    {
        Debug.Log("OnCrash enemy = " + enemy);
        OnCrash(damage);
    }

    public void Fire()
    {
        GameObject go = Instantiate(Bullet);

        Bullet bullet = go.GetComponent<Bullet>();
        bullet.Fire(OwnerSide.Player, FireTransform.position, FireTransform.right, BulletSpeed, Damage);
    }
}
