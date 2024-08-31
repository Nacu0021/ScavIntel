using UnityEngine;
using RWCustom;
using MoreSlugcats;

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
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            On.ShelterDoor.Update += ShelterDoor_Update;
            On.AbstractRoom.MoveEntityToDen += AbstractRoom_MoveEntityToDen;
            On.AbstractRoom.MoveEntityOutOfDen += AbstractRoom_MoveEntityOutOfDen;
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
        }

        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            orig.Invoke(self, ID);

            if (ID == ProcessManager.ProcessID.Game)
            {
                GlobalInfo.Reset(true);
            }
        }

        private static void AbstractRoom_MoveEntityOutOfDen(On.AbstractRoom.orig_MoveEntityOutOfDen orig, AbstractRoom self, AbstractWorldEntity ent)
        {
            orig.Invoke(self, ent);

            if (ent is AbstractCreature crit && crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
            {
                GlobalInfo.UpdateAvailableScavs(self.world);
            }
        }

        private static void AbstractRoom_MoveEntityToDen(On.AbstractRoom.orig_MoveEntityToDen orig, AbstractRoom self, AbstractWorldEntity ent)
        {
            orig.Invoke(self, ent);

            if (ent is AbstractCreature crit && crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger)
            {
                GlobalInfo.UpdateAvailableScavs(self.world);
            }
        }

        private static void ShelterDoor_Update(On.ShelterDoor.orig_Update orig, ShelterDoor self, bool eu)
        {
            orig.Invoke(self, eu);

            if (!GlobalInfo.cycleStartInit && self.closedFac < 0.04f)
            {
                GlobalInfo.UpdateGlobalScavCount(self.room.world);
                GlobalInfo.UpdateAvailableScavs(self.room.world);
                GlobalInfo.cycleStartInit = true;
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

            GlobalInfo.UpdateAvailableScavs(self.activeWorld);
            GlobalInfo.UpdateGlobalScavCount(self.activeWorld);
        }

        private static void ScavengersWorldAI_Update(On.ScavengersWorldAI.orig_Update orig, ScavengersWorldAI self)
        {
            int previousScavCount = self.scavengers.Count;
            int previousSquadCount = self.playerAssignedSquads.Count;
            orig.Invoke(self);
            if (previousScavCount != self.scavengers.Count)
            {
                GlobalInfo.UpdateAvailableScavs(self.world);
            }
            if (previousSquadCount != self.playerAssignedSquads.Count)
            {
                GlobalInfo.UpdateSquadCount(self);
            }
            GlobalInfo.CooldownToSeconds(self.playerSquadCooldown);
        }

        private static void ScavengersWorldAI_AddScavenger(On.ScavengersWorldAI.orig_AddScavenger orig, ScavengersWorldAI self, ScavengerAbstractAI newScav)
        {
            int previousCount = self.scavengers.Count;
            orig.Invoke(self, newScav);
            if (previousCount != self.scavengers.Count)
            {
                GlobalInfo.UpdateAvailableScavs(self.world);
            }
        }

        private static void PlayerSessionRecord_AddKill(On.PlayerSessionRecord.orig_AddKill orig, PlayerSessionRecord self, Creature victim)
        {
            orig.Invoke(self, victim);

            if (victim.Template.type == CreatureTemplate.Type.Scavenger || victim.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
            {
                GlobalInfo.AddKill(victim.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite);
            }
        }
    }
}
