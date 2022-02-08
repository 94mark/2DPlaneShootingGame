using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    Vector3 MoveVector = Vector3.zero;

    [SerializeField]
    float Speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMove();
    }

    void UpdateMove()
    {
        transform.position += MoveVector;
    }

    public void ProcessInput(Vector3 moveDirection)
    {
        MoveVector = moveDirection * Speed * Time.deltaTime;
    }
}
