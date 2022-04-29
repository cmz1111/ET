using System;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class OperaComponentAwakeSystem : AwakeSystem<OperaComponent>
    {
        public override void Awake(OperaComponent self)
        {
            self.mapMask = LayerMask.GetMask("Map");
        }
    }

    [ObjectSystem]
    public class OperaComponentUpdateSystem : UpdateSystem<OperaComponent>
    {
        public override void Update(OperaComponent self)
        {
            self.Update();
        }
    }
    
    [FriendClass(typeof(OperaComponent))]
    public static class OperaComponentSystem
    {
        public static void Update(this OperaComponent self)
        {
            if (InputHelper.GetMouseButtonDown(1))
            {
                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                UnityEngine.RaycastHit hit;
                if (UnityEngine.Physics.Raycast(ray, out hit, 1000, self.mapMask))
                {
                    self.ClickPoint = hit.point;
                    self.frameClickMap.X = self.ClickPoint.x;
                    self.frameClickMap.Y = self.ClickPoint.y;
                    self.frameClickMap.Z = self.ClickPoint.z;
                    self.ZoneScene().GetComponent<SessionComponent>().Session.Send(self.frameClickMap);
                }
            }

            // KeyCode.R
            if (InputHelper.GetKeyDown(114))
            {
                CodeLoader.Instance.LoadLogic();
                Game.EventSystem.Add(CodeLoader.Instance.GetHotfixTypes());
                Game.EventSystem.Load();
                Log.Debug("hot reload success!");
            }
            
            // KeyCode.T
            if (InputHelper.GetKeyDown(116))
            {
                C2M_TransferMap c2MTransferMap = new C2M_TransferMap();
                self.ZoneScene().RemoveComponent<KeyCodeComponent>();
                self.ZoneScene().GetComponent<SessionComponent>().Session.Call(c2MTransferMap).Coroutine();
            }
            
            KeyCodeComponent keyCode = KeyCodeComponent.Instance;
            if (keyCode != null)
            {
                var unit = UnitHelper.GetMyUnitFromZoneScene(keyCode.ZoneScene());
                var CurCombat = unit?.GetComponent<CombatUnitComponent>();
                var spellPreviewComponent = CurCombat?.GetComponent<SpellPreviewComponent>();
                if (spellPreviewComponent == null)
                {
                    return;
                }
                for (int i = 0; i < keyCode.Skills.Length; i++)
                {
                    if (InputHelper.GetKeyDown(keyCode.Skills[i]) && spellPreviewComponent.InputSkills.ContainsKey(keyCode.Skills[i]))
                    {
                        var spellSkill = spellPreviewComponent.InputSkills[keyCode.Skills[i]];
                        if (spellSkill == null || !spellSkill.CanUse()) return;
                        spellPreviewComponent.PreviewingSkill = spellSkill;
                        spellPreviewComponent.EnterPreview();
                    }
                }
            }
        }
    }
}