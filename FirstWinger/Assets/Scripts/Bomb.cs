using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bomb : Bullet
{
    const float MaxRotateTime = 30.0f;
    const float MaxRotateZ = 90.0f;

    [SerializeField]
    Rigidbody selfRigidbody;

    [SerializeField]
    Vector3 Force;

    [SyncVar]
    float RotateStartTime = 0.0f;

    [SyncVar]
    [SerializeField]
    float CurrentRotateZ;

    Vector3 currentEulerAngles = Vector3.zero;

    protected override void UpdateTransform()
    {
        if (!NeedMove)
            return;

        if (CheckScreenBottom())
            return;

        UpdateRotate();
    }

    bool CheckScreenBottom()
    {
        Transform mainBQQuadTransform = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().MainBGQuadTransform;

        if(transform.position.y < -mainBQQuadTransform.localScale.y * 0.5f)
        {
            Vector3 newPos = transform.position;
            newPos.y = -mainBQQuadTransform.localScale.y * 0.5f;
            transform.position = newPos;

            selfRigidbody.useGravity = false;
            selfRigidbody.velocity = Vector3.zero;
            NeedMove = false;
            return true;
        }

        return false;
    }

    void UpdateRotate()
    {
        CurrentRotateZ = Mathf.Lerp(CurrentRotateZ, MaxRotateZ, (Time.time - RotateStartTime) / MaxRotateTime);
        currentEulerAngles.z = -CurrentRotateZ;

        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = currentEulerAngles;
        transform.localRotation = rot;
    }

    public override void Fire(int ownerInstanceID, Vector3 firePosition, Vector3 direction, float speed, int damage)
    {
        base.Fire(ownerInstanceID, firePosition, direction, speed, damage);

        AddForce(Force);
    }

    void InternelAddForce(Vector3 force)
    {
        selfRigidbody.velocity = Vector3.zero;
        selfRigidbody.AddForce(force);
        RotateStartTime = Time.time;
        CurrentRotateZ = 0.0f;
        transform.localRotation = Quaternion.identity;
        selfRigidbody.useGravity = true;
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
                InternelAddForce(force);
        }
    }

    [Command]
    public void CmdAddForce(Vector3 force)
    {
        InternelAddForce(force);
        base.SetDirtyBit(1);
    }

    [ClientRpc]
    public void RpcAddForce(Vector3 force)
    {
        InternelAddForce(force);
        base.SetDirtyBit(1);
    }    
}
