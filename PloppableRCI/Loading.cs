using System.IO;
using System.Collections.Generic;
using ICities;
using UnityEngine;
using ColossalFramework.UI;


namespace PloppableRICO
{
    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public class Loading : LoadingExtensionBase
    {
        // Internal instances.
        public GameObject RICODataManager;
        public static RICOPrefabManager xmlManager;
        public static ConvertPrefabs convertPrefabs;

        // RICO definitions.
        public static PloppableRICODefinition localRicoDef;
        public static PloppableRICODefinition mod1RicoDef;
        public static PloppableRICODefinition mod2RicoDef;

        // Internal flags.
        private static bool isModEnabled;


        /// <summary>
        /// Called by the game when the mod is initialised at the start of the loading process.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnCreated(ILoading loading)
        {
            // Don't do anything if not in game (e.g. if we're going into an editor).
            if (loading.currentMode != AppMode.Game)
            {
                isModEnabled = false;
                Debug.Log("RICO Revisited: not loading into game, skipping activation.");
            }
            else
            {
                // Check for conflicting (and other) mods.
                isModEnabled = ModUtils.CheckMods();
            }

            // If we're not enabling the mod due to one of the above checks failing, unapply Harmony patches before returning without doing anything.
            if (!isModEnabled)
            {
                Patcher.UnpatchAll();
                return;
            }

            // Otherwise, game on!
            Debug.Log("RICO Revisited v" + PloppableRICOMod.version + " loading.");
            
            // Create instances if they don't already exist.
            if (convertPrefabs == null)
            {
                convertPrefabs = new ConvertPrefabs();
            }

            if (xmlManager == null)
            {
                xmlManager = new RICOPrefabManager
                {
                    prefabHash = new Dictionary<BuildingInfo, BuildingData>(),
                    prefabList = new List<BuildingData>()
                };
            }

            // Read any local RICO settings.
            string ricoDefPath = "LocalRICOSettings.xml";
            localRicoDef = null;

            if (!File.Exists(ricoDefPath))
            {
                Debug.Log("RICO Revisited: no " + ricoDefPath + " file found.");
            }
            else
            {
                localRicoDef = RICOReader.ParseRICODefinition("", ricoDefPath, insanityOK: true);

                if (localRicoDef == null)
                {
                    Debug.Log("RICO Revisited: no valid definitions in " + ricoDefPath);
                }
            }

            base.OnCreated(loading);
        }


        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Alert the user to any mod conflicts.
            ModUtils.NotifyConflict();

            // Don't do anything further if mod hasn't activated (conflicting mod detected, or loading into editor instead of game).
            if (!isModEnabled)
            {
                return;
            }

            base.OnLevelLoaded(mode);

            // Don't do anything if in asset editor.
            if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
                return;

            // Init GUI.
            PloppableTool.Initialize();
            RICOSettingsPanel.Initialize();

            // Add buttons to access building details from zoned building info panels.
            RICOSettingsPanel.instance.AddInfoPanelButtons();

            // Report any loading errors.
            Debugging.ReportErrors();

            Debug.Log("RICO Revisited: loading complete.");
        }
    }
}