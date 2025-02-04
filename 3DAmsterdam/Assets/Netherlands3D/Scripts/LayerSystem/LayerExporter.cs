﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Interface.SidePanel;


namespace Netherlands3D.Interface
{
	public class LayerExporter : MonoBehaviour
	{
		private string selectedExportFormat = "";

		[SerializeField]
		List<LayerSystem.Layer> selectableLayers;

		private bool[] exportLayerToggles = new bool[4] { true, true, true, true };

		private Bounds exportBounds;

		public GridSelection gridSelection;

		public void WaitForGridBounds()
		{
			//Make sure you only subscribe once
			gridSelection.onGridSelected.AddListener(SetBounds);
		}

		public void SetBounds(Bounds gridBounds)
		{
			exportBounds = gridBounds;
			DisplayUI();
		}

		private void DisplayUI()
		{
			//TODO: send this boundingbox to the mesh selection logic, and draw the sidepanel
			PropertiesPanel.Instance.OpenObjectInformation("Grid selectie", true, 10);

			gridSelection.RenderGridToThumbnail();

			PropertiesPanel.Instance.AddTitle("Lagen");
			PropertiesPanel.Instance.AddActionCheckbox("Gebouwen", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer0Toggle", 1)), (action) =>
			{
				exportLayerToggles[0] = action;
				PlayerPrefs.SetInt("exportLayer0Toggle", Convert.ToInt32(exportLayerToggles[0]));
			});
			PropertiesPanel.Instance.AddActionCheckbox("Bomen", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer1Toggle", 1)), (action) =>
			{
				exportLayerToggles[1] = action;
				PlayerPrefs.SetInt("exportLayer1Toggle", Convert.ToInt32(exportLayerToggles[1]));
			});
			PropertiesPanel.Instance.AddActionCheckbox("Maaiveld", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer2Toggle", 1)), (action) =>
			{
				exportLayerToggles[2] = action;
				PlayerPrefs.SetInt("exportLayer2Toggle", Convert.ToInt32(exportLayerToggles[2]));
			});
			PropertiesPanel.Instance.AddActionCheckbox("Ondergrond", Convert.ToBoolean(PlayerPrefs.GetInt("exportLayer3Toggle", 1)), (action) =>
			{
				exportLayerToggles[3] = action;
				PlayerPrefs.SetInt("exportLayer3Toggle", Convert.ToInt32(exportLayerToggles[3]));
			});

			var exportFormats = new string[] { "AutoCAD DXF (.dxf)", "Collada DAE (.dae)" };
			selectedExportFormat = PlayerPrefs.GetString("exportFormat", exportFormats[0]);
			PropertiesPanel.Instance.AddActionDropdown(exportFormats, (action) =>
			{
				selectedExportFormat = action;
				PlayerPrefs.SetString("exportFormat", action);

			}, PlayerPrefs.GetString("exportFormat", exportFormats[0]));

			PropertiesPanel.Instance.AddLabel("Pas Op! bij een selectie van meer dan 16 tegels is het mogelijk dat uw browser niet genoeg geheugen heeft en crasht");

			PropertiesPanel.Instance.AddActionButtonBig("Downloaden", (action) =>
			{
				List<LayerSystem.Layer> selectedLayers = new List<LayerSystem.Layer>();
				for (int i = 0; i < selectableLayers.Count; i++)
				{
					if (exportLayerToggles[i])
					{
						selectedLayers.Add(selectableLayers[i]);
					}
				}
				print(selectedExportFormat);
				switch (selectedExportFormat)
				{
					case "AutoCAD DXF (.dxf)":
						Debug.Log("Start building DXF");
						GetComponent<DXFCreation>().CreateDXF(exportBounds, selectedLayers);
						break;
					case "Collada DAE (.dae)":
						Debug.Log("Start building collada");
						GetComponent<ColladaCreation>().CreateCollada(exportBounds, selectedLayers);
						break;
					default:
						WarningDialogs.Instance.ShowNewDialog("Exporteer " + selectedExportFormat + " nog niet geactiveerd.");
						break;
				}
			});
		}
	}
}