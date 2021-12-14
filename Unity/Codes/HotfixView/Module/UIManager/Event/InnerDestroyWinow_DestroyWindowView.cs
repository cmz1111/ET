﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    public class InnerDestroyWinow_DestroyWindowView : AEvent<UIEventType.InnerDestroyWindow>
	{
		protected override async ETTask Run(UIEventType.InnerDestroyWindow args)
		{
			var target = args.target;
			Entity view = target.GetComponent(target.ViewType);
			if (view != null)
			{
				var obj = view.GetGameObject();
				if (obj)
				{
					if (GameObjectPoolComponent.Instance == null)
						GameObject.Destroy(obj);
					else
						GameObjectPoolComponent.Instance.RecycleGameObject(obj);
				}
				view.BeforeOnDestroy();
				UIEventSystem.Instance.OnDestroy(view);
			}
			await ETTask.CompletedTask;
		}
	}
}
