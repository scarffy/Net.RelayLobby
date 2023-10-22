using System.Collections;
using System.Collections.Generic;
using ProgrammingTask.NetPlayer;
using Unity.Netcode;
using UnityEngine;

namespace ProgrammingTask
{
    /// <summary>
    /// Handles player initial spawn logic
    /// </summary>
    public class PlayerHandler : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        
        public override void OnNetworkSpawn()
        {
            //! Subcribe to load scene event
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
        
        /// <summary>
        /// This function run completely on server side
        /// </summary>
        /// <param name="sceneEvent"></param>
        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            //! This run completely on server side (Didn't know this before)
            if (sceneEvent.SceneName == "Game" && sceneEvent.SceneEventType == SceneEventType.LoadComplete)
            {
                StartCoroutine(MovePlayerToSpawnPosition());
                StartCoroutine(SetupPlayer());
            }
        }

        //! For some reason I couldn't make it move to spawn desired. I give up. 
        //! TODO: Revisit this when I have extra extra time
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

        //! For some weird reason, this work fine but move player doesn't work.
        //! Why? Logic error?
        private IEnumerator SetupPlayer()
        {
            yield return new WaitUntil(() => SpawnManager.Instance != null);
            SetupPlayer_ClientRpc();
        }

        [ClientRpc]
        private void SetupPlayer_ClientRpc()
        {
            if(IsLocalPlayer)
                _playerMovement.SetupPlayer();
        }
    }
}
