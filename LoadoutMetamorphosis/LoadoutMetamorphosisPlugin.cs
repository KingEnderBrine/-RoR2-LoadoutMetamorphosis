using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion(LoadoutMetamorphosis.LoadoutMetamorphosisPlugin.Version)]
namespace LoadoutMetamorphosis
{
    [BepInDependency(InLobbyConfigIntegration.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(PartialMetamorphosisIntegration.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(GUID, Name, Version)]
    public class LoadoutMetamorphosisPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.KingEnderBrine.LoadoutMetamorphosis";
        public const string Name = "Loadout Metamorphosis";
        public const string Version = "1.1.0";

        internal static ConfigEntry<bool> IsEnabled { get; set; }

        public void Start()
        {
            IsEnabled = Config.Bind("Main", "enabled", true, "Is mod enabled");

            HookEndpointManager.Modify(typeof(CharacterMaster).GetMethod(nameof(CharacterMaster.SpawnBody)), (ILContext.Manipulator)CharacterMasterSpawnBody);
            InLobbyConfigIntegration.OnStart();
        }

        public void Destroy()
        {
            HookEndpointManager.Unmodify(typeof(CharacterMaster).GetMethod(nameof(CharacterMaster.SpawnBody)), (ILContext.Manipulator)CharacterMasterSpawnBody);
            InLobbyConfigIntegration.OnDestroy();
        }

        private static void CharacterMasterSpawnBody(ILContext il)
        {
            var c = new ILCursor(il);
            var bodyIndex = -1;
            c.GotoNext(
                x => x.MatchLdloc(out bodyIndex),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(out _),
                x => x.MatchCallOrCallvirt(typeof(CharacterBody), nameof(CharacterBody.SetLoadoutServer)));
            c.Index++;
            c.Emit(OpCodes.Dup);
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<CharacterBody, Loadout, CharacterMaster, Loadout>>(BeforeSetLoadout);
        }

        private static Loadout BeforeSetLoadout(CharacterBody body, Loadout loadout, CharacterMaster master)
        {
            if (!master.playerCharacterMasterController ||
                !RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.RandomSurvivorOnRespawn) ||
                !PartialMetamorphosisIntegration.ShouldChange(master))
            {
                return loadout;
            }

            var newLoadout = new Loadout();
            loadout.Copy(newLoadout);

            var userUnlockables = master.playerCharacterMasterController.networkUser.unlockables;
            var bodySkills = BodyCatalog.GetBodyPrefabSkillSlots(body.bodyIndex);
            for (var i = 0; i < bodySkills.Length; i++)
            {
                var skill = bodySkills[i];
                var unlockedVariants = new List<uint>();
                for (uint j = 0; j < skill.skillFamily.variants.Length; j++)
                {
                    var variant = skill.skillFamily.variants[j];
                    if (!variant.unlockableDef || userUnlockables.Contains(variant.unlockableDef))
                    {
                        unlockedVariants.Add(j);
                    }
                }

                newLoadout.bodyLoadoutManager.SetSkillVariant(body.bodyIndex, i, unlockedVariants[UnityEngine.Random.Range(0, unlockedVariants.Count)]);
            }

            return newLoadout;
        }
    }
}