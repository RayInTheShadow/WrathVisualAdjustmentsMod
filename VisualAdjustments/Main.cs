﻿using UnityEngine;
using UnityModManagerNet;
using Kingmaker.Blueprints;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.ResourceLinks;
using static VisualAdjustments.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.Visual.Sound;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility;
using Kingmaker.Blueprints.Classes;
using HarmonyLib;
using Kingmaker.Cheats;
using Kingmaker.UI.MVVM._PCView.CharGen.Phases.Common;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.CharacterSystem;

namespace VisualAdjustments
{
#if DEBUG
    [EnableReloading]
#endif
    public class CharInfo
    {
        public string GUID;
        public string Name;
    }
    public class Main
    {
        public static SimpleBlueprint[] blueprints;
        const float DefaultLabelWidth = 200f;
        const float DefaultSliderWidth = 300f;
        public static UnityModManager.ModEntry.ModLogger logger;
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        public static void Error(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString());
        }
        public static void Error(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        public static Texture2D dot;
        public static ColorPicker HairColorPicker = new ColorPicker();
        public static ColorPicker SkinColorPicker = new ColorPicker();
        public static ColorPicker PrimaryColorPicker = new ColorPicker();
        public static ColorPicker SecondaryColorPicker = new ColorPicker();
        public static bool unlockcustomization;
        public static bool enabled;
        public static bool showsettings = false;
        public static bool classesloaded = false;
        public static Settings settings;
        public static Dictionary<string, EquipmentEntity.OutfitPart> CapeOutfitParts = new Dictionary<string, EquipmentEntity.OutfitPart>();
        public static UnityModManager.ModEntry ModEntry;
        /// public static ReferenceArrayProxy<BlueprintCharacterClass,BlueprintCharacterClassReference> classes = Game.Instance.BlueprintRoot.Progression.CharacterClasses;
        /// public static string[] classes;
        public static List<CharInfo> classes = new List<CharInfo> { };
        /*public static string[] classes = new string[] {
            "Default",
            "Alchemist",
            "Barbarian",
            "Bard",
            "Cleric",
            "Druid",
            "Fighter",
            "Inquisitor",
            "Kineticist",
            "Magus",
            "Monk",
            "Paladin",
            "Ranger",
            "Rogue",
            "Slayer",
            "Sorcerer",
            "Wizard",
            "None"
        };*/
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                ModEntry = modEntry;
                logger = modEntry.Logger;
                settings = Settings.Load(modEntry);
                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
               /// colorpicker = CreateColorPicker();
                //dot = ColorPickerLoad.GetTexture(ModEntry.Path + "\\ColorPicker\\dot.png");
                modEntry.OnToggle = OnToggle;
                modEntry.OnGUI = onGUI;
                modEntry.OnSaveGUI = OnSaveGUI;
#if DEBUG
                modEntry.OnUnload = Unload;
#endif
                if(Main.blueprints == null)
                {
                    Main.blueprints = Util.GetBlueprints();
                }
            }
            catch (Exception e)
            {
                Log(e.ToString() + "\n" + e.StackTrace);
                throw e;
            }
            return true;
        }

        public static void GenerateHairColor(UnitEntityData data)
        {
            /// might have to add race identifier to Main.HairColors
            /// bypass primarycolorsprofile and set 2dtexture directly
            try
            {
                var doll = DollResourcesManager.GetDoll(data);
                if (doll == null) return;
                var settings = Main.settings.GetCharacterSettings(data);
                var colornum = settings.hairColor;
                var settingcol = new Color(colornum[0], colornum[1], colornum[2]);
                if (!doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(a => a.GetPixel(1, 1).ToString() == settingcol.ToString()))
                {
                var texture = new Texture2D(256, 1, TextureFormat.ARGB32, false)
                {
                    filterMode = FilterMode.Bilinear
                };
                for (var y = 0; y < 1; y++)
                {
                    for (var x = 0; x < 256; x++)
                    {
                        texture.SetPixel(x, y, settingcol);
                    }
                }
                texture.Apply();
               /// if (!doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(a => a.GetPixel(1, 1).ToString() == texture.GetPixel(1, 1).ToString()))
                /// if (!HairColors.ContainsKey(jas.ToString()))
                {
                   /// HairColors.Add(jas.ToString(), texture);
                    doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Add(texture);
                }
                ///something with the indexing it messing up and returning -1
                ///var index = doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(a => a.GetPixel(1,1).ToString() == texture.GetPixel(1,1).ToString());
                }
                var index = doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(a => a.GetPixel(1, 1).ToString() == settingcol.ToString());
                settings.HairColor = doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.IndexOf(index);
                doll.SetHairColor(settings.HairColor);
                //Main.logger.Log(settings.HairColor.ToString());
                ///Main.logger.Log(jas.ToString());
                /*if(!doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Any(a => a == texture))
                {
                    doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Add(texture);
                }*/
                /// doll.SetHairColor(doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.IndexOf(texture));
            }
            catch(Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
        public static void GenerateSkinColor(UnitEntityData data)
        {
            /// might have to add race identifier to Main.HairColors
            /// bypass primarycolorsprofile and set 2dtexture directly
            try
            {
                var doll = DollResourcesManager.GetDoll(data);
                if (doll == null) return;
                var settings = Main.settings.GetCharacterSettings(data);
                var colornum = settings.skinColor;
                var settingcol = new Color(colornum[0], colornum[1], colornum[2]);
                if (!doll.Head.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(a => a.GetPixel(1, 1).ToString() == settingcol.ToString()))
                {
                    var texture = new Texture2D(256, 1, TextureFormat.ARGB32, false)
                    {
                        filterMode = FilterMode.Bilinear
                    };
                    for (var y = 0; y < 1; y++)
                    {
                        for (var x = 0; x < 256; x++)
                        {
                            texture.SetPixel(x, y, settingcol);
                        }
                    }
                    texture.Apply();
                    if (!doll.Head.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(a => a.GetPixel(1, 1).ToString() == texture.GetPixel(1, 1).ToString()))
                    /// if (!HairColors.ContainsKey(jas.ToString()))
                    {
                        /// HairColors.Add(jas.ToString(), texture);
                        doll.Head.m_Entity.PrimaryColorsProfile.Ramps.Add(texture);
                    }
                    ///something with the indexing it messing up and returning -1
                    ///var index = doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(a => a.GetPixel(1,1).ToString() == texture.GetPixel(1,1).ToString());
                }
                var index = doll.Head.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(a => a.GetPixel(1, 1).ToString() == settingcol.ToString());
                settings.SkinColor = doll.Head.m_Entity.PrimaryColorsProfile.Ramps.IndexOf(index);
                doll.SetSkinColor(settings.SkinColor);
                //Main.logger.Log(settings.SkinColor.ToString());
                ///Main.logger.Log(jas.ToString());
                /*if(!doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Any(a => a == texture))
                {
                    doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Add(texture);
                }*/
                /// doll.SetHairColor(doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.IndexOf(texture));
            }
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
        static public void GenerateOutfitcolor(UnitEntityData dat)
        {
            try
            {
                var doll = DollResourcesManager.GetDoll(dat);
                // if (doll == null) return;
                if (dat.View == null) return;
                if (dat.View.CharacterAvatar == null) return;
                var settings = Main.settings.GetCharacterSettings(dat);
                var colornum = settings.primColor;
                var colornum2 = settings.secondColor;
                var settingcol = new Color(colornum[0], colornum[1], colornum[2]);
                var settingcolsecondary = new Color(colornum2[0], colornum2[1], colornum2[2]);
                //if (doll != null)
                {
                 ///   foreach (var a in doll.Clothes.Select(a => a.Load()))
                    foreach(var a in dat.View.CharacterAvatar.EquipmentEntities.Where(A => A.PrimaryColorsProfile != null && A.PrimaryColorsProfile.Ramps.Count > 75))
                    {
                        //   Main.logger.Log(a.ToString());
                        if (a.PrimaryColorsProfile != null)// && a.PrimaryColorsProfile.Ramps.Count > 0)
                        {
                            if (!a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(h => h.GetPixel(1, 1).ToString() == settingcol.ToString()))
                            {
                                var texture = new Texture2D(256, 1, TextureFormat.ARGB32, false)
                                {
                                    filterMode = FilterMode.Bilinear
                                };
                                for (var y = 0; y < 1; y++)
                                {
                                    for (var x = 0; x < 256; x++)
                                    {
                                        texture.SetPixel(x, y, settingcol);
                                    }
                                }
                                texture.Apply();
                                if (!a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(x => x.GetPixel(1, 1).ToString() == texture.GetPixel(1, 1).ToString()))
                                {
                                    a.PrimaryColorsProfile.Ramps.Add(texture);
                                    //  Main.logger.Log("addedtex");
                                    //  Main.logger.Log(texture.GetPixel(1, 1).ToString());
                                }
                                //  else
                                {
                                    // Main.logger.Log("Didntaddtex");
                                }
                            }
                            if (!a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(h => h.GetPixel(1, 1).ToString() == settingcolsecondary.ToString()))
                            {
                                var texture = new Texture2D(256, 1, TextureFormat.ARGB32, false)
                                {
                                    filterMode = FilterMode.Bilinear
                                };
                                for (var y = 0; y < 1; y++)
                                {
                                    for (var x = 0; x < 256; x++)
                                    {
                                        texture.SetPixel(x, y, settingcolsecondary);
                                    }
                                }
                                texture.Apply();
                                if (!a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(x => x.GetPixel(1, 1).ToString() == texture.GetPixel(1, 1).ToString()))
                                {
                                    a.PrimaryColorsProfile.Ramps.Add(texture);
                                    //  Main.logger.Log("addedtex");
                                    //  Main.logger.Log(texture.GetPixel(1, 1).ToString());
                                }
                                //  else
                                {
                                    // Main.logger.Log("Didntaddtex");
                                }
                            }
                            var index = a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(w => w.GetPixel(1, 1).ToString() == settingcol.ToString());
                            settings.PrimaryColor = a.PrimaryColorsProfile.Ramps.IndexOf(index);
                            settings.companionPrimary = a.PrimaryColorsProfile.Ramps.IndexOf(index);
                            var indexsec = a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(w => w.GetPixel(1, 1).ToString() == settingcolsecondary.ToString());
                            settings.SecondaryColor = a.PrimaryColorsProfile.Ramps.IndexOf(indexsec);
                            settings.companionSecondary = a.PrimaryColorsProfile.Ramps.IndexOf(indexsec);
                            // doll.SetSkinColor(settings.SkinColor);
                            /*if (a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(c => c.GetPixel(1, 1).ToString() == settingcol.ToString()))
                            {
                                var index = a.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).FirstOrDefault(c => c.GetPixel(1, 1).ToString() == settingcol.ToString());
                                foreach(var asdasddffads in a.PrimaryColorsProfile.Ramps.Where(nb => nb.isReadable))
                                {
                                    Main.logger.Log(asdasddffads.ToString());
                                    Main.logger.Log(asdasddffads.GetPixel(1,1).ToString());
                                }
                                if (a.PrimaryColorsProfile.Ramps.FindIndex(h => h.isReadable && h.GetPixel(1,1).ToString() == index.GetPixel(1,1).ToString()) != -1)
                                {
                                    settings.PrimaryColor = a.PrimaryColorsProfile.Ramps.FindIndex(h => h.isReadable && h.GetPixel(1, 1).ToString() == index.GetPixel(1, 1).ToString());
                                    Main.logger.Log("index stuff");
                                }
                               /* foreach(var xsdaf in a.PrimaryColorsProfile.Ramps)
                                {
                                    if(xsdaf.isReadable)
                                    {
                                        Main.logger.Log(settingcol.ToString());
                                        Main.logger.Log(xsdaf.GetPixel(1, 1).ToString());
                                        if (xsdaf.GetPixel(1,1).ToString() == settingcol.ToString())
                                        {
                                            Main.logger.Log("indexmatch");
                                            settings.PrimaryColor = a.PrimaryColorsProfile.Ramps.IndexOf(index);
                                            Main.logger.Log(a.PrimaryColorsProfile.Ramps.IndexOf(index).ToString());
                                        }
                                    }
                                }*/
                            /* else
                             {
                                 Main.logger.Log("NoIndexMatch");
                             }
                         }*/
                        }
                    }
                }
            }
            /* else
             {
                 foreach(var asdasdasd in dat.View.CharacterAvatar.EquipmentEntities)
                 {
                     if (asdasdasd != null && asdasdasd.PrimaryColorsProfile != null && asdasdasd.PrimaryColorsProfile.Ramps != null)
                     {
                         Main.logger.Log(asdasdasd.ToString());
                         Main.logger.Log(asdasdasd.PrimaryColorsProfile.Ramps.Count.ToString());
                     }
                 }
                 var bb = dat.View.CharacterAvatar.EquipmentEntities.First();
                 Main.logger.Log(bb.ToString());
                 Main.logger.Log("nodoll");
                 if (!bb.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(h => h.GetPixel(1, 1).ToString() == settingcol.ToString()))
                 {
                     var texture = new Texture2D(256, 1, TextureFormat.ARGB32, false)
                     {
                         filterMode = FilterMode.Bilinear
                     };
                     for (var y = 0; y < 1; y++)
                     {
                         for (var x = 0; x < 256; x++)
                         {
                             texture.SetPixel(x, y, settingcol);
                         }
                     }
                     texture.Apply();
                     if (!bb.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(x => x.GetPixel(1, 1).ToString() == texture.GetPixel(1, 1).ToString()))
                     {
                         bb.PrimaryColorsProfile.Ramps.Add(texture);
                         Main.logger.Log("addedtexnodoll");
                         //  Main.logger.Log(texture.GetPixel(1, 1).ToString());
                     }
                     //  else
                     {
                          Main.logger.Log("Didntaddtexnodoll");
                     }
                 }
                 if (!bb.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(h => h.GetPixel(1, 1).ToString() == settingcolsecondary.ToString()))
                 {
                     var texture = new Texture2D(256, 1, TextureFormat.ARGB32, false)
                     {
                         filterMode = FilterMode.Bilinear
                     };
                     for (var y = 0; y < 1; y++)
                     {
                         for (var x = 0; x < 256; x++)
                         {
                             texture.SetPixel(x, y, settingcolsecondary);
                         }
                     }
                     texture.Apply();
                     if (!bb.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).Any(x => x.GetPixel(1, 1).ToString() == texture.GetPixel(1, 1).ToString()))
                     {
                         bb.PrimaryColorsProfile.Ramps.Add(texture);
                         //  Main.logger.Log("addedtex");
                         //  Main.logger.Log(texture.GetPixel(1, 1).ToString());
                     }
                     //  else
                     {
                         // Main.logger.Log("Didntaddtex");
                     }
                 }

             }
             // var index = doll.Head.m_Entity.PrimaryColorsProfile.Ramps.Where(b => b.isReadable).First(a => a.GetPixel(1, 1).ToString() == settingcol.ToString());
             //settings.SkinColor = doll.Head.m_Entity.PrimaryColorsProfile.Ramps.IndexOf(index);
             doll.SetPrimaryEquipColor(settings.PrimaryColor);
             //Main.logger.Log(settings.SkinColor.ToString());
             ///Main.logger.Log(jas.ToString());
             /*if(!doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Any(a => a == texture))
             {
                 doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Add(texture);
             }*//*
             /// doll.SetHairColor(doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.IndexOf(texture));
         }*/
            catch (Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }

     /*   private static Texture2D CreateGradient(Color colorStart, Color colorEnd, int width = 256, int height = 1)
        {
            var gradient = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };

            float inv = 1f / width;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    float t = x * inv;
                    Color color = Color.Lerp(colorStart, colorEnd, t);
                    gradient.SetPixel(x, y, color);
                }
            }

            gradient.Apply();

            return gradient;
        }*/
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            new Harmony(modEntry.Info.Id).UnpatchAll(modEntry.Info.Id);
            return true;
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        // Called when the mod is turned to on/off.
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            enabled = value;
            return true; // Permit or not.
        }
       /* public static ColorPicker CreateColorPicker()
        {
            var asd = new ColorPicker 
            { 
            colorPicker = ColorPickerLoad.GetTexture(ModEntry.Path + "\\ColorPicker\\color.png")
            };
            return asd;
        }*/
        public static void GetClasses()
        {
            if (classes.Count == 0)
            {
                ///Main.logger.Log("bru");
                foreach (BlueprintCharacterClass c in Main.blueprints.OfType<BlueprintCharacterClass>())
                {
                    if (!c.PrestigeClass && c.ComponentsArray.Length != 0 && !c.IsMythic && !c.ToString().Contains("Mythic") && !c.ToString().Contains("Animal") && !c.ToString().Contains("Scion"))
                    {
                        try
                        {
                            var charinfo = new CharInfo();
                            charinfo.Name = c.Name;
                            charinfo.GUID = c.AssetGuid.ToString();
                            if (classes.Count == 0)
                            {
                                var charinf = new CharInfo();
                                charinf.Name = "None";
                                var charinf2 = new CharInfo();
                                charinf2.Name = "Default";
                                classes.Add(charinf);
                                classes.Add(charinf2);
                            }
                            else
                            {
                                if (!classes.Any(asd => asd.Name == charinfo.Name))
                                {
                                    classes.Add(charinfo);
                                }
                            }
                            /*if(!classes.Contains(c.ToString()))
                            { */
                            /*classes.AddItem(c.ToString());
                            Main.logger.Log(c.ToString());*/
                            ///}
                        }
                        catch (Exception e) { Main.logger.Log(e.ToString()); }
                    }
                    Main.classesloaded = true;
                }
            }
        }
        public static void onGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                if (!enabled) return;
               /* if(GUILayout.Button("Fix Grey Characters",GUILayout.Width(200f)))
                {
                    foreach (var ch in Game.Instance.Player.AllCharacters)
                    {
                      CharacterManager.RebuildCharacter(ch);
                    }
                }*/
                ModKit.UI.DisclosureToggle("Settings",ref showsettings);
                if(showsettings)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    GUILayout.BeginVertical();
                    ModKit.UI.Toggle("Unlock all Portraits", ref settings.AllPortraits);
                    ModKit.UI.Toggle("Unlock Hair", ref settings.UnlockHair,HairUnlocker.RestoreOptions,HairUnlocker.UnlockHair);
                    ModKit.UI.Label("Warning: Unlock hair might have some hairstyles that clip or are otherwise malformed");
                    /*ModKit.UI.Toggle("Unlock Hair Options",ref HairUnlocker.Main.settings.UnlockHair);
                    if (HairUnlocker.Main.settings.UnlockHair) ModKit.UI.Toggle("Unlock All Hair Options (Includes incompatible options)",ref HairUnlocker.Main.settings.UnlockAllHair);
                    ModKit.UI.Toggle("Unlock Horns",ref HairUnlocker.Main.settings.UnlockHorns);
                    ModKit.UI.Toggle("Unlock Tails",ref HairUnlocker.Main.settings.UnlockTail);
                    ModKit.UI.Toggle("Unlock Female Dwarf Beards (Includes incompatible options",ref HairUnlocker.Main.settings.UnlockFemaleDwarfBeards);*/
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
                ///Asd();
                ///Main.logger.Log(classes.Count.ToString());
                /*foreach(CharInfo s in classes)
                {
                    Main.logger.Log(s.Name + s.GUID);
                }*/
                if (Game.Instance.Player.AllCharacters.Count > 0)
                {
                    foreach (UnitEntityData unitEntityData in Game.Instance.Player.AllCharacters)
                    {
                        if (!settings.characterSettings.Values.Any(settin => settin.uniqueid == unitEntityData.UniqueId))
                        {
                            if (!unitEntityData.IsPet)
                            {
                                var charinfo = new CharInfo
                                {
                                    GUID = unitEntityData.Progression.GetEquipmentClass().AssetGuidThreadSafe,
                                    Name = unitEntityData.Progression.GetEquipmentClass().Name
                                };
                                if(unitEntityData.IsStoryCompanion())
                                {
                                    charinfo.Name = "Default";
                                }
                                var fb = new CharacterSettings
                                {
                                    characterName = unitEntityData.CharacterName,
                                    classOutfit = charinfo,
                                    uniqueid = unitEntityData.UniqueId
                                };
                                settings.AddCharacterSettings(unitEntityData, fb);
                                Main.GetIndices(unitEntityData);
                                Main.SetEELs(unitEntityData, DollResourcesManager.GetDoll(unitEntityData));
                                /* Settings.CharacterSettings characterSettings = new CharacterSettings{
                                 characterSettings.characterName = unitEntityData.CharacterName;
                                 characterSettings.classOutfit = new CharInfo
                                 {
                                     ///GUID = unitEntityData.Progression.GetEquipmentClass().AssetGuidThreadSafe,
                                     Name = "Default"
                                 };
                                 characterSettings.PrimaryColor = unitEntityData.Descriptor.m_LoadedDollData.ClothesPrimaryIndex;
                                 characterSettings.SecondaryColor = unitEntityData.Descriptor.m_LoadedDollData.ClothesSecondaryIndex;
                                 settings.AddCharacterSettings(unitEntityData, characterSettings);*/
                                ///if (!unitEntityData.IsStoryCompanion())
                                {
                                    /// var races = BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>();
                                    /// var gender = unitEntityData.Gender;
                                    /// var race = unitEntityData.Progression.Race;
                                    /// var customizationOptions = gender != Gender.Male ? race.FemaleOptions : race.MaleOptions;
                                    /// GetIndices(unitEntityData, characterSettings, DollResourcesManager.GetDoll(unitEntityData), customizationOptions);
                                }
                            }
                            else
                            {
                                var fb = new CharacterSettings
                                {
                                    characterName = unitEntityData.CharacterName,
                                    uniqueid = unitEntityData.UniqueId
                                };
                                settings.AddCharacterSettings(unitEntityData, fb);
                            }
                        }
                    }
                    foreach (UnitEntityData unitEntityData in Game.Instance.Player.PartyAndPets)
                    {
                        var characterSettings = settings.GetCharacterSettings(unitEntityData);
                        if (!unitEntityData.IsPet)
                        {
                            if (characterSettings.classOutfit == null)
                            {
                                characterSettings = Main.settings.GetCharacterSettings(unitEntityData);
                                characterSettings.classOutfit.Name = unitEntityData.Descriptor.Progression.GetEquipmentClass().Name;
                                characterSettings.classOutfit.GUID = unitEntityData.Descriptor.Progression.GetEquipmentClass().AssetGuidThreadSafe;
                            }
                            GUILayout.Space(4f);
                            GUILayout.BeginHorizontal();
                            ModKit.UI.Label(string.Format("{0}", unitEntityData.CharacterName), GUILayout.Width(DefaultLabelWidth));
                            ModKit.UI.DisclosureToggle("Select Outfit",ref characterSettings.showClassSelection);
                            var doll = DollResourcesManager.GetDoll(unitEntityData);
                            ModKit.UI.DisclosureToggle("Select Doll", ref characterSettings.showDollSelection);
                            ModKit.UI.DisclosureToggle("Select Equipment", ref characterSettings.showEquipmentSelection);
                            ModKit.UI.DisclosureToggle("Select Overrides",ref characterSettings.showOverrideSelection);
                            ///characterSettings.ReloadStuff = GUILayout.Toggle(characterSettings.ReloadStuff, "Reload", GUILayout.ExpandWidth(false));
#if (DEBUG)
                          /*  characterSettings.showInfo = */ModKit.UI.DisclosureToggle("Show Info", ref characterSettings.showInfo);
#endif
                            GUILayout.EndHorizontal();
                            if (characterSettings.ReloadStuff == true)
                            {
                                CharacterManager.UpdateModel(unitEntityData.View);
                            }
                            if (characterSettings.showClassSelection)
                            {
                                ChooseClassOutfit(characterSettings, unitEntityData);
                                GUILayout.Space(5f);
                            }
                            if (doll != null && characterSettings.showDollSelection)
                            {
                                ChooseDoll(unitEntityData);
                                GUILayout.Space(5f);
                            }
                            if (doll == null && characterSettings.showDollSelection)
                            {
                                ChooseCompanionColor(characterSettings, unitEntityData);
                                GUILayout.Space(5f);
                            }
                            if (characterSettings.showEquipmentSelection)
                            {
                                ChooseEquipment(unitEntityData, characterSettings);
                                GUILayout.Space(5f);
                            }
                            if (characterSettings.showOverrideSelection)
                            {
                                ChooseEquipmentOverride(unitEntityData, characterSettings);
                                GUILayout.Space(5f);
                            }
#if (DEBUG)
                            if (characterSettings.showInfo)
                            {
                                InfoManager.ShowInfo(unitEntityData);
                                GUILayout.Space(5f);
                            }
#endif
                        }
                        else
                        {
                            GUILayout.Space(4f);
                            GUILayout.BeginHorizontal();
                            ModKit.UI.Label(string.Format("{0}", unitEntityData.CharacterName),  GUILayout.Width(DefaultLabelWidth));
                            ModKit.UI.DisclosureToggle("Show Override Selection",ref characterSettings.showOverrideSelection);
                            GUILayout.EndHorizontal();
                            if (characterSettings.showOverrideSelection)
                            {
                                ChooseEquipmentOverridePet(unitEntityData, characterSettings);
                            }
                        }
                    }
                    if (Game.Instance.Player.AllCharacters.Except(Game.Instance.Player.Party).Count() > 0)
                    {
                        GUILayout.Space(20);
                        GUILayout.BeginHorizontal();
                        ModKit.UI.Label(string.Format("{0}", "Remote Characters"), GUILayout.Width(400f));
                        GUILayout.EndHorizontal();
                        foreach (UnitEntityData unitEntityData in Game.Instance.Player.AllCharacters.Except(Game.Instance.Player.PartyAndPets))
                        {
                            var characterSettings = settings.GetCharacterSettings(unitEntityData);
                            if (!unitEntityData.IsPet)
                            {
                                if (characterSettings.classOutfit == null)
                                {
                                    characterSettings = Main.settings.GetCharacterSettings(unitEntityData);
                                    characterSettings.classOutfit.Name = unitEntityData.Descriptor.Progression.GetEquipmentClass().Name;
                                    characterSettings.classOutfit.GUID = unitEntityData.Descriptor.Progression.GetEquipmentClass().AssetGuidThreadSafe;
                                }
                                GUILayout.Space(4f);
                                GUILayout.BeginHorizontal();
                                ModKit.UI.Label(string.Format("{0}", unitEntityData.CharacterName), GUILayout.Width(DefaultLabelWidth));
                                ModKit.UI.DisclosureToggle("Select Outfit", ref characterSettings.showClassSelection);
                                var doll = DollResourcesManager.GetDoll(unitEntityData);
                                ModKit.UI.DisclosureToggle("Select Doll", ref characterSettings.showDollSelection);
                                ModKit.UI.DisclosureToggle("Select Equipment", ref characterSettings.showEquipmentSelection);
                                ModKit.UI.DisclosureToggle("Select Overrides", ref characterSettings.showOverrideSelection);
                                ///characterSettings.ReloadStuff = GUILayout.Toggle(characterSettings.ReloadStuff, "Reload", GUILayout.ExpandWidth(false));
#if (DEBUG)
                                /*  characterSettings.showInfo = */
                                ModKit.UI.DisclosureToggle("Show Info", ref characterSettings.showInfo);
#endif
                                GUILayout.EndHorizontal();
                                /*if (characterSettings.ReloadStuff == true)
                                {
                                    CharacterManager.UpdateModel(unitEntityData.View);
                                }*/
                                if (characterSettings.showClassSelection)
                                {
                                    ChooseClassOutfit(characterSettings, unitEntityData);
                                    GUILayout.Space(5f);
                                }
                                if (doll != null && characterSettings.showDollSelection)
                                {
                                    ChooseDoll(unitEntityData);
                                    GUILayout.Space(5f);
                                }
                                if (doll == null && characterSettings.showDollSelection)
                                {
                                    ChooseCompanionColor(characterSettings, unitEntityData);
                                    GUILayout.Space(5f);
                                }
                                if (characterSettings.showEquipmentSelection)
                                {
                                    ChooseEquipment(unitEntityData, characterSettings);
                                    GUILayout.Space(5f);
                                }
                                if (characterSettings.showOverrideSelection)
                                {
                                    ChooseEquipmentOverride(unitEntityData, characterSettings);
                                    GUILayout.Space(5f);
                                }
#if (DEBUG)
                                if (characterSettings.showInfo)
                                {
                                    InfoManager.ShowInfo(unitEntityData);
                                    GUILayout.Space(5f);
                                }
#endif
                            }
                            else
                            {
                                GUILayout.Space(4f);
                                GUILayout.BeginHorizontal();
                                ModKit.UI.Label(string.Format("{0}", unitEntityData.CharacterName), GUILayout.Width(DefaultLabelWidth));
                                ModKit.UI.DisclosureToggle("Show Override Selection", ref characterSettings.showOverrideSelection);
                                GUILayout.EndHorizontal();
                                if (characterSettings.showOverrideSelection)
                                {
                                    ChooseEquipmentOverridePet(unitEntityData, characterSettings);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log(e.ToString() + " " + e.StackTrace);
            }
        }
        static void ChooseClassOutfit(CharacterSettings characterSettings, UnitEntityData unitEntityData)
        {
            var focusedStyle = new GUIStyle(GUI.skin.button);
            focusedStyle.normal.textColor = Color.yellow;
            focusedStyle.focused.textColor = Color.yellow;
            GUILayout.BeginHorizontal();
            foreach (var _class in classes)
            {
                if (_class.Name == "Magus")
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                var style = characterSettings.classOutfit == _class ? focusedStyle : GUI.skin.button;
                if (GUILayout.Button(_class.Name, style, GUILayout.Width(100f)))
                {
                    characterSettings.classOutfit = _class;
                    CharacterManager.RebuildCharacter(unitEntityData);
                    unitEntityData.View.UpdateClassEquipment();
                }
            }
            GUILayout.EndHorizontal();
        }
        static void ChoosePortrait(UnitEntityData unitEntityData)
        {
            if (unitEntityData.Portrait.IsCustom)
            {
                var key = unitEntityData.Descriptor.UISettings.CustomPortraitRaw.CustomId;
                var currentIndex = DollResourcesManager.CustomPortraits.IndexOf(key);
                GUILayout.BeginHorizontal();
                ModKit.UI.Label("Portrait ", GUILayout.Width(DefaultLabelWidth));
                var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(currentIndex, 0, DollResourcesManager.CustomPortraits.Count, GUILayout.Width(DefaultSliderWidth)), 0);
                if (GUILayout.Button("Prev", GUILayout.Width(55)) && currentIndex >= 0)
                {
                    newIndex = currentIndex - 1;
                }
                if (GUILayout.Button("Next", GUILayout.Width(55)) && currentIndex < DollResourcesManager.CustomPortraits.Count - 1)
                {
                    newIndex = currentIndex + 1;
                }
                if (GUILayout.Button("Use Normal", GUILayout.Width(125)))
                {
                    unitEntityData.Descriptor.UISettings.SetPortrait(ResourcesLibrary.TryGetBlueprint<BlueprintPortrait>("621ada02d0b4bf64387babad3a53067b"));
                    EventBus.RaiseEvent<IUnitPortraitChangedHandler>(delegate (IUnitPortraitChangedHandler h)
                    {
                        h.HandlePortraitChanged(unitEntityData);
                    });
                    return;
                }
                var value = newIndex >= 0 && newIndex < DollResourcesManager.CustomPortraits.Count ? DollResourcesManager.CustomPortraits[newIndex] : null;
                ModKit.UI.Label(" " + value, GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();
                if (newIndex != currentIndex && value != null)
                {
                    unitEntityData.Descriptor.UISettings.SetPortrait(new PortraitData(value));
                    EventBus.RaiseEvent<IUnitPortraitChangedHandler>(delegate (IUnitPortraitChangedHandler h)
                    {
                        h.HandlePortraitChanged(unitEntityData);
                    });
                }
            }
            else
            {
                var key = unitEntityData.Descriptor.UISettings.PortraitBlueprint?.name;
                var currentIndex = DollResourcesManager.Portrait.IndexOfKey(key ?? "");
                GUILayout.BeginHorizontal();
                ModKit.UI.Label("Portrait ", GUILayout.Width(DefaultLabelWidth));
                var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(currentIndex, 0, DollResourcesManager.Portrait.Count, GUILayout.Width(DefaultSliderWidth)), 0);
                if (GUILayout.Button("Prev", GUILayout.Width(55)) && currentIndex >= 0)
                {
                    newIndex = currentIndex - 1;
                }
                if (GUILayout.Button("Next", GUILayout.Width(55)) && currentIndex < DollResourcesManager.Portrait.Count - 1)
                {
                    newIndex = currentIndex + 1;
                }
                if (GUILayout.Button("Use Custom", GUILayout.Width(125f)))
                {
                    unitEntityData.Descriptor.UISettings.SetPortrait(CustomPortraitsManager.Instance.CreateNewOrLoadDefault());
                    EventBus.RaiseEvent<IUnitPortraitChangedHandler>(delegate (IUnitPortraitChangedHandler h)
                    {
                        h.HandlePortraitChanged(unitEntityData);
                    });
                    return;
                }
                var value = newIndex >= 0 && newIndex < DollResourcesManager.Portrait.Count ? DollResourcesManager.Portrait.Values[newIndex] : null;
                ModKit.UI.Label(" " + value, GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();
                if (newIndex != currentIndex && value != null)
                {
                    unitEntityData.Descriptor.UISettings.SetPortrait(value);
                    EventBus.RaiseEvent<IUnitPortraitChangedHandler>(delegate (IUnitPortraitChangedHandler h)
                    {
                        h.HandlePortraitChanged(unitEntityData);
                    });
                }
            }
        }
        static void ChooseAsks(UnitEntityData unitEntityData)
        {
            int currentIndex = -1;
            if (unitEntityData.Descriptor.CustomAsks != null)
            {
                currentIndex = DollResourcesManager.Asks.IndexOfKey(unitEntityData.Descriptor.CustomAsks.name);
            }
            GUILayout.BeginHorizontal();
            ModKit.UI.Label("Custom Voice ", GUILayout.Width(DefaultLabelWidth));
            var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(currentIndex, -1, DollResourcesManager.Asks.Count - 1, GUILayout.Width(DefaultSliderWidth)), 0);
            if (GUILayout.Button("Prev", GUILayout.Width(55)) && currentIndex >= 0)
            {
                newIndex = currentIndex - 1;
            }
            if (GUILayout.Button("Next", GUILayout.Width(55)) && currentIndex < DollResourcesManager.Asks.Count)
            {
                newIndex = currentIndex + 1;
            }
            var value = (newIndex >= 0 && newIndex < DollResourcesManager.Asks.Count) ? DollResourcesManager.Asks.Values[newIndex] : null;
            if (GUILayout.Button("Preview", GUILayout.ExpandWidth(false)))
            {
                var component = value?.GetComponent<UnitAsksComponent>();
                if (component != null && component.PreviewSound != "")
                {
                    component.PlayPreview();
                }
                else if (component != null && component.Selected.HasBarks)
                {
                    var bark = component.Selected.Entries.Random();
                    AkSoundEngine.PostEvent(bark.AkEvent, unitEntityData.View.gameObject);
                }
            }
            ModKit.UI.Label(" " + (value?.name ?? "None"), GUILayout.ExpandWidth(false));


            GUILayout.EndHorizontal();
            if (newIndex != currentIndex)
            {
                unitEntityData.Descriptor.CustomAsks = value;
                unitEntityData.View?.UpdateAsks();
            }
        }
        static void ChooseFromList<T>(string label, IReadOnlyList<T> list, ref int currentIndex, Action onChoose)
        {
            if (list.Count == 0) return;
            GUILayout.BeginHorizontal();
            ModKit.UI.Label(label + " ", GUILayout.Width(DefaultLabelWidth));
            var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(currentIndex, 0, list.Count - 1, GUILayout.Width(DefaultSliderWidth)), 0);
            ModKit.UI.Label(" " + newIndex, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Prev",GUILayout.Width(55f)) && newIndex > 0) newIndex--;
            if (GUILayout.Button("Next",GUILayout.Width(55f)) && newIndex < list.Count - 1) newIndex++;
            GUILayout.EndHorizontal();
            if (newIndex != currentIndex && newIndex < list.Count)
            {
                currentIndex = newIndex;
                onChoose();
            }
        }
        static void ChooseFromList2<T>(string label, IReadOnlyList<T> list, ref int currentIndex, Action onChoose)
        {
            if (list.Count == 0) return;
            ///GUILayout.BeginHorizontal();
            ModKit.UI.Label(label + " ", GUILayout.Width(DefaultLabelWidth));
            var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(currentIndex, 0, list.Count - 1, GUILayout.Width(DefaultSliderWidth)), 0);
            ModKit.UI.Label(" " + newIndex, GUILayout.ExpandWidth(false));
            if (newIndex != currentIndex && newIndex < list.Count)
            {
                currentIndex = newIndex;
                onChoose();
            }
            ///GUILayout.EndHorizontal();
        }
        static void ChooseEEL(ref int setting, UnitEntityData unitEntityData, DollState doll, string label, EquipmentEntityLink[] links, EquipmentEntityLink link, Action<EquipmentEntityLink> setter)
        {
            var settings = Main.settings.GetCharacterSettings(unitEntityData);
            if (links.Length == 0)
            {
                ModKit.UI.Label($"Missing equipment for {label}");
            }
            var index = setting;
            ///var index = links.ToList().FindIndex((eel) => eel != null && eel.AssetId == link?.AssetId);
            ChooseFromList(label, links, ref index, () =>
            {

                //Main.SetEELs(unitEntityData, doll);

                setter(links[index]);

            });
            if(setting != index)
            {
                setting = index;
               // unitEntityData.Parts.Get<UnitPartDollData>().Default = doll.CreateData();
                // CharacterManager.RebuildCharacter(unitEntityData);
                Main.SetEELs(unitEntityData, doll);
            }
        }
        static void ChooseEEL(ref int setting, UnitEntityData unitEntityData, DollState doll, string label, EquipmentEntityLink[] links, EquipmentEntityLink link)
        {
            if (links.Length == 0)
            {
                ModKit.UI.Label($"Missing equipment for {label}");
            }
            var index = links.ToList().FindIndex((eel) => eel != null && eel.AssetId == link?.AssetId); ///var index = setting;
            ChooseFromList(label, links, ref index, () =>
            {
                unitEntityData.Descriptor.Doll = doll.CreateData();
            });
            if (setting != index)
            {
                setting = index;
                SetEELs(unitEntityData, doll);
                //CharacterManager.RebuildCharacter(unitEntityData);
            }
        }
        private static void ChooseRamp(ref int setting, UnitEntityData unitEntityData, DollState doll, string label, List<Texture2D> textures, int currentRamp, Action<int> setter)
        {
            try
            {
                GUILayout.BeginHorizontal();
                ChooseFromList(label, textures, ref currentRamp, () =>
                {
                    setter(currentRamp);
                 //   var DollPart = unitEntityData.Parts.Get<UnitPartDollData>();
                //    DollPart.Default = doll.CreateData();
                 ////   Traverse.Create(DollPart).Field("ActiveDoll").SetValue(doll.CreateData());
                /// unitEntityData.Parts.Get<UnitPartDollData>().ActiveDoll = doll.CreateData();
                });
                if (setting != currentRamp)
                {
                    setting = currentRamp;
                   // CharacterManager.RebuildCharacter(unitEntityData);
                    SetEELs(unitEntityData, DollResourcesManager.GetDoll(unitEntityData));
                }
                GUILayout.EndHorizontal();
            }
            catch(Exception e)
            {
                Main.logger.Log(e.ToString());
            }
        }
       /* static void ChooseRamp(UnitEntityData unitEntityData, DollState doll, string label, List<Texture2D> textures, int currentRamp, Action<int> setter)
        {
            ChooseFromList(label, textures, ref currentRamp, () =>
            {
                SetEELs(unitEntityData, doll);
                setter(currentRamp);
                unitEntityData.Descriptor.Doll = doll.CreateData();
                CharacterManager.RebuildCharacter(unitEntityData);
                SetEELs(unitEntityData, doll);
            });
        }*/
        public static int GetRaceIndex(UnitEntityData data)
        {
            var race = data.Progression.Race;
            var Settings = settings.GetCharacterSettings(data);
            //Main.logger.Log(race.Name.ToString());
            var result = -2;
            if (race.Name.ToString().Contains("Human"))
            {
                result = 0;
            }
            else if (race.Name.ToString().Contains("Elf") && !race.Name.ToString().Contains("Half"))
            {
                result = 1;
            }
            else if (race.Name.ToString().Contains("Dwarf"))
            {
                result = 2;
            }
            else if (race.Name.ToString().Contains("Gnome"))
            {
                result = 3;
            }
            else if (race.Name.ToString().Contains("Halfling"))
            {
                result = 4;
            }
            else if (race.Name.ToString().Contains("Half-Elf"))
            {
                result = 5;
            }
            else if (race.Name.ToString().Contains("Half-Orc"))
            {
                result = 6;
            }
            else if (race.Name.ToString().Contains("Aasimar"))
            {
                result = 7;
            }
            else if (race.Name.ToString().Contains("Tiefling"))
            {
                result = 8;
            }
            else if (race.Name.ToString().Contains("Oread"))
            {
                result = 9;
            }
            else if (race.Name.ToString().Contains("Dhampir"))
            {
                result = 10;
            }
            else if (race.Name.ToString().Contains("Kitsune"))
            {
                result = 11;
            }
            Settings.RaceIndex = result;
            Main.logger.Log(result.ToString());
            return result;
        }
        // still doesnt move the silder
      /*  static void ChooseRace(UnitEntityData unitEntityData, DollState doll)
        {
            var Settings = settings.GetCharacterSettings(unitEntityData);
            var index = -3;
            if (Settings.RaceIndex == -1 || Settings.RaceIndex == -2)
            {
                Settings.RaceIndex = GetRaceIndex(unitEntityData);
            }
            ///Main.logger.Log(unitEntityData.Progression.Race.Name.ToString());
            ///var currentRace = Settings.dollrace;
            var races = BlueprintRoot.Instance.Progression.CharacterRaces;
            var racelist = BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>();
            if (index == -3)
            {
                index = Settings.RaceIndex;
            }
            index = Settings.RaceIndex;
           /// var index = Array.FindIndex(racelist, (race) => race == currentRace);
           /// Main.logger.Log(index.ToString());
           var newindex = (int)Math.Round(GUILayout.HorizontalSlider(index, (float)0.0, (float)racelist.Count() - (float)1, GUILayout.Width(DefaultSliderWidth)), 1);
            ModKit.UI.Label(" " + index, GUILayout.ExpandWidth(false));
            ModKit.UI.Label(" " + newindex, GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal();

            ChooseFromList("Race", racelist,ref index, () => {
                doll.SetRace(racelist[index]);
                unitEntityData.Descriptor.m_LoadedDollData = doll.CreateData();
                CharacterManager.RebuildCharacter(unitEntityData);
            });
            ModKit.UI.Label(" " + racelist[index].Name);
            GUILayout.EndHorizontal();
        }*/
        /*static void ChooseRace(UnitEntityData unitEntityData, DollState doll)
        {
            var Settings = settings.GetCharacterSettings(unitEntityData);
            var races = BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>();
            var currentRace = races.ElementAt(Settings.RaceIndex);
            Main.logger.Log(currentRace.ToString()+ " " +currentRace.Name);
            var index = Array.FindIndex<BlueprintRace>(races, (race) => race == currentRace);
            var sus = Settings.RaceIndex;
            if (Settings.RaceIndex == -1)
            {
                Settings.RaceIndex = Array.FindIndex<BlueprintRace>(races, (race) => race == currentRace);
            }
            GUILayout.BeginHorizontal();
            ChooseFromList2("Race", races, ref sus, () => {
                doll.SetRace(races[sus]);
                unitEntityData.Descriptor.m_LoadedDollData = doll.CreateData();
                CharacterManager.RebuildCharacter(unitEntityData);
            });
            ModKit.UI.Label(" " + races[sus].Name);
            if(sus != index)
            {
                sus = index;
                Settings.RaceIndex = sus;
            }
            GUILayout.EndHorizontal();
        }*/
        static void ChooseRace(UnitEntityData unitEntityData, DollState doll)
        {
            var Settings = settings.GetCharacterSettings(unitEntityData);
            var races = BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>();
            var currentRace = races.ElementAt(Settings.RaceIndex);
            if (doll.Race != currentRace)
            {
                doll.SetRace(currentRace);
            }
            var initindx = Settings.RaceIndex;
            GUILayout.BeginHorizontal();
            ModKit.UI.Label("Race" + " ", GUILayout.Width(DefaultLabelWidth));
            Settings.RaceIndex = (int)Math.Round(GUILayout.HorizontalSlider((float)Settings.RaceIndex, 0, races.Length - 1, GUILayout.Width(DefaultSliderWidth)));
            /// ModKit.UI.Label(asdd.ToString()+ " " + races[asdd].Name);
            ModKit.UI.Label(" " + Settings.RaceIndex, GUILayout.ExpandWidth(false));
            ModKit.UI.Label(" " + races[Settings.RaceIndex].Name, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Prev", GUILayout.Width(55f)) && Settings.RaceIndex > 0) Settings.RaceIndex--;
            if (GUILayout.Button("Next", GUILayout.Width(55f)) && Settings.RaceIndex < races.Length - 1) Settings.RaceIndex++;
            GUILayout.EndHorizontal();
            if (Settings.RaceIndex != initindx && Settings.RaceIndex < races.Count())
            {
                doll.SetRace(races[Settings.RaceIndex]);
                unitEntityData.Descriptor.Doll = doll.CreateData();
                CharacterManager.RebuildCharacter(unitEntityData);
            }
        }

        static void ChooseVisualPreset(UnitEntityData unitEntityData, DollState doll, string label, BlueprintRaceVisualPreset[] presets,
            BlueprintRaceVisualPreset currentPreset)
        {
            var index = Array.FindIndex(presets, (vp) => vp == currentPreset);
            ChooseFromList(label, presets, ref index, () =>
            {
                doll.SetRacePreset(presets[index]);
                unitEntityData.Descriptor.Doll = doll.CreateData();
                CharacterManager.RebuildCharacter(unitEntityData);
            });
        }
        public static void GetIndices(UnitEntityData dat)
        {
            try
            {
                var doll2 = DollResourcesManager.GetDoll(dat);
                if (dat.IsStoryCompanion() && doll2 == null) return;
               // Main.logger.Log("triedgetindices");
                var doll = DollResourcesManager.GetDoll(dat);
                var gender = dat.Gender;
                var race = doll.Race;
                CustomizationOptions customizationOptions = gender != Gender.Male ? race.FemaleOptions : race.MaleOptions;
                ///CustomizationOptions customizationOptions = new CustomizationOptions();
                var charsettings = settings.GetCharacterSettings(dat);
                /*if (gender == Gender.Male)
                {
                    customizationOptions = new CustomizationOptions
                    {
                        Beards = race.MaleOptions.Beards,
                        Eyebrows = race.MaleOptions.Eyebrows,
                        Hair = race.MaleOptions.Hair,
                        Heads = race.MaleOptions.Beards,
                        Horns = race.MaleOptions.Beards,
                        TailSkinColors = race.MaleOptions.TailSkinColors
                    };
                }
                else if (gender == Gender.Female)
                {
                    customizationOptions = new CustomizationOptions
                    {
                        Beards = race.FemaleOptions.Beards,
                        Eyebrows = race.FemaleOptions.Eyebrows,
                        Hair = race.FemaleOptions.Hair,
                        Heads = race.FemaleOptions.Beards,
                        Horns = race.FemaleOptions.Beards,
                        TailSkinColors = race.FemaleOptions.TailSkinColors
                    };
                }*/
                ///var doll = dat.Descriptor.m_LoadedDollData;
                /// var doll = DollResourcesManager.CreateDollState(dat);

                if (doll == null)
                {
                    try
                    {
                        doll = DollResourcesManager.CreateDollState(dat);
                    }
                    catch (Exception e) { Main.logger.Error(e.Message + "  " + e.StackTrace); }
                }
                charsettings.Face = Array.IndexOf(customizationOptions.Heads, doll.Head.m_Link);
                if (customizationOptions.Hair.Count() > 0) charsettings.Hair = Array.IndexOf(customizationOptions.Hair, doll.Hair.m_Link);
                if (customizationOptions.Beards.Count() > 0) charsettings.Beards = Array.IndexOf(customizationOptions.Beards, doll.Beard.m_Link);
                if (customizationOptions.Horns.Count() > 0) charsettings.Horns = Array.IndexOf(customizationOptions.Horns, doll.Horn.m_Link);

                /// Main.logger.Log("hornpassed");
                if (customizationOptions.Hair.Count() > 0) charsettings.HairColor = doll.HairRampIndex;
                /// Main.logger.Log("haircolorpassed");
                charsettings.SkinColor = doll.SkinRampIndex;
                ///charsettings.SkinColor = 1;
                ///Main.logger.Log("skincolorpassed");
                if (customizationOptions.Horns.Count() > 0) charsettings.HornsColor = doll.HornsRampIndex;
                /// Main.logger.Log("horncolorpassed");
                charsettings.PrimaryColor = doll.EquipmentRampIndex;
                charsettings.SecondaryColor = doll.EquipmentRampIndexSecondary;
                charsettings.RaceIndex = Array.IndexOf(BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>(), doll.Race);


                ///Main.logger.Log("Got Indices");
            }
            catch (Exception e)
            {
                Main.logger.Log(e.Message);
                Main.logger.Log(e.StackTrace);
                Main.logger.Log(e.Source);
                Main.logger.Log(e.InnerException.Message);
            };
        }
        public static void SetEELs(UnitEntityData dat, DollState doll, bool shouldRebuild = true)
        {
            try
            {
                if (doll == null) return;
                var Settings = settings.GetCharacterSettings(dat);
                var gender = dat.Gender;
                var races = BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>();
                BlueprintRace race;
                if(Settings.RaceIndex != -1)
                { 
                 race = races[Settings.RaceIndex]; 
                }
                else
                {
                    if (!dat.Descriptor.Progression.Race.NameForAcronym.Contains("Mongrel"))
                    {
                        race = dat.Progression.Race;
                    }
                    else
                    {
                        race = Utilities.GetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
                    }
                }
                doll.SetRace(race);
                doll.SetRacePreset(race.Presets.First());
                if(dat.Gender == Gender.Male)
                {
                    dat.View.CharacterAvatar.Skeleton = doll.RacePreset.MaleSkeleton;
                    dat.View.CharacterAvatar.m_Skeleton = doll.RacePreset.MaleSkeleton;
                    dat.View.CharacterAvatar.m_SkeletonChanged = true;
                }
                else
                {
                    dat.View.CharacterAvatar.Skeleton = doll.RacePreset.FemaleSkeleton;
                    dat.View.CharacterAvatar.m_Skeleton = doll.RacePreset.FemaleSkeleton;
                    dat.View.CharacterAvatar.m_SkeletonChanged = true;
                }
                dat.View.CharacterAvatar.UpdateSkeleton();
               // dat.View.CharacterAvatar.m_Skeleton = (doll.Gender != Gender.Male) ? doll.RacePreset.FemaleSkeleton : doll.RacePreset.MaleSkeleton;
                CustomizationOptions customizationOptions = gender != Gender.Male ? race.FemaleOptions : race.MaleOptions;
                Settings.Face = Main.EELIndex(Settings.Face, customizationOptions.Heads.Length);
                doll.SetHead(customizationOptions.Heads[Settings.Face]);
                if (customizationOptions.Hair.Length > 0)Settings.Hair = Main.EELIndex(Settings.Hair, customizationOptions.Hair.Length);

                if (customizationOptions.Hair.Length > 0) doll.SetHair(customizationOptions.Hair[Settings.Hair]);

                if (customizationOptions.Beards.Length > 0) Settings.Beards = Main.EELIndex(Settings.Beards, customizationOptions.Beards.Length);

                if (customizationOptions.Beards.Length > 0) doll.SetBeard(customizationOptions.Beards[Settings.Beards]);

                if (customizationOptions.Horns.Length > 0) Settings.Horns = Main.EELIndex(Settings.Horns, customizationOptions.Horns.Length);

                if (customizationOptions.Horns.Length > 0) doll.SetHorn(customizationOptions.Horns[Settings.Horns]);

                if (customizationOptions.Hair.Length > 0) Settings.HairColor = Main.EELIndex(Settings.HairColor, doll.Hair.m_Entity.PrimaryColorsProfile.Ramps.Count);

                if (customizationOptions.Hair.Length > 0) doll.SetHairColor(Settings.HairColor);

                if (customizationOptions.Horns.Length > 0) Settings.HornsColor = Main.EELIndex(Settings.HornsColor, doll.GetHornsRamps().Count);

                if (customizationOptions.Horns.Length > 0) doll.SetHornsColor(Settings.HornsColor);

                Settings.SkinColor = Main.EELIndex(Settings.SkinColor, doll.Head.m_Entity.PrimaryColorsProfile.Ramps.Count);

                doll.SetSkinColor(Settings.SkinColor);

                Settings.PrimaryColor = EELIndex(Settings.PrimaryColor, doll.GetOutfitRampsPrimary().Count);

                Settings.SecondaryColor = EELIndex(Settings.SecondaryColor, doll.GetOutfitRampsSecondary().Count); 

                //doll.SetHair(customizationOptions.Hair[Settings.Hair]);

                doll.SetEquipColors(Settings.PrimaryColor, Settings.SecondaryColor);
                if (Settings.classOutfit.Name != "Default")
                {
                    doll.SetPrimaryEquipColor(Settings.PrimaryColor);
                    doll.SetSecondaryEquipColor(Settings.SecondaryColor);
                }
                dat.Parts.Get<UnitPartDollData>().Default = doll.CreateData();
              ///  CharacterManager.UpdateModel(dat.View);
                if (shouldRebuild)
                {
                    CharacterManager.RebuildCharacter(dat);
                }
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }
        static void ChooseDoll(UnitEntityData unitEntityData)
        {
            try
            {
                if (!unitEntityData.IsMainCharacter && !unitEntityData.IsCustomCompanion() && GUILayout.Button("Destroy Doll", GUILayout.Width(DefaultLabelWidth)))
                {
                    unitEntityData.Parts.Get<UnitPartDollData>().ViewWillDetach();
                    unitEntityData.Parts.Get<UnitPartDollData>().OnViewWillDetach();
                    unitEntityData.Parts.Get<UnitPartDollData>().RemoveSelf();
                    unitEntityData.Parts.Remove<UnitPartDollData>();
                    unitEntityData.PostLoad();
                    unitEntityData.Parts.PostLoad();
                    unitEntityData.Descriptor.Doll = null;
                    unitEntityData.Descriptor.m_LoadedDollData = null;
                    unitEntityData.Descriptor.ForcceUseClassEquipment = false;
                    ///   unitEntityData.View.CharacterAvatar.LoadBakedCharacter();
                    ///   unitEntityData.DetachView();
                    /// unitEntityData.View.Destroy();
                    /// unitEntityData.OnViewWillDetach();
                    ///unitEntityData.AttachView(unitEntityData.CreateViewForData());
                    /// unitEntityData.View.CharacterAvatarUpdated();
                    CharacterManager.RebuildCharacter(unitEntityData);
                }
                var Settings = settings.GetCharacterSettings(unitEntityData);
                var races = BlueprintRoot.Instance.Progression.CharacterRaces.ToArray<BlueprintRace>();
                var gender = unitEntityData.Gender;
                BlueprintRace race;
                if (Settings.RaceIndex == -1)
                {
                    if (!unitEntityData.Descriptor.Progression.Race.NameForAcronym.Contains("Mongrel"))
                    {
                        race = unitEntityData.Progression.Race;
                        if (Settings.RaceIndex == -1)
                        {
                            Settings.RaceIndex = races.IndexOf(race);
                        }
                    }
                    else
                    {
                        race = (Utilities.GetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4"));
                        if (Settings.RaceIndex == -1)
                        {
                            Settings.RaceIndex = races.IndexOf(race);
                        }
                    }
                }
                else
                {
                    race = races[Settings.RaceIndex];
                }
                ///var race = unitEntityData.Progression.Race;
                CustomizationOptions customizationOptions = gender != Gender.Male ? race.FemaleOptions : race.MaleOptions;
                /* if (Settings.Beards == -1 || Settings.Face == -1 || Settings.Hair == -1 || Settings.HairColor == -1 || Settings.Horns == -1 || Settings.HornsColor == -1 || Settings.PrimaryColor == -1 || Settings.SecondaryColor == -1 || Settings.SkinColor == -1)
                 {
                     GetIndices(unitEntityData,Settings, DollResourcesManager.GetDoll(unitEntityData), customizationOptions);
                 }*/
                /*if (!unitEntityData.Descriptor.Progression.Race.NameForAcronym.Contains("Mongrel"))
                {
                    race = races.ElementAt(Settings.RaceIndex);
                }*/
                customizationOptions = gender != Gender.Male ? race.FemaleOptions : race.MaleOptions;
                if(Main.unlockcustomization)
                {
                  
                }
                var doll2 = DollResourcesManager.GetDoll(unitEntityData);
                /// var race = doll.Race;
                doll2.SetRace(race);
                ChooseRace(unitEntityData, doll2);
                if (doll2.Race != races[Settings.RaceIndex]) doll2.SetRace(races[Settings.RaceIndex]);
                var doll = DollResourcesManager.GetDoll(unitEntityData);
                ChooseEEL(ref Settings.Face, unitEntityData, doll, "Face", customizationOptions.Heads, doll.Head.m_Link, (EquipmentEntityLink ee) => doll.SetHead(ee));
                if (customizationOptions.Hair.Count() > 0) ChooseEEL(ref Settings.Hair, unitEntityData, doll, "Hair", customizationOptions.Hair, doll.Hair.m_Link, (EquipmentEntityLink ee) => doll.SetHair(ee));
                if (customizationOptions.Beards.Count() > 0) ChooseEEL(ref Settings.Beards, unitEntityData, doll, "Beards", customizationOptions.Beards, doll.Beard.m_Link, (EquipmentEntityLink ee) => doll.SetBeard(ee));
                if (customizationOptions.Horns.Count() > 0) ChooseEEL(ref Settings.Horns, unitEntityData, doll, "Horns", customizationOptions.Horns, doll.Horn.m_Link, (EquipmentEntityLink ee) => doll.SetHorn(ee));
                ChooseRamp(ref Settings.HairColor, unitEntityData, doll, "Hair Color", doll.GetHairRamps(), Settings.HairColor, (int index) => doll.SetHairColor(index));
                ChooseRamp(ref Settings.SkinColor, unitEntityData, doll, "Skin Color", doll.GetSkinRamps(), Settings.SkinColor, (int index) => doll.SetSkinColor(index));
                // Horn color doesnt exist in wrath?
               // ChooseRamp(ref Settings.HornsColor, unitEntityData, doll, "Horn Color", doll.GetHornsRamps(), Settings.HornsColor, (int index) => doll.SetHornsColor(index));
               // Main.logger.Log(doll.GetHornsRamps().Count.ToString());;
                ChooseRamp(ref Settings.PrimaryColor, unitEntityData, doll, "Primary Outfit Color", doll.GetOutfitRampsPrimary(), Settings.PrimaryColor, (int index) => doll.SetPrimaryEquipColor(index));
                ChooseRamp(ref Settings.SecondaryColor, unitEntityData, doll, "Secondary Outfit Color", doll.GetOutfitRampsSecondary(), Settings.SecondaryColor, (int index) => doll.SetSecondaryEquipColor(index));
                if(Settings.PrimaryColor != doll.EquipmentRampIndex || Settings.SecondaryColor != doll.EquipmentRampIndexSecondary)
                {
                    doll.SetEquipColors(Settings.PrimaryColor,Settings.SecondaryColor);
                }
                ReferenceArrayProxy<BlueprintRaceVisualPreset, BlueprintRaceVisualPresetReference> presets = doll.Race.Presets;
                BlueprintRaceVisualPreset racePreset = doll.RacePreset;
                /*if (unitEntityData.Descriptor.LeftHandedOverride == true && GUILayout.Button("Set Right Handed", GUILayout.Width(DefaultLabelWidth)))
                {
                    unitEntityData.Descriptor.LeftHandedOverride = false;
                    unitEntityData.Descriptor.m_LoadedDollData = doll.CreateData();
                    ViewManager.ReplaceView(unitEntityData, null);
                    unitEntityData.View.HandsEquipment.HandleEquipmentSetChanged();
                }
                else if (unitEntityData.Descriptor.LeftHandedOverride == false && GUILayout.Button("Set Left Handed", GUILayout.Width(DefaultLabelWidth)))
                {
                    unitEntityData.Descriptor.LeftHandedOverride = true;
                    unitEntityData.Descriptor.m_LoadedDollData = doll.CreateData();
                    ViewManager.ReplaceView(unitEntityData, null);
                    unitEntityData.View.HandsEquipment.HandleEquipmentSetChanged();
                }*/
                ChoosePortrait(unitEntityData);
                ChooseAsks(unitEntityData);
                ModKit.UI.Toggle("Custom Hair Color", ref Settings.customHairColor);
                if (Settings.customHairColor)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    ModKit.UI.DisclosureToggle("Show Picker", ref Settings.showHair);
                    GUILayout.EndHorizontal();
                    if (Settings.showHair)
                    {
                        HairColorPicker.OnGUI(Settings, unitEntityData, new Color(Settings.hairColor[0], Settings.hairColor[1], Settings.hairColor[2]), ref Settings.hairColor, Main.GenerateHairColor);
                    }
                    GUILayout.Space(5f);
                }
                ModKit.UI.Toggle("Custom Skin Color", ref Settings.customSkinColor);
                if (Settings.customSkinColor)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    ModKit.UI.DisclosureToggle("Show Picker", ref Settings.showSkin);
                    GUILayout.EndHorizontal();
                    if (Settings.showSkin)
                    {
                        SkinColorPicker.OnGUI(Settings, unitEntityData, new Color(Settings.skinColor[0], Settings.skinColor[1], Settings.skinColor[2]), ref Settings.skinColor, Main.GenerateSkinColor);
                    }
                    GUILayout.Space(5f);
                }
                ModKit.UI.Toggle("Custom Outfit Colors", ref Settings.customOutfitColors);
                if (Settings.customOutfitColors)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    ModKit.UI.DisclosureToggle("Show Primary Picker", ref Settings.showPrimColor);
                    GUILayout.EndHorizontal();
                    if (Settings.showPrimColor)
                    {
                        PrimaryColorPicker.OnGUI(Settings, unitEntityData, new Color(Settings.primColor[0], Settings.primColor[1], Settings.primColor[2]), ref Settings.primColor,Main.GenerateOutfitcolor);
                    }
                    GUILayout.Space(5f);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    ModKit.UI.DisclosureToggle("Show Secondary Picker", ref Settings.showSecondColor);
                    GUILayout.EndHorizontal();
                    if (Settings.showSecondColor)
                    {
                        SecondaryColorPicker.OnGUI(Settings, unitEntityData, new Color(Settings.secondColor[0], Settings.secondColor[1], Settings.secondColor[2]), ref Settings.secondColor, Main.GenerateOutfitcolor);
                    }
                    GUILayout.Space(5f);
                }
                // if (unitEntityData.IsMainCharacter || unitEntityData.IsCustomCompanion()) ChooseAsks(unitEntityData);
            }
            catch (Exception e) { Main.logger.Log(e.ToString()); }
        }

        static void ChooseCompanionColor(CharacterSettings characterSettings, UnitEntityData unitEntityData)
        {
            try
            {
                if (GUILayout.Button("Create Doll", GUILayout.Width(DefaultLabelWidth)))
                {
                    var race = unitEntityData.Descriptor.Progression.Race;
                    var options = unitEntityData.Descriptor.Gender == Gender.Male ? race.MaleOptions : race.FemaleOptions;
                    var dollState = new DollState();
                    if (unitEntityData.Descriptor.Progression.Race.name != "MongrelmanRace")
                    {
                        dollState.SetRace(unitEntityData.Descriptor.Progression.Race); //Race must be set before class
                                                                                       //This is a hack to work around harmony not allowing calls to the unpatched   
                    }
                    else
                    {
                        dollState.SetRace(Utilities.GetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4"));
                    }
                    CharacterManager.disableEquipmentClassPatch = true;
                    dollState.SetClass(unitEntityData.Descriptor.Progression.GetEquipmentClass());
                    CharacterManager.disableEquipmentClassPatch = false;
                    dollState.SetGender(unitEntityData.Descriptor.Gender);
                    dollState.SetRacePreset(race.Presets[0]);
                    unitEntityData.Descriptor.LeftHandedOverride = false;
                    if (options.Hair.Length > 0) dollState.SetHair(options.Hair[0]);
                    if (options.Heads.Length > 0) dollState.SetHead(options.Hair[0]);
                    if (options.Beards.Length > 0) dollState.SetBeard(options.Hair[0]);
                    //dollState.Validate();
                    SetEELs(unitEntityData, dollState);
                    // unitEntityData.Descriptor.Doll = dollState.CreateData();
                    unitEntityData.Parts.Add<UnitPartDollData>();
                    unitEntityData.Parts.Get<UnitPartDollData>().Default = dollState.CreateData();
                    unitEntityData.Parts.Get<UnitPartDollData>().OnDidAttachToEntity();
                    unitEntityData.Parts.Get<UnitPartDollData>().OnViewDidAttach();
                    unitEntityData.View.HandsEquipment.UpdateLocatorTrackers();
                    // Traverse.Create(unitEntityData.Parts.Get<UnitPartDollData>()).Field("ActiveDoll").SetValue(dollState.CreateData()); 
                //    unitEntityData.Descriptor.ForcceUseClassEquipment = true;
                    CharacterManager.RebuildCharacter(unitEntityData);
                    // SetEELs(unitEntityData,dollState);
                    ///SetEELs(unitEntityData, dollState);
                  //  CharacterManager.RebuildCharacter(unitEntityData);
                    //SetEELs(unitEntityData, dollState);
                  //  CharacterManager.UpdateModel(unitEntityData.View);
                    // SetEELs(unitEntityData, dollState);
                    //CharacterManager.RebuildCharacter(unitEntityData);
                    // SetEELs(unitEntityData, dollState);
                   // SetEELs(unitEntityData, dollState, true);
                }         
            }
            catch(Exception e)
            {
                Main.logger.Log(e.ToString());
            }
            ModKit.UI.Label("Note: Colors only applies to non-default outfits, the default companion custom voice is None");
            {
                GUILayout.BeginHorizontal();
                ModKit.UI.Label("Primary Outfit Color ", GUILayout.Width(DefaultLabelWidth));
                var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(characterSettings.companionPrimary, -1, 90, GUILayout.Width(DefaultSliderWidth)), 0);
                ModKit.UI.Label(" " + newIndex, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                if (newIndex != characterSettings.companionPrimary)
                {
                    characterSettings.companionPrimary = newIndex;
                    CharacterManager.RebuildCharacter(unitEntityData);
                    CharacterManager.UpdateModel(unitEntityData.View);
                }
            }
            {
                GUILayout.BeginHorizontal();
                ModKit.UI.Label("Secondary Outfit Color ", GUILayout.Width(DefaultLabelWidth));
                var newIndex = (int)Math.Round(GUILayout.HorizontalSlider(characterSettings.companionSecondary, -1, 90, GUILayout.Width(DefaultSliderWidth)), 0);
                ModKit.UI.Label(" " + newIndex, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                if (newIndex != characterSettings.companionSecondary)
                {
                    characterSettings.companionSecondary = newIndex;
                    if(characterSettings.customOutfitColors)
                    {
                        Main.GenerateOutfitcolor(unitEntityData);
                    }
                    CharacterManager.RebuildCharacter(unitEntityData);
                    CharacterManager.UpdateModel(unitEntityData.View);
                }
            }
            ModKit.UI.Toggle("Custom Outfit Colors", ref characterSettings.customOutfitColors);
            if (characterSettings.customOutfitColors)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                ModKit.UI.DisclosureToggle("Show Primary Picker", ref characterSettings.showPrimColor);
                GUILayout.EndHorizontal();
                if (characterSettings.showPrimColor)
                {
                    PrimaryColorPicker.OnGUI(characterSettings, unitEntityData, new Color(characterSettings.primColor[0], characterSettings.primColor[1], characterSettings.primColor[2]), ref characterSettings.primColor, Main.GenerateOutfitcolor);
                }
                GUILayout.Space(5f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                ModKit.UI.DisclosureToggle("Show Secondary Picker", ref characterSettings.showSecondColor);
                GUILayout.EndHorizontal();
                if (characterSettings.showSecondColor)
                {
                    SecondaryColorPicker.OnGUI(characterSettings, unitEntityData, new Color(characterSettings.secondColor[0], characterSettings.secondColor[1], characterSettings.secondColor[2]), ref characterSettings.secondColor, Main.GenerateOutfitcolor);
                }
                GUILayout.Space(5f);
            }
            ChoosePortrait(unitEntityData);
            ChooseAsks(unitEntityData);
        }
        static void ChooseToggle(string label, ref bool currentValue, Action onChoose)
        {
            bool temp = currentValue;
            ModKit.UI.Toggle(label, ref temp);
            if (temp != currentValue)
            {
                currentValue = temp;
                onChoose();
            }
        }
        public static int EELIndex(int setting, int length)
        {
            if (setting >= 0 && setting < length) return setting;
            else return 0;
        }
        static void ChooseEquipment(UnitEntityData unitEntityData, CharacterSettings characterSettings)
        {
            void onHideEquipment()
            {
                CharacterManager.RebuildCharacter(unitEntityData);
                CharacterManager.UpdateModel(unitEntityData.View);
            }
            void onHideBuff()
            {
                foreach (var buff in unitEntityData.Buffs) buff.ClearParticleEffect();
                /*beta3*/
                //unitEntityData.SpawnBuffsFxs();
            }
            void onWeaponChanged()
            {
                ///if(unitEntityData.View.HandsEquipment.GetWeaponModel(false) != characterSettings.overrideWeapons)
                unitEntityData.View.HandsEquipment.UpdateAll();
            }
            characterSettings.hideAll = !unitEntityData.UISettings.ShowClassEquipment;
            ChooseToggle("Hide All Equipment", ref characterSettings.hideAll, onHideEquipment);
            unitEntityData.UISettings.ShowClassEquipment = !characterSettings.hideAll;
            if(unitEntityData.UISettings.ShowClassEquipment)
            {
                if (!characterSettings.hideArmor) characterSettings.hideArmor = true;
                if (!characterSettings.hideHelmet) characterSettings.hideHelmet = true;
                if (!characterSettings.hideGlasses) characterSettings.hideGlasses = true;
                if (!characterSettings.hideShirt) characterSettings.hideShirt = true;
                if (!characterSettings.hideItemCloak) characterSettings.hideItemCloak = true;
                if (!characterSettings.hideBracers) characterSettings.hideBracers = true;
                if (!characterSettings.hideGloves) characterSettings.hideGloves = true;
                if (!characterSettings.hideBoots) characterSettings.hideBoots = true;
                if (!characterSettings.hideCap) characterSettings.hideCap = true;
            }
            ChooseToggle("Hide Cap", ref characterSettings.hideCap, onHideEquipment);
            ChooseToggle("Hide Helmet", ref characterSettings.hideHelmet, onHideEquipment);
            ChooseToggle("Hide Glasses", ref characterSettings.hideGlasses, onHideEquipment);
            ChooseToggle("Hide Shirt", ref characterSettings.hideShirt, onHideEquipment);
            ChooseToggle("Hide Class Equipment", ref characterSettings.hideClassCloak, onHideEquipment);
            ChooseToggle("Hide Cloak", ref characterSettings.hideItemCloak, onHideEquipment);
            ChooseToggle("Hide Armor", ref characterSettings.hideArmor, onHideEquipment);
            ChooseToggle("Hide Bracers", ref characterSettings.hideBracers, onHideEquipment);
            ChooseToggle("Hide Gloves", ref characterSettings.hideGloves, onHideEquipment);
            ChooseToggle("Hide Boots", ref characterSettings.hideBoots, onHideEquipment);
            ChooseToggle("Hide Inactive Weapons", ref characterSettings.hideWeapons, onWeaponChanged);
            ChooseToggle("Hide Sheaths/Scabbards",ref characterSettings.hideSheaths,onWeaponChanged);
            ChooseToggle("Hide Belt Slots", ref characterSettings.hideBeltSlots, onWeaponChanged);
            ChooseToggle("Hide Quiver", ref characterSettings.hideQuiver, onWeaponChanged);
            ChooseToggle("Hide Weapon Enchantments", ref characterSettings.hideWeaponEnchantments, onWeaponChanged);
            ChooseToggle("Hide Wings", ref characterSettings.hideWings, onHideBuff);
            ChooseToggle("Hide Horns", ref characterSettings.hideHorns, onHideEquipment);
            ChooseToggle("Hide Tail", ref characterSettings.hideTail, onHideEquipment);
        }

        /*
         * m_Size is updated from GetSizeScale (EntityData.Descriptor.State.Size) and 
         * is with m_OriginalScale to adjust the transform.localScale 
         * Adjusting GetSizeScale will effect character corpulence and cause gameplay sideeffects
         * Changing m_OriginalScale will effect ParticlesSnapMap.AdditionalScaleGUID
         */
        static void ChooseSizeAdditive(UnitEntityData unitEntityData, CharacterSettings characterSettings)
        {
            GUILayout.BeginHorizontal();
            ModKit.UI.Label("Additive Scale Factor", GUILayout.Width(300));
            var sizeModifier = GUILayout.HorizontalSlider(characterSettings.additiveScaleFactor, -4, 4, GUILayout.Width(DefaultSliderWidth));
            if (!characterSettings.overrideScaleFloatMode) sizeModifier = (int)sizeModifier;
            characterSettings.additiveScaleFactor = sizeModifier;
            var sign = sizeModifier >= 0 ? "+" : "";
            ModKit.UI.Label($" {sign}{sizeModifier:0.##}", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
        static void ChooseSizeOverride(UnitEntityData unitEntityData, CharacterSettings characterSettings)
        {
            GUILayout.BeginHorizontal();
            ModKit.UI.Label("Override Scale Factor", GUILayout.Width(300));
            var sizeModifier = GUILayout.HorizontalSlider(characterSettings.overrideScaleFactor, 0, 8, GUILayout.Width(DefaultSliderWidth));
            if (!characterSettings.overrideScaleFloatMode) sizeModifier = (int)sizeModifier;
            characterSettings.overrideScaleFactor = sizeModifier;
            ModKit.UI.Label($" {(Size)(sizeModifier)}", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
        static void ChooseEquipmentOverridePet(UnitEntityData unitEntityData, CharacterSettings characterSettings)
        {
            ModKit.UI.Label("View",  GUILayout.Width(DefaultLabelWidth));
            void onView() => ViewManager.ReplaceView(unitEntityData, characterSettings.overrideView);
            Util.ChooseSlider("Override View", EquipmentResourcesManager.Units, ref characterSettings.overrideView, onView);
            void onChooseScale()
            {
                Traverse.Create(unitEntityData.View).Field("m_Scale").SetValue(unitEntityData.View.GetSizeScale() + 0.01f);
            }
            ModKit.UI.Label("Scale",  GUILayout.Width(DefaultLabelWidth));
            GUILayout.BeginHorizontal();
            ChooseToggle("Enable Override Scale", ref characterSettings.overrideScale, onChooseScale);
            ChooseToggle("Restrict to polymorph", ref characterSettings.overrideScaleShapeshiftOnly, onChooseScale);
            ChooseToggle("Use Additive Factor", ref characterSettings.overrideScaleAdditive, onChooseScale);
            ChooseToggle("Use Cheat Mode", ref characterSettings.overrideScaleCheatMode, onChooseScale);
            ChooseToggle("Use Continuous Factor", ref characterSettings.overrideScaleFloatMode, onChooseScale);
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
            if (characterSettings.overrideScale && characterSettings.overrideScaleAdditive) ChooseSizeAdditive(unitEntityData, characterSettings);
            if (characterSettings.overrideScale && !characterSettings.overrideScaleAdditive) ChooseSizeOverride(unitEntityData, characterSettings);
            ModKit.UI.Label("Portrait/Voice", GUILayout.Width(DefaultLabelWidth));
            ChoosePortrait(unitEntityData);
            ChooseAsks(unitEntityData);
        }
        static void ChooseEquipmentOverride(UnitEntityData unitEntityData, CharacterSettings characterSettings)
        {
            void onEquipment()
            {
                CharacterManager.RebuildCharacter(unitEntityData);
                CharacterManager.UpdateModel(unitEntityData.View);
            }
            ModKit.UI.Label("Equipment",  GUILayout.Width(DefaultLabelWidth));
            void onView() => ViewManager.ReplaceView(unitEntityData, characterSettings.overrideView);
            Util.ChooseSlider("Override Helm", EquipmentResourcesManager.Helm, ref characterSettings.overrideHelm, onEquipment);
            Util.ChooseSlider("Override Cloak", EquipmentResourcesManager.Cloak, ref characterSettings.overrideCloak, onEquipment);
            Util.ChooseSlider("Override Shirt", EquipmentResourcesManager.Shirt, ref characterSettings.overrideShirt, onEquipment);
            Util.ChooseSlider("Override Glasses/Mask", EquipmentResourcesManager.Glasses, ref characterSettings.overrideGlasses, onEquipment);
            Util.ChooseSlider("Override Armor", EquipmentResourcesManager.Armor, ref characterSettings.overrideArmor, onEquipment);
            Util.ChooseSlider("Override Bracers", EquipmentResourcesManager.Bracers, ref characterSettings.overrideBracers, onEquipment);
            Util.ChooseSlider("Override Gloves", EquipmentResourcesManager.Gloves, ref characterSettings.overrideGloves, onEquipment);
            Util.ChooseSlider("Override Boots", EquipmentResourcesManager.Boots, ref characterSettings.overrideBoots, onEquipment);
            Util.ChooseSlider("Override Tattoos", EquipmentResourcesManager.Tattoos, ref characterSettings.overrideTattoo, onEquipment);
           // Util.ChooseSlider("Override Misc", EquipmentResourcesManager.Tattoos, ref characterSettings.overrideOther, onEquipment);
            ModKit.UI.Label("Weapons",  GUILayout.Width(DefaultLabelWidth));
            foreach (var kv in EquipmentResourcesManager.Weapons)
            {
                var animationStyle = kv.Key;
                var weaponLookup = kv.Value;
                characterSettings.overrideWeapons.TryGetValue(animationStyle, out BlueprintRef currentValue);
                void onWeapon()
                {
                    characterSettings.overrideWeapons[animationStyle] = currentValue;
                    unitEntityData.View.HandsEquipment.UpdateAll();
                }
                Util.ChooseSlider($"Override {animationStyle} ", weaponLookup, ref currentValue, onWeapon);
            }
            GUILayout.BeginHorizontal();
            ModKit.UI.Label("Main Weapon Enchantments",  GUILayout.Width(DefaultLabelWidth));
            if (GUILayout.Button("Add Enchantment", GUILayout.ExpandWidth(false)))
            {
                characterSettings.overrideMainWeaponEnchantments.Add(null);
            }
            GUILayout.EndHorizontal();
            void onWeaponEnchantment()
            {
                unitEntityData.View.HandsEquipment.UpdateAll();
            }
            for (int i = 0; i < characterSettings.overrideMainWeaponEnchantments.Count; i++)
            {
                Util.ChooseSliderList($"Override Main Hand", EquipmentResourcesManager.WeaponEnchantments,
                    characterSettings.overrideMainWeaponEnchantments, i, onWeaponEnchantment);
            }
            GUILayout.BeginHorizontal();
            ModKit.UI.Label("Offhand Weapon Enchantments",  GUILayout.Width(DefaultLabelWidth));
            if (GUILayout.Button("Add Enchantment", GUILayout.ExpandWidth(false)))
            {
                characterSettings.overrideOffhandWeaponEnchantments.Add("");
            }
            GUILayout.EndHorizontal();
            for (int i = 0; i < characterSettings.overrideOffhandWeaponEnchantments.Count; i++)
            {
                Util.ChooseSliderList($"Override Off Hand", EquipmentResourcesManager.WeaponEnchantments,
                    characterSettings.overrideOffhandWeaponEnchantments, i, onWeaponEnchantment);
            }
            ModKit.UI.Label("View",  GUILayout.Width(DefaultLabelWidth));
            Util.ChooseSlider("Override View", EquipmentResourcesManager.Units, ref characterSettings.overrideView, onView);
            void onChooseScale()
            {
                Traverse.Create(unitEntityData.View).Field("m_Scale").SetValue(unitEntityData.View.GetSizeScale() + 0.01f);
            }
            ModKit.UI.Label("Scale",  GUILayout.Width(DefaultLabelWidth));
            GUILayout.BeginHorizontal();
            ChooseToggle("Enable Override Scale", ref characterSettings.overrideScale, onChooseScale);
            ChooseToggle("Restrict to polymorph", ref characterSettings.overrideScaleShapeshiftOnly, onChooseScale);
            ChooseToggle("Use Additive Factor", ref characterSettings.overrideScaleAdditive, onChooseScale);
            ChooseToggle("Use Cheat Mode", ref characterSettings.overrideScaleCheatMode, onChooseScale);
            ChooseToggle("Use Continuous Factor", ref characterSettings.overrideScaleFloatMode, onChooseScale);
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
            if (characterSettings.overrideScale && characterSettings.overrideScaleAdditive) ChooseSizeAdditive(unitEntityData, characterSettings);
            if (characterSettings.overrideScale && !characterSettings.overrideScaleAdditive) ChooseSizeOverride(unitEntityData, characterSettings);
        }
    }
    [HarmonyPatch(typeof(BlueprintsCache), "Init")]
    static class ResourcesLibrary_InitializeLibrary_Patch
    {
        static void Postfix()
        {
            /*var bps = Util.GetBlueprints();
            var portraits = bps.OfType<BlueprintPortrait>();
            var classes = bps.OfType<BlueprintCharacterClass>();
            Main.blueprints = portraits.Concat<SimpleBlueprint>(classes).ToArray();*/
            Main.blueprints = Util.GetBlueprints();
        }
    }
}
