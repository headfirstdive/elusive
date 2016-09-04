#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Elusive.Utilities.AnimationController
{
    public class MecanimConstants
    {

        private const string FOLDER_LOCATION = "scripts/auto-generated/";
        private const string NAMESPACE = "Constants";
        private static bool SHOW_SUCCESS_MESSAGE = true;
        private const string MECANIM_STATES_FILE_NAME = "MecanimStates.cs";
        private const string DIGIT_PREFIX = "k";


        [MenuItem("Assets/Mecanim/Generate Mecanim Constants...")]
        private static void RebuildMecanimConstantsMenuItem()
        {
            RebuildMecanimClasses();
        }


        public static void RebuildMecanimClasses()
        {
            var folderPath = string.Format("{0}/{1}", Application.dataPath, FOLDER_LOCATION);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, MECANIM_STATES_FILE_NAME);
            string assetPath = Path.Combine("Assets", string.Format("{0}/{1}", FOLDER_LOCATION, MECANIM_STATES_FILE_NAME));

            File.WriteAllText(filePath, getMecanimStateClassContent(MECANIM_STATES_FILE_NAME).Replace(".cs", string.Empty));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Debug.Log("ConstantsGeneratorKit complete. Constants classes built to " + FOLDER_LOCATION);
        }


        private class MecanimState
        {
            public List<string> stateBools = new List<string>();
            public List<string> stateTriggers = new List<string>();
            public StateLayer stateLayer = new StateLayer();
        }


        private class StateLayer
        {
            public List<string> layer = new List<string>();
            public List<int> layerIndex = new List<int>();
            public List<string> states = new List<string>();
            public List<int> statesHash = new List<int>();
        }

        private static string getMecanimStateClassContent(string className)
        {
            var output = "";
            output += "// This class is auto-generated do not modify\n";
            output += "namespace " + NAMESPACE + "\n";
            output += "{\n";
            output += "\n";

            string[] assets = AssetDatabase.GetAllAssetPaths();

            GetMecanimControllersList(assets, ref output);


            foreach (var asset in assets)
            {
                if (asset.EndsWith(".controller"))
                {
                    var s = asset.Remove(0, asset.LastIndexOf("/") + 1);
                    className = s.Replace(" ", string.Empty);
                    className = className.Replace(".controller", string.Empty);
                    output += "\tpublic static class " + className + "\n";
                    output += "\t{\n";

                    MecanimState stateMachine = GetAnimationControllerStateParameters(asset);

                    // bool enums
                    GetMecanimControllerBoolEnumList(stateMachine, ref output);

                    // trigger enums
                    GetMecanimControllerTriggerEnumList(stateMachine, ref output);

                    // States enums
                    GetMecanimControllerStateEnumList(stateMachine, ref output);

                    // Parameters
                    GetMecanimControllerParameters(stateMachine, ref output);

                    // States
                    GetMecanimControllerStates(stateMachine, ref output);

                    // State hashes
                    GetMecanimControllerStateHashes(stateMachine, ref output);

                    // Layers
                    GetMecanimControllerLayers(stateMachine, ref output);

                    output += "\t}\n";
                    output += "\n";
                    output += "\n";
                }
            }
            output += "}";

            return output;
        }


        private static void GetMecanimControllersList(string[] assets, ref string output)
        {
            output += "\tpublic static class MecanimControllers\n";
            output += "\t{\n";

            output += "\t\tpublic enum Controller\n";
            output += "\t\t{\n";
            output += "\t\t\tNone,\n";

            int assetCount = 0;
            foreach (var controller in assets)
            {
                if (controller.EndsWith(".controller"))
                {
                    var s = controller.Remove(0, controller.LastIndexOf("/") + 1);
                    string controllerClass = s.Replace(" ", string.Empty);
                    controllerClass = controllerClass.Replace(".controller", string.Empty);
                    if (assetCount < assets.Length - 1) output += "\t\t\t" + controllerClass + ",\n";
                    else output += "\t\t\t" + controllerClass + "\n";
                    assetCount++;
                }
            }

            output += "\t\t}\n";
            output += "\t}\n";
            output += "\n";
        }


        private static void GetMecanimControllerBoolEnumList(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateBools.Count != 0)
            {
                output += "\n";
                output += "\t\t//bools enum list\n";

                output += "\t\t" + "public enum Bool\n";
                output += "\t\t{\n";
                output += "\t\t\tNone,\n";
                for (var i = 0; i < stateMachine.stateBools.Count; i++)
                {
                    output += "\t\t\t[StringValue(" + '"' + stateMachine.stateBools[i] + '"' + ")] ";
                    string pString = i < stateMachine.stateBools.Count - 1 ? "," : "";
                    output += ToCamelCase(stateMachine.stateBools[i]) + pString + "\n";
                }
                output += "\t\t}\n";
            }
        }


        private static void GetMecanimControllerTriggerEnumList(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateTriggers.Count != 0)
            {
                output += "\n";
                output += "\t\t//triggers enum list\n";

                output += "\t\t" + "public enum Trigger\n";
                output += "\t\t{\n";
                output += "\t\t\tNone,\n";
                for (var i = 0; i < stateMachine.stateTriggers.Count; i++)
                {
                    output += "\t\t\t[StringValue(" + '"' + stateMachine.stateTriggers[i] + '"' + ")] ";
                    string tString = i < stateMachine.stateTriggers.Count - 1 ? "," : "";
                    output += ToCamelCase(stateMachine.stateTriggers[i]) + tString + "\n";
                }
                output += "\t\t}\n";
            }
        }

        private static void GetMecanimControllerStateEnumList(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateTriggers.Count != 0)
            {
                output += "\n";
                output += "\t\t//states enum list\n";

                output += "\t\t" + "public enum State\n";
                output += "\t\t{\n";
                output += "\t\t\tNone,\n";

                for (int i = 0; i < stateMachine.stateLayer.states.Count; i++)
                {
                    string tString = i < stateMachine.stateLayer.states.Count - 1 ? "," : "";
                    output += "\t\t\t" + ToCamelCase(stateMachine.stateLayer.states[i]) + tString + "\n";
                }
                output += "\t\t}\n";
            }
        }


        private static void GetMecanimControllerParameters(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateBools.Count > 0 || stateMachine.stateTriggers.Count > 0)
            {
                output += "\n";
                output += "\t\t//parameters\n";
                // bools
                for (var i = 0; i < stateMachine.stateBools.Count; i++)
                {
                    output += "\t\t" + "public const string b" + ToCamelCase(stateMachine.stateBools[i]) + " = " + '"' + stateMachine.stateBools[i] + '"' + ";" + "\n";
                }

                //output += "\n";

                // triggers
                for (var i = 0; i < stateMachine.stateTriggers.Count; i++)
                {
                    output += "\t\t" + "public const string t" + ToCamelCase(stateMachine.stateTriggers[i]) + " = " + '"' + stateMachine.stateTriggers[i] + '"' + ";" + "\n";
                }
            }
        }


        private static void GetMecanimControllerLayers(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateLayer.layer.Count != 0)
            {
                output += "\n";
                output += "\t\t//layers\n";

                for (int i = 0; i < stateMachine.stateLayer.layer.Count; i++)
                {
                    output += "\t\t" + "public const string " + ToCamelCase(stateMachine.stateLayer.layer[i]) + " = " + '"' + stateMachine.stateLayer.layer[i] + '"' + ";" + "\n";
                    output += "\t\t" + "public const int " + ToCamelCase(stateMachine.stateLayer.layer[i] + "Index") + " = " + stateMachine.stateLayer.layerIndex[i] + ";" + "\n";
                }
            }
        }


        private static void GetMecanimControllerStates(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateLayer.states.Count != 0)
            {
                output += "\n";
                output += "\t\t//states\n";

                for (int i = 0; i < stateMachine.stateLayer.states.Count; i++)
                {
                    output += "\t\t" + "public const string " + ToCamelCase(stateMachine.stateLayer.states[i]) + " = " + '"' + stateMachine.stateLayer.states[i] + '"' + ";" + "\n";
                }
            }
        }


        private static void GetMecanimControllerStateHashes(MecanimState stateMachine, ref string output)
        {
            if (stateMachine.stateLayer.states.Count != 0)
            {
                output += "\n";
                output += "\t\t//state hashes\n";

                for (int i = 0; i < stateMachine.stateLayer.states.Count; i++)
                {
                    output += "\t\t" + "public const int " + ToCamelCase(stateMachine.stateLayer.states[i] + "Hash") + " = " + stateMachine.stateLayer.statesHash[i] + ";" + "\n";
                }
            }
        }


        private static MecanimState GetAnimationControllerStateParameters(string path)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEditor.Animations.AnimatorController)) as UnityEditor.Animations.AnimatorController;
            AnimatorControllerParameter[] parameters = controller.parameters;
            AnimatorControllerLayer[] layers = controller.layers;

            MecanimState stateMachine = new MecanimState();
            foreach (var parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    stateMachine.stateBools.Add(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Trigger)
                    stateMachine.stateTriggers.Add(parameter.name);
            }

            int index = 0;
            foreach (var layer in layers)
            {
                stateMachine.stateLayer.layer.Add(layer.name);
                stateMachine.stateLayer.layerIndex.Add(index);

                index++;
                foreach (var state in layer.stateMachine.states)
                {
                    stateMachine.stateLayer.states.Add(state.state.name);
                    stateMachine.stateLayer.statesHash.Add(state.state.nameHash);
                }

                foreach (var machine in layer.stateMachine.stateMachines)
                {
                    foreach (var state in machine.stateMachine.states)
                    {
                        stateMachine.stateLayer.states.Add(state.state.name);
                        stateMachine.stateLayer.statesHash.Add(state.state.nameHash);
                    }

                }
            }

            return stateMachine;
        }


        private static string ToCamelCase(string input)
        {
            input = input.Replace(" ", "");

            if (char.IsLower(input[0]))
                input = char.ToUpper(input[0]) + input.Substring(1);

            // uppercase letters before dash or underline
            Func<char, int, string> func = (x, i) => {
                if (x == '-' || x == '_')
                    return "";

                if (i > 0 && (input[i - 1] == '-' || input[i - 1] == '_'))
                    return x.ToString().ToUpper();

                return x.ToString();
            };
            input = string.Concat(input.Select(func).ToArray());

            // digits are a no-no so stick prefix in front
            if (char.IsDigit(input[0]))
                return DIGIT_PREFIX + input;
            return input;
        }
    }

    
