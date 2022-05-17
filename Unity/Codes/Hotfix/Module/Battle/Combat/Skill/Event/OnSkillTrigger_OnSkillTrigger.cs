﻿using System.Collections.Generic;
namespace ET
{

    public class OnSkillTrigger_OnSkillTrigger : AEvent<EventType.OnSkillTrigger>
    {
        protected override void Run(EventType.OnSkillTrigger args)
        {
            if (args.Type == AOITriggerType.Enter)
            {
                OnColliderIn(args.From, args.To, args.Para,args.CostId, args.Cost,args.Config);
            }
            else if (args.Type == AOITriggerType.Exit)
            {
                OnColliderOut(args.From, args.To, args.Para,args.CostId, args.Cost,args.Config);
            }
        }  
        
        /// <summary>
        /// 进入触发器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        public void OnColliderIn(AOIUnitComponent from, AOIUnitComponent to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config)
        {
            var combatU = to.Parent.GetComponent<CombatUnitComponent>();

            // Log.Info("触发"+type.ToString()+to.Id+"  "+from.Id);
            // Log.Info("触发"+type.ToString()+to.Position+" Dis: "+Vector3.Distance(to.Position,from.Position));
            int formulaId = 0;//公式
            if (stepPara.Paras.Length > 1)
            {
                int.TryParse(stepPara.Paras[1].ToString(), out formulaId);
            }
            float percent = 1;//实际伤害百分比
            if (stepPara.Paras.Length > 2)
            {
                float.TryParse(stepPara.Paras[2].ToString(), out percent);
            }

            int maxNum = 0;
            if (stepPara.Paras.Length > 3)
            {
                int.TryParse(stepPara.Paras[3].ToString(), out maxNum);
            }

            if (maxNum != 0 && stepPara.Count >= maxNum) return;//超上限
            stepPara.Count++;
            
            List<int[]> buffInfo = null;//添加的buff
            if (stepPara.Paras.Length > 4)
            {
                buffInfo = stepPara.Paras[4] as List<int[]>;
                if (buffInfo == null)
                {
                    string[] vs = stepPara.Paras[4].ToString().Split(';');
                    buffInfo = new List<int[]>();
                    for (int i = 0; i < vs.Length; i++)
                    {
                        var data = vs[i].Split(',');
                        int[] temp = new int[data.Length];
                        for (int j = 0; j < data.Length; j++)
                        {
                            temp[j] = int.Parse(data[i]);
                        }
                        buffInfo.Add(temp);
                    }
                    stepPara.Paras[4] = buffInfo;
                }
            }
            
            if(buffInfo!=null&&buffInfo.Count>0)
            {
                var buffC = combatU.GetComponent<BuffComponent>();
                
                for (int i = 0; i < buffInfo.Count; i++)
                {
                    
                    buffC.AddBuff(buffInfo[i][0],TimeHelper.ClientNow() + buffInfo[i][1]);
                }
            }

            FormulaConfig formula = FormulaConfigCategory.Instance.Get(formulaId);
            if (formula!=null)
            {
                FormulaStringFx fx = FormulaStringFx.GetInstance(formula.Formula);
                NumericComponent f = from.GetParent<Unit>().GetComponent<NumericComponent>();
                NumericComponent t = to?.GetParent<Unit>().GetComponent<NumericComponent>();
                float value = fx.GetData(f, t);
                
                int realValue = (int)value;

                if (realValue != 0)
                {
                    float now = t.GetAsFloat(NumericType.HpBase);
                    Log.Info(now);
                    if (now <= realValue)
                    {
                        t.Set(NumericType.HpBase, 0);
                    }
                    else
                    {
                        t.Set(NumericType.HpBase, now - realValue);
                    }

                    EventSystem.Instance.Publish(new EventType.AfterCombatUnitGetDamage()
                    {
                        From = from.Parent.GetComponent<CombatUnitComponent>(),
                        Unit = combatU,
                        Value = realValue
                    });
                }
            }
        }
        /// <summary>
        /// 离开触发器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        public void OnColliderOut(AOIUnitComponent from, AOIUnitComponent to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config)
        {
            // Log.Info("触发"+type.ToString()+to.Id+"  "+from.Id);
            // Log.Info("触发"+type.ToString()+to.Position+" Dis: "+Vector3.Distance(to.Position,from.Position));
            if (stepPara.Paras.Length > 4)
            {
                List<int[]> buffInfo = stepPara.Paras[4] as List<int[]>;
                if (buffInfo != null&&buffInfo.Count>0)
                {
                    var buffC = to.Parent.GetComponent<CombatUnitComponent>().GetComponent<BuffComponent>();
                    for (int i = 0; i < buffInfo.Count; i++)
                    {
                        if (buffInfo[i][2] == 1)
                        {
                            buffC.RemoveByConfigId(buffInfo[i][0]);
                        }
                    }
                }
            }
        }
    }
}