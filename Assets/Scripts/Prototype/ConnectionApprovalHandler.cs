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

        [Space]
        [SerializeField] private string _playerName = "";

        [SerializeField] private int _playerIndex = -1;
        
        private void Start()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

            while (_playerName == "")
            {
                UILobby lobby = FindObjectOfType<UILobby>();
                _playerName = lobby.GetPlayerName;
            }
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

            if(playerDatas.ContainsKey(_playerName))
            {
                if (playerDatas.TryGetValue(_playerName, out PlayerData data))
                {
                    data.clientId = request.ClientNetworkId;
                    _playerIndex = data.buttonChosen;
                }
            }
            
            response.Pending = false;
        }


        public string GetPlayerName => _playerName;
        public int GetPlayerIndex => _playerIndex;
        public int SetPlayerIndex(int index) => _playerIndex = index;
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