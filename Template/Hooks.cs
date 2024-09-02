using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System.Linq;

namespace ScavIntel
{
    internal class Hooks
    {
        internal static ScavInfo GlobalInfo;

        internal static void Apply()
        {
            GlobalInfo = new ScavInfo();
            On.PlayerSessionRecord.AddKill += PlayerSessionRecord_AddKill;
            //On.ScavengersWorldAI.AddScavenger += ScavengersWorldAI_AddScavenger;
            On.ScavengersWorldAI.Update += ScavengersWorldAI_Update;
            On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;
            On.OverWorld.LoadWorld += OverWorld_LoadWorld;
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            On.ShelterDoor.Update += ShelterDoor_Update;
            On.AbstractRoom.MoveEntityToDen += AbstractRoom_MoveEntityToDen;
            On.AbstractRoom.MoveEntityOutOfDen += AbstractRoom_MoveEntityOutOfDen;
            On.ProcessManager.PreSwitchMainProcess += ProcessManager_PreSwitchMainProcess;
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        }

        private static void ProcessManager_PreSwitchMainProcess(On.ProcessManager.orig_PreSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            orig.Invoke(self, ID);

            if (ID == ProcessManager.ProcessID.Game)
            {
                GlobalInfo.Reset(true);
            }
        }

        private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, SlugcatStats.Name playerCharacterNumber, bool singleRoomWorld)
        {
            orig.Invoke(self, worldName, playerCharacterNumber, singleRoomWorld);

            if (!GlobalInfo.cycleStartInit)
            {
                GlobalInfo.UpdateGlobalScavCount(self.activeWorld);
                GlobalInfo.UpdateAvailableScavs(self.activeWorld);
                GlobalInfo.cycleStartInit = true;
            }
        }

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);
            self.AddPart(new IntelHUD(self));
        }

        private static void AbstractRoom_MoveEntityOutOfDen(On.AbstractRoom.orig_MoveEntityOutOfDen orig, AbstractRoom self, AbstractWorldEntity ent)
        {
            orig.Invoke(self, ent);

            if (ent is AbstractCreature crit && (crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite))
            {
                Plugin.logger.LogWarning("OUT OF DEN");
                GlobalInfo.UpdateAvailableScavs(self.world);
            }
        }

        private static void AbstractRoom_MoveEntityToDen(On.AbstractRoom.orig_MoveEntityToDen orig, AbstractRoom self, AbstractWorldEntity ent)
        {
            orig.Invoke(self, ent);

            if (ent is AbstractCreature crit && (crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite))
            {
                Plugin.logger.LogWarning("INTO DEN");
                GlobalInfo.UpdateAvailableScavs(self.world);
            }
        }

        private static void ShelterDoor_Update(On.ShelterDoor.orig_Update orig, ShelterDoor self, bool eu)
        {
            orig.Invoke(self, eu);

            if (self.closedFac < 0.04f)
            {
               //GlobalInfo.UpdateGlobalScavCount(self.room.world);
               //GlobalInfo.UpdateAvailableScavs(self.room.world);
               //GlobalInfo.cycleStartInit = true;

                foreach (var g in self.room.game.cameras[0].hud.parts)
                {
                    if (g is IntelHUD hud)
                    {
                        hud.simulatedMapPress = 200;
                        break;
                    }
                }
            }
        }

        private static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            orig.Invoke(self, game, survived, newMalnourished);

            GlobalInfo.Reset(true);
        }

        private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self)
        {
            orig.Invoke(self);

            GlobalInfo.UpdateGlobalScavCount(self.activeWorld);
            GlobalInfo.UpdateAvailableScavs(self.activeWorld);
        }

        private static void ScavengersWorldAI_Update(On.ScavengersWorldAI.orig_Update orig, ScavengersWorldAI self)
        {
            int previousScavCount = self.scavengers.Count;
            int previousSquadCount = self.playerAssignedSquads.Count;
            orig.Invoke(self);
            if (previousScavCount != self.scavengers.Count)
            {
                GlobalInfo.UpdateAvailableScavs(self.world);
                GlobalInfo.UpdateSquadCount(self);
            }
            if (previousSquadCount != self.playerAssignedSquads.Count)
            {
                Plugin.logger.LogWarning("Current amount of player assigned squads: " + self.playerAssignedSquads.Count);
                GlobalInfo.UpdateSquadCount(self);
            }
            GlobalInfo.CooldownToSeconds(self.playerSquadCooldown);
        }

        //private static void ScavengersWorldAI_AddScavenger(On.ScavengersWorldAI.orig_AddScavenger orig, ScavengersWorldAI self, ScavengerAbstractAI newScav)
        //{
        //    int previousCount = self.scavengers.Count;
        //    orig.Invoke(self, newScav);
        //    if (previousCount != self.scavengers.Count)
        //    {
        //        GlobalInfo.UpdateAvailableScavs(self.world);
        //    }
        //}

        private static void PlayerSessionRecord_AddKill(On.PlayerSessionRecord.orig_AddKill orig, PlayerSessionRecord self, Creature victim)
        {
            orig.Invoke(self, victim);

            if (victim.Template.type == CreatureTemplate.Type.Scavenger || victim.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
            {
                GlobalInfo.AddKill(victim.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite);
                victim.room.world.scavengersWorldAI.scavengers.Remove(victim.abstractCreature.abstractAI as ScavengerAbstractAI);
                GlobalInfo.UpdateAvailableScavs(victim.room.world);
                if (victim.room.world.scavengersWorldAI != null && ((victim as Scavenger).abstractCreature.abstractAI as ScavengerAbstractAI).squad != null)
                {
                    GlobalInfo.UpdateSquadCount(victim.room.world.scavengersWorldAI);
                }
            }
        }
    }
}
