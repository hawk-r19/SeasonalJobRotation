using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using Il2Cpp;

namespace WorkerRotation
{
    internal class TestMod : MelonMod
    {
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                MelonLogger.Msg("V pressed");
                GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                if (gameManager != null)
                {
                    Vector3 mousePosition = Input.mousePosition;
                    Vector3 terrainWorldPointUnderScreenPoint = gameManager.terrainManager.GetTerrainWorldPointUnderScreenPoint(mousePosition);
                    gameManager.villagerPopulationManager.SpawnVillagerImmigration(terrainWorldPointUnderScreenPoint, true);
                }
            }
        }
    }
}
