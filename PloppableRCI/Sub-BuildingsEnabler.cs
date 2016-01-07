﻿using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.Packaging;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
<<<<<<< HEAD

// This is Boformers Sub-Building Enabler mod. I've added some code that's commented. Many thanks to him for his work.

=======
/*
Tthis is boformers sub building enabler mod. I just added a few properties to the xml, 
and a few lines of code between the ADDED comments. Many thanks to him for his work. 
*/
>>>>>>> origin/master
namespace PloppableRICO
{
	public class Sub_BuildingsEnabler
	{


		public PloppableResidential thread;

		public List<String[]> Run (List<string[]> BNames)
		{
			var subBuildingsDefParseErrors = new HashSet<string> ();
			var checkedPaths = new List<string> ();

			for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount (); i++) {
				var prefab = PrefabCollection<BuildingInfo>.GetLoaded (i);

				if (prefab == null)
					continue;

				// search for SubBuildingsEnabler.xml
				var asset = PackageManager.FindAssetByName (prefab.name);
				if (asset == null || asset.package == null)
					continue;

				var crpPath = asset.package.packagePath;
				if (crpPath == null)
					continue;

				var subBuildingsDefPath = Path.Combine (Path.GetDirectoryName (crpPath), "PloppableRICODefinition.xml");

				// skip files which were already parsed
				if (checkedPaths.Contains (subBuildingsDefPath))
					continue;
				checkedPaths.Add (subBuildingsDefPath);

				if (!File.Exists (subBuildingsDefPath))
					continue;

				SubBuildingsDefinition subBuildingsDef = null;

				var xmlSerializer = new XmlSerializer (typeof(SubBuildingsDefinition));
				try {
					using (StreamReader streamReader = new System.IO.StreamReader (subBuildingsDefPath)) {
						subBuildingsDef = xmlSerializer.Deserialize (streamReader) as SubBuildingsDefinition;
					}
				} catch (Exception e) {
					Debug.LogException (e);
					subBuildingsDefParseErrors.Add (asset.package.packageName + " - " + e.Message);
					continue;
				}

				if (subBuildingsDef == null || subBuildingsDef.Buildings == null || subBuildingsDef.Buildings.Count == 0) {
					subBuildingsDefParseErrors.Add (asset.package.packageName + " - subBuildingsDef is null or empty.");
					continue;
				}

				foreach (var parentBuildingDef in subBuildingsDef.Buildings) {
					if (parentBuildingDef == null || parentBuildingDef.Name == null) {
						subBuildingsDefParseErrors.Add (asset.package.packageName + " - Building name missing.");
						continue;
					}

					var parentBuildingPrefab = FindPrefab (parentBuildingDef.Name, asset.package.packageName);

					if (parentBuildingPrefab == null) {
						subBuildingsDefParseErrors.Add (asset.package.packageName + " - Building with name " + parentBuildingDef.Name + " not loaded.");
						continue;
					}

					if (parentBuildingDef.SubBuildings == null || parentBuildingDef.SubBuildings.Count == 0) {
						subBuildingsDefParseErrors.Add (asset.package.packageName + " - No sub buildings specified for " + parentBuildingDef.Name + ".");
						continue;
					}

					var subBuildings = new List<BuildingInfo.SubInfo> ();


					/////////////


					Debug.Log (BNames.ToString ());
					//parentBuildingPrefab.m_class = new ItemClass ();
					//parentBuildingPrefab.m_class.m_service = ItemClass.Service.Monument;

					////////////////

					foreach (var subBuildingDef in parentBuildingDef.SubBuildings) {
						if (subBuildingDef == null || subBuildingDef.Name == null) {
							subBuildingsDefParseErrors.Add (parentBuildingDef.Name + " - Sub-building name missing.");
							continue;
						}

						var subBuildingPrefab = FindPrefab (subBuildingDef.Name, asset.package.packageName);

						if (subBuildingPrefab == null) {
							subBuildingsDefParseErrors.Add (parentBuildingDef.Name + " - Sub-building with name " + subBuildingDef.Name + " not loaded.");
							continue;
						}

						//////////////////////////////////This is code that I've added for the Ploppable RICO mod

						string[] NamesType = new string[2];
						NamesType [0] = parentBuildingPrefab.name;
						NamesType [1] = subBuildingDef.type;
						BNames.Add (NamesType);

						if (subBuildingDef.type == "Residential") {
							
							//Assign the custom AI threads to prefab
							subBuildingPrefab.gameObject.AddComponent<PloppableResidential> ();
							subBuildingPrefab.m_buildingAI = subBuildingPrefab.GetComponent<PloppableResidential> ();
							PloppableResidential prefabai = subBuildingPrefab.m_buildingAI as PloppableResidential;

							//Pull settings from XML and assign them to custom AI
							prefabai.m_levelmax = subBuildingDef.levelmax;
							prefabai.m_levelmin = subBuildingDef.levelmin;
							prefabai.m_housemulti = subBuildingDef.multi;
							prefabai.m_constructionTime = 0;

							//Initialzie
							subBuildingPrefab.m_buildingAI.m_info = subBuildingPrefab;
							subBuildingPrefab.m_buildingAI.InitializePrefab ();
							subBuildingPrefab.InitializePrefab ();
							// generate new infos for leveling
							makeInfos (subBuildingPrefab, subBuildingDef.type);
					
	
						}

						if (subBuildingDef.type == "Office") {

	
							subBuildingPrefab.gameObject.AddComponent<PloppableOffice> ();

							subBuildingPrefab.m_buildingAI = subBuildingPrefab.GetComponent<PloppableOffice> ();
							PloppableOffice prefabai = subBuildingPrefab.m_buildingAI as PloppableOffice;

							prefabai.m_housemulti = subBuildingDef.multi;
							prefabai.m_constructionTime = 0;

							subBuildingPrefab.m_buildingAI.m_info = subBuildingPrefab;
							subBuildingPrefab.m_buildingAI.InitializePrefab ();
							subBuildingPrefab.InitializePrefab ();

							makeInfos (subBuildingPrefab, subBuildingDef.type);
						}
							
						/////////////////////////////ADDED


						var subBuilding = new BuildingInfo.SubInfo {
							m_buildingInfo = subBuildingPrefab,
							m_position = new Vector3 (subBuildingDef.PosX, subBuildingDef.PosY, subBuildingDef.PosZ),
							m_angle = subBuildingDef.Angle,
							m_fixedHeight = subBuildingDef.FixedHeight,
						};

						subBuildings.Add (subBuilding);

						// this is usually done in the InitializePrefab method
						if (subBuildingDef.FixedHeight && !parentBuildingPrefab.m_fixedHeight)
							parentBuildingPrefab.m_fixedHeight = true;
					}

					if (subBuildings.Count == 0) {
						subBuildingsDefParseErrors.Add ("No sub buildings specified for " + parentBuildingDef.Name + ".");
						continue;
					}

					parentBuildingPrefab.m_subBuildings = subBuildings.ToArray ();
				}
			}

