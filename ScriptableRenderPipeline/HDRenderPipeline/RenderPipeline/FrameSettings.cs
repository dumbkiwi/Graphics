﻿using System;
using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [Serializable]
    public class FrameSettings
    {
        // The settings here are per frame settings.
        // Each camera must have its own per frame settings
        [Serializable]
        public class LightingSettings
        {
            // Setup by users
            public bool enableShadow = true;
            public bool enableSSR = true; // Depends on DepthPyramid
            public bool enableSSAO = true;
            public bool enableSSSAndTransmission = true;

            // Setup by system
            public float diffuseGlobalDimmer = 1.0f;
            public float specularGlobalDimmer = 1.0f;
        }

        [Serializable]
        public class RenderSettings
        {
            // Setup by users
            public bool enableForwardRenderingOnly = false; // TODO: Currently there is no way to strip the extra forward shaders generated by the shaders compiler, so we can switch dynamically.
            public bool enableDepthPrepassWithDeferredRendering = false;
            public bool enableAlphaTestOnlyInDeferredPrepass = false;

            public bool enableTransparentPrePass = true;
            public bool enableMotionVectors = true;
            public bool enableDBuffer = true;
            public bool enableAtmosphericScattering = true;
            public bool enableRoughRefraction = true; // Depends on DepthPyramid - If not enable, just do a copy of the scene color (?) - how to disable rough refraction ?
            public bool enableTransparentPostPass = true;
            public bool enableDistortion = true;
            public bool enablePostprocess = true;


            public bool enableStereo = false;
            public bool enableAsyncCompute = false;

            public bool enableOpaqueObjects = true;
            public bool enableTransparentObjects = true;

            public bool enableMSAA = false;

            // Setup by system
            public bool enableMaterialDisplayDebug = false;
            public bool enableShadowMask = false;
        }

        public LightingSettings lightingSettings = new LightingSettings();
        public RenderSettings renderSettings = new RenderSettings();
        public LightLoopSettings lightLoopSettings = new LightLoopSettings();

        // Init a FrameSettings from renderpipeline settings, frame settings and debug settings (if any)
        // This will aggregate the various option
        static public FrameSettings InitializeFrameSettings(Camera camera, GlobalFrameSettings globalFrameSettings, FrameSettings frameSettings)
        {
            FrameSettings aggregate = new FrameSettings();

            // When rendering reflection probe we disable specular as it is view dependent
            if (camera.cameraType == CameraType.Reflection)
            {
                aggregate.lightingSettings.diffuseGlobalDimmer  = 1.0f;
                aggregate.lightingSettings.specularGlobalDimmer = 0.0f;
            }
            else
            {
                aggregate.lightingSettings.diffuseGlobalDimmer  = 1.0f;
                aggregate.lightingSettings.specularGlobalDimmer = 1.0f;
            }

            aggregate.lightingSettings.enableShadow                             = frameSettings.lightingSettings.enableShadow;
            aggregate.lightingSettings.enableSSR                                = frameSettings.lightingSettings.enableSSR && globalFrameSettings.lightingSettings.supportSSR;
            aggregate.lightingSettings.enableSSAO                               = frameSettings.lightingSettings.enableSSAO && globalFrameSettings.lightingSettings.supportSSAO;
            aggregate.lightingSettings.enableSSSAndTransmission                 = frameSettings.lightingSettings.enableSSSAndTransmission && globalFrameSettings.lightingSettings.supportSSSAndTransmission;

            // We have to fall back to forward-only rendering when scene view is using wireframe rendering mode
            // as rendering everything in wireframe + deferred do not play well together
            aggregate.renderSettings.enableForwardRenderingOnly                 = frameSettings.renderSettings.enableForwardRenderingOnly || GL.wireframe;
            aggregate.renderSettings.enableDepthPrepassWithDeferredRendering    = frameSettings.renderSettings.enableDepthPrepassWithDeferredRendering;
            aggregate.renderSettings.enableAlphaTestOnlyInDeferredPrepass       = frameSettings.renderSettings.enableAlphaTestOnlyInDeferredPrepass;

            aggregate.renderSettings.enableTransparentPrePass                   = frameSettings.renderSettings.enableTransparentPrePass;
            aggregate.renderSettings.enableMotionVectors                        = frameSettings.renderSettings.enableMotionVectors;
            aggregate.renderSettings.enableDBuffer                              = frameSettings.renderSettings.enableDBuffer && globalFrameSettings.renderSettings.supportDBuffer;
            aggregate.renderSettings.enableAtmosphericScattering                = frameSettings.renderSettings.enableAtmosphericScattering;
            aggregate.renderSettings.enableRoughRefraction                      = frameSettings.renderSettings.enableRoughRefraction;
            aggregate.renderSettings.enableTransparentPostPass                  = frameSettings.renderSettings.enableTransparentPostPass;
            aggregate.renderSettings.enableDistortion                           = frameSettings.renderSettings.enableDistortion;
            aggregate.renderSettings.enablePostprocess                          = frameSettings.renderSettings.enablePostprocess;

            aggregate.renderSettings.enableStereo                               = frameSettings.renderSettings.enableStereo && UnityEngine.XR.XRSettings.isDeviceActive && (camera.stereoTargetEye == StereoTargetEyeMask.Both);
            // Force forward if we request stereo. TODO: We should not enforce that, users should be able to chose deferred
            aggregate.renderSettings.enableForwardRenderingOnly                 = aggregate.renderSettings.enableForwardRenderingOnly || aggregate.renderSettings.enableStereo;

            aggregate.renderSettings.enableAsyncCompute                         = frameSettings.renderSettings.enableAsyncCompute;

            aggregate.renderSettings.enableOpaqueObjects                        = frameSettings.renderSettings.enableOpaqueObjects;
            aggregate.renderSettings.enableTransparentObjects                   = frameSettings.renderSettings.enableTransparentObjects;

            aggregate.renderSettings.enableMSAA                                 = frameSettings.renderSettings.enableMSAA && globalFrameSettings.renderSettings.supportMSAA;

            aggregate.renderSettings.enableMaterialDisplayDebug                 = false;
            aggregate.renderSettings.enableShadowMask                           = globalFrameSettings.lightingSettings.supportShadowMask;

            aggregate.lightLoopSettings = LightLoopSettings.InitializeLightLoopSettings(camera, aggregate, globalFrameSettings, frameSettings);

            return aggregate;
        }
    }
}
