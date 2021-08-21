using System;
using UnityModManagerNet;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using ModEntry = UnityModManagerNet.UnityModManager.ModEntry;

namespace ModDebugger
{
    public enum Param
    {
        Controller,
        Conductor,
        BBManager,
        SteamIntegration,
        Vfx,
        Sfx,
        LogoText,
        DiscordController,
        scnLevelSelect,
        UIController,
        CustomLevel,
        Notification,
        NewgroundsAPImanager,
        VfxPlus,
        CalibrationLine,
        CalibrationPlanet,
        scnEditor,
        LevelMaker,
        Camera,
        scnCLS,
        AudioManager,
        ModEntry
    }
    public class Mod
    {
        public string Name;
        public Assembly Assembly;
        public ModEntry modEntry;
        public Type[] types;
        public bool IsExpanded { get; set; }
        public bool IsEnabled { get; set; }
        private bool run = true;
        public Mod(ModEntry entry)
        {
            this.modEntry = entry;
            this.Assembly = entry.Assembly;
            this.Name = entry.Info.DisplayName;
        }
        public bool[] listsmethod;
        public string Log;
        //public bool entry;
        private void gui()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Logger");
            Log = GUILayout.TextArea(Log);
            if(GUILayout.Button("Log!"))
            {
                modEntry.Logger.Log(Log);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            for (int i = 0; i < types.Length; i++)
            {
                listsmethod[i] = GUILayout.Toggle(listsmethod[i], types[i].Name);
                if (listsmethod[i])
                {
                    MethodInfo[] methods = types[i].GetMethods(AccessTools.all);
                    FieldInfo[] fields = types[i].GetFields(AccessTools.all);
                    PropertyInfo[] props = types[i].GetProperties(AccessTools.all);
                    for (int ii = 0; ii < methods.Length; ii++)
                    {
                        string parameters = "";
                        ParameterInfo[] pis = methods[ii].GetParameters();
                        foreach (ParameterInfo pi in pis)
                        {
                            parameters += (pi.ParameterType + " " + pi.Name + (pis.Length >= 2 ? " " : ""));
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label((methods[ii].IsPublic ? "public " : "private ") + (methods[ii].IsStatic ? "static " : "") + methods[ii].Name + "(" + parameters + ")");
                        if (GUILayout.Button("Invoke"))
                        {
                            try
                            {
                                try
                                {
                                    if (Main.@params.Count <= 0)
                                    {
                                        methods[ii].Invoke(Main.MakeNewInstance ? Assembly.CreateInstance(types[i].Name) : null, new object[] { });
                                    }
                                    else
                                    {
                                        methods[ii].Invoke(Main.MakeNewInstance ? Assembly.CreateInstance(types[i].Name) : null, Main.@params.ToArray());
                                    }
                                }
                                catch
                                {
                                    modEntry.Logger.Log("Can't Invoke!!");
                                }
                            }
                            catch (Exception e)
                            {
                                if (Main.Settings.LogException)
                                {
                                    modEntry.Logger.Log(e.Message);
                                }
                            }
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    for (int ii = 0; ii < fields.Length; ii++)
                    {
                        try
                        {
                            string value = "";
                            try
                            {
                                value = fields[ii].GetValue(Main.MakeNewInstance ? Assembly.CreateInstance(types[i].Name) : null).ToString();
                            }
                            catch
                            {
                                value = fields[ii].GetValue(null).ToString();
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Label((fields[ii].IsPublic ? "public " : "private ") + (fields[ii].IsStatic ? "static " : "") + fields[ii].Name + ": " + value);
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        catch (Exception e)
                        {
                            if (Main.Settings.LogException)
                            {
                                modEntry.Logger.Log(e.ToString());
                            }
                        }
                    }
                    for (int ii = 0; ii < props.Length; ii++)
                    {
                        try
                        {
                            string value = "";
                            try
                            {
                                value = props[ii].GetValue(Main.MakeNewInstance ? Assembly.CreateInstance(types[i].Name) : null).ToString();
                            }
                            catch
                            {
                                value = props[ii].GetValue(null).ToString();
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(props[ii].Name + ": " + value);
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        catch (Exception e)
                        {
                            if (Main.Settings.LogException)
                            {
                                modEntry.Logger.Log(e.ToString());
                            }
                        }
                    }
                }
            }
        }
        public void Run()
        {
            if (Assembly != null && run)
            {
                types = Assembly.GetTypes();
                listsmethod = new bool[types.Length];
            }
            run = false;
        }
        public void Gui()
        {
            Run();
            // Draw header
            GUILayout.BeginHorizontal();
            bool newIsExpanded = GUILayout.Toggle(
                IsExpanded,
                IsEnabled ? (IsExpanded ? "◢" : "▶") : "",
                new GUIStyle()
                {
                    fixedWidth = 10,
                    normal = new GUIStyleState() { textColor = Color.white },
                    fontSize = 15,
                    margin = new RectOffset(4, 2, 6, 6),
                });
            bool newIsEnabled = GUILayout.Toggle(
                IsEnabled,
                Name,
                new GUIStyle(GUI.skin.toggle)
                {
                    /*fontStyle = GlobalSettings.Language.IsSymbolLanguage()
                        ? FontStyle.Normal
                        : FontStyle.Bold,
                    font = GlobalSettings.Language.IsSymbolLanguage()
                        ? TweakAssets.KoreanBoldFont
                        : null,*/
                    margin = new RectOffset(0, 4, 4, 4),
                });
            GUILayout.Label("-");
            GUILayout.Label(
                modEntry.Info.Author,
                new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic });
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Handle enable/disable change
            if (newIsEnabled != IsEnabled)
            {
                IsEnabled = newIsEnabled;
                if (newIsEnabled)
                {
                    //EnableTweak();
                    newIsExpanded = true;
                }
                else
                {
                    //DisableTweak();
                }
            }

            // Handle expand/collapse change
            if (newIsExpanded != IsExpanded)
            {
                IsExpanded = newIsExpanded;
                if (!newIsExpanded)
                {
                    //module.OnHideGUI();
                }
            }

            // Draw custom options
            if (IsExpanded && IsEnabled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(24f);
                GUILayout.BeginVertical();
                gui();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(12f);
            }
        }
    }
    public class Main
    {
        public static readonly BindingFlags all = BindingFlags.CreateInstance | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.OptionalParamBinding | BindingFlags.Public | BindingFlags.PutDispProperty | BindingFlags.PutRefDispProperty | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.Static | BindingFlags.SuppressChangeType;
        public static void AddParam(Param key)
        {
            switch (key)
            {
                case Param.Controller:
                    {
                        @params.Add(scrController.instance ?? new scrController());
                        break;
                    }
                case Param.UIController:
                    {
                        @params.Add(scrUIController.instance ?? new scrUIController());
                        break;
                    }
                case Param.DiscordController:
                    {
                        @params.Add(DiscordController.instance ?? new DiscordController());
                        break;
                    }
                case Param.AudioManager:
                    {
                        @params.Add(AudioManager.Instance ?? new AudioManager());
                        break;
                    }
                case Param.BBManager:
                    {
                        @params.Add(BBManager.instance ?? new BBManager());
                        break;
                    }
                case Param.Camera:
                    {
                        @params.Add(scrCamera.instance ?? new scrCamera());
                        break;
                    }
                case Param.CalibrationLine:
                    {
                        @params.Add(scrCalibrationLine.instance ?? new scrCalibrationLine());
                        break;
                    }
                case Param.CalibrationPlanet:
                    {
                        @params.Add(scrCalibrationPlanet.instance ?? new scrCalibrationPlanet());
                        break;
                    }
                case Param.Conductor:
                    {
                        @params.Add(scrConductor.instance ?? new scrConductor() );
                        break;
                    }
                case Param.CustomLevel:
                    {
                        @params.Add(CustomLevel.instance ?? new CustomLevel());
                        break;
                    }
                case Param.NewgroundsAPImanager:
                    {
                        @params.Add(scrNewgroundsAPIManager.instance ?? new scrNewgroundsAPIManager());
                        break;
                    }
                case Param.LevelMaker:
                    {
                        @params.Add(scrLevelMaker.instance ?? new scrLevelMaker());
                        break;
                    }
                case Param.LogoText:
                    {
                        @params.Add(scrLogoText.instance ?? new scrLogoText());
                        break;
                    }
                case Param.Notification:
                    {
                        @params.Add(Notification.instance ?? new Notification());
                        break;
                    }
                case Param.scnLevelSelect:
                    {
                        @params.Add(scnLevelSelect.instance ?? new scnLevelSelect());
                        break;
                    }
                case Param.Sfx:
                    {
                        @params.Add(scrSfx.instance ?? new scrSfx());
                        break;
                    }
                case Param.Vfx:
                    {
                        @params.Add(scrVfx.instance ?? new scrVfx());
                        break;
                    }
                case Param.VfxPlus:
                    {
                        @params.Add(scrVfxPlus.instance ?? new scrVfxPlus());
                        break;
                    }
                case Param.SteamIntegration:
                    {
                        @params.Add(SteamIntegration.Instance ?? new SteamIntegration());
                        break;
                    }
                case Param.scnEditor:
                    {
                        @params.Add(scnEditor.instance ?? new scnEditor());
                        break;
                    }
                case Param.scnCLS:
                    {
                        @params.Add(scnCLS.instance ?? new scnCLS());
                        break;
                    }
            }
        }
        public static List<Mod> mods = new List<Mod>();
        public static ModEntry ModEntry { get; set; }
        public static ModEntry.ModLogger Logger { get; set; }
        public static DebuggerSettings Settings { get; set; } = new DebuggerSettings();
        public static bool Load(ModEntry modEntry)
        {
            ModEntry = modEntry;
            Logger = ModEntry.Logger;
            ModEntry.OnToggle = (entry, value) =>
            {
                if (value)
                {
                    foreach (ModEntry me in UnityModManager.modEntries)
                    {
                        Mod mod = new Mod(me);
                        mods.Add(mod);
                    }
                }
                else
                {

                }
                return true;
            };
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = (entry) => Settings.Save(modEntry);
            Settings = UnityModManager.ModSettings.Load<DebuggerSettings>(modEntry);
            return true;
        }
        public static List<object> @params = new List<object>();
        public static bool CustomParameter;
        public static bool MakeNewInstance;
        public static void OnGUI(ModEntry modEntry)
        {
            Settings.LogException = GUILayout.Toggle(Settings.LogException, "Log Exception");
            MakeNewInstance = GUILayout.Toggle(MakeNewInstance, "MakeNewInstance");
            CustomParameter = GUILayout.Toggle(CustomParameter, "CustomParameters");
            if (CustomParameter)
            {
                foreach (Param param in Enum.GetValues(typeof(Param)))
                {
                    GUILayout.BeginHorizontal();
                    if (!(param == Param.ModEntry))
                    {
                        if (GUILayout.Button("Add " + param.ToString()))
                        {
                            AddParam(param);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < UnityModManager.modEntries.Count; i++)
                        {
                            if (GUILayout.Button(UnityModManager.modEntries[i].Info.DisplayName))
                            {
                                @params.Add(UnityModManager.modEntries[i]);
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                foreach (object obj in @params)
                {
                    GUILayout.Label(obj.GetType().Name);
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Parameters"))
                {
                    @params.Clear();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            foreach (Mod mod in mods)
            {
                if (mod.Assembly != null)
                {
                    mod.Gui();
                }
            }
            
        }
    }
    public class DebuggerSettings : UnityModManager.ModSettings
    {
        public override void Save(ModEntry modEntry)
        {
            Save(this, modEntry);
        }
        public bool LogException = false;
    }
}
