// MemoryRepository.cs
// 메모리 캐시 레이어.
// 읽기: 캐시 있으면 즉시 반환, 없으면 LocalRepository에서 로드 후 캐싱.
// 쓰기: 메모리 즉시 갱신, LocalRepository에도 비동기 저장.

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YeokCheonEngine.SaveSystem
{
    public sealed class MemoryRepository : ISaveRepository
    {
        private readonly ISaveRepository _local; // LocalRepository
        private SaveData _cache;                 // 메모리 캐시

        public MemoryRepository(ISaveRepository local)
        {
            _local = local;
        }

        public async UniTask<SaveData> LoadAsync()
        {
            // 캐시 히트: 즉시 반환 (파일 I/O 없음).
            if (_cache != null) return _cache;

            // 캐시 미스: 파일에서 로드 후 캐싱.
            _cache = await _local.LoadAsync();
            return _cache;
        }

        public async UniTask SaveAsync(SaveData data)
        {
            // 메모리 즉시 갱신.
            _cache = data;

            // 파일에도 비동기 저장.
            await _local.SaveAsync(data);
        }

        // 현재 캐시 스냅샷 반환 (null일 수 있음).
        // GameSaveSystem의 TakeSnapshot()에서 사용.
        public SaveData GetCached() => _cache;

        // 캐시 직접 설정. SaveLoader가 복원 후 호출.
        public void SetCache(SaveData data) => _cache = data;
    }
}