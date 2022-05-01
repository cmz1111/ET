﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    public class AOIGrid: Entity,IAwake,IDestroy
    {
        public int xMax;//实际地图范围
        public int xMin;//实际地图范围
        public int yMin;//实际地图范围
        public int yMax;//实际地图范围
        public int posx;//AOI格子中的坐标
        public int posy;//AOI格子中的坐标
        public float halfDiagonal;//半对角线长度
        public Dictionary<UnitType,List<AOIUnitComponent>> idUnits;
        public List<AOIUnitComponent> ListenerUnits;//关注此Grid的Unit
        public List<AOITriggerComponent> Triggers;//关注此Grid的触发器
    }
    
}
