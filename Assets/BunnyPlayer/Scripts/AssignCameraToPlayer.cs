using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System;

public class AssignCameraToPlayer : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCameraBase CMC;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CMC = GameObject.FindGameObjectWithTag("VirtualCamera"). GetComponent<CinemachineCamera>();
        
        if (IsOwner)
        {
            AssignCamera();
        }
    }

    void AssignCamera()
    {
        CMC.Follow = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
        CMC.LookAt = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
        Destroy(this);
    }

}
