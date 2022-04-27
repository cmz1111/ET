using System;
using UnityEngine;

namespace ET
{
    public class GameObjectComponent: Entity, IAwake,IAwake<GameObject>,IAwake<GameObject,Action>, IDestroy
    {
        public GameObject GameObject;
        public Action OnDestroyAction;
        public bool IsDebug;
    }
}