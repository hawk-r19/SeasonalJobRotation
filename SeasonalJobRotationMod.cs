using System;
using System.Linq;
using UnityEngine;
using MelonLoader;
using Il2Cpp;

namespace SeasonalJobRotation
{
	public class SeasonalJobRotationMod : MelonMod
    {
        private bool workEnabled = true;
        private GameManager gm;
		private TimeManager timeManager;
        private ResourceManager resourceManager;
        private int[] farmerCounts;
        private bool triggeredThisWinter = false;
        private const int delay = 100;
        private int ticks = 0;
        private MelonPreferences_Category userData;
        private MelonPreferences_Entry<int> daysEarly;

        private bool delayed()
		{
			if (ticks < delay)
			{
				ticks++;
				return false;
			}
			ticks = 0;
			return true;
		}

		private void TryInit()
		{
			gm = UnityEngine.Object.FindObjectOfType<GameManager>();
			if ((UnityEngine.Object)(object)gm != null)
			{
                timeManager = gm.timeManager;
                resourceManager = gm.resourceManager;
                Il2CppSystem.Collections.Generic.List<Cropfield> fields = resourceManager.cropFields;
                farmerCounts = new int[fields.Count];
                for(int i = 0; i < fields.Count; ++i)
                {
                    farmerCounts[i] = fields[i].allocatedWorkers;
                }
                userData = MelonPreferences.CreateCategory("SeasonalJobRotation");
                daysEarly = userData.CreateEntry<int>("DaysBeforeSpring", 5);
                if (daysEarly.Value < 5) daysEarly.Value = 5;
                if(daysEarly.Value > 60) daysEarly.Value = 60;
                base.LoggerInstance.Msg("init done! DaysBeforeSpring set to " + daysEarly.Value);
			}
		}

		public override void OnUpdate()
		{
			//if (Input.GetKeyDown(KeyCode.Insert)) testSwap();

			if (!delayed()) return;

			if (gm == null)
			{
				TryInit();
				return;
			}

            float dayOfYear = (timeManager.currentDate.month - 1) * TimeManager.DAYS_PER_MONTH + timeManager.currentDate.day;
            if (!triggeredThisWinter && (dayOfYear >= TimeManager.FIRST_DAY_OF_WINTER || 
                dayOfYear < TimeManager.FIRST_DAY_OF_SPRING - daysEarly.Value)) OnWinterStart();
            else if (triggeredThisWinter && dayOfYear >= TimeManager.FIRST_DAY_OF_SPRING - daysEarly.Value && 
                dayOfYear < TimeManager.FIRST_DAY_OF_SUMMER) OnWinterEnd();
            
		}

		private void OnWinterStart()
		{
            base.LoggerInstance.Msg("Winter started, rotating workers");
			triggeredThisWinter = true;

            int shackCount = resourceManager.foragerShacks.Count;
            foreach (ForagerShack shack in resourceManager.foragerShacks) shack.SetWorkEnabled(false, true);

            int arborCount = resourceManager.arboristBuildings.Count;
            foreach (ArboristBuilding arbor in resourceManager.arboristBuildings) arbor.SetWorkEnabled(false, true);

            int barnCount = resourceManager.barns.Count;
            foreach (Barn barn in resourceManager.barns) barn.SetWorkEnabled(false, true);

            //farmers
            Il2CppSystem.Collections.Generic.List<Cropfield> fields = resourceManager.cropFields;
            farmerCounts = new int[fields.Count];
            for (int i = 0; i < fields.Count; ++i)
            {
                farmerCounts[i] = fields[i].allocatedWorkers;
                fields[i].userDefinedMaxWorkers = 0;
                fields[i].allocatedWorkers = 0;
            }
            if (!gm.villagerAutoSwapOccupationManager.isAutoSwapActive)
            { //occupation auto-fill is off, need to manually swap farmers
                gm.villagerAutoSwapOccupationManager.SwapFarmers(-1 * farmerCounts.Sum(), true);
            }
            base.LoggerInstance.Msg("Turned off " + shackCount + " forager shacks, " + arborCount +
                " arborist buildings, " + barnCount + " barns, and removed " + farmerCounts.Sum() + " farmers.");
        }

