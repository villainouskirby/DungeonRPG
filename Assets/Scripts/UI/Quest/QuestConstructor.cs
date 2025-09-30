using System;

namespace DBUtility
{
    public static class QuestConstructor
    {
        public static QuestInfo GetQuestInfo(Quest_Info_Quest questInfo)
        {
            var info = new QuestInfo(questInfo);

            info.Missions[0] = GetMission(info.Info.con_id1);
            info.Missions[1] = GetMission(info.Info.con_id2);
            info.Missions[2] = GetMission(info.Info.con_id3);
            
            for (int i = 0; i < 3; i++)
            {
                info.Missions[i].OnMissionClearChanged = info.CheckQuestClear;
            }

            // 보상 아이템 추가

            return info;
        }

        public static QuestInfo GetQuestInfo(int index)
        {
            return GetQuestInfo(Quest_Info.Quest[index]);
        }

        public static QuestInfo GetQuestInfo(string id)
        {
            return GetQuestInfo(Array.Find(Quest_Info.Quest, info => info.id == id));
        }

        private static Mission GetMission(string id)
        {
            Mission mission;

            switch (id.Split('_')[0])
            {
                case "IVS":
                    mission = new InvestigationMission();
                    break;

                case "GTR":
                    mission = new GatheringMission();
                    break;

                case "HNT":
                    mission = new HuntingMission();
                    break;

                default:
                    return null;
            }

            mission.Init(id);
            mission.RegisterProcess();

            return mission;
        }
    }
}
