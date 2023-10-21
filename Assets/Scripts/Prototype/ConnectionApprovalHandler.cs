using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ProgrammingTask
{
    public class ConnectionApprovalHandler : MonoBehaviour
    {
        private int MaxPlayers = 10;
        
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

            if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
            {
                response.Approved = false;
                response.Reason = "Server is Full";
            }

            response.Pending = false;
        }
    }
}