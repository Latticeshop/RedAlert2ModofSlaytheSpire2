using System;

namespace RedAlert2ModCode.Common.Localization;

public static class UiLocalization
{
    private static string CurrentLanguage => GetCurrentLanguage();

    private static string GetCurrentLanguage()
    {
        try
        {
            var locManagerType = Type.GetType("MegaCrit.Sts2.Core.Localization.LocManager, MegaCrit.Sts2.Core");
            if (locManagerType != null)
            {
                var instanceProp = locManagerType.GetProperty("Instance");
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null);
                    if (instance != null)
                    {
                        var languageProp = locManagerType.GetProperty("Language");
                        if (languageProp != null)
                        {
                            var lang = languageProp.GetValue(instance);
                            return lang?.ToString() ?? "zhs";
                        }
                    }
                }
            }
        }
        catch
        {
        }
        return "zhs";
    }

    public static class EngineerChoices
    {
        public static string CaptureOilDerrickTitle => CurrentLanguage == "eng" ? "Capture Oil Derrick" : "占领油井";
        public static string CaptureOilDerrickDesc => CurrentLanguage == "eng" ? "Add one Oil Derrick card to hand" : "将一张「油井」加入手牌";

        public static string RepairBuildingTitle => CurrentLanguage == "eng" ? "Repair Building" : "修理建筑";
        public static string RepairBuildingDesc => CurrentLanguage == "eng" ? "Gain 3 Plated Armor" : "获得3点覆甲";

        public static string CaptureAirfieldTitle => CurrentLanguage == "eng" ? "Capture Airfield" : "占领机场";
        public static string CaptureAirfieldDesc => CurrentLanguage == "eng" ? "Add one Paratrooper card to hand" : "加入一张卡牌「伞兵」";

        public static string CaptureHospitalTitle => CurrentLanguage == "eng" ? "Capture Hospital" : "占领市民医院";
        public static string CaptureHospitalDesc => CurrentLanguage == "eng" ? "Gain 1 Dexterity" : "获得1点敏捷";

        public static string CaptureWorkshopTitle => CurrentLanguage == "eng" ? "Capture Workshop" : "占领机械商店";
        public static string CaptureWorkshopDesc => CurrentLanguage == "eng" ? "Gain 1 Strength" : "获得1点力量";

        public static string CaptureTechOutpostTitle => CurrentLanguage == "eng" ? "Capture Tech Outpost" : "占领科技前哨站";
        public static string CaptureTechOutpostDesc => CurrentLanguage == "eng" ? "Gain Patriot Missile and Repair Depot" : "获得能力「爱国者飞弹」和「维修厂」";

        public static string RepairBridgeTitle => CurrentLanguage == "eng" ? "Repair Bridge" : "维修桥梁";
        public static string RepairBridgeDesc => CurrentLanguage == "eng" ? "Exhaust a card, draw 2 cards" : "选择消耗一张手牌，抽两张牌";
    }

    public static class UiStrings
    {
        public static string EngineerChoiceTitle => CurrentLanguage == "eng" ? "Choose a Command" : "选择一个指令";
        public static string ChronoWarpTitle => CurrentLanguage == "eng" ? "Choose a Pile" : "选择一个牌堆";
        public static string DeployChoiceTitle => CurrentLanguage == "eng" ? "Choose Action" : "选择行动";

        public static string ProductionQueueTitle => CurrentLanguage == "eng" ? "Select production queues to start or stop" : "请选择要启动或停止的生产序列";
        public static string ProductionQueueCancel => CurrentLanguage == "eng" ? "X Cancel" : "X 取消";
        public static string ProductionQueueConfirm => CurrentLanguage == "eng" ? "Confirm Selection" : "确认选择";
        public static string ProductionQueueStopped => CurrentLanguage == "eng" ? "Stopped" : "已停产";
        public static string ProductionQueueRunning => CurrentLanguage == "eng" ? "Running" : "生产中";

        public static string PileDraw => CurrentLanguage == "eng" ? "Draw Pile" : "摸牌堆";
        public static string PileHand => CurrentLanguage == "eng" ? "Hand" : "手牌";
        public static string PileDiscard => CurrentLanguage == "eng" ? "Discard Pile" : "弃牌堆";
    }

    public static class FlakTrack
    {
        public static string Title => CurrentLanguage == "eng" ? "Choose Flak Track Action" : "选择防空履带车的行动";
        public static string DeployTitle => CurrentLanguage == "eng" ? "Deploy" : "部署";
        public static string DeployDesc => CurrentLanguage == "eng" ? "Store infantry units in hand" : "存储当前手牌中的士兵单位";
        public static string DefendTitle => CurrentLanguage == "eng" ? "Defend" : "防御";
        public static string DefendDesc => CurrentLanguage == "eng" ? "Draw cards and gain Block" : "抽牌并获得格挡";
    }

    public static class TeslaTrooper
    {
        public static string Title => CurrentLanguage == "eng" ? "Choose Tesla Trooper Action" : "选择磁暴步兵的行动";
        public static string DeployTitle => CurrentLanguage == "eng" ? "Deploy" : "部署";
        public static string DeployDesc => CurrentLanguage == "eng" ? "Charge Tesla Coil, next damage +50%" : "给磁暴线圈充能，下次伤害提升50%";
        public static string OrbTitle => CurrentLanguage == "eng" ? "Generate Lightning Orb" : "生成闪电球";
        public static string OrbDesc => CurrentLanguage == "eng" ? "Gain 1 Lightning Orb" : "获得一个闪电球";
    }

    public static class GuardianGi
    {
        public static string Title => CurrentLanguage == "eng" ? "Choose Guardian GI Action" : "选择重装大兵的行动";
        public static string DeployTitle => CurrentLanguage == "eng" ? "Deploy" : "部署";
        public static string DefendTitle => CurrentLanguage == "eng" ? "Defend" : "防御";
        public static string DeployDesc(decimal damage) => CurrentLanguage == "eng" ? $"Deal {damage} damage, apply 1 Vulnerable" : $"造成 {damage} 点伤害，赋予 1 层易伤";
        public static string DefendDesc(decimal block) => CurrentLanguage == "eng" ? $"Gain {block} Block" : $"获得 {block} 点格挡";
    }

    public static class NightHawk
    {
        public static string Title => CurrentLanguage == "eng" ? "Choose Night Hawk Action" : "选择夜莺直升机的行动";
        public static string DeployTitle => CurrentLanguage == "eng" ? "Deploy" : "部署";
        public static string DeployDesc => CurrentLanguage == "eng" ? "Store infantry units in hand" : "存储当前手牌中的士兵单位";
        public static string AttackTitle => CurrentLanguage == "eng" ? "Attack" : "攻击";
        public static string AttackDesc => CurrentLanguage == "eng" ? "Gain Dexterity and Attack" : "获得敏捷和攻击";
    }
}