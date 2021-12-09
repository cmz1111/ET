﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public static class UIBaseContainerSystem
    {
        public static Dictionary<string, Dictionary<Type, Entity>> GetCompoennts(this Entity self)
        {
            if (!UIManagerComponent.Instance.componentsMap.TryGetValue(self.Id,out var res))
            {
                res = new Dictionary<string, Dictionary<Type, Entity>>() ;
                UIManagerComponent.Instance.componentsMap.Add(self.Id, res);
            }
            return res;
        }
        public static int GetLength(this Entity self)
        {
            if (!UIManagerComponent.Instance.lengthMap.TryGetValue(self.Id, out var res))
            {
                res = 1;
                UIManagerComponent.Instance.lengthMap.Add(self.Id, res);
            }
            return res;
        }
        public static void SetLength(this Entity self,int length)
        {
            UIManagerComponent.Instance.lengthMap[self.Id] = length;
        }
        public static void AfterOnEnable(this Entity self)
        {
            self.Walk((component) =>
            {
                UIEventSystem.Instance.OnEnable(component);
            });
        }

        public static void BeforeOnDisable(this Entity self)
        {
            self.Walk((component) =>
            {
                UIEventSystem.Instance.OnDisable(component);
            });
        }

        public static void BeforeOnDestroy(this Entity self)
        {
            var keys1 = self.GetCompoennts().Keys.ToList();
            for (int i = keys1.Count - 1; i >= 0; i--)
            {
                if (self.GetCompoennts()[keys1[i]] != null)
                {
                    var keys2 = self.GetCompoennts()[keys1[i]].Keys.ToList();
                    for (int j = keys2.Count - 1; j >= 0; j--)
                    {
                        UIEventSystem.Instance.OnDestroy(self.GetCompoennts()[keys1[i]][keys2[j]]);
                    }
                }
            }
            self.SetLength(self.GetLength()-1);
            if (self.GetLength() <= 0)
            {
                if (UIManagerComponent.Instance.pathMap.TryGetValue(self.Id, out var path))
                    self.Parent.InnerRemoveUIComponent(self, path);
                else
                    Log.Info("Close window here, type name: "+self.GetType().Name);
            }    
            else
                Log.Error("OnDestroy fail, length != 0");
            UIManagerComponent.Instance.componentsMap.Remove(self.Id);
            UIManagerComponent.Instance.lengthMap.Remove(self.Id);
            UIManagerComponent.Instance.pathMap.Remove(self.Id);
            self.Dispose();
        }

        //遍历：注意，这里是无序的
        static void Walk(this Entity self,Action<Entity> callback)
        {
            foreach (var item in self.GetCompoennts())
            {
                if (item.Value != null)
                {
                    foreach (var item2 in item.Value)
                    {
                        callback(item2.Value);
                    }
                }
            }
        }

        //记录Component
        static void RecordUIComponent(this Entity self,string name, Type component_class, Entity component)
        {
            if (self.GetCompoennts().TryGetValue(name, out var obj))
            {
                if (obj.ContainsKey(component_class))
                {
                    Log.Error("Aready exist component_class : " + component_class.Name);
                    return;
                }
            }
            else//如果必要，创建新的记录，对应Unity下一个Transform下所有挂载脚本的记录表
            {
                self.GetCompoennts()[name] = new Dictionary<Type, Entity>();
            }
            self.GetCompoennts()[name][component_class] = component;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="name">游戏物体名称</param>
        public static T AddUIComponentNotCreate<T>(this Entity self,string name) where T : Entity
        {
            Type type = typeof(T);
            T component_inst = self.AddChild<T>();
            UIManagerComponent.Instance.pathMap[component_inst.Id] = name;

            self.RecordUIComponent(name, type, component_inst);
            self.SetLength(self.GetLength()+1);
            return component_inst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public static T AddUIComponent<T>(this Entity self,string path = "") where T : Entity
        {
            Type type = typeof(T);
            T component_inst = self.AddChild<T>();
            UIManagerComponent.Instance.pathMap[component_inst.Id] = path;

            self.RecordUIComponent(path, type, component_inst);
            Game.EventSystem.Publish(new UIEventType.AddComponent() { Path = path, entity = component_inst }).Coroutine();
            UIEventSystem.Instance.OnCreate(component_inst);
            self.SetLength(self.GetLength() + 1);
            return component_inst;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">相对路径</param>
        public static T AddUIComponent<T, A>(this Entity self,string path, A a) where T : Entity
        {
            Type type = typeof(T);
            T component_inst = self.AddChild<T>();
            UIManagerComponent.Instance.pathMap[component_inst.Id] = path;

            Game.EventSystem.Publish(new UIEventType.AddComponent() { Path = path, entity = component_inst }).Coroutine();
            UIEventSystem.Instance.OnCreate(component_inst, a);

            self.RecordUIComponent(path, type, component_inst);
            self.SetLength(self.GetLength() + 1);
            return component_inst;
        }
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public static T AddUIComponent<T, A, B>(this Entity self, string path, A a, B b) where T : Entity
        {
            Type type = typeof(T);
            T component_inst = self.AddChild<T>();
            UIManagerComponent.Instance.pathMap[component_inst.Id] = path;

            Game.EventSystem.Publish(new UIEventType.AddComponent() { Path = path, entity = component_inst }).Coroutine();
            UIEventSystem.Instance.OnCreate(component_inst, a, b);

            self.RecordUIComponent(path, type, component_inst);
            self.SetLength(self.GetLength() + 1);
            return component_inst;
        }
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">路径</param>
        public static T AddUIComponent<T, A, B, C>(this Entity self, string path, A a, B b, C c) where T : Entity
        {
            Type type = typeof(T);
            T component_inst = self.AddChild<T>();
            UIManagerComponent.Instance.pathMap[component_inst.Id] = path;

            Game.EventSystem.Publish(new UIEventType.AddComponent() {Path = path, entity = component_inst }).Coroutine();
            UIEventSystem.Instance.OnCreate(component_inst, a, b, c);

            self.RecordUIComponent(path, type, component_inst);
            self.SetLength(self.GetLength() + 1);
            return component_inst;
        }
        public static void SetActive(this Entity self, bool active)
        {

            if (active)
            {
                UIEventSystem.Instance.OnEnable(self);
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
            }
            else
            {
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
                UIEventSystem.Instance.OnDisable(self);
            }
        }

        public static void SetActive<T>(this Entity self, bool active, T param1)
        {
            if (active)
            {
                UIEventSystem.Instance.OnEnable(self, param1);
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
            }
            else
            {
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
                UIEventSystem.Instance.OnDisable(self, param1);
            }
        }
        public static void SetActive<T, P>(this Entity self, bool active, T param1, P param2)
        {
            if (active)
            {
                UIEventSystem.Instance.OnEnable(self, param1, param2);
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
            }
            else
            {
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
                UIEventSystem.Instance.OnDisable(self, param1, param2);
            }
        }
        public static void SetActive<T, P, K>(this Entity self, bool active, T param1, P param2, K param3)
        {
            if (active)
            {
                UIEventSystem.Instance.OnEnable(self, param1, param2, param3);
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
            }
            else
            {
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
                UIEventSystem.Instance.OnDisable(self, param1, param2, param3);
            }
        }

        public static void SetActive<T, P, K, V>(this Entity self, bool active, T param1, P param2, K param3, V param4)
        {
            if (active)
            {
                UIEventSystem.Instance.OnEnable(self, param1, param2, param3, param4);
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
            }
            else
            {
                Game.EventSystem.Publish(new UIEventType.SetActive() { entity = self, Active = active }).Coroutine();
                UIEventSystem.Instance.OnDisable(self, param1, param2, param3, param4);
            }
        }
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetUIComponent<T>(this Entity self, string path = "") where T : Entity
        {
            if (self.GetCompoennts().TryGetValue(path, out var obj))
            {
                Type type = typeof(T);
                if (obj.TryGetValue(type, out var component))
                {
                    return component as T;
                }
            }
            return null;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        public static void RemoveUIComponent<T>(this Entity self, string path = "") where T : Entity
        {
            var component = self.GetUIComponent<T>(path);
            if (component != null)
            {
                UIEventSystem.Instance.OnDestroy(component);
                self.GetCompoennts()[path].Remove(typeof(T));
            }
        }

        /// <summary>
        /// 移除组件回调方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        public static void InnerRemoveUIComponent(this Entity self, Entity component, string path)
        {
            if (component != null)
            {
                self.GetCompoennts()[path].Remove(component.GetType());
                self.SetLength(self.GetLength()-1);
                if (self.GetCompoennts()[path].Count <= 0)
                {
                    self.GetCompoennts().Remove(path);
                }
            }
        }

    }
}
