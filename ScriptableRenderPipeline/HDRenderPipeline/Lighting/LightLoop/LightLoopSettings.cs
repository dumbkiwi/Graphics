using System;
using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [Serializable]
    public class LightLoopSettings
    {
        // Setup by the users
        public bool enableTileAndCluster;
        public bool enableComputeLightEvaluation;
        public bool enableComputeLightVariants;
        public bool enableComputeMaterialVariants;
        // Deferred opaque always use FPTL, forward opaque can use FPTL or cluster, transparent always use cluster
        // When MSAA is enabled, we only support cluster (Fptl is too slow with MSAA), and we don't support MSAA for deferred path (mean it is ok to keep fptl)
        public bool enableFptlForForwardOpaque;
        public bool enableBigTilePrepass;

        // Setup by system
        public bool isFptlEnabled;

        public LightLoopSettings()
        {
            enableTileAndCluster = true;
            enableComputeLightEvaluation = true;
            enableComputeLightVariants = true;
            enableComputeMaterialVariants = true;

            enableFptlForForwardOpaque = true;
            enableBigTilePrepass = true;

            isFptlEnabled = true;
        }

        // aggregateFrameSettings already contain the aggregation of RenderPipelineSettings and FrameSettings (regular and/or debug)
        static public LightLoopSettings InitializeLightLoopSettings(Camera camera, FrameSettings aggregateFrameSettings,
                                                                    GlobalFrameSettings globalFrameSettings, FrameSettings frameSettings)
        {
            LightLoopSettings aggregate = new LightLoopSettings();

            aggregate.enableTileAndCluster          = frameSettings.lightLoopSettings.enableTileAndCluster;
            aggregate.enableComputeLightEvaluation  = frameSettings.lightLoopSettings.enableComputeLightEvaluation;
            aggregate.enableComputeLightVariants    = frameSettings.lightLoopSettings.enableComputeLightVariants;
            aggregate.enableComputeMaterialVariants = frameSettings.lightLoopSettings.enableComputeMaterialVariants;
            aggregate.enableFptlForForwardOpaque    = frameSettings.lightLoopSettings.enableFptlForForwardOpaque;
            aggregate.enableBigTilePrepass          = frameSettings.lightLoopSettings.enableBigTilePrepass;

            // Deferred opaque are always using Fptl. Forward opaque can use Fptl or Cluster, transparent use cluster.
            // When MSAA is enabled we disable Fptl as it become expensive compare to cluster
            // In HD, MSAA is only supported for forward only rendering, no MSAA in deferred mode (for code complexity reasons)
            aggregate.enableFptlForForwardOpaque = aggregate.enableFptlForForwardOpaque && aggregateFrameSettings.renderSettings.enableMSAA;
            // If Deferred, enable Fptl. If we are forward renderer only and not using Fptl for forward opaque, disable Fptl
            aggregate.isFptlEnabled = !aggregateFrameSettings.renderSettings.enableForwardRenderingOnly || aggregate.enableFptlForForwardOpaque;

            return aggregate;
        }
    }
}
