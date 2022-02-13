﻿namespace ET
{
    public class RegisterI18NEntity_RegisterI18NEntity: AEvent<UIEventType.RegisterI18NEntity>
    {
        protected override async ETTask Run(UIEventType.RegisterI18NEntity args)
        {
            I18NComponent.Instance.RegisterI18NEntity(args.entity);
            await ETTask.CompletedTask;
        }
    }
}