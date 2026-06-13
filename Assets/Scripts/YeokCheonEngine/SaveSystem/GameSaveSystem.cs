// GameSaveSystem.cs
// 자동 저장 타이머 (30초마다) + 수동 저장(TakeSnapshot).
// VContainer의 IInitializable + ITickable + IDisposable 구현.

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace YeokCheonEngine.SaveSystem
{
    public sealed class GameSaveSystem : IInitializable, ITickable, IDisposable
    {
        private const float AutoSaveInterval = 30f; // 초

        private readonly MemoryRepository _memory;
        private readonly SaveSyncer       _syncer;

        private float _timer;
        private bool  _isDirty; // 변경 사항이 있으면 true → 저장 필요

        public GameSaveSystem(MemoryRepository memory, SaveSyncer syncer)
        {
            _memory = memory;
            _syncer = syncer;
        }

        // VContainer: 게임 시작 시 1회 호출.
        public void Initialize()
        {
            _timer   = 0f;
            _isDirty = false;

            // 초기 데이터 로드 (SaveLoader가 이걸 읽어서 복원함).
            _ = _memory.LoadAsync();
        }

        // VContainer: 매 프레임 호출.
        public void Tick()
        {
            if (!_isDirty) return;

            _timer += Time.deltaTime;
            if (_timer >= AutoSaveInterval)
            {
                _timer   = 0f;
                TakeSnapshot().Forget(); // 비동기 저장 (await 안 함 → 프레임 안 막음)
            }
        }

        // 변경 발생 시 외부에서 호출 (예: 경지 오름, 스킬 획득 등).
        public void MarkDirty() => _isDirty = true;

        // 현재 캐시 상태를 파일에 저장.
        public async UniTask TakeSnapshot()
        {
            _syncer.Sync(); // ★ 저장 전에 현재 상태 동기화
            
            var data = _memory.GetCached();
            if (data == null) return;

            data.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _memory.SaveAsync(data);

            _isDirty = false;
            Debug.Log("[GameSaveSystem] 저장 완료.");
        }

        // 앱 종료 시 강제 저장.
        public void Dispose()
        {
            if (_isDirty)
                TakeSnapshot().Forget();
        }
    }
}