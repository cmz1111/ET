﻿using IFix.Editor;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class BuildHelper
    {
        private const string relativeDirPrefix = "../Release";

        public static string BuildFolder = "../Release/{0}/StreamingAssets/";

        static Dictionary<PlatformType, BuildTarget> buildmap = new Dictionary<PlatformType, BuildTarget>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTarget.Android },
            { PlatformType.PC , BuildTarget.StandaloneWindows64 },
            { PlatformType.IOS , BuildTarget.Android },
            { PlatformType.MacOS , BuildTarget.StandaloneOSX },
        };

        static Dictionary<PlatformType, BuildTargetGroup> buildGroupmap = new Dictionary<PlatformType, BuildTargetGroup>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTargetGroup.Android },
            { PlatformType.PC , BuildTargetGroup.Standalone },
            { PlatformType.IOS , BuildTargetGroup.iOS },
            { PlatformType.MacOS , BuildTargetGroup.Standalone },
        };
        private static void KeystoreSetting()
        {
            PlayerSettings.Android.keystoreName = "ET.keystore";
            PlayerSettings.Android.keyaliasName = "et";
            PlayerSettings.keyaliasPass = "123456";
            PlayerSettings.keystorePass = "123456";
        }

        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder,bool isInject)
        {
            EditorUserSettings.SetConfigValue(AddressableTools.is_packing, "1");
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildHandle(type, buildOptions, isBuildExe,clearFolder, isInject);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate ()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildHandle(type, buildOptions, isBuildExe, clearFolder, isInject);
                    }
                };
                if(buildGroupmap.TryGetValue(type,out var group))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, buildmap[type]);
                }
                else
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildmap[type]);
                }
               
            }
        }
        public static void HandleProject()
        {
            
            //清除图集
            //AltasHelper.ClearAllAtlas();

            AASUtility.CleanPlayerContent();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //生成图集
            // AltasHelper.GeneratingAtlas();

            //Marked AssetsPackage Addressable
            AddressableTools.RunCheckAssetBundle();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Build Default Build Script
            AddressableTools.BuildPlayerContent();

        }
        static void BuildHandle(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder,bool isInject)
        {
            
            
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "ET";
            string exeName = programName;
            switch (type)
            {
                case PlatformType.PC:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    IFixEditor.Patch();
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    buildTarget = BuildTarget.Android;
                    exeName += ".apk";
                    IFixEditor.CompileToAndroid();
                    break;
                case PlatformType.IOS:
                    buildTarget = BuildTarget.iOS;
                    IFixEditor.CompileToIOS();
                    break;
                case PlatformType.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    IFixEditor.Patch();
                    break;
            }
            if (isInject)
            {
                //Inject
                IFixEditor.InjectAssemblys();
            }
            HandleProject();
            //string fold = string.Format(BuildFolder, type);

            if (clearFolder && Directory.Exists(relativeDirPrefix))
            {
                Directory.Delete(relativeDirPrefix, true);
                Directory.CreateDirectory(relativeDirPrefix);
            }
            else
            {
                Directory.CreateDirectory(relativeDirPrefix);
            }

            if (isBuildExe)
            {
                AssetDatabase.Refresh();
                string[] levels = {
                    "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
                };
                UnityEngine.Debug.Log("开始EXE打包");
                BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
                UnityEngine.Debug.Log("完成exe打包");
            }
        }
    }
}
