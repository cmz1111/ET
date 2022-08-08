﻿using System;
using System.Collections.Generic;
using MongoDB.Bson;
using UnityEngine;

namespace ET
{
	[ActorMessageHandler]
	public class M2M_UnitAreaTransferRequestHandler : AMActorRpcHandler<Scene, M2M_UnitAreaTransferRequest, M2M_UnitAreaTransferResponse>
	{
		protected override async ETTask Run(Scene scene, M2M_UnitAreaTransferRequest request, M2M_UnitAreaTransferResponse response, Action reply)
		{
			await ETTask.CompletedTask;
			UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
			Unit unit = request.Unit;
			var oldUnit = unitComponent.Get(unit.Id);
			if (oldUnit==null)
			{
				unitComponent.AddChild(unit);
				unitComponent.Add(unit);

				foreach (var item in request.Map)
				{
					var entity = request.Entitys[item.ChildIndex];
					Entity parent;
					if (item.ParentIndex == -1) //父组件为自己
						parent = unit;
					else
						parent = request.Entitys[item.ParentIndex];

					if (item.IsChild == 0)
						parent.AddComponent(entity);
					else
						parent.AddChild(entity);
				}

				unit.AddComponent<MoveComponent>();
				MapSceneConfig conf = MapSceneConfigCategory.Instance.Get((int) scene.Id);
				unit.AddComponent<PathfindingComponent, string>(conf.Recast);
			}
			else
			{
				unit = oldUnit;
			}
			unit.AddComponent<MailBoxComponent>();
			if (oldUnit==null)
			{
				var numericComponent = unit.GetComponent<NumericComponent>();

				// 加入aoi
				var aoiu = unit.AddComponent<AOIUnitComponent, Vector3, Quaternion, UnitType, int>(unit.Position, unit.Rotation, unit.Type,
					numericComponent.GetAsInt(NumericType.AOI));

				aoiu.AddSphereCollider(0.5f);
			}
			else
			{
				var aoiu = unit.GetComponent<AOIUnitComponent>();
				aoiu.GetComponent<GhostComponent>().IsGoast = false;
			}
			response.NewInstanceId = unit.InstanceId;
			reply();
		}
	}
}