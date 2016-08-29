#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Elusive.Utilities
{

    public class AnimationAssistant
    {

        [MenuItem("Assets/Animation/Create Animation Clip")]
        private static void CreateClip()
        {
            Object[] selectedControllers = Selection.GetFiltered(typeof(RuntimeAnimatorController), SelectionMode.Assets);
            
            // Get the first controller from the selected controllers
            RuntimeAnimatorController targetController = null;
            if (selectedControllers.Length > 0)
                targetController = selectedControllers[0] as RuntimeAnimatorController;

            if (targetController == null) return;
            var animationClip = new AnimationClip {name = "New Animation"};

            // Add an animation clip to the controller
            AssetDatabase.AddObjectToAsset(animationClip, targetController);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(targetController));
        }


        [MenuItem("Assets/Animation/Group Animation Clip")]
        private static void AddClip()
        {
            Object[] selectedControllers = Selection.GetFiltered(typeof(RuntimeAnimatorController), SelectionMode.Assets);
            Object[] selectedClips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);

            string s = AssetDatabase.GetAssetPath(Selection.activeObject);
            string parentDirectory = s.Remove(s.LastIndexOf('/'));
            string backupFolderName = "_backup";

            RuntimeAnimatorController targetController = null;
            // Take the first animator controller that is selected. Ignore the rest that might be selected
            if (selectedControllers.Length > 0) targetController = selectedControllers[0] as RuntimeAnimatorController;

            if (targetController == null) return;
            var saveOption = EditorUtility.DisplayDialogComplex(
                "Would you like to keep the original Animation Clip?",
                "You can remove the original Animation Clip, or you can keep a backup copy.",
                "Keep a Copy", "Remove Original Clip", "Cancel");

            // Display dialog returns 2 for cancel. Don't proceed.
            if (saveOption == 2) return;
            foreach (var clip in selectedClips)
            {

                string path = AssetDatabase.GetAssetPath(clip);

                var animationClipCopy = new AnimationClip();

                var animationClip = clip as AnimationClip;
                animationClipCopy.name = animationClip.name;

                AnimationUtility.SetAnimationClipSettings(animationClipCopy, AnimationUtility.GetAnimationClipSettings(animationClip));
                AnimationClipCurveData[] curveData = AnimationUtility.GetAllCurves(animationClip, true);

                foreach (var data in curveData)
                {
                    animationClipCopy.SetCurve(data.path, data.type, data.propertyName, data.curve);
                }

                bool hasBackupDirectory = Directory.Exists(Path.Combine(parentDirectory, backupFolderName));
                string backupDirectory = string.Format("{0}/{1}/{2}", parentDirectory, backupFolderName, clip.name);
                if (!hasBackupDirectory)
                    AssetDatabase.CreateFolder(parentDirectory, backupFolderName);

                switch (saveOption)
                {
                    // Save a Copy
                    case 0:
                        AssetDatabase.AddObjectToAsset(animationClipCopy, targetController);
                        AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(clip), backupDirectory);
                        break;
                    // Remove the original
                    case 1:
                        AssetDatabase.AddObjectToAsset(animationClipCopy, targetController);
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(clip));
                        break;
                }

            }

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(targetController));
        }



        [MenuItem("Assets/Animation/Remove Animation Clip")]
        private static void RemoveClip()
        {
            Object[] selectedAnimationClips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Deep);

            bool remove = EditorUtility.DisplayDialog(
                "Delete the selected clips permanently?",
                "You cannot undo this action",
                "Remove Clip(s)", "Cancel");

            if (!remove) return;

            string mainAsset = string.Empty;
            foreach (var clip in selectedAnimationClips)
            {
                mainAsset = AssetDatabase.GetAssetPath(clip);
                Object.DestroyImmediate(clip, true);
            }

            if (mainAsset != string.Empty) AssetDatabase.ImportAsset(mainAsset);
        }



        [MenuItem("Assets/Animation/Create Animation Clip", true)]
        private static bool ValidateCreateClip()
        {
            Object[] selectedControllers = Selection.GetFiltered(typeof(RuntimeAnimatorController), SelectionMode.Assets);
            foreach (var controller in selectedControllers)
            {
                if (!AssetDatabase.IsMainAsset(controller)) return false;
            }

            return selectedControllers.Length > 0;
        }


        [MenuItem("Assets/Animation/Group Animation Clip", true)]
        private static bool ValidateAddClip()
        {
            Object[] selectedControllers = Selection.GetFiltered(typeof(RuntimeAnimatorController), SelectionMode.Assets);
            Object[] selectedClips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);

            return selectedControllers.Length > 0 && selectedClips.Length > 0;
        }


        [MenuItem("Assets/Animation/Remove Animation Clip", true)]
        private static bool ValidateRemoveClip()
        {
            Object[] selectedClips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Deep);

            foreach (var clip in selectedClips)
            {
                if (!AssetDatabase.IsSubAsset(clip)) return false;
            }

            return selectedClips.Length > 0;
        }


    }

}

#endif