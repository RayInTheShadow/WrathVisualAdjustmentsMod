﻿using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ResourceLinks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualAdjustments
{
    static class HairUnlocker
    {
        static Dictionary<string, CustomizationOptions> originalOptions = new Dictionary<string, CustomizationOptions>();
       public static void UnlockHair()
        {
            if (!Main.settings.UnlockHair) return;
            BlueprintRace[][] groups;
            if (Main.settings.UnlockHair)
            {
                groups = new BlueprintRace[][]
                {
                    Game.Instance.BlueprintRoot.Progression.CharacterRaces.ToArray<BlueprintRace>()
                };

            }
            else
            {
                groups = new BlueprintRace[][]
                 {
                    new BlueprintRace[]{
                        ResourcesLibrary.TryGetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4"), //Human
                        ResourcesLibrary.TryGetBlueprint<BlueprintRace>("b7f02ba92b363064fb873963bec275ee"), //Aasimar
                    },
                    new BlueprintRace[]{
                        ResourcesLibrary.TryGetBlueprint<BlueprintRace>("ef35a22c9a27da345a4528f0d5889157"), //Gnome
                        ResourcesLibrary.TryGetBlueprint<BlueprintRace>("b0c3ef2729c498f47970bb50fa1acd30"), //Halfling
                    }
                 };
            }
            foreach (var group in groups)
            {
                foreach (var from in group)
                {
                    foreach (var to in group)
                    {
                        if (from == to) continue;
                        AddHair(from, to);
                    }
                }
            }
        }
        static void AddHair(BlueprintRace sourceRace, BlueprintRace targetRace)
        {
            foreach (var gender in new Gender[] { Gender.Male, Gender.Female })
            {
                var originalSource = GetOriginalOptions(sourceRace, gender);
                var originalTarget = GetOriginalOptions(targetRace, gender);
                var newSource = gender == Gender.Male ? sourceRace.MaleOptions : sourceRace.FemaleOptions;
                var newTarget = gender == Gender.Male ? targetRace.MaleOptions : targetRace.FemaleOptions;
                newTarget.Hair = Combine(newSource.Hair, newTarget.Hair);
                newTarget.Beards = Combine(newSource.Beards, newTarget.Beards);
            }
        }
        static CustomizationOptions GetOriginalOptions(BlueprintRace race, Gender gender)
        {
            if (originalOptions.ContainsKey(race.name + gender))
            {
                return originalOptions[race.name + gender];
            }
            throw new KeyNotFoundException($"Couldn't find key {race.name + gender}");
        }
        static EquipmentEntityLink[] Combine(EquipmentEntityLink[] from, EquipmentEntityLink[] to)
        {
            var result = new List<EquipmentEntityLink>(to);
            foreach (var eel in from)
            {
                if (result.Exists(toEEL => eel.AssetId == toEEL.AssetId))
                {
                    continue;
                }
                result.Add(eel);
            }
            return result.ToArray();
        }
        public static void RestoreOptions()
        {
            foreach (var race in Game.Instance.BlueprintRoot.Progression.CharacterRaces.ToArray<BlueprintRace>())
            {
                race.MaleOptions = GetOriginalOptions(race, Gender.Male);
                race.FemaleOptions = GetOriginalOptions(race, Gender.Female);
            }
        }
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class ResourcesLibrary_InitializeLibrary2_Patch
        {
            static void Postfix()
            {
                {
                    if (!Main.enabled) return;
                    try
                    {
                        foreach (var race in Game.Instance.BlueprintRoot.Progression.CharacterRaces.ToArray<BlueprintRace>())
                        {
                            HairUnlocker.originalOptions[race.name + Gender.Male] = race.MaleOptions;
                            HairUnlocker.originalOptions[race.name + Gender.Female] = race.FemaleOptions;
                        }
                        // loaded = true;
                        // if (!enabled) return;
                        HairUnlocker.UnlockHair();
                    }
                    catch (Exception ex)
                    {
                        Main.logger.Error(ex.ToString());
                    }
                }
            }
        }
    }
}