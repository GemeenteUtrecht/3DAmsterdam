﻿using Netherlands3D.Cameras;
using Netherlands3D.Help;
using Netherlands3D.Interface;
using Netherlands3D.ObjectInteraction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Masking
{
    public class MaskDome : Interactable
    {
        private RuntimeMask runtimeMask;
        void Start()
        {
            runtimeMask = GetComponent<RuntimeMask>();
        }

		private void OnEnable()
		{
            HelpMessage.Instance.Show("Beweeg uw muis over het maaiveld om er doorheen te kijken");
            TakeInteractionPriority();
		}
		private void OnDisable()
		{
            StopInteraction();
        }

		public override void Escape()
		{
			base.Escape();
            gameObject.SetActive(false);
		}

		void Update()
        {
            MoveWithPointer();
            runtimeMask.ApplyNewPositionAndScale();
        }

        private void MoveWithPointer()
        {
            if (Selector.Instance.HoveringInterface())
            {
                transform.transform.localScale = Vector3.zero;
                return;
            }

            transform.position = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
            transform.transform.localScale = Vector3.one * runtimeMask.MaskScaleMultiplier * CameraModeChanger.Instance.ActiveCamera.transform.position.y;
        }
    }
}