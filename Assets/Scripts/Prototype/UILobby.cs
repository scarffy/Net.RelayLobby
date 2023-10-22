using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProgrammingTask
{
    public class UILobby : MonoBehaviour
    {
        [Header("UI-Panel")] [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _lobbyPanel;

        [Header("UI-Button")] 
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quickJoinButton;
        [SerializeField] private Button _leaveLobbyButton;

        [SerializeField] private Button _hostStartGameButton;

        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private float heartbeatTimer;
        private float lobbyUpdateTimer;

        [Header("Player Info Settings")] 
        [SerializeField] private string _playerName;

        [SerializeField] private bool _isHost = false;

        [Space] [SerializeField] private string _joinRelayCode = "";

        [Space] [SerializeField] private ConnectionApprovalHandler _approvalHandler;
        
        private async void Start()
        {
            _playButton.onClick.AddListener(CreateLobby);
            _quickJoinButton.onClick.AddListener (QuickJoinLobby);
            _leaveLobbyButton.onClick.AddListener(LeaveLobby);
            _hostStartGameButton.onClick.AddListener(StartGame);

            _playerName = "anon" + UnityEngine.Random.Range(10, 99);

            await UnityServices.InitializeAsync();
            Debug.Log(UnityServices.State);

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId} {_playerName}");
            };
            
            //! User sign in as anon
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void Update()
        {
            HandleLobbyHeartbeat();
            HandleLobbyUpdate();
        }

        private async Task<string> CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                Debug.LogWarning($"Join code {joinCode}");

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                
                NetworkManager.Singleton.StartHost();
                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
                return null;
            }
        }

        private async void JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                
                NetworkManager.Singleton.StartClient();
                
                SetLobbyMode(LobbyMode.game);
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
                throw;
            }
        }
        
        /// <summary>
        /// To prevent lobby from deactivated
        /// </summary>
        private async void HandleLobbyHeartbeat()
        {
            if (_hostLobby != null)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer < 0f)
                {
                    float heartbeatTimerMax = 15f;
                    heartbeatTimer = heartbeatTimerMax;

                    await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
                }
            }
        }
        
        /// <summary>
        /// Handle anything regarding lobby's data
        /// </summary>
        private async void HandleLobbyUpdate()
        {
            if (_joinedLobby != null)
            {
                lobbyUpdateTimer -= Time.deltaTime;
                if (lobbyUpdateTimer < 0f)
                {
                    //! Can't get lower than 1.f
                    float lobbyUpdateTimerMax = 1.5f;
                    lobbyUpdateTimer = lobbyUpdateTimerMax;

                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
                    _joinedLobby = lobby;

                    if (_joinedLobby.Data["StartGame"].Value != "0")
                    {
                        if (!_isHost)
                        {
                            JoinRelay(_joinedLobby.Data["StartGame"].Value);
                        }

                        _joinedLobby = null;
                    }
                    
                    //! Update button lobby
                    UpdateTeamButton(lobby);
                    UpdateTeamList(lobby);
                }
            }
        }

        private void UpdateTeamButton(Lobby lobby)
        {
            foreach (Player player in lobby.Players)
            {
                UILobbyTeam.Instance.UpdateCurrentOccupiedButtons(lobby.Players);
            }
        }

        private void UpdateTeamList(Lobby lobby)
        {
            if (_approvalHandler.playerDatas.Count <= 0)
            {
                Debug.Log("PlayerData Dict is empty");
                return;
            }
            else
            {
                foreach (var player in lobby.Players)
                {
                    if (!_approvalHandler.playerDatas.ContainsKey(player.Data["PlayerName"].Value))
                    {
                        PlayerData data = new PlayerData();
                        data.buttonChosen = int.Parse(player.Data["OccupiedButton"].Value);
                        data.ETeamColor = (ETeamColor)Enum.Parse(typeof(ETeamColor),player.Data["TeamColor"].Value);
                        
                        _approvalHandler.playerDatas.TryAdd(player.Data["PlayerName"].Value, data);
                    }
                    else
                    {
                        PlayerData data = new PlayerData();
                        data.buttonChosen = int.Parse(player.Data["OccupiedButton"].Value);
                        data.ETeamColor = (ETeamColor)Enum.Parse(typeof(ETeamColor),player.Data["TeamColor"].Value);
                        data.playerName = player.Data["PlayerName"].Value;
                        data.playerLobbyId = player.Id;

                        _approvalHandler.playerDatas[player.Data["PlayerName"].Value].playerName = data.playerName;
                        _approvalHandler.playerDatas[player.Data["PlayerName"].Value].ETeamColor = data.ETeamColor;
                        _approvalHandler.playerDatas[player.Data["PlayerName"].Value].buttonChosen = data.buttonChosen;
                        _approvalHandler.playerDatas[player.Data["PlayerName"].Value].playerLobbyId = data.playerLobbyId;
      
                    }
                }
                
                foreach (var playerData in _approvalHandler.playerDatas)
                {
                    Debug.Log($"[PlayerData List Update]: Key:{playerData.Key} Name:{playerData.Value.playerName}" +
                              $" LobbyId:{playerData.Value.playerLobbyId} TeamColor:{playerData.Value.ETeamColor}" +
                              $" ButtonChose:{playerData.Value.buttonChosen} ClientId:{playerData.Value.clientId}");
                }
            }
            
        }

        private async void CreateLobby()
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 10;
            
            Player player = GetPlayer();
            
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "StartGame", new DataObject(DataObject.VisibilityOptions.Member, "0")
                    }
                }
            };
            
            Lobby lobby =
                await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers,options);
            
            _hostLobby = lobby;
            _joinedLobby = lobby;

            _isHost = true;
            
            SetLobbyMode(LobbyMode.lobby);
            
            AddPlayerToDictionary(_joinedLobby);
        }

        private async void ListLobbies()
        {
            try
            {
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

                Debug.Log($"Lobbies found: {queryResponse.Results.Count}");
                foreach (Lobby lobby in queryResponse.Results)
                {
                    Debug.Log($"{lobby.Name}: {lobby.Players.Count}/{lobby.MaxPlayers}");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }
        }
        private async void QuickJoinLobby()
        {
            try
            {
                QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
                {
                    Player = GetPlayer()
                };
                
                Player player = GetPlayer();
                
                Lobby joinedLobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
                
                _joinedLobby = joinedLobby;

                SetLobbyMode(LobbyMode.lobby);
                
                PrintPlayers(joinedLobby);
                
                if (!_approvalHandler.playerDatas.ContainsKey(_playerName))
                {
                    AddPlayerToDictionary(joinedLobby);
                }
                else
                {
                    Debug.Log("Failed to add player to dictionary. Data already existed");
                }

                _hostStartGameButton.gameObject.SetActive(false);
                
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }
        }
        
        /// <summary>
        /// Set Player default value in lobby
        /// </summary>
        /// <returns></returns>
        private Player GetPlayer()
        {
            return new Player {
                Data = new Dictionary<string, PlayerDataObject> 
                {
                    { 
                        "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)
                    },
                    {
                        "TeamColor", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ETeamColor.None.ToString())
                    },
                    {
                        "OccupiedButton", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "-1")
                    }
                }
            };
        }

        private void AddPlayerToDictionary(Lobby lobby)
        {
            foreach (Player player in lobby.Players)
            {
                PlayerData data = new PlayerData();
                data.ETeamColor = (ETeamColor)Enum.Parse(typeof(ETeamColor),player.Data["TeamColor"].Value);
                data.buttonChosen = int.Parse(player.Data["OccupiedButton"].Value);
                data.playerName = player.Data["PlayerName"].Value;
                data.playerLobbyId = player.Id;
                
                if(_approvalHandler.playerDatas.TryAdd(player.Data["PlayerName"].Value,data))
                {
                    Debug.Log($"[AddPlayerToDictionary] New player {player.Data["PlayerName"].Value} added");
                }
                else
                {
                    Debug.Log("[AddPlayerToDictionary] Player data has existed");
                }
            }
        }

        private void PrintPlayers(Lobby lobby)
        {
            foreach (Player player in lobby.Players)
            {
                Debug.Log($"{player.Id} {player.Data["PlayerName"].Value} {player.Data["TeamColor"].Value} {player.Data["OccupiedButton"].Value}");
            }

            foreach (var playerData in _approvalHandler.playerDatas)
            {
                Debug.Log($"[PlayerData Print]: Key:{playerData.Key} Name:{playerData.Value.playerName}" +
                          $" LobbyId:{playerData.Value.playerLobbyId} TeamColor:{playerData.Value.ETeamColor}" +
                          $" ButtonChose:{playerData.Value.buttonChosen} ClientId:{playerData.Value.clientId}");
            }
        }
        
        private async void UpdatePlayerName(string newPlayerName)
        {
            _playerName = newPlayerName;
            try
            {
                Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId,
                    new UpdatePlayerOptions
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { 
                                "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)
                            }
                        }
                    });
                _joinedLobby = lobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }
        }

        public async void UpdatePlayerTeam(ETeamColor eTeamColor)
        {
            try
            {
                Lobby lobby =  await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId,
                    new UpdatePlayerOptions
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            {
                                "TeamColor", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, eTeamColor.ToString())
                            }
                        }
                    });

                if (_approvalHandler.playerDatas.TryGetValue(_playerName, out var data))
                {
                    data.ETeamColor = eTeamColor;
                }

                _joinedLobby = lobby;
                
                 PrintPlayers(lobby);

            }
            catch (Exception e)
            {
               Debug.LogWarning(e);
            }   
        }

        public async void UpdateOccupiedButton(int indexOccupied)
        {
            try
            {
                string occupiedButton = "OccupiedButton";

                //! Update lobby occupy
                Lobby lobby =  await LobbyService.Instance.UpdatePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            occupiedButton, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, indexOccupied.ToString())
                        }
                    }
                });
                
                if (_approvalHandler.playerDatas.TryGetValue(_playerName, out var data))
                {
                    data.buttonChosen = indexOccupied;
                }
                
                _joinedLobby = lobby;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private async void LeaveLobby()
        {
            try
            {
                //! This function also can be used to kick another player
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                SetLobbyMode(LobbyMode.menu);
                _hostLobby = null;
                _joinedLobby = null;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private async void MigrateLobbyHost()
        {
            try
            {
                _hostLobby = await Lobbies.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
                {
                    HostId = _joinedLobby.Players[1].Id
                });
                _joinedLobby = _hostLobby;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void SetLobbyMode(LobbyMode lobbyMode)
        {
            switch (lobbyMode)
            {
                case LobbyMode.menu:
                    _menuPanel.SetActive(true);
                    _lobbyPanel.SetActive(false);
                    break;
                
                case LobbyMode.lobby:
                    _menuPanel.SetActive(false);
                    _lobbyPanel.SetActive(true);
                    break;
                case LobbyMode.game:
                    _menuPanel.SetActive(false);
                    _lobbyPanel.SetActive(false);
                    break;
            }
        }

        private async void StartGame()
        {
            if (_isHost)
            {
                try
                {
                    string relayCode = await CreateRelay();
                    
                    Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
                        {
                            Data = new Dictionary<string, DataObject>
                            {
                                {
                                    "StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode)
                                }
                            }
                        });

                    _joinedLobby = lobby;
                    SetLobbyMode(LobbyMode.game);
                    ChangeScene();
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        private void ChangeScene()
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            if(status != SceneEventProgressStatus.Started)
                Debug.LogWarning("Scene failed to load");
        }

        public string GetPlayerName => _playerName;
        
        public enum LobbyMode
        {
            menu,
            lobby,
            game
        }
        
        public enum ETeamColor
        {
            Blue,
            Red,
            None
        }
    }
}