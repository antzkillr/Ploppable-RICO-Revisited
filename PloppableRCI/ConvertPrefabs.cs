using System;


namespace PloppableRICO
{
    /// <summary>
    ///This class assigns the RICO settings to the prefabs. 
    /// </summary>
    internal class ConvertPrefabs
    {
        /// <summary>
        /// Interpret and apply RICO settings to a building prefab.
        /// </summary>
        /// <param name="buildingData">RICO building data to apply</param>
        /// <param name="prefab">The building prefab to be changed</param>
        internal void ConvertPrefab(RICOBuilding buildingData, BuildingInfo prefab)
        {
            if (prefab != null)
            {
                // Check eligibility for any growable assets.
                if (buildingData.growable)
                {
                    // Growables can't have any dimension greater than 4.
                    if (prefab.GetWidth() > 4 || prefab.GetLength() > 4)
                    {
                        buildingData.growable = false;
                        Debugging.Message("building '" + prefab.name + "' can't be growable because it is too big");
                    }

                    // Growables can't have net structures.
                    if (prefab.m_paths != null && prefab.m_paths.Length != 0)
                    {
                        buildingData.growable = false;
                        Debugging.Message("building '" + prefab.name + "' can't be growable because it contains network assets");
                    }
                }

                if (buildingData.service == "dummy")
                {
                    var ai = prefab.gameObject.AddComponent<DummyBuildingAI>();

                    // Use beautification ItemClass to avoid issues, and never make growable.
                    InitializePrefab(prefab, ai, "Beautification Item", false);

                    // Final circular reference.
                    prefab.m_buildingAI.m_info = prefab;
                }
                else if (buildingData.service == "residential")
                {
                    var ai = buildingData.growable ? prefab.gameObject.AddComponent<GrowableResidentialAI>() : prefab.gameObject.AddComponent<PloppableResidentialAI>();
                    if (ai == null) throw (new Exception("Residential-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_homeCount = buildingData.homeCount;

                    if (buildingData.subService == "low eco")
                    {
                        // Apply eco service if GC installed, otherwise use normal low residential.
                        if (Util.isGCinstalled())
                        {
                            InitializePrefab(prefab, ai, "Low Residential Eco - Level" + buildingData.level, buildingData.growable);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "Low Residential - Level" + buildingData.level, buildingData.growable);
                        }
                    }
                    else if (buildingData.subService == "high eco")
                    {
                        // Apply eco service if GC installed, otherwise use normal high residential.
                        if (Util.isGCinstalled())
                        {
                            InitializePrefab(prefab, ai, "High Residential Eco - Level" + buildingData.level, buildingData.growable);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "High Residential - Level" + buildingData.level, buildingData.growable);
                        }
                    }
                    else if (buildingData.subService == "high")
                    {
                        // Stock standard high commercial.
                        InitializePrefab(prefab, ai, "High Residential - Level" + buildingData.level, buildingData.growable);
                    }
                    else
                    {
                        // Fall back to low residential as default.
                        InitializePrefab(prefab, ai, "Low Residential - Level" + buildingData.level, buildingData.growable);

                        // If invalid subservice, report.
                        if (buildingData.subService != "low")
                        {
                            Debugging.ErrorBuffer.AppendLine("Residential building " + buildingData.name + " has invalid subservice " + buildingData.subService + "; reverting to low residential.");
                        }
                    }
                }
                else if (buildingData.service == "office")
                {
                    var ai = buildingData.growable ? prefab.gameObject.AddComponent<GrowableOfficeAI>() : prefab.gameObject.AddComponent<PloppableOfficeAI>();
                    if (ai == null) throw (new Exception("Office-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;

                    if (buildingData.subService == "high tech")
                    {
                        // Apply IT cluster if GC installed, otherwise use Level 3 office.
                        if (Util.isGCinstalled())
                        {
                            InitializePrefab(prefab, ai, "Office - Hightech", buildingData.growable);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "Office - Level3", buildingData.growable);
                        }
                    }
                    else
                    {
                        // Not IT cluster - boring old ordinary office.
                        InitializePrefab(prefab, ai, "Office - Level" + buildingData.level, buildingData.growable);
                    }
                }
                else if (buildingData.service == "industrial")
                {
                    var ai = buildingData.growable ? prefab.gameObject.AddComponent<GrowableIndustrialAI>() : prefab.gameObject.AddComponent<PloppableIndustrialAI>();
                    if (ai == null) throw (new Exception("Industrial-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_pollutionEnabled = buildingData.pollutionEnabled;

                    if (Util.industryServices.Contains(buildingData.subService))
                        InitializePrefab(prefab, ai, Util.ucFirst(buildingData.subService) + " - Processing", buildingData.growable);
                    else
                        InitializePrefab(prefab, ai, "Industrial - Level" + buildingData.level, buildingData.growable);
                }
                else if (buildingData.service == "extractor")
                {
                    var ai = buildingData.growable ? prefab.gameObject.AddComponent<GrowableExtractorAI>() : prefab.gameObject.AddComponent<PloppableExtractorAI>();
                    if (ai == null) throw (new Exception("Extractor-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;
                    ai.m_pollutionEnabled = buildingData.pollutionEnabled;

                    if (Util.industryServices.Contains(buildingData.subService))
                        InitializePrefab(prefab, ai, Util.ucFirst(buildingData.subService) + " - Extractor", buildingData.growable);
                }

                else if (buildingData.service == "commercial")
                {
                    var ai = buildingData.growable ? prefab.gameObject.AddComponent<GrowableCommercialAI>() : prefab.gameObject.AddComponent<PloppableCommercialAI>();
                    if (ai == null) throw (new Exception("Commercial-AI not found."));

                    ai.m_ricoData = buildingData;
                    ai.m_workplaceCount = buildingData.workplaceCount;
                    ai.m_constructionCost = buildingData.constructionCost;

                    if (buildingData.subService == "eco")
                    {
                        // Apply eco specialisation if GC installed, otherwise use Level 1 low commercial.
                        if (Util.isGCinstalled())
                        {
                            // Eco commercial buildings only import food goods.
                            ai.m_incomingResource = TransferManager.TransferReason.Food;
                            InitializePrefab(prefab, ai, "Eco Commercial", buildingData.growable);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "Low Commercial - Level1", buildingData.growable);
                        }
                    }
                    else if (buildingData.subService == "tourist")
                    {
                        // Apply tourist specialisation if AD installed, otherwise use Level 1 low commercial.
                        if (Util.isADinstalled())
                        {
                            InitializePrefab(prefab, ai, "Tourist Commercial - Land", buildingData.growable);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "Low Commercial - Level1", buildingData.growable);
                        }
                    }
                    else if (buildingData.subService == "leisure")
                    {
                        // Apply leisure specialisation if AD installed, otherwise use Level 1 low commercial.
                        if (Util.isADinstalled())
                        {
                            InitializePrefab(prefab, ai, "Leisure Commercial", buildingData.growable);
                        }
                        else
                        {
                            InitializePrefab(prefab, ai, "Low Commercial - Level1", buildingData.growable);
                        }
                    }
                    else if (buildingData.subService == "high")
                    {
                        // Bog standard high commercial.
                        InitializePrefab(prefab, ai, "High Commercial - Level" + buildingData.level, buildingData.growable);
                    }
                    else
                    {
                        // Fall back to low commercial as default.
                        InitializePrefab(prefab, ai, "Low Commercial - Level" + buildingData.level, buildingData.growable);

                        // If invalid subservice, report.
                        if (buildingData.subService != "low")
                        {
                            Debugging.ErrorBuffer.AppendLine("Commercial building " + buildingData.name + " has invalid subService " + buildingData.subService + "; reverting to low commercial.");
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Applies settings to a BuildingInfo prefab.
        /// </summary>
        /// <param name="prefab">The prefab to modify.</param>
        /// <param name="ai">The building AI to apply.</param>
        /// <param name="aiClass">The AI class string to apply.</param>
        /// <param name="growable">Whether the prefab should be growable.</param>
        private void InitializePrefab(BuildingInfo prefab, BuildingAI ai, String aiClass, bool growable)
        {
            // Non-zero construction time important for other mods (Real Time, Real Construction) - only for private building AIs.
            if (ai is PrivateBuildingAI)
            {
                ((PrivateBuildingAI)ai).m_constructionTime = 30;
            }

            prefab.m_buildingAI = ai;
            prefab.m_buildingAI.m_info = prefab;
            prefab.m_class = ItemClassCollection.FindClass(aiClass);
            prefab.m_placementStyle = growable ? ItemClass.Placement.Automatic : ItemClass.Placement.Manual;
            prefab.m_autoRemove = false;

        }
    }
}