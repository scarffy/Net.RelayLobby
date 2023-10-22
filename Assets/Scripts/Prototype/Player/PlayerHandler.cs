using System.Collections;
using System.Collections.Generic;
using ProgrammingTask.NetPlayer;
using Unity.Netcode;
using UnityEngine;

namespace ProgrammingTask
{
    public class PlayerHandler : NetworkBehaviour
    {
        [SerializeField] private int _playerChosenPosition;

        [SerializeField] private PlayerMovement _playerMovement;
        
        public override void OnNetworkSpawn()
        {
            if(!IsOwner)
                return;
            base.OnNetworkSpawn();

            ConnectionApprovalHandler approvalHandler = FindObjectOfType<ConnectionApprovalHandler>();
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            
            if (approvalHandler.playerDatas.TryGetValue(approvalHandler.GetPlayerName, out PlayerData data))
            {
                _playerChosenPosition = data.buttonChosen;
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!IsOwner)
                return;
            
            if (sceneEvent.SceneName == "Game" && sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                StartCoroutine(MovePlayerToSpawnPosition());
                _playerMovement.SetupPlayer();
            }
        }

        private IEnumerator MovePlayerToSpawnPosition()
        {
            yield return new WaitUntil(() => SpawnManager.Instance != null);
            MoveToSpawnPosition_ServerRpc();
        }


        [ServerRpc]
        public void MoveToSpawnPosition_ServerRpc()
        {
            transform.position = SpawnManager.Instance.GetSpawnPosition(_playerChosenPosition);
        }
    }
}
