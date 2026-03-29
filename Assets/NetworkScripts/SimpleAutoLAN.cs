using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SimpleAutoLAN : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float searchTime = 3f;        // How long to search before becoming host
    [SerializeField] private ushort gamePort = 7777;       // Port for actual Netcode connection
    [SerializeField] private int discoveryPort = 8888;     // UDP port for broadcasting

    private UdpClient udpClient;
    private bool isSearching = false;
    private Coroutine timeoutCoroutine;

    private void Start()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
            transport.SetConnectionData("0.0.0.0", gamePort);

        StartCoroutine(AutoConnect());
    }

    private IEnumerator AutoConnect()
    {
        Debug.Log("Searching for existing host on LAN...");

        isSearching = true;

        // Start listening for broadcasts
        udpClient = new UdpClient(discoveryPort);
        udpClient.BeginReceive(new System.AsyncCallback(OnReceive), null);

        // Start a broadcast thread (send "HELLO" every 0.5s)
        StartCoroutine(SendDiscoveryBroadcast());

        timeoutCoroutine = StartCoroutine(SearchTimeout());

        yield return null;
    }

    private IEnumerator SearchTimeout()
    {
        yield return new WaitForSeconds(searchTime);

        if (isSearching)
        {
            Debug.Log("No host found. Becoming Host...");
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

            if (message == "NGO_LAN_DISCOVERY" && isSearching)
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
        if (udpClient != null) udpClient.Close();

        // Start broadcasting so others can find us
        udpClient = new UdpClient();
        StartCoroutine(SendDiscoveryBroadcast());

        Debug.Log("Starting as Host...");
        NetworkManager.Singleton.StartHost();
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