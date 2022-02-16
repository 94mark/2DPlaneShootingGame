using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class NetworkConnectionInfo
{
    //호스트로 실행 여부
    public bool Host;

    //클라이언트로 실행시 접속할 호스트의 IP 주소
    public string IPAddress;

    //클라이언트로 실행시 접속할 호스트의 Port
    public int Port;
}
