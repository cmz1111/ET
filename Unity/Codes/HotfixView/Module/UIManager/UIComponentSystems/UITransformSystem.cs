﻿using UnityEngine;
namespace ET
{
    [UISystem]
    public class UITransformOnCreateSystem: OnCreateSystem<UITransform,Transform>
    {
        public override void OnCreate(UITransform self,Transform transform)
        {
            self.__transform = transform;
        }
    }
    
    public static class UITransformSystem
    {
        public static Transform GetTransform(this UIBaseContainer self)
        {
            return self.GetUIComponent<UITransform>("").transform;
        }
        
        public static GameObject GetGameObject(this UIBaseContainer self)
        {
            return self.GetUIComponent<UITransform>("").transform.gameObject;
        }
    }
}