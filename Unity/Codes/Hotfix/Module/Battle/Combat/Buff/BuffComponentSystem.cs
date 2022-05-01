﻿using System.Collections.Generic;

namespace ET
{
    [Timer(TimerType.RemoveBuff)]
    public class RemoveBuff: ATimer<Buff>
    {
        public override void Run(Buff self)
        {
            try
            {
                if(self==null||self.IsDisposed) return;
                self.GetParent<BuffComponent>().Remove(self.Id);
            }
            catch (System.Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    
    [ObjectSystem]
    [FriendClass(typeof(CombatUnitComponent))]
    public class BuffComponentAwakeSystem : AwakeSystem<BuffComponent>
    {
        public override void Awake(BuffComponent self)
        {
            self.Groups = DictionaryComponent<int, Buff>.Create();
            self.ActionControls = DictionaryComponent<int, int>.Create();
            self.unit = self.GetParent<CombatUnitComponent>().unit;
        }
    }
	
    [ObjectSystem]
    public class BuffComponentDestroySystem : DestroySystem<BuffComponent>
    {
        public override void Destroy(BuffComponent self)
        {
            self.Groups.Dispose();
            self.ActionControls.Dispose();
        }
    }
	[FriendClass(typeof(BuffComponent))]
    [FriendClass(typeof(Buff))]
    public static class BuffComponentSystem
    {
        public static Buff AddBuff(this BuffComponent self, int id,long timestamp)
        {
            BuffConfig conf = BuffConfigCategory.Instance.Get(id);
            if (self.Groups.ContainsKey(conf.Group))
            {
                var old = self.Groups[conf.Group];
                if (old.Config.Priority > conf.Priority) {
                    Log.Info("添加BUFF失败，优先级"+old.Config.Id+" > "+conf.Id);
                    return null; //优先级低
                }
                Log.Info("优先级高或相同，替换旧的");
                self.Remove(self.Groups[conf.Group].Id);
            }
            
            Buff buff = self.AddChild<Buff,int,long>(id,timestamp);
            self.Groups[conf.Group] = buff;
            TimerComponent.Instance.NewOnceTimer(timestamp, TimerType.RemoveBuff, buff);
            EventSystem.Instance.Publish(new EventType.AfterAddBuff(){Buff = buff});
            return buff;
        }
        /// <summary>
        /// 通过Buff的唯一Id取
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Buff Get(this BuffComponent self, long id)
        {
            Buff buff = self.GetChild<Buff>(id);
            return buff;
        }
        /// <summary>
        /// 通过Buff配置表的id取
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Buff GetByConfigId(this BuffComponent self, int id)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(id);
            if (self.Groups.ContainsKey(config.Group))
            {
                Buff buff = self.GetChild<Buff>(self.Groups[config.Group].Id);
                if (buff.ConfigId == id)
                {
                    return buff;
                }
            }

            return null;
        }
        /// <summary>
        /// 通过Buff的唯一Id移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        public static void Remove(this BuffComponent self, long id)
        {
            Buff buff = self.GetChild<Buff>(id);
            if(buff==null) return;
            EventSystem.Instance.Publish(new EventType.AfterRemoveBuff(){Buff = buff});
            self.Groups.Remove(buff.Config.Group);
            buff.Dispose();
        }
        /// <summary>
        /// 通过Buff配置表的id移除buff
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        public static void RemoveByConfigId(this BuffComponent self, int id)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(id);
            if (self.Groups.ContainsKey(config.Group))
            {
                Buff buff = self.GetChild<Buff>(self.Groups[config.Group].Id);
                if (buff.ConfigId == id)
                {
                    self.Groups.Remove(buff.Config.Group);
                    buff?.Dispose();
                }
            }
        }
#if !SERVER
        /// <summary>
        /// 展示所有BUFF
        /// </summary>
        /// <param name="self"></param>
        public static void ShowAllBuffView(this BuffComponent self)
        {
            foreach (var item in self.Groups)
            {
                EventSystem.Instance.Publish(new EventType.AfterAddBuff(){Buff = item.Value});
            }
        }
        
        /// <summary>
        /// 隐藏所有BUFF效果
        /// </summary>
        /// <param name="self"></param>
        public static void HideAllBuffView(this BuffComponent self)
        {
            foreach (var item in self.Groups)
            {
                EventSystem.Instance.Publish(new EventType.AfterRemoveBuff(){Buff = item.Value});
            }
        }
#endif
    }

}