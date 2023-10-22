using System.Collections;
using System.Collections.Generic;
using ProgrammingTask.NetPlayer;
using Unity.Netcode;
using UnityEngine;

namespace ProgrammingTask
{
    public class PlayerHandler : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        
        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
        
        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            //! This run completely on server side
            if (sceneEvent.SceneName == "Game" && sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                Debug.Log("With scene event");
                StartCoroutine(MovePlayerToSpawnPosition());
                StartCoroutine(SetupPlayer());
            }
        }

        //! For some reason I couldn't make it move to spawn desired. I give up. 
        private IEnumerator MovePlayerToSpawnPosition()
        {
            yield return new WaitUntil(() => SpawnManager.Instance != null);
            
            MoveToSpawnPosition_ClientRpc();
        }

        [ClientRpc]
        public void MoveToSpawnPosition_ClientRpc()
        {
            if(!IsLocalPlayer)
                return;
            
            ConnectionApprovalHandler approvalHandler = FindObjectOfType<ConnectionApprovalHandler>();
            int index = approvalHandler.GetPlayerIndex;
            
            transform.position = SpawnManager.Instance.GetSpawnPosition(index);
            Debug.Log("Moving player to random position");
        }

        //! For some weird reason, this work fine
        private IEnumerator SetupPlayer()
        {
            yield return new WaitUntil(() => SpawnManager.Instance != null);
            SetupPlayer_ClientRpc();
            Debug.Log("Setup player");
        }

        [ClientRpc]
        private void SetupPlayer_ClientRpc()
        {
            if(IsLocalPlayer)
                _playerMovement.SetupPlayer();
        }
    }
}
