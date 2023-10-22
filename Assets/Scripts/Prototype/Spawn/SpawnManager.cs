using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace ProgrammingTask
{
    /// <summary>
    /// Player handler get desired spawn position
    /// </summary>
    public class SpawnManager : NetworkBehaviour
    {
        public static SpawnManager Instance;
        
        [SerializeField] 
        private SpawnPoints[] _spawnPointsArray = Array.Empty<SpawnPoints>();

        private void Start()
        {
            Instance = this;
            _spawnPointsArray = FindObjectsOfType<SpawnPoints>();
        }
        
        public Vector3 GetSpawnPosition(int index)
        {
            foreach (var spawnPoint in _spawnPointsArray)
            {
                if (spawnPoint.GetSpawnIndex == index)
                {
                    return spawnPoint.transform.position;
                }
            }

            return Vector3.zero;
        }
    }
}