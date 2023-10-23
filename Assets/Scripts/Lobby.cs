using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Lobby : NetworkBehaviour {

    public LobbyUi lobbyUi;
    public NetworkedPlayers networkedPlayers;

    void Start() {
        if(IsServer) {
            ServerPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ServerOnNetworkedPlayersChanged;
            lobbyUi.ShowStart(true);
            lobbyUi.OnStartClicked += ServerOnStartClicked;
        } else if(!IsHost) {
            ClientPopulateCards();
            networkedPlayers.allNetPlayers.OnListChanged += ClientOnNetworkedPlayersChanged;
            lobbyUi.ShowStart(false);
            lobbyUi.OnReadyToggled += ClientOnReadyToggled;
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnect;
        }

        lobbyUi.OnChangeNameClicked += OnChangeNameClicked;
    }

    private void ServerOnNetworkedPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent) { 
        ServerPopulateCards();
        lobbyUi.EnableStart(networkedPlayers.AllPlayersReady());
    }

    private void ClientOnNetworkedPlayersChanged(NetworkListEvent<NetworkPlayerInfo> changeEvent)
    {
        ClientPopulateCards();
    }

    private void ClientOnReadyToggled(bool newValue) { 
        UpdateReadyServerRpc(newValue);
    }

    private void OnChangeNameClicked(string newValue)
    {
        UpdatePlayerNameServerRpc(newValue);
    }

    private void ServerOnKickClicked(ulong clientId) {
        NetworkManager.DisconnectClient(clientId);
    }

    private void ServerOnStartClicked() {
        NetworkManager.SceneManager.LoadScene("Arena1Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void PopulateMyInfo() {
        NetworkPlayerInfo myInfo = networkedPlayers.GetMyInfo();
        if (myInfo.clientId != ulong.MaxValue)
        {
            lobbyUi.SetPlayerName(myInfo.playerName.ToString());
        }
    }

    [ClientRpc]
    private void PopulateClientInfoClientRpc(ClientRpcParams clientRpcParams = default) {
        PopulateMyInfo();
    }

    private void ServerPopulateCards() {
        lobbyUi.playerCards.Clear();

        foreach(NetworkPlayerInfo info in networkedPlayers.allNetPlayers) {
            PlayerCard pc = lobbyUi.playerCards.AddCard("Some player");
            pc.ready = info.ready;
            pc.clientId = info.clientId;
            pc.color = info.color;
            pc.playerName = info.playerName.ToString();
            if (info.clientId == NetworkManager.LocalClientId) {
                pc.ShowKick(false);
            } else {
                pc.ShowKick(true);
            }
            pc.OnKickClicked += ServerOnKickClicked;
            pc.UpdateDisplay();
        }
        PopulateMyInfo();
    }

    private void ClientPopulateCards() {
        lobbyUi.playerCards.Clear();

        foreach (NetworkPlayerInfo info in networkedPlayers.allNetPlayers) {
            PlayerCard pc = lobbyUi.playerCards.AddCard("Some player");
            pc.ready = info.ready;
            pc.clientId = info.clientId;
            pc.color = info.color;
            pc.playerName = info.playerName.ToString();
            pc.ShowKick(false);
            pc.UpdateDisplay();
        }
        PopulateMyInfo();
    }

    private void ClientOnClientDisconnect(ulong clientId) { 
        lobbyUi.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateReadyServerRpc(bool newValue, ServerRpcParams rpcParams = default) {
        networkedPlayers.UpdateReady(rpcParams.Receive.SenderClientId, newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerNameServerRpc(string newValue, ServerRpcParams rpcParams = default) {
        string newName = networkedPlayers.UpdatePlayerName(rpcParams.Receive.SenderClientId, newValue);
        if (newName != newValue) {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
                }
            };
            PopulateClientInfoClientRpc(clientRpcParams);
        }
    }

}
