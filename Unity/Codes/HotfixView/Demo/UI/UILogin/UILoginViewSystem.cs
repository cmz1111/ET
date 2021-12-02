﻿using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace ET
{
	[UISystem]
	public class UILoginViewOnCreateSystem : OnCreateSystem<UILoginView>
	{
		public override void OnCreate(UILoginView self)
		{
			self.loginBtn = self.AddUIComponent<UIButton>("Panel/LoginBtn");
			self.registerBtn = self.AddUIComponent<UIButton>("Panel/RegisterBtn");
			self.loginBtn.SetOnClick(() => { self.OnLogin(); });
			self.registerBtn.SetOnClick(() => { self.OnRegister(); });
			self.account = self.AddUIComponent<UIInput>("Panel/Account");
			self.password = self.AddUIComponent<UIInput>("Panel/Password");
			self.ipaddr = self.AddUIComponent<UIInputTextmesh>("Panel/GM/InputField");
			self.loginBtn.AddUIComponent<UIRedDotComponent, string>("","Test");
			var settings = self.AddUIComponent<UIBaseContainer>("Panel/GM/Setting");
			self.btns = new List<UIButton>();
			for (int i = 0; i < 2; i++)
			{
				int id = i + 1;
				var btn = settings.AddUIComponent<UIButton>("Setting" + (i + 1));
				btn.SetOnClick(() =>
				{
					self.OnBtnClick(id);
				});
				self.btns.Add(btn);
			}
		}
	}
	[UISystem]
	public class UILoginViewOnEnableSystem : OnEnableSystem<UILoginView, Scene>
	{
		public override void OnEnable(UILoginView self, Scene scene)
		{
			self.scene = scene;
			self.ipaddr.SetText(ServerConfigManagerComponent.Instance.GetCurConfig().RealmIp);
			self.account.SetText(PlayerPrefs.GetString(CacheKeys.Account, ""));
			self.password.SetText(PlayerPrefs.GetString(CacheKeys.Password, ""));
		}
	}
	public static class UILoginViewSystem
	{
		
		public static void OnLogin(this UILoginView self)
		{
			self.loginBtn.SetInteractable(false);
			GlobalComponent.Instance.Account = self.account.GetText();
			PlayerPrefs.SetString(CacheKeys.Account, self.account.GetText());
			PlayerPrefs.SetString(CacheKeys.Password, self.password.GetText());
			LoginHelper.Login(self.scene, self.ipaddr.GetText(), self.account.GetText(), self.password.GetText(), () =>
			{
				self.loginBtn.SetInteractable(true);
			});
		}
		public static void OnBtnClick(this UILoginView self,int id)
        {
			self.ipaddr.SetText(ServerConfigManagerComponent.Instance.ChangeEnv(id).RealmIp);
		}

		public static void OnRegister(this UILoginView self)
		{
			Game.EventSystem.Publish(new UIEventType.ShowToast() { Text = "测试OnRegister" });
			RedDotComponent.Instance.RefreshRedDotViewCount("Test1", 1);
		}
	}
}
