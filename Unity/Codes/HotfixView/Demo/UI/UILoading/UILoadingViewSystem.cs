﻿using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[UISystem]
	[FriendClass(typeof(UILoadingView))]
	public class UILoadingViewOnCreateSystem : OnCreateSystem<UILoadingView>
	{
		public override void OnCreate(UILoadingView self)
		{
			UILoadingView.Instance = self;
			self.slider = self.AddUIComponent<UISlider>("Loadingscreen/Slider");
		}
	}
	[UISystem]
	[FriendClass(typeof(UILoadingView))]
	public class UILoadingViewOnDestroySystem : OnDestroySystem<UILoadingView>
	{
		public override void OnDestroy(UILoadingView self)
		{
			UILoadingView.Instance = null;
		}
	}
	[FriendClass(typeof(UILoadingView))]
	public static class UILoadingViewSystem
	{
		public static void SetSlidValue(this UILoadingView self, float pro)
        {
			self.slider.SetValue(pro);
		}
	
    }
}
