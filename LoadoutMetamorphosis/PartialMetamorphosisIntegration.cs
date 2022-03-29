using BepInEx.Bootstrap;
using PartialMetamorphosis;
using RoR2;
using System.Runtime.CompilerServices;

namespace LoadoutMetamorphosis
{
    public static class PartialMetamorphosisIntegration
    {
        public const string GUID = "com.KingEnderBrine.PartialMetamorphosis";
        private static bool Enabled => Chainloader.PluginInfos.ContainsKey(GUID);
        
        public static bool ShouldChange(CharacterMaster master)
        {
            if (Enabled)
            {
                return ShouldChangeInternal(master);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ShouldChangeInternal(CharacterMaster master)
        {
            return PartialMetamorphosisPlugin.ShouldChangeCharacter(master);
        }
    }
}
