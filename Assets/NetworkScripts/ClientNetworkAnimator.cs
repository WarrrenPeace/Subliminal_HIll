using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Networking;

public class ClientNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