			if (subBuildingsDefParseErrors.Count > 0) {
				var errorMessage = "Error while parsing sub-building definition file(s). Contact the author of the assets. \n"
				                   + "List of errors:\n";
				foreach (var error in subBuildingsDefParseErrors)
					errorMessage += error + '\n';

				UIView.library.ShowModal<ExceptionPanel> ("ExceptionPanel").SetMessage ("Sub-Buildings Enabler", errorMessage, true);
			}

			return BNames;
		}

		//////////////////////////////////This is code that I've added for the Ploppable RICO mod
	

		public void makeInfos (BuildingInfo Holder, string Type)
		{

			//BNames.Add(Holder.name);
			if (PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_level1") != null) {

				Debug.Log ("Building Info Found and destroyed");
				
			}

			if (PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_level1") == null) {
				
				BuildingInfo Level1 = BuildingInfo.Instantiate (Holder);
				Level1.name = Holder.name + "_Level1";
				this.SetThings (Holder, Level1);

				BuildingInfo Level2 = BuildingInfo.Instantiate (Holder);
				Level2.name = Holder.name + "_Level2";
				this.SetThings (Holder, Level2);

				BuildingInfo Level3 = BuildingInfo.Instantiate (Holder);
				Level3.name = Holder.name + "_Level3";
				this.SetThings (Holder, Level3);

				BuildingInfo Level4 = BuildingInfo.Instantiate (Holder);
				Level4.name = Holder.name + "_Level4";
				this.SetThings (Holder, Level4);

				BuildingInfo Level5 = BuildingInfo.Instantiate (Holder);
				Level5.name = Holder.name + "_Level5";
				this.SetThings (Holder, Level5);

				BuildingInfo[] bray = new BuildingInfo[] { Level1, Level2, Level3, Level4, Level5 };
				string[] stra = new string[] { Level1.name, Level2.name, Level3.name, Level4.name, Level5.name };

				PrefabCollection<BuildingInfo>.InitializePrefabs ("BuildingInfo", bray, stra); //initlaize the instances so they can be referenced by the Buliding objects. 
				PrefabCollection<BuildingInfo>.BindPrefabs ();
				Debug.Log ("Building Info Gnerated");
			}

			if (Type == "Residential") {
	
				if (ItemClassCollection.FindClass (Holder.name + "_cLevel1") != null) {

					ItemClass[] cray = new ItemClass[] { 
					
						ItemClassCollection.FindClass (Holder.name + "_cLevel1"),
						ItemClassCollection.FindClass (Holder.name + "_cLevel2"),
						ItemClassCollection.FindClass (Holder.name + "_cLevel3"),
						ItemClassCollection.FindClass (Holder.name + "_cLevel4"),
						ItemClassCollection.FindClass (Holder.name + "_cLevel5")
					};

					ItemClassCollection.DestroyClasses (cray);
					Debug.Log ("cLevel Found and destroyed");
				}
				


				ItemClass cLevel1 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel1.name = Holder.name + "_cLevel1";
				cLevel1.m_service = ItemClass.Service.Residential;
				cLevel1.m_subService = ItemClass.SubService.ResidentialHigh;
				cLevel1.m_level = ItemClass.Level.Level1;

				ItemClass cLevel2 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel2.name = Holder.name + "_cLevel2";
				cLevel2.m_service = ItemClass.Service.Residential;
				cLevel2.m_subService = ItemClass.SubService.ResidentialHigh;
				cLevel2.m_level = ItemClass.Level.Level2;

				ItemClass cLevel3 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel3.name = Holder.name + "_cLevel3";
				cLevel3.m_service = ItemClass.Service.Residential;
				cLevel3.m_subService = ItemClass.SubService.ResidentialHigh;
				cLevel3.m_level = ItemClass.Level.Level3;

				ItemClass cLevel4 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel4.name = Holder.name + "_cLevel4";
				cLevel4.m_service = ItemClass.Service.Residential;
				cLevel4.m_subService = ItemClass.SubService.ResidentialHigh;
				cLevel4.m_level = ItemClass.Level.Level4;

				ItemClass cLevel5 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel5.name = Holder.name + "_cLevel5";
				cLevel5.m_service = ItemClass.Service.Residential;
				cLevel5.m_subService = ItemClass.SubService.ResidentialHigh;
				cLevel5.m_level = ItemClass.Level.Level5;


				ItemClass[] ccray = new ItemClass[] { cLevel1, cLevel2, cLevel3, cLevel4, cLevel5 };

				ItemClassCollection.InitializeClasses (ccray);

				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level1").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel1");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel2");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel3");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level4").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel4");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level5").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel5");
			}

			if (Type == "Office") {

				if (ItemClassCollection.FindClass (Holder.name + "_cLevel1") != null) {

					ItemClass[] cray = new ItemClass[] { 

						ItemClassCollection.FindClass (Holder.name + "_cLevel1"),
						ItemClassCollection.FindClass (Holder.name + "_cLevel2"),
						ItemClassCollection.FindClass (Holder.name + "_cLevel3"),
					};

					ItemClassCollection.DestroyClasses (cray);
					Debug.Log ("cLevel Found and destroyed");
				}

				ItemClass cLevel1 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel1.name = Holder.name + "_cLevel1";
				cLevel1.m_service = ItemClass.Service.Office;
	
				cLevel1.m_level = ItemClass.Level.Level1;

				ItemClass cLevel2 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel2.name = Holder.name + "_cLevel2";
				cLevel2.m_service = ItemClass.Service.Office;
				cLevel2.m_level = ItemClass.Level.Level2;

				ItemClass cLevel3 = ScriptableObject.CreateInstance<ItemClass> ();
				cLevel3.name = Holder.name + "_cLevel3";
				cLevel3.m_service = ItemClass.Service.Office;
				cLevel3.m_level = ItemClass.Level.Level3;

				ItemClass[] ccray = new ItemClass[] { cLevel1, cLevel2, cLevel3 };

				ItemClassCollection.InitializeClasses (ccray);

				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level1").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel1");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level2").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel2");
				PrefabCollection<BuildingInfo>.FindLoaded (Holder.name + "_Level3").m_class = ItemClassCollection.FindClass (Holder.name + "_cLevel3");
			}


			//}

			//return BNames;
		}

		public void SetThings (BuildingInfo original, BuildingInfo newone)
		{

			//This changes some settings in the newly instanceated BuildingInfos. 

			newone.m_buildingAI = original.m_buildingAI;
			newone.m_AssetEditorTemplate = false;
			newone.m_prefabInitialized = false;
			newone.m_instanceChanged = true;
			newone.m_autoRemove = false;
		}

		/////////////////////////////ADDED
			

		private BuildingInfo FindPrefab (string prefabName, string packageName)
		{
			var prefab = PrefabCollection<BuildingInfo>.FindLoaded (prefabName);
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (prefabName + "_Data");
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (PathEscaper.Escape (prefabName) + "_Data");
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (packageName + "." + prefabName + "_Data");
			if (prefab == null)
				prefab = PrefabCollection<BuildingInfo>.FindLoaded (packageName + "." + PathEscaper.Escape (prefabName) + "_Data");

			return prefab;
		}
	}

	public class SubBuildingsDefinition
	{
		public List<Building> Buildings { get; set; }

		public SubBuildingsDefinition ()
		{
			Buildings = new List<Building> ();
		}

		public class Building
		{
			[XmlAttribute ("name"), DefaultValue (null)]
			public string Name { get; set; }

			public List<SubBuilding> SubBuildings { get; set; }

			public Building ()
			{
				SubBuildings = new List<SubBuilding> ();
			}
		}

		public class SubBuilding
		{
			[XmlAttribute ("name"), DefaultValue (null)]
			public string Name { get; set; }

			[XmlAttribute ("pos-x"), DefaultValue (0f)]
			public float PosX { get; set; }

			[XmlAttribute ("pos-y"), DefaultValue (0f)]
			public float PosY { get; set; }

			[XmlAttribute ("pos-z"), DefaultValue (0f)]
			public float PosZ { get; set; }

			[XmlAttribute ("angle"), DefaultValue (0f)]
			public float Angle { get; set; }

			[XmlAttribute ("fixed-height"), DefaultValue (true)]
			public bool FixedHeight { get; set; }

			//////////////////////////////////These are properties that I've added for the Ploppable RICO mod

			[XmlAttribute ("type"), DefaultValue ("dummy")]
			public string type { get; set; }

			[XmlAttribute ("multi"), DefaultValue (0)]
			public int multi { get; set; }

			[XmlAttribute ("levelmin"), DefaultValue (1)]
			public int levelmin { get; set; }

			[XmlAttribute ("levelmax"), DefaultValue (1)]
			public int levelmax { get; set; }

			//////////////////////////////////ADDED

			public SubBuilding ()
			{
				FixedHeight = true;
			}
		}
	}
	
}



