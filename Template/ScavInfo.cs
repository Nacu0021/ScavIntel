using UnityEngine;
using MoreSlugcats;
using System.Collections.Generic;
using System;

namespace ScavIntel
{
    public class ScavInfo
    {
        public int[] killedScavs;
        public int[] availableScavs;
        public int[] allScavs;
        public float squadCooldown;
        public int squadScavsCount;
        public bool cycleStartInit;
        public Action StatsUpdated;

        //public int Normal1A => killedScavs[0];
        //public int Elite1A => killedScavs[1];
        //public int Normal2A => killedScavs[0] + availableScavs[0];
        //public int Elite2A => killedScavs[1] + availableScavs[1];
        //public int Normal3A => allScavs[0];
        //public int Elite3A => allScavs[1];

        public ScavInfo()
        {
            // 0 is default scav, 1 is elite
            Reset(true);
        }

        public void Reset(bool total)
        {
            killedScavs = new int[2];
            availableScavs = new int[2];
            allScavs = new int[2];
            squadCooldown = 0;
            squadScavsCount = 0;
            if (total)
            {
                cycleStartInit = false;
            }
        }
            
        public void AddKill(bool elite)
        {
            if (elite) killedScavs[1]++;
            else killedScavs[0]++;

            //StatsUpdated?.Invoke();

            Plugin.logger.LogMessage($"Player killed a{(elite ? "n elite" : "")} scavenger!! Current kills: {(killedScavs[elite ? 1 : 0])}(1a)");
        }

        public void UpdateAvailableScavs(World world)
        {
            Plugin.logger.LogMessage($"UPDATING AVAILABLE SCAVS");
            if (world.scavengersWorldAI == null) return;

            int scavCount = 0;
            int eliteCount = 0;

            foreach (var scav in world.scavengersWorldAI.scavengers)
            {
                bool nightCheck = scav.parent.nightCreature && world.rainCycle.dayNightCounter < 600;
                bool inDenCheck = scav.destination == scav.denPosition && scav.parent.pos.room == world.offScreenDen.index && !cycleStartInit;
                bool precycleCheck = ModManager.MSC && scav.parent.preCycle && world.rainCycle.maxPreTimer <= 0;
                Plugin.logger.LogMessage($"Scavenger {scav.parent}. Dead - {scav.parent.state.dead}. Den - {(scav.parent.InDen || scav.parent.pos.room == world.offScreenDen.index) && scav.parent.WantToStayInDenUntilEndOfCycle()}. Night check 1 - {nightCheck}. InDenCheck - {inDenCheck}.");
                if (scav.parent.state.dead || precycleCheck || ((scav.parent.InDen || scav.parent.pos.room == world.offScreenDen.index) && scav.parent.WantToStayInDenUntilEndOfCycle()) || nightCheck || inDenCheck) continue; //|| nightCheck2
                if (scav.parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
                {
                    eliteCount++;
                }
                else
                {
                    scavCount++;
                }
                //Plugin.logger.LogMessage($"Scavenger {scav.parent} passed airport security :o");
            }

            availableScavs[0] = scavCount;
            availableScavs[1] = eliteCount;
            StatsUpdated?.Invoke();

            Plugin.logger.LogMessage($"New available scav counts: {availableScavs[0]} - {availableScavs[1]}. 2a: {availableScavs[0] + killedScavs[0]} - {availableScavs[1] + killedScavs[1]}");
        }

        public void UpdateGlobalScavCount(World world)
        {
            Plugin.logger.LogMessage($"UPDATING GLOBAL SCAV COUNT");
            if (world.scavengersWorldAI == null) return;

            int scavCount = allScavs[0];
            int eliteCount = allScavs[1];

            foreach (var scav in world.scavengersWorldAI.scavengers)
            {
                //bool inDenCheck = scav.destination == scav.denPosition && scav.parent.pos.room == world.offScreenDen.index && cycleStartInit;
                Plugin.logger.LogMessage($"Scavenger {scav.parent}. Dead - {scav.parent.state.dead}.");
                bool precycleCheck = ModManager.MSC && scav.parent.preCycle && world.rainCycle.maxPreTimer <= 0;
                if (scav.parent.state.dead || precycleCheck) continue;// || ((scav.parent.InDen || scav.parent.pos.room == world.offScreenDen.index) && scav.parent.WantToStayInDenUntilEndOfCycle()) || inDenCheck) continue;
                if (scav.parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
                {
                    eliteCount++;
                }
                else
                {
                    scavCount++;
                }
            }

            allScavs[0] = scavCount;
            allScavs[1] = eliteCount;
            StatsUpdated?.Invoke();

            Plugin.logger.LogMessage($"New global scav count 3a: {allScavs[0]} - {allScavs[1]}");
        }

        public void UpdateSquadCount(ScavengersWorldAI scavsAI)
        {
            int count = 0;

            foreach (var squad in scavsAI.playerAssignedSquads)
            {
                if (squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature && squad.targetCreature != null && squad.targetCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    count += squad.members.Count;
                }
            }

            squadScavsCount = count;
        }

        public void CooldownToSeconds(int v)
        {
            squadCooldown = Mathf.FloorToInt((float)v / 40f);
        }
    }
}
