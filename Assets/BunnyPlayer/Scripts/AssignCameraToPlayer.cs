using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System;

public class AssignCameraToPlayer : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCameraBase CMC;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CMC = GetComponent<CinemachineCamera>();
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.OnConnectionEvent += OnClientConnected;
        }
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnConnectionEvent += OnClientConnected;
        }
    }

    private void OnClientConnected(NetworkManager manager, ConnectionEventData data)
    {
        Debug.Log(NetworkManager.Singleton.LocalClient.PlayerObject);
        CMC.Follow = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
        CMC.LookAt = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
    }

}