		private void OnWinterEnd()
		{
            base.LoggerInstance.Msg("Winter ended, rotating workers");
			triggeredThisWinter = false;

            int shackCount = resourceManager.foragerShacks.Count;
            foreach (ForagerShack shack in resourceManager.foragerShacks) shack.SetWorkEnabled(true, true);

            int arborCount = resourceManager.arboristBuildings.Count;
            foreach (ArboristBuilding arbor in resourceManager.arboristBuildings) arbor.SetWorkEnabled(true, true);
            
            int barnCount = resourceManager.barns.Count;
            foreach (Barn barn in resourceManager.barns) barn.SetWorkEnabled(true, true);

            //farmers
            Il2CppSystem.Collections.Generic.List<Cropfield> fields = resourceManager.cropFields;
            for (int i = 0; i < fields.Count; i++)
            {
                fields[i].userDefinedMaxWorkers = farmerCounts[i];
                fields[i].allocatedWorkers = farmerCounts[i];
            }
            if (!resourceManager.villagerAutoSwapOccupationManager.isAutoSwapActive)
            { //occupation auto-fill is off, need to manually swap farmers
                gm.villagerAutoSwapOccupationManager.SwapFarmers(farmerCounts.Sum(), true);
            }
            base.LoggerInstance.Msg("Turned on " + shackCount + " forager shacks, " + arborCount +
                " arborist buildings, " + barnCount + " barns, and added " + farmerCounts.Sum() + " farmers.");
        }

        private void testSwap()
        {
            if(workEnabled) //onWinterStart
            {
                workEnabled = false;

                int shackCount = resourceManager.foragerShacks.Count;
                foreach (ForagerShack shack in resourceManager.foragerShacks) shack.SetWorkEnabled(false, true);

                int arborCount = resourceManager.arboristBuildings.Count;
                foreach (ArboristBuilding arbor in resourceManager.arboristBuildings) arbor.SetWorkEnabled(false, true);

                //farmers
                Il2CppSystem.Collections.Generic.List<Cropfield> fields = resourceManager.cropFields;
                farmerCounts = new int[fields.Count];
                for (int i = 0; i < fields.Count; ++i)
                {
                    farmerCounts[i] = fields[i].allocatedWorkers;
                    fields[i].userDefinedMaxWorkers = 0;
                    fields[i].allocatedWorkers = 0;
                }
                if (!gm.villagerAutoSwapOccupationManager.isAutoSwapActive)
                { //occupation auto-fill is off, need to manually swap farmers
                    gm.villagerAutoSwapOccupationManager.SwapFarmers(-1 * farmerCounts.Sum(), true);
                }
                base.LoggerInstance.Msg("Turned off " + shackCount + " forager shacks, " + arborCount +
                    " arborist buildings, and swapped " + farmerCounts.Sum() + " farmers off of fields.");
            }
            else //OnWinterEnd
            {
                workEnabled = true;

                int shackCount = resourceManager.foragerShacks.Count;
                foreach (ForagerShack shack in resourceManager.foragerShacks) shack.SetWorkEnabled(true, true);

                int arborCount = resourceManager.arboristBuildings.Count;
                foreach (ArboristBuilding arbor in resourceManager.arboristBuildings) arbor.SetWorkEnabled(true, true);

                //farmers
                Il2CppSystem.Collections.Generic.List<Cropfield> fields = resourceManager.cropFields;
                for (int i = 0; i < fields.Count; i++)
                {
                    fields[i].userDefinedMaxWorkers = farmerCounts[i];
                    fields[i].allocatedWorkers = farmerCounts[i];
                }
                if (!resourceManager.villagerAutoSwapOccupationManager.isAutoSwapActive)
                { //occupation auto-fill is off, need to manually swap farmers
                    gm.villagerAutoSwapOccupationManager.SwapFarmers(farmerCounts.Sum(), true);
                }
                base.LoggerInstance.Msg("Turned on " + shackCount + " forager shacks, " + arborCount +
                    " arborist buildings, and swapped " + farmerCounts.Sum() + " farmers onto fields.");
            }
        }
    }
}
