using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class ConnectionStatus : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI status;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        status = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        if (NetworkManager.Singleton == null) 
        {return;}

        if (NetworkManager.Singleton.IsHost)
            {status.text = "HOST";}

        else if (NetworkManager.Singleton.IsClient) 
            {status.text = "CLIENT (Connected)";}
            
        else
            {status.text = "Searching...";}
    }
}
