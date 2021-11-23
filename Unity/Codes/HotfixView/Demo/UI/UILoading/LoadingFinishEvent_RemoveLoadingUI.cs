﻿namespace ET
{
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent<EventType.LoadingFinish>
    {
        protected override async ETTask Run(EventType.LoadingFinish args)
        {
            //await UIHelper.Remove(args.Scene, UIType.UILoading);
            //UIManagerComponent.Instance.DestroyWindow<UILoadingView>();//Destroy掉的才能被销毁
            await ETTask.CompletedTask;
        }
    }
}
