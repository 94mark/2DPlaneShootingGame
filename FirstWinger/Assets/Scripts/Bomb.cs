using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bomb : Bullet
{
    [SerializeField]
    Rigidbody selfRigidbody;

    [SerializeField]
    Vector3 Force;

    protected override void UpdateMove()
    {
        if (!NeedMove)
            return;
    }

    public override void Fire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        base.Fire(ownerInstanceID, firePosition, direction, speed, damage);

        selfRigidbody.velocity = Vector3.zero;
        AddForce(Force);
    }

    public void AddForce(Vector3 force)
    {
        if (isServer)
        {
            RpcAddForce(force);
        }
        else
        {
            CmdAddForce(force);
            if (isLocalPlayer)
                selfRigidbody.AddForce(force);
        }
    }

    [Command]
    public void CmdAddForce(Vector3 force)
    {
        selfRigidbody.AddForce(force);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcAddForce(Vector3 force)
    {
        selfRigidbody.AddForce(force);
        base.SetDirtyBit(1);
    }    
}
