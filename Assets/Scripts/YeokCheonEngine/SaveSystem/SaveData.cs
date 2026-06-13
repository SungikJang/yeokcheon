// SaveData.cs
// 게임 전체 저장 데이터 구조체.
// 모든 도메인의 상태를 하나의 오브젝트로 직렬화.

using System;

namespace YeokCheonEngine.SaveSystem
{
    [Serializable]
    public sealed class SaveData
    {
        public CultivationSave Cultivation = new();
        public SkillSave        Skill       = new();
        public SectSave         Sect        = new();
        public SessionSave      Session     = new();

        // 마지막 저장 시각 (오프라인 보상 계산에 사용).
        public long LastSavedAt; // Unix timestamp (초)
    }

    [Serializable] public sealed class CultivationSave
    {
        public int    RealmTier      = 0;
        public double CurrentExp     = 0;
        public long   LastTickAt     = 0; // Unix timestamp
        public bool   IsInCultivation;
        public double ExpPerTick;
        public long   LastSeenServerTimestamp;
    }

    [Serializable] public sealed class SkillSave
    {
        public int[]    OwnedSkillIds    = Array.Empty<int>();
        public int[]    DantianSlotIds   = new int[3]; // 장착 슬롯 3개
        public int      SkillStones      = 0;
    }

    [Serializable] public sealed class SectSave
    {
        public int    SectId     = -1; // -1 = 무소속
        public int    SectGrade  = 0;
        public long   JoinedAt   = 0;
    }

    [Serializable] public sealed class SessionSave
    {
        public string PlayerId   = "";
        public string PlayerName = "수련생";
        public long   CreatedAt  = 0;
    }
}