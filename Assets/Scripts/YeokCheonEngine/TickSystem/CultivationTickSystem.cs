// Assets/Scripts/YeokCheonEngine/TickSystem/CultivationTickSystem.cs

using System;
using UnityEngine;
using VContainer.Unity;
using YeokCheonEngine.EngineSystem;
using YeokCheonDomain.Cultivation;

namespace YeokCheonEngine.TickSystem
{
    // 시간 공급자 — Phase2에서 서버 시간으로 교체 가능
    public interface ITimeProvider
    {
        long UtcNowUnixSeconds { get; }
    }

    public sealed class SystemTimeProvider : ITimeProvider
    {
        public long UtcNowUnixSeconds =>
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public sealed class CultivationTickSystem : IInitializable, ITickable, IDisposable
    {
        private const float TICK_INTERVAL  = 1f;      // 1초마다 틱
        private const long  OFFLINE_CAP    = 28800;   // 오프라인 보상 최대 8시간

        private readonly GlobalEngine  _globalEngine;
        private readonly ITimeProvider _timeProvider;

        private float _accumulator;
        private bool  _initialized;

        public CultivationTickSystem(GlobalEngine globalEngine, ITimeProvider timeProvider)
        {
            _globalEngine = globalEngine;
            _timeProvider = timeProvider;
        }

        // ── 앱 시작: 오프라인 보상 계산 ──────────────────────────────────

        public void Initialize()
        {
            var state    = _globalEngine.GetState<CultivationState>();
            var lastSeen = state.LastSeenServerTimestamp;
            var now      = _timeProvider.UtcNowUnixSeconds;

            if (lastSeen > 0 && now > lastSeen && state.IsInCultivation)
            {
                // 경과 시간 (최대 8시간 캡)
                var elapsed = Math.Min(now - lastSeen, OFFLINE_CAP);
                var gained  = state.ExpPerTick * elapsed;

                if (gained > 0)
                {
                    _globalEngine.Dispatch<CultivationState>(
                        new CultivationState.ApplyOfflineExpTrigger
                        {
                            ExpGained      = gained,
                            ElapsedSeconds = elapsed,
                        });

                    Debug.Log($"[TickSystem] 오프라인 보상 {gained:N0} EXP / {elapsed}초");
                }
            }

            // 현재 시각 기록
            RecordTimestamp();
            _initialized = true;

            // 포그라운드/백그라운드 감지
            Application.focusChanged += OnFocusChanged;
        }

        // ── 매 프레임: 1초마다 틱 ────────────────────────────────────────

        public void Tick()
        {
            if (!_initialized) return;

            _accumulator += Time.deltaTime;
            if (_accumulator < TICK_INTERVAL) return;
            _accumulator -= TICK_INTERVAL;

            var state = _globalEngine.GetState<CultivationState>();
            if (!state.IsInCultivation) return;

            var exp = (long)state.ExpPerTick;
            if (exp <= 0) return;

            _globalEngine.Dispatch<CultivationState>(
                new CultivationState.ApplyExpTickTrigger { ExpGained = exp });
        }

        // ── 포커스 변경 ───────────────────────────────────────────────────

        private void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
                RecordTimestamp();   // 백그라운드 진입: 시각 저장
            else
                Initialize();        // 포그라운드 복귀: 오프라인 보상 재계산
        }

        private void RecordTimestamp()
        {
            _globalEngine.Dispatch<CultivationState>(
                new CultivationState.RecordTimestampTrigger
                {
                    UnixSeconds = _timeProvider.UtcNowUnixSeconds,
                });
        }

        public void Dispose()
        {
            Application.focusChanged -= OnFocusChanged;
        }
    }
}