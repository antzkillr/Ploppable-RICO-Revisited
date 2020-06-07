﻿using System.Runtime.CompilerServices;
using UnityEngine;
using HarmonyLib;


namespace PloppableRICO
{
    /// <summary>
    /// Harmony patch and reverse patch to catch exceptions when no valid monuments are avaliable (presumably because they've all been converted to Ploppable RICO buildings and/or skipped by LSM prefab skipping).
    /// </summary>
    [HarmonyPatch(typeof(UnlockingPanel))]
    [HarmonyPatch("RefreshMonumentsPanel")]
    internal static class RefreshMonumentsPanelPatch
    {
        /// <summary>
        /// Simple Prefix patch to catch Monuments panel setup exceptions.
        /// All we do is call (via reverse patch) the original method and painlessly catch any exceptions.
        /// </summary>
        /// <param name="__instance">Harmony original instance reference</param>
        /// <returns></returns>
        private static bool Prefix(UnlockingPanel __instance)
        {
            try
            {
                RefreshMonumentsPanelRev(__instance);
            }
            catch
            {
                Debug.Log("RICO Revisited: caught monuments panel exception - no valid monuments?");
            }

            // Don't call base method after this.
            return false;
        }


        /// <summary>
        /// Harmony reverse patch to access original private method.
        /// </summary>
        /// <param name="instance">Harmony original instance reference</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(UnlockingPanel)), "RefreshMonumentsPanel")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RefreshMonumentsPanelRev(object instance)
        {
            string message = "Ploppable RICO Revisited: RefreshMonumentsPanel reverse Harmony patch wasn't applied.";
            Debug.Log(message);
        }
    }
}