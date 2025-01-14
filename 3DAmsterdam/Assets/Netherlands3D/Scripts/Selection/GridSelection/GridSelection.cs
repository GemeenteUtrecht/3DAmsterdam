﻿using Netherlands3D.Cameras;
using Netherlands3D.InputHandler;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Netherlands3D.Interface.SidePanel;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Netherlands3D.Interface
{
	public class GridSelection : Interactable
	{
		[SerializeField]
		private GameObject gridSelectionBlock;

		private Vector3 gridBlockPosition;
		private Vector3Int mouseGridPosition;

		private IAction toggleAction;
		private IAction drawAction;
		private IAction clearAction;

		private bool drawing = false;
		private bool add = true;

		private GameObject scaleBlock;
		private Dictionary<Vector3Int, GameObject> voxels;
		private int maxVoxels = 200;

		[SerializeField]
		private bool freePaint = false;
		private Vector3Int startGridPosition;

		[System.Serializable]
		public class BoundsEvent : UnityEvent<Bounds> { };
		public BoundsEvent onGridSelected;

		private void Awake()
		{
			ActionMap = ActionHandler.actions.GridSelection;

			toggleAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.ToggleVoxel);
			toggleAction.SubscribePerformed(Toggle);

			drawAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.DrawVoxels);
			drawAction.SubscribePerformed(Drawing);
			drawAction.SubscribeCancelled(Drawing);

			if (freePaint)
			{
				clearAction = ActionHandler.instance.GetAction(ActionHandler.actions.GridSelection.EraseVoxels);
				clearAction.SubscribePerformed(Clear);
				clearAction.SubscribeCancelled(Clear);
			}

			voxels = new Dictionary<Vector3Int, GameObject>();
		}

		/// <summary>
		/// Fresh start for the grid selection tool with optional material override (to have a unique block color)
		/// </summary>
		/// <param name="toolMaterial">Optional material override for the selection blocks</param>
		public void StartSelection( Material toolMaterial)
		{
			if(toolMaterial)
			{
				SetMainMaterial(toolMaterial);
			}

			onGridSelected.RemoveAllListeners();
			gameObject.SetActive(true);
			//Fresh start, clear a previous selection block visual
			if (scaleBlock) Destroy(scaleBlock);
		}

		void Start()
		{
			this.transform.position = new Vector3(0, Config.activeConfiguration.zeroGroundLevelY, 0);
			SetGridSize();
		}
		private void Toggle(IAction action)
		{
			if (action.Performed)
			{

				if (Selector.Instance.HoveringInterface()) return;

				if (freePaint)
				{
					DrawVoxelsUnderMouse(true);
				}
				else
				{
					startGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
					ScaleSingleVoxelUnderMouse(false);
				}

				FinishSelection();
			}
		}

		private void Drawing(IAction action)
		{
			if (action.Cancelled)
			{
				drawing = false;
				FinishSelection();
			}
			else if (!Selector.Instance.HoveringInterface() && action.Performed)
			{
				drawing = true;
				add = true;
				if (!freePaint)
				{
					startGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
				}
			}
		}
		private void Clear(IAction action)
		{
			if (action.Cancelled)
			{
				drawing = false;
			}
			else if (action.Performed)
			{
				drawing = true;
				add = false;
			}
		}

		private void SetMainMaterial(Material material)
		{
			gridSelectionBlock.GetComponent<MeshRenderer>().sharedMaterial = material;
			if(scaleBlock)
				scaleBlock.GetComponent<MeshRenderer>().sharedMaterial = material;
		}

		private void OnEnable()
		{
			VisualGrid.Instance.Show();
			TakeInteractionPriority();
		}
		private void OnDisable()
		{
			VisualGrid.Instance.Hide();
			StopInteraction();
		}

		public override void Escape()
		{
			base.Escape();
			gameObject.SetActive(false);
		}

		private void Update()
		{
			if (Selector.Instance.HoveringInterface())
			{
				gridSelectionBlock.SetActive(false);
			}
			else
			{
				gridSelectionBlock.SetActive(true);
				MoveSelectionBlock();
				if (drawing)
				{
					if (freePaint)
					{
						DrawVoxelsUnderMouse();
					}
					else
					{
						ScaleSingleVoxelUnderMouse();
					}				
				}
			}
		}

		private void DrawVoxelsUnderMouse(bool toggled = false)
		{
			if (Selector.Instance.HoveringInterface()) return;
			
			mouseGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
			if (!voxels.ContainsKey(mouseGridPosition) && add && voxels.Count < maxVoxels)
			{
				voxels.Add(mouseGridPosition, Instantiate(gridSelectionBlock, new Vector3(mouseGridPosition.x, mouseGridPosition.y, mouseGridPosition.z), Quaternion.identity, gridSelectionBlock.transform.parent));
			}
			else if ((toggled || !add) && voxels.ContainsKey(mouseGridPosition))
			{
				Destroy(voxels[mouseGridPosition]);
				voxels.Remove(mouseGridPosition);
			}
		}

		private void ScaleSingleVoxelUnderMouse(bool calculateScale = true)
		{
			if (Selector.Instance.HoveringInterface()) return;

			//Just make sure there is one voxel that we can scale
			if (!scaleBlock)
			{
				voxels.Clear();
				voxels.Add(mouseGridPosition, Instantiate(gridSelectionBlock, new Vector3(mouseGridPosition.x, mouseGridPosition.y, mouseGridPosition.z), Quaternion.identity, gridSelectionBlock.transform.parent));
				scaleBlock = voxels.First().Value;
			}
			
			mouseGridPosition = GetGridPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());		
			scaleBlock.transform.position = mouseGridPosition;

			if (calculateScale)
			{
				var xDifference = (mouseGridPosition.x - startGridPosition.x);
				var zDifference = (mouseGridPosition.z - startGridPosition.z);

				scaleBlock.transform.position = startGridPosition;
				scaleBlock.transform.Translate(xDifference / 2.0f, 0, zDifference / 2.0f);
				scaleBlock.transform.localScale = new Vector3(
						(mouseGridPosition.x - startGridPosition.x) + ((xDifference < 0 ) ? -VisualGrid.Instance.CellSize : VisualGrid.Instance.CellSize),
						VisualGrid.Instance.CellSize,
						(mouseGridPosition.z - startGridPosition.z) + ((zDifference < 0) ? -VisualGrid.Instance.CellSize : VisualGrid.Instance.CellSize)
				);
			}
			else{
				//Just make sure it is default size
				scaleBlock.transform.localScale = Vector3.one * VisualGrid.Instance.CellSize;
			}
		}

		private Vector3Int GetGridPosition(Vector3 samplePosition)
		{
			samplePosition.x += (VisualGrid.Instance.CellSize * 0.5f);
			samplePosition.z += (VisualGrid.Instance.CellSize * 0.5f);

			samplePosition.x = (Mathf.Round(samplePosition.x / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);
			samplePosition.z = (Mathf.Round(samplePosition.z / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);

			Vector3Int roundedPosition = new Vector3Int
			{
				x = Mathf.RoundToInt(samplePosition.x),
				y = Mathf.RoundToInt(Config.activeConfiguration.zeroGroundLevelY + (VisualGrid.Instance.CellSize * 0.5f)),
				z = Mathf.RoundToInt(samplePosition.z)
			};

			return roundedPosition;
		}

		private void SetGridSize()
		{
			gridSelectionBlock.transform.localScale = new Vector3(VisualGrid.Instance.CellSize, VisualGrid.Instance.CellSize, VisualGrid.Instance.CellSize);
		}

		private void MoveSelectionBlock()
		{
			gridBlockPosition = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
			//Offset to make up for grid object origin (centered)
			gridBlockPosition.x += (VisualGrid.Instance.CellSize * 0.5f);
			gridBlockPosition.z += (VisualGrid.Instance.CellSize * 0.5f);

			//Snap block to grid
			gridBlockPosition.x = (Mathf.Round(gridBlockPosition.x / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);
			gridBlockPosition.z = (Mathf.Round(gridBlockPosition.z / VisualGrid.Instance.CellSize) * VisualGrid.Instance.CellSize) - (VisualGrid.Instance.CellSize * 0.5f);

			gridSelectionBlock.transform.position = gridBlockPosition;
			gridSelectionBlock.transform.Translate(Vector3.up * (VisualGrid.Instance.CellSize * 0.5f));
		}

		public void RenderGridToThumbnail()
		{
			//Lets render a ortographic thumbnail for a proper grid topdown view
			gridSelectionBlock.SetActive(false);
			PropertiesPanel.Instance.RenderThumbnailContaining(
				scaleBlock.GetComponent<MeshRenderer>().bounds,
				PropertiesPanel.ThumbnailRenderMethod.ORTOGRAPHIC,
				scaleBlock.GetComponent<MeshRenderer>().bounds.center + Vector3.up * 150.0f
			);
			gridSelectionBlock.SetActive(true);
		}

		private void FinishSelection()
		{
			if(scaleBlock)
				onGridSelected.Invoke(scaleBlock.GetComponent<MeshRenderer>().bounds);
		}
	}
}