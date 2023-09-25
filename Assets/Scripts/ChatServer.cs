using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour {
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];
    private ulong[] sClientIds = new ulong[1];

    void Start() {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if(IsServer) {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
            if (IsHost) { 
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}"); 
            } else {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
        } else {
            DisplayMessageLocally(SYSTEM_ID, $"You are a client {NetworkManager.LocalClientId}");
        }
    }

    private void ServerOnClientConnected(ulong clientId) {
        SendServerMessageServerRpc($"Player {clientId} has connected");
        ServerSendSingleMessage($"Hello! You {clientId} have connected to the server, well done!", clientId);
    }

    private void ServerOnClientDisconnected(ulong clientId) {
        SendServerMessageServerRpc($"Player {clientId} has disconnected"); 
    }

    private void DisplayMessageLocally(ulong from, string message) {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;
        if(from == NetworkManager.LocalClientId) {
            fromStr = "you";
            textColor = Color.magenta;
        } else if(from == SYSTEM_ID) {
            fromStr = "SYS";
            textColor = Color.green;
        }
        chatUi.addEntry(fromStr, message, textColor);
    }
    
    private void OnChatUiMessageEntered(string message) {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default) {
        if (message.StartsWith("@")) {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);
            ServerSendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);

        } else {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendServerMessageServerRpc(string message, ServerRpcParams serverRpcParams = default) {
        ReceiveChatMessageClientRpc(message, SYSTEM_ID);
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default) {
        DisplayMessageLocally(from, message);
    }

    private void ServerSendDirectMessage(string message, ulong from, ulong to) {
        bool connected = false;
        foreach (ulong clientId in NetworkManager.ConnectedClientsIds) {
            if (clientId == to) connected = true;
        }

        if (connected) {
            dmClientIds[0] = from;
            dmClientIds[1] = to;
            ClientRpcParams rpcParams = default;
            rpcParams.Send.TargetClientIds = dmClientIds;
            ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
        } else {
            ServerSendSingleMessage($"There is no connected user ({to})", from);
        }
        
    }

    private void ServerSendSingleMessage(string message, ulong to)
    {
        sClientIds[0] = to;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = sClientIds;
        ReceiveChatMessageClientRpc(message, SYSTEM_ID, rpcParams);
    }

}