//#if !DISABLE_AUTO_GENERATION
//// this post processor listens for changes to the TagManager and automatically rebuilds all classes if it sees a change
//public class ConstandsGeneratorPostProcessor : AssetPostprocessor
//{
//    // for some reason, OnPostprocessAllAssets often gets called multiple times in a row. This helps guard against rebuilding classes
//    // when not necessary.
//    static DateTime? _lastTagsAndLayersBuildTime;
//    static DateTime? _lastScenesBuildTime;


//    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//    {
//        var resourcesDidChange = importedAssets.Any(s => Regex.IsMatch(s, @"/Resources/.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

//        if (!resourcesDidChange)
//            resourcesDidChange = movedAssets.Any(s => Regex.IsMatch(s, @"/Resources/.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

//        if (!resourcesDidChange)
//            resourcesDidChange = deletedAssets.Any(s => Regex.IsMatch(s, @"/Resources/.*", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

//        if (resourcesDidChange)
//            ConstantsGeneratorKit.rebuildConstantsClasses(true, false, false);

//        //for (int i = 0; i < importedAssets.Length; i++)
//        //{
//        //	Debug.Log(importedAssets[i]);
//        //}

//        // layers and tags changes
//        if (importedAssets.Contains("ProjectSettings/TagManager.asset"))
//        {
//            if (!_lastTagsAndLayersBuildTime.HasValue || _lastTagsAndLayersBuildTime.Value.AddSeconds(5) < DateTime.Now)
//            {
//                _lastTagsAndLayersBuildTime = DateTime.Now;
//                ConstantsGeneratorKit.rebuildConstantsClasses(false, false);
//            }
//        }


//        // scene changes
//        if (importedAssets.Contains("ProjectSettings/EditorBuildSettings.asset"))
//        {
//            if (!_lastScenesBuildTime.HasValue || _lastScenesBuildTime.Value.AddSeconds(5) < DateTime.Now)
//            {
//                _lastScenesBuildTime = DateTime.Now;
//                ConstantsGeneratorKit.rebuildConstantsClasses(false, true);
//            }
//        }
//    }
//#endif
}
#endif
