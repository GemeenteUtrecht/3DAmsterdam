﻿using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Amsterdam3D.Rendering
{
    public class RenderSettings : MonoBehaviour
    {
        private bool antiAliasing = true;
        private bool postEffects = true;

        [SerializeField]
        private Volume[] postProcessingVolumes;

        [SerializeField]
        private GameObject[] postEffectGameObjects;

        /// <summary>
        /// Toggles antialiasing on or off.
        /// </summary>
        /// <param name="antiAliasingOn"></param>
        public void ToggleAA(bool antiAliasingOn)
        {
            antiAliasing = antiAliasingOn;

            UniversalAdditionalCameraData universalCameraData = CameraModeChanger.Instance.ActiveCamera.GetComponent<UniversalAdditionalCameraData>();
            
            //Enables or disables antialiasing on the camera, depending on what kind of camera we use.
            if (universalCameraData)
            {
                universalCameraData.antialiasing = (antiAliasing) ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None;
            }
            else
            {
                QualitySettings.antiAliasing = (antiAliasing) ? 2 : 0;
            }

            SetPostProcessing();
        }


        public void TogglePostEffects(bool effectsOn)
        {
            postEffects = effectsOn;

            foreach(Volume volume in postProcessingVolumes)
                volume.enabled = effectsOn;

            foreach (GameObject gameObject in postEffectGameObjects)
                gameObject.SetActive(false);

            SetPostProcessing();
        }

        private void SetPostProcessing()
        {
            //Post processing can be disabled entirely if there are no AA or effects enabled
            UniversalAdditionalCameraData universalCameraData = CameraModeChanger.Instance.ActiveCamera.GetComponent<UniversalAdditionalCameraData>();
            universalCameraData.renderPostProcessing = (antiAliasing || postEffects);
        }

        public void SetShadowQuality(float shadowQuality)
        {
            //TODO: Probably need to swap render profiles here..Directly changing the res. as a value is not in URP yet.
            var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            //urp.mainLightShadowmapResolution = calculatehere; //This is only a getter now. Its on the unity backlog to be able to be set as well
        }

        public void SetRenderScale(float renderScale)
        {
            var urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            urp.renderScale = renderScale;
        }
    }
}