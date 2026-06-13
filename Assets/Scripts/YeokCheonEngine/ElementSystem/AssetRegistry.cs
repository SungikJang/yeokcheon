// AssetRegistry.cs
// 타입 → 프리팹 매핑 ScriptableObject.
// 에디터에서 등록하고 런타임에 EntityManager/ViewManager가 조회한다.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace YeokCheonEngine.ElementSystem
{
    // 에디터 우클릭 메뉴에서 생성 가능하게 등록
    [CreateAssetMenu(
        fileName = "AssetRegistry",
        menuName  = "YeokCheonEngine/AssetRegistry")]
    public sealed class AssetRegistry : ScriptableObject
    {
        // 에디터 직렬화용 엔트리 구조체.
        // TypeFullName은 에디터에서 문자열로 입력.
        // 런타임에는 Type으로 변환해서 Dictionary에 저장.
        [Serializable]
        public struct Entry
        {
            public string     TypeFullName; // 예: "YeokCheonDomain.Battle.EnemyUnit"
            public GameObject Prefab;
        }

        [SerializeField] private List<Entry> _entityEntries = new();
        [SerializeField] private List<Entry> _viewEntries   = new();

        // 런타임 조회용 딕셔너리. Type 키 → 할당 없음.
        private Dictionary<Type, GameObject> _entityMap;
        private Dictionary<Type, GameObject> _viewMap;

        // ScriptableObject가 메모리에 로드될 때 자동 호출.
        // 문자열 기반 List → Type 기반 Dictionary 변환.
        private void OnEnable() => Build();

        private void Build()
        {
            _entityMap = new(_entityEntries.Count);
            foreach (var e in _entityEntries)
            {
                // 문자열 → System.Type 변환.
                // 어셈블리 이름까지 포함해야 찾을 수 있는 경우도 있음.
                var type = Type.GetType(e.TypeFullName);
                if (type != null)
                    _entityMap[type] = e.Prefab;
                else
                    Debug.LogWarning($"[AssetRegistry] 타입을 찾을 수 없음: {e.TypeFullName}");
            }

            _viewMap = new(_viewEntries.Count);
            foreach (var e in _viewEntries)
            {
                var type = Type.GetType(e.TypeFullName);
                if (type != null)
                    _viewMap[type] = e.Prefab;
                else
                    Debug.LogWarning($"[AssetRegistry] 타입을 찾을 수 없음: {e.TypeFullName}");
            }
        }

        // Entity 프리팹 조회. 없으면 null 반환 (코드로만 구성하는 Entity용).
        public GameObject GetEntityPrefab<T>()
        {
            _entityMap ??= new(); // null이면 초기화 (Build가 아직 안 된 경우 방어).
            _entityMap.TryGetValue(typeof(T), out var prefab);
            return prefab;
        }

        // View 프리팹 조회.
        public GameObject GetViewPrefab<T>()
        {
            _viewMap ??= new();
            _viewMap.TryGetValue(typeof(T), out var prefab);
            return prefab;
        }

        // 런타임 동적 등록 (테스트, DLC 에셋 추가 시).
        public void RegisterEntity<T>(GameObject prefab)
        {
            _entityMap ??= new();
            _entityMap[typeof(T)] = prefab;
        }

        public void RegisterView<T>(GameObject prefab)
        {
            _viewMap ??= new();
            _viewMap[typeof(T)] = prefab;
        }
    }
}