using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace ProgrammingTask
{
    /// <summary>
    /// This carry forward to next scene as PlayerHandler will use some of the value here
    /// </summary>
    public class ConnectionApprovalHandler : MonoBehaviour
    {
        private int MaxPlayers = 10;
        
        /// <summary>
        /// Data stored for Lobby
        /// </summary>
        public Dictionary<string, PlayerData> playerDatas = new Dictionary<string, PlayerData>();

        [Header("Local player data")]
        //! Currently not syncing data across network
        [SerializeField] private string _playerName = "";
        [SerializeField] private int _playerIndex = -1;
        
        private void Start()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

            //! Maybe if have time in future, to refactor this using delegate action instead
            //! While loop might crash
            while (_playerName == "")
            {
                UILobby lobby = FindObjectOfType<UILobby>();
                _playerName = lobby.GetPlayerName;
            }
        }

        //! For some reason, CreatePlayerObject spawn player in previous scene instead of new 'Game' scene
        //! Hence, doing workaround with PlayerHandler
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

            //! Probably better to use delegate action instead
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