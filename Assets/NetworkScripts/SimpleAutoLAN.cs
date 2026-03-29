using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SimpleAutoLAN : MonoBehaviour //Pretty sure this is AI but idk how to do this so it will work for now
{
    [Header("Settings")]
    [SerializeField] private float searchTime = 3f;        // How long to search before becoming host
    [SerializeField] private ushort gamePort = 7777;       // Port for actual Netcode connection
    [SerializeField] private int discoveryPort = 8888;     // UDP port for broadcasting

    private UdpClient udpClient;
    private bool isSearching = false;
    private Coroutine timeoutCoroutine;
    private bool isHost = false;

    private void Start()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
            transport.SetConnectionData("0.0.0.0", gamePort);

        StartCoroutine(AutoConnect());
    }

    private IEnumerator AutoConnect()
    {
        Debug.Log("Trying to start LAN discovery...");

        isSearching = true;

        // Try to bind to discovery port (only the Host will succeed)
        try
        {
            udpClient = new UdpClient(discoveryPort);
            udpClient.BeginReceive(new System.AsyncCallback(OnReceive), null);

            Debug.Log("Successfully bound to discovery port → Will become Host if no one else is found");
            
            // Start broadcasting
            StartCoroutine(SendDiscoveryBroadcast());

            timeoutCoroutine = StartCoroutine(SearchTimeout());
        }
        catch (SocketException)
        {
            // Port is already in use → Another instance is the Host
            Debug.Log("Discovery port already in use by another instance. Joining as Client...");
            isSearching = false;
            BecomeClient();
        }

        yield return null;
    }

    private IEnumerator SearchTimeout()
    {
        yield return new WaitForSeconds(searchTime);

        if (isSearching)
        {
            Debug.Log("No other host found. Becoming Host...");
            BecomeHost();
        }
    }

    private IEnumerator SendDiscoveryBroadcast()
    {
        while (isSearching)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes("NGO_LAN_DISCOVERY");
                udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, discoveryPort));
            }
            catch { }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnReceive(System.IAsyncResult result)
    {
        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.EndReceive(result, ref remoteEP);

            string message = Encoding.UTF8.GetString(data);

            if (message == "NGO_LAN_DISCOVERY" && isSearching && !isHost)
            {
                // Found a host!
                StopCoroutine(timeoutCoroutine);
                isSearching = false;

                Debug.Log($"Host found at {remoteEP.Address}! Connecting...");

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport != null)
                    transport.SetConnectionData(remoteEP.Address.ToString(), gamePort);

                NetworkManager.Singleton.StartClient();
            }
        }
        catch { }

        // Continue listening
        if (isSearching && udpClient != null)
            udpClient.BeginReceive(new System.AsyncCallback(OnReceive), null);
    }

    private void BecomeHost()
    {
        isSearching = false;
        isHost = true;

        if (udpClient != null) udpClient.Close();

        // Start broadcasting so others can find us
        udpClient = new UdpClient();
        StartCoroutine(SendDiscoveryBroadcast());

        Debug.Log("Starting as Host...");
        NetworkManager.Singleton.StartHost();
    }

    private void BecomeClient()
    {
        // Connect to localhost (the other instance that is hosting)
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
            transport.SetConnectionData("127.0.0.1", gamePort);

        Debug.Log("Joining as Client to localhost...");
        NetworkManager.Singleton.StartClient();
    }

    private void OnDestroy()
    {
        isSearching = false;
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}