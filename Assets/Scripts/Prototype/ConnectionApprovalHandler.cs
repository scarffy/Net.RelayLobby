using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ProgrammingTask
{
    public class ConnectionApprovalHandler : MonoBehaviour
    {
        private int MaxPlayers = 10;

        public Dictionary<string, PlayerData> playerDatas = new Dictionary<string, PlayerData>();
        
        private void Start()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = null;

            if (playerDatas.TryGetValue(FindObjectOfType<UILobby>().GetPlayerName, out var data))
            {
                data.clientId = request.ClientNetworkId;
            }
            
            response.Position = GetPlayerSpawnPosition();
            
            if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
            {
                response.Approved = false;
                response.Reason = "Server is Full";
            }
            
            response.Pending = false;
        }

        private Vector3 GetPlayerSpawnPosition()
        {
            return new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
        }
    }

    [Serializable]
    public class PlayerData
    {
        public ulong clientId;
        public string playerName = "anon";
        public string playerLobbyId = "none";
        public UILobby.ETeamColor ETeamColor = UILobby.ETeamColor.None;
        public int buttonChosen = -1;
    }
}