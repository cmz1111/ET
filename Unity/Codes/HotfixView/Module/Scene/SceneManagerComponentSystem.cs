﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{

    [ObjectSystem]
    public class SceneManagerComponentAwakeSystem : AwakeSystem<SceneManagerComponent>
    {
        Dictionary<string,SceneConfig> GetSceneConfig()
        {
            SceneConfig LoadingScene = new SceneConfig
            {
                SceneAddress = "Scenes/LoadingScene/Loading.unity",
                Name = SceneNames.Loading,
            };
            SceneConfig LoginScene = new SceneConfig
            {
                SceneAddress = "Scenes/LoginScene/Login.unity",
                Name = SceneNames.Login,
            };
            SceneConfig Map1Scene = new SceneConfig
            {
                SceneAddress = "Scenes/MapScene/Map1.unity",
                Name = SceneNames.Map1,
            };
            SceneConfig Map2Scene = new SceneConfig
            {
                SceneAddress = "Scenes/MapScene/Map2.unity",
                Name = SceneNames.Map2,
            };
            var res = new Dictionary<string, SceneConfig>();
            res.Add(LoadingScene.Name, LoadingScene);
            res.Add(Map1Scene.Name, Map1Scene);
            res.Add(Map2Scene.Name, Map2Scene);
            res.Add(LoginScene.Name, LoginScene);
            return res;
        }

        
        public override void Awake(SceneManagerComponent self)
        {
            self.ScenesChangeIgnoreClean = new List<string>();
            self.DestroyWindowExceptNames = new List<string>();
            SceneManagerComponent.Instance = self;
            self.SceneConfigs = GetSceneConfig();
            self.current_scene = SceneNames.None;
        }
    }

    [ObjectSystem]
    public class SceneManagerComponentDestroySystem : DestroySystem<SceneManagerComponent>
    {
        public override void Destroy(SceneManagerComponent self)
        {
            self.ScenesChangeIgnoreClean = null;
            self.DestroyWindowExceptNames = null;
            self.SceneConfigs = null;
            SceneManagerComponent.Instance = null;
        }
    }
    //--[[
    //-- 场景管理系统：调度和控制场景异步加载以及进度管理，展示loading界面和更新进度条数据，GC、卸载未使用资源等
    //-- 注意：
    //-- 1、资源预加载放各个场景类中自行控制
    //-- 2、场景loading的UI窗口这里统一管理，由于这个窗口很简单，更新进度数据时直接写Model层
    //--]]
    [FriendClass(typeof(SceneManagerComponent))]
    public static class SceneManagerComponentSystem
    {
        
        //切换场景
        async static ETTask InnerSwitchScene(this SceneManagerComponent self,SceneConfig scene_config,bool needclean = false,SceneLoadComponent slc = null)
        {
            float slid_value = 0;
            Log.Info("InnerSwitchScene start open uiloading");
            //打开loading界面
            await Game.EventSystem.PublishAsync(new UIEventType.LoadingBegin());
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });

            CameraManagerComponent.Instance.SetCameraStackAtLoadingStart();

            //等待资源管理器加载任务结束，否则很多Unity版本在切场景时会有异常，甚至在真机上crash
            Log.Info("InnerSwitchScene ProsessRunning Done ");
            while (ResourcesComponent.Instance.IsProsessRunning())
            {
                await TimerComponent.Instance.WaitAsync(1);
            }
            //清理旧场景

            slid_value += 0.01f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            await TimerComponent.Instance.WaitAsync(1);

            //清理UI
            Log.Info("InnerSwitchScene Clean UI");
            await UIManagerComponent.Instance.DestroyWindowExceptNames(self.DestroyWindowExceptNames.ToArray());
            
            slid_value += 0.01f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            //清除ImageLoaderManager里的资源缓存 这里考虑到我们是单场景
            Log.Info("InnerSwitchScene ImageLoaderManager Cleanup");
            ImageLoaderComponent.Instance.Clear();
            //清除预设以及其创建出来的gameobject, 这里不能清除loading的资源
            Log.Info("InnerSwitchScene GameObjectPool Cleanup");
            string[] cleanup_besides_path = self.ScenesChangeIgnoreClean.ToArray();
            if (needclean)
            {
                GameObjectPoolComponent.Instance.Cleanup(true, cleanup_besides_path);
                slid_value += 0.01f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                //清除除loading外的资源缓存 
                List<UnityEngine.Object> gos = new List<UnityEngine.Object>();
                foreach (var path in cleanup_besides_path)
                {
                    var go = GameObjectPoolComponent.Instance.GetCachedGoWithPath(path);
                    if (go != null)
                    {
                        gos.Add(go);
                    }
                }
                Log.Info("InnerSwitchScene ResourcesManager ClearAssetsCache excludeAssetLen = " + gos.Count);
                ResourcesComponent.Instance.ClearAssetsCache(gos.ToArray());
                slid_value += 0.01f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            }
            else
            {
                slid_value += 0.02f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            }
            await ResourcesComponent.Instance.LoadSceneAsync(self.GetSceneConfigByName(SceneNames.Loading).SceneAddress, false);
            Log.Info("LoadSceneAsync Over");
            slid_value += 0.01f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            //GC：交替重复2次，清干净一点
            GC.Collect();
            GC.Collect();

            var res = Resources.UnloadUnusedAssets();
            while (!res.isDone)
            {
                await TimerComponent.Instance.WaitAsync(1);
            }
            slid_value += 0.1f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            Log.Info("初始化目标场景 Start");
            //初始化目标场景
            

            slid_value += 0.02f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            Log.Info("异步加载目标场景 Start");
            //异步加载目标场景
            await ResourcesComponent.Instance.LoadSceneAsync(scene_config.SceneAddress, false);

            slid_value += 0.65f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            //准备工作：预加载资源等
            if (slc != null)
            {
                await slc.OnPrepare((progress) =>
                {
                    Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value + 0.15f * progress });
                    if (progress > 1) Log.Error("scene load waht's the fuck!");
                });
            }

            slid_value += 0.15f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            CameraManagerComponent.Instance.SetCameraStackAtLoadingDone();
            self.current_scene = scene_config.Name;

            slid_value = 1;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            //等久点，跳的太快
            await TimerComponent.Instance.WaitAsync(500);
            //加载完成，关闭loading界面
            await Game.EventSystem.PublishAsync(new UIEventType.LoadingFinish());
            //释放loading界面引用的资源
            GameObjectPoolComponent.Instance.CleanupWithPathArray(true, cleanup_besides_path);
            self.busing = false;

        }
        //切换场景
        public static async ETTask SwitchScene(this SceneManagerComponent self, SceneConfig scene_config,bool needclean = false,SceneLoadComponent slc = null)
        {
            if (self.busing) return;
            if (scene_config==null) return;
            if (self.current_scene == scene_config.Name)
                return;
            self.busing = true;
            await self.InnerSwitchScene(scene_config,needclean,slc);
        }
        //切换场景
        public static async ETTask SwitchScene(this SceneManagerComponent self, string scene_name, bool needclean = false,SceneLoadComponent slc = null)
        {
            if (self.busing) return;
            var scene_config = self.GetSceneConfigByName(scene_name);
            if (scene_config == null) return;
            if (self.current_scene == scene_config.Name)
                return;
            self.busing = true;
            await self.InnerSwitchScene(scene_config,needclean,slc);
        }

        public static string GetCurrentSceneName(this SceneManagerComponent self)
        {
            return self.current_scene;
        }

        public static bool IsInTargetScene(this SceneManagerComponent self,SceneConfig scene_config)
        {
            return self.current_scene == scene_config.Name;
        }

        public static SceneConfig GetSceneConfigByName(this SceneManagerComponent self, string name)
        {
            if (self.SceneConfigs.TryGetValue(name, out var res))
            {
                return res;
            }
            return null;
        }
        
    }
}
