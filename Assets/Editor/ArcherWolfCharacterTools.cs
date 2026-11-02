using UnityEditor;
using UnityEngine;

namespace ArcherWolf.EditorTools
{
    public static class ArcherWolfCharacterTools
    {
        const string CharacterFbxPath = "Assets/Art/Models/Player/character.fbx";

        [MenuItem("Archer Wolf/Fix Character Import (Humanoid)")]
        static void FixCharacterImport()
        {
            var importer = AssetImporter.GetAtPath(CharacterFbxPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[Archer Wolf] Could not find ModelImporter at {CharacterFbxPath}");
                return;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            AssetDatabase.WriteImportSettingsIfDirty(CharacterFbxPath);
            AssetDatabase.ImportAsset(CharacterFbxPath, ImportAssetOptions.ForceUpdate);

            var avatar = AssetDatabase.LoadAssetAtPath<Avatar>(CharacterFbxPath);
            if (avatar == null)
            {
                Debug.LogError("[Archer Wolf] Reimport finished but no Avatar asset was produced. Open the FBX's Rig tab and check 'Configure Avatar' manually.");
            }
            else if (!avatar.isValid || !avatar.isHuman)
            {
                Debug.LogWarning("[Archer Wolf] Avatar was created but is not a valid Humanoid avatar. Select the FBX, go to the Rig tab, and click 'Configure Avatar' to fix bone mapping by hand.");
            }
            else
            {
                Debug.Log("[Archer Wolf] character.fbx reimported as a valid Humanoid avatar. Next: select the 'Erika Archer' player object in the Gameplay scene, confirm its Animator's Avatar field points at this new avatar, then rebuild the ragdoll via Invector/Basic Locomotion/Components/Ragdoll.");
            }
        }
    }
}
