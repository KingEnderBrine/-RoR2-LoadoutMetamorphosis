using InLobbyConfig;
using InLobbyConfig.Fields;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LoadoutMetamorphosis
{
    public static class InLobbyConfigIntegration
    {
        private static object ModConfig { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnAwake()
        {
            var config = new ModConfigEntry
            {
                DisplayName = "Loadout Metamorphosis",
                EnableField = new BooleanConfigField("", () => LoadoutMetamorphosisPlugin.IsEnabled.Value, (newValue) => LoadoutMetamorphosisPlugin.IsEnabled.Value = newValue),

            };

            ModConfigCatalog.Add(config);
            ModConfig = config;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OnDestroy()
        {
            ModConfigCatalog.Remove(ModConfig as ModConfigEntry);
        }
    }
}
