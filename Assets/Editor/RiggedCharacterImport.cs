#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Afterrust.EditorTools
{
    /// <summary>
    /// Rigged characters (Meshy/Mixamo) placed in any "Characters/.../Rigged/" folder are
    /// imported as Humanoid. Humanoid retargeting ignores per-bone translation keys, which is
    /// what makes limbs stretch like rubber when a Generic clip from another skeleton plays.
    /// Applied only on first import so later manual Inspector tweaks are preserved.
    /// </summary>
    public sealed class RiggedCharacterImport : AssetPostprocessor
    {
        public static bool IsRiggedCharacterPath(string path)
        {
            string p = path.Replace('\\', '/');
            return p.Contains("/Characters/") && p.Contains("/Rigged/") &&
                   p.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);
        }

        private void OnPreprocessModel()
        {
            if (!assetImporter.importSettingsMissing || !IsRiggedCharacterPath(assetPath)) return;
            ApplyModelSettings((ModelImporter)assetImporter);
        }

        private void OnPreprocessAnimation()
        {
            if (!assetImporter.importSettingsMissing || !IsRiggedCharacterPath(assetPath)) return;
            ApplyClipSettings((ModelImporter)assetImporter);
        }

        /// <returns>true when a setting changed.</returns>
        public static bool ApplyModelSettings(ModelImporter importer)
        {
            bool changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                changed = true;
            }
            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                changed = true;
            }
            if (!importer.importAnimation)
            {
                importer.importAnimation = true;
                changed = true;
            }
            return changed;
        }

        /// <summary>Loop locomotion clips and bake root motion into the pose (in-place walk).</summary>
        public static bool ApplyClipSettings(ModelImporter importer)
        {
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0) return false;

            bool changed = false;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                string n = (clip.name + " " + importer.assetPath).ToLowerInvariant();
                bool locomotion = n.Contains("walk") || n.Contains("idle") || n.Contains("run");
                if (!locomotion) continue;
                if (!clip.loopTime || !clip.lockRootRotation || !clip.lockRootHeightY ||
                    !clip.lockRootPositionXZ) changed = true;
                clip.loopTime = true;
                clip.loopPose = true;
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionY = true;
                clip.keepOriginalPositionXZ = true;
            }
            if (changed) importer.clipAnimations = clips;
            return changed;
        }
    }
}
#endif
