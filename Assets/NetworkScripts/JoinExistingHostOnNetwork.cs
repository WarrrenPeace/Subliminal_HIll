using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class JoinExistingHostOnNetwork : NetworkBehaviour
{
    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("This is the host.");
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("This is a client.");
        }
        
        
    }
}
