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
        public int squadCooldown;
        public int squadScavsCount;
        public bool cycleStartInit;
        public Action StatsUpdated;

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

        public void UpdateAvailableScavs(World world, bool fromKill)
        {
            Plugin.logger.LogMessage($"UPDATING AVAILABLE SCAVS");
            if (world.scavengersWorldAI == null) return;

            int lastScavCount = availableScavs[0];
            int lastEliteCount = availableScavs[1];

            int scavCount = 0;
            int eliteCount = 0;

            foreach (var scav in world.scavengersWorldAI.scavengers)
            {
                bool nightCheck = scav.parent.nightCreature && world.rainCycle.dayNightCounter < 600;
                bool inDenCheck = scav.destination == scav.denPosition && scav.parent.pos.room == world.offScreenDen.index && !cycleStartInit;
                bool precycleCheck = ModManager.MSC && scav.parent.preCycle && world.rainCycle.maxPreTimer <= 0;
                //Plugin.logger.LogMessage($"Scavenger {scav.parent}. Dead - {scav.parent.state.dead}. Den - {(scav.parent.InDen || scav.parent.pos.room == world.offScreenDen.index) && scav.parent.WantToStayInDenUntilEndOfCycle()}. Night check 1 - {nightCheck}. InDenCheck - {inDenCheck}.");
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

            if (!cycleStartInit || world.game.cameras == null || world.game.cameras[0].hud == null) return;
            foreach (var g in world.game.cameras[0].hud.parts)
            {
                if (g is IntelHUD hud)
                {
                    if (scavCount != lastScavCount)
                    {
                        hud.showScavengerData = 70;
                        hud.scavengerDataOnLast = true;
                        hud.scavengerDataFromKill = fromKill;
                    }
                    if (eliteCount != lastEliteCount)
                    {
                        hud.showEliteData = 70;
                    }
                    break;
                }
            }
        }

        public void UpdateGlobalScavCount(World world)
        {
            Plugin.logger.LogMessage($"UPDATING GLOBAL SCAV COUNT");
            if (world.scavengersWorldAI == null) return;

            int lastScavCount = allScavs[0];
            int lastEliteCount = allScavs[1];

            int scavCount = allScavs[0];
            int eliteCount = allScavs[1];

            foreach (var scav in world.scavengersWorldAI.scavengers)
            {
                //bool inDenCheck = scav.destination == scav.denPosition && scav.parent.pos.room == world.offScreenDen.index && cycleStartInit;
                //Plugin.logger.LogMessage($"Scavenger {scav.parent}. Dead - {scav.parent.state.dead}.");
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

            //if (world.game.cameras == null || world.game.cameras[0].hud == null) return;
            //foreach (var g in world.game.cameras[0].hud.parts)
            //{
            //    if (g is IntelHUD hud)
            //    {
            //        if (scavCount != lastScavCount)
            //        {
            //            hud.showScavengerData = 50;
            //        }
            //        if (eliteCount != lastEliteCount)
            //        {
            //            hud.showEliteData = 50;
            //        }
            //        break;    
            //    }
            //}
        }

        public void UpdateSquadCount(ScavengersWorldAI scavsAI)
        {
            int lastCount = squadScavsCount;
            int count = 0;

            foreach (var squad in scavsAI.playerAssignedSquads)
            {
                if (squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.HuntCreature && squad.targetCreature != null && squad.targetCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    count += squad.members.Count;
                }
            }

            squadScavsCount = count;

            if (scavsAI.world.game.cameras == null || scavsAI.world.game.cameras[0].hud == null) return;
            foreach (var g in scavsAI.world.game.cameras[0].hud.parts)
            {
                if (g is IntelHUD hud)
                {
                    if (lastCount != squadScavsCount)
                    {
                        hud.showSquadCount = 100;
                    }
                    break;
                }
            }
        }

        public void CooldownToSeconds(int v, World world)
        {
            int lastCooldown = squadCooldown;
            squadCooldown = Mathf.FloorToInt((float)v / 40f);

            if (world.game.cameras == null || world.game.cameras[0].hud == null) return;
            foreach (var g in world.game.cameras[0].hud.parts)
            {
                if (g is IntelHUD hud)
                {
                    if (squadCooldown > 0 && squadCooldown < 6)
                    {
                        hud.showCooldown = 100;
                    }
                    if (squadCooldown == 0 && lastCooldown > squadCooldown)
                    {
                        hud.showSquadCount = 100;
                    }
                    break;
                }
            }
        }
    }
}
