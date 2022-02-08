using UnityEngine;
using System;
namespace ET
{
    public class AppStart_Init : AEvent<EventType.AppStart>
    {
        protected override async ETTask Run(EventType.AppStart args)
        {
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();
            Game.Scene.AddComponent<ResourcesComponent>();
            Game.Scene.AddComponent<MaterialComponent>();
            Game.Scene.AddComponent<ImageLoaderComponent>();
            Game.Scene.AddComponent<ImageOnlineComponent>();
            Game.Scene.AddComponent<GameObjectPoolComponent>();
            Game.Scene.AddComponent<UIManagerComponent>();
            Game.Scene.AddComponent<CameraManagerComponent>();
            Game.Scene.AddComponent<SceneManagerComponent>();
            Game.Scene.AddComponent<ToastComponent>();
            
            // 加载配置
            Game.Scene.AddComponent<ConfigComponent>();
            ConfigComponent.Instance.Load();
            
            Game.Scene.AddComponent<I18NComponent>();//多语言系统
            Game.Scene.AddComponent<RedDotComponent>();//红点系统
            Game.Scene.AddComponent<ServerConfigManagerComponent>();
            
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            
            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<SessionStreamDispatcher>();
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();
            
            Game.Scene.AddComponent<GlobalComponent>();
            Game.Scene.AddComponent<AIDispatcherComponent>();
            await UIManagerComponent.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
            if(Define.Networked||Define.ForceUpdate) 
                    //下方代码会初始化Addressables,手机关闭网络等情况访问不到cdn的时候,会卡10s左右。todo:游戏启动时在mono层检查网络
                await UIManagerComponent.Instance.OpenWindow<UIUpdateView,Action>(UIUpdateView.PrefabPath,StartGame);//下载热更资源
            else
                StartGame();
        }
        
        public void StartGame()
        {
            Scene zoneScene = SceneFactory.CreateZoneScene(1, "Game", Game.Scene);

            Game.EventSystem.Publish(new EventType.AppStartInitFinish() { ZoneScene = zoneScene });
        }
    }
}
