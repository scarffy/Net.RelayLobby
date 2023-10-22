using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammingTask
{

    public class SpawnPoints : MonoBehaviour
    {
        [SerializeField] private int _spawnIndex = -1;

        public int GetSpawnIndex => _spawnIndex;
    }
}