using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GuidedMissile : Bullet
{
    /// <summary>
    /// Ÿ�� ������ ������ ��ȯ�ϴ� ����
    /// </summary>
    const float ChaseFector = 1.5f;

    /// <summary>
    /// Ÿ�� ������ �����ϴ� �ð�(�߻�ð��� ����)
    /// </summary>
    const float ChasingStartTime = 0.7f;

    /// <summary>
    /// Ÿ�� ������ �����ϴ� �ð�(�߻�ð��� ����)
    /// </summary>
    const float ChasingEndTime = 4.5f;

    /// <summary>
    /// ��ǥ Actor�� ActorInstanceID
    /// </summary>
    [SyncVar]
    [SerializeField]
    int TargetInstanceID;

    /// <summary>
    /// �̵� ����
    /// </summary>
    [SerializeField]
    Vector3 ChaseVector;

    [SerializeField]
    Vector3 rotateVector = Vector3.zero;

    /// <summary>
    /// ����ȸ���� ������ ���� �÷���
    /// </summary>
    [SerializeField]
    bool FlipDirection = true;  // ����Ʈ ���°� Left ������ ��� true


    bool needChase = true;

    public void FireChase(int targetInstanceID, int ownerInstanceID, Vector3 direction, float speed, int damage)
    {
        if (!isServer)
            return;

        RpcSetTargetInstanceID(targetInstanceID);        // Host �÷��̾��ΰ�� RPC
        base.Fire(ownerInstanceID, direction, speed, damage);
    }

    [ClientRpc]
    public void RpcSetTargetInstanceID(int targetInstanceID)
    {
        TargetInstanceID = targetInstanceID;
        base.SetDirtyBit(1);
    }

    protected override void UpdateTransform()
    {
        UpdateMove();
        UpdateRotate();
    }

    protected override void UpdateMove()
    {
        if (!NeedMove)
            return;

        Vector3 moveVector = MoveDirection.normalized * Speed * Time.deltaTime;
        // Ÿ���� �����ϱ� ���� ���
        float deltaTime = Time.time - FiredTime;

        if (deltaTime > ChasingStartTime && deltaTime < ChasingEndTime)
        {
            Actor target = SystemManager.Instance.GetCurrentSceneMain<InGameSceneMain>().ActorManager.GetActor(TargetInstanceID);
            if (target != null)
            {
                // ���� ��ġ���� Ÿ�ٱ��� ����
                Vector3 targetVector = target.transform.position - transform.position;

                // �̵� ���Ϳ� Ÿ�� ������ ������ ���͸� ���
                ChaseVector = Vector3.Lerp(moveVector.normalized, targetVector.normalized, ChaseFector * Time.deltaTime);

                // �̵� ���Ϳ� �������͸� ���ϰ� ���ǵ忡 ���� ���̸� �ٽ� ���
                moveVector += ChaseVector.normalized;
                moveVector = moveVector.normalized * Speed * Time.deltaTime;

                // ���� ���� �̵����͸� �ʵ忡 �����ؼ� ���� UpdateMove���� ��밡���ϰ� �Ѵ�
                MoveDirection = moveVector.normalized;
            }
        }

        moveVector = AdjustMove(moveVector);
        transform.position += moveVector;

        // moveVector �������� ȸ����Ű�� ���� ���
        rotateVector.z = Vector2.SignedAngle(Vector2.right, moveVector);
        if (FlipDirection)
            rotateVector.z += 180.0f;
    }

    void UpdateRotate()
    {
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = rotateVector;
        transform.rotation = quat;

    }
}