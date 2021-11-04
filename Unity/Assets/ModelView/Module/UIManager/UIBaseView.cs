﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// UI窗口
    /// 所有窗口都应继承此类并指定预制体路径，预制体名和类名尽量保持一致。
    /// 建议命名规范:
    /// 1.全屏大界面（锚点锚到四个角的类型）  UIxxxView；
    /// 2.上下左右其中一个方向满屏（锚点是一根线的类型）  UIxxxPanel；
    /// 3.上下左右都不满屏的窗口（锚点是一个点的类型）  UIxxxWin
    /// </summary>
    public abstract class UIBaseView: UIBaseComponent
    {
        public string __BaseViewName
        {
            get => name;
            set
            {
                if (string.IsNullOrEmpty(name))
                    name = value;
            }
        }
        string name;
        public abstract string PrefabPath { get; }

        public override GameObject gameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
                _transform = _gameObject.transform;
            }
        }
        public override Transform transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                _gameObject = _transform.gameObject;
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Messager.Instance.AddListener(MessagerId.OnLanguageChange, OnSwitchLanguage);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Messager.Instance.RemoveListener(MessagerId.OnLanguageChange, OnSwitchLanguage);
        }

        #region 多语言相关
        //当语言改变时的回调,让子类自己实现
        public virtual void OnSwitchLanguage(object sender,EventArgs args) {
        }
        #endregion
    }
}
