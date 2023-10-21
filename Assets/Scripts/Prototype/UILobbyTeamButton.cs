using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ProgrammingTask
{
    public class UILobbyTeamButton : MonoBehaviour
    {
        [SerializeField] private Button _teamButton;
        [SerializeField] private TextMeshProUGUI _teamButtonText;

        [Header("Settings")] 
        [SerializeField] private int _buttonIdex;

        private const string JOIN_TEAM = "Join Team";

        public UILobby.ETeamColor eTeamColour = UILobby.ETeamColor.Blue;

        private bool _isOccupied = false;
        
        private void Awake()
        {
            if (_teamButton == null)
                _teamButton = GetComponent<Button>();
            if (_teamButtonText == null)
                _teamButtonText = GetComponentInChildren<TextMeshProUGUI>();
            
            _teamButton.onClick.AddListener(ChooseTeam);
        }

        /// <summary>
        /// Send button index and team
        /// </summary>
        private void ChooseTeam()
        {
            if(GetOccupied())
                return;

            UILobbyTeam.Instance.ChooseTeam(eTeamColour,eventCallback);
        }

        private void eventCallback(bool isSuccess)
        {
            if (isSuccess)
            {
                //! Handle success event
                _isOccupied = true;
                //_teamButtonText.SetText("Occupied");
                UILobbyTeam.Instance.SetCurrentOccupiedButton(_buttonIdex);
            }
            else
            {
                //! Handle fail event
                _isOccupied = false;
                _teamButtonText.SetText(JOIN_TEAM);
            }
        }

        public bool GetOccupied()
        {
            return _isOccupied;
        }

        public void SetOccupy(bool bIsOccupied)
        {
            _isOccupied = bIsOccupied;
            if (_isOccupied)
            {
                _teamButtonText.SetText("Occupied");
            }
            else
                _teamButtonText.SetText("Join Team");
        }

        public void SetOccupy(bool bIsOccupied, string playerName)
        {
            _isOccupied = bIsOccupied;
            if (_isOccupied)
            {
                _teamButtonText.SetText(playerName);
            }
            else
                _teamButtonText.SetText("Join Team");
        }

        public int ButtonIndex => _buttonIdex;
    }
}