using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace  ProgrammingTask
{
    public class UILobbyTeam : MonoBehaviour
    {
        public static UILobbyTeam Instance;

        [SerializeField] private UILobby _uiLobby;

        [Header("UI-Team Button")] 
        [SerializeField] 
        private UILobbyTeamButton[] _teamButtonsList = Array.Empty<UILobbyTeamButton>();
        private UILobbyTeamButton teamButton;

        [Space] 
        [SerializeField] private int _currentlyOccupied = -1;
        
        #region  private variables heartbeat
        private string teamColor = "";
        private int occupiedButtonIndex = 0;
        private string occupiedButton = "";
        private UILobby.ETeamColor eTeamColor = UILobby.ETeamColor.None;
        #endregion

        private void Awake()
        {
            if (Instance == null && Instance != this)
                Instance = this;
            
            _teamButtonsList = FindObjectsOfType<UILobbyTeamButton>(true);
        }

        public void ChooseTeam(UILobby.ETeamColor eTeamColor, Action<bool> callback)
        {
            //! Call lobby event
            _uiLobby.UpdatePlayerTeam(eTeamColor);
            if (callback != null)
                callback.Invoke(true);
        }

        public void SetCurrentOccupiedButton(int newOccupyIndex)
        {
            _currentlyOccupied = newOccupyIndex;
            _uiLobby.UpdateOccupiedButton(newOccupyIndex);
        }

        /// <summary>
        /// Refresh all occupied buttons
        /// </summary>
        /// <param name="playerList"></param>
        public void UpdateCurrentOccupiedButtons(List<Unity.Services.Lobbies.Models.Player> playerList)
        {
            if (playerList.Count <= 0)
                return;
            
            //! Set initial state to false
            foreach (var t in _teamButtonsList)
            {
                t.SetOccupy(false);
            }
            
            //! Set the state to current state
            foreach (Unity.Services.Lobbies.Models.Player player in playerList)
            {
                if (_teamButtonsList.Length <= 0)
                     return;
                
                teamColor = player.Data["TeamColor"].Value;
                eTeamColor = (UILobby.ETeamColor)Enum.Parse(typeof(UILobby.ETeamColor),teamColor);
                occupiedButton = player.Data["OccupiedButton"].Value;
                occupiedButtonIndex = int.Parse(occupiedButton);

                foreach (var e in _teamButtonsList)
                {
                    if (e.ButtonIndex == occupiedButtonIndex)
                    {
                        if (e.eTeamColour == eTeamColor)
                        {
                            e.SetOccupy(true, player.Data["PlayerName"].Value);
                        }
                    }
                }
            }
        }
        
        public int GetCurrentOccupiedButton() => _currentlyOccupied;
    }
}
