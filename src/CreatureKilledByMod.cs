using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace CreatureKilledBy;

public class CreatureKilledByMod : ModSystem {
    private Harmony? harmony;

    public override bool ShouldLoad(EnumAppSide side) {
        return side.IsClient();
    }

    public override void StartClientSide(ICoreClientAPI api) {
        harmony = new Harmony(Mod.Info.ModID);
        harmony.Patch(typeof(EntityBehaviorHarvestable).GetMethod("GetInfoText", BindingFlags.Instance | BindingFlags.Public),
            prefix: typeof(CreatureKilledByMod).GetMethod("OnGetInfoText"));
    }

    public override void Dispose() {
        harmony?.UnpatchAll(Mod.Info.ModID);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static bool OnGetInfoText(EntityBehaviorHarvestable __instance, StringBuilder infotext) {
        try {
            if (!__instance.entity.Alive) {
                if (__instance.GetProperty<bool>("GotCrushed")) {
                    infotext.AppendLine(Lang.Get("Looks crushed. Won't be able to harvest as much from this carcass.", Array.Empty<object>()));
                }

                string deathByEntityCode = __instance.entity.WatchedAttributes.GetString("deathByEntity");
                if (deathByEntityCode != null && !__instance.entity.WatchedAttributes.HasAttribute("deathByPlayer")) {
                    string code = "deadcreature-killed";
                    EntityProperties props = __instance.entity.World.GetEntityType(new AssetLocation(deathByEntityCode));
                    JsonObject? attributes = props?.Attributes;
                    if (attributes != null && attributes["killedByInfoText"].Exists) {
                        code = props!.Attributes["killedByInfoText"].AsString();
                    }

                    infotext.AppendLine($"{Lang.Get(code, Array.Empty<object>())} [{Lang.Get($"item-creature-{deathByEntityCode.Split(":")[1]}")}]");
                }
            }

            if (!__instance.GetField<bool>("fixedWeight")) {
                infotext.AppendLine(Lang.Get(__instance.AnimalWeight switch {
                    >= 0.95f => "creature-weight-good",
                    >= 0.75f => "creature-weight-ok",
                    >= 0.5f => "creature-weight-low",
                    _ => "creature-weight-starving"
                }, Array.Empty<object>()));
            }

            return false;
        }
        catch (Exception) {
            return true;
        }
    }
}
