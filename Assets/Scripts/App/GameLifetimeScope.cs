// Assets/Scripts/App/GameLifetimeScope.cs

using UnityEngine;
using VContainer;
using VContainer.Unity;
using YeokCheonEngine.ElementSystem;
using YeokCheonEngine.ElementSystem.EntitySystem;
using YeokCheonEngine.ElementSystem.ViewSystem;
using YeokCheonEngine.EngineSystem;
using YeokCheonEngine.SaveSystem;
using YeokCheonDomain.Cultivation;
using YeokCheonDomain.Sect;
using YeokCheonDomain.Session;
using YeokCheonDomain.Skill;

using GameSaveSystem = YeokCheonEngine.SaveSystem.GameSaveSystem;

namespace App
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        // Inspector에서 드래그로 연결
        [SerializeField] private AssetRegistry _assetRegistry;

        protected override void Configure(IContainerBuilder builder)
        {
            // ── 인프라 ───────────────────────────────────────────────
            builder.RegisterInstance(_assetRegistry);          // ScriptableObject는 이미 존재
            builder.Register<EntityManager>(Lifetime.Singleton);
            builder.Register<ViewManager>(Lifetime.Singleton);

            // ── GlobalEngine ──────────────────────────────────────────
            builder.Register<GlobalEngine>(Lifetime.Singleton);

            // ── 도메인 SubEngine ───────────────────────────────────────
            builder.Register<CultivationEngine>(Lifetime.Singleton);
            builder.Register<SectEngine>(Lifetime.Singleton);
            builder.Register<SessionEngine>(Lifetime.Singleton);
            builder.Register<SkillEngine>(Lifetime.Singleton);

            // ── GlobalEngine에 SubEngine 연결 (빌드 완료 후) ───────────
            builder.RegisterBuildCallback(SetupGlobalEngine);
            
            // ── SaveSystem ─────────────────────────────────────────────
            builder.Register<ISaveRepository, LocalRepository>(Lifetime.Singleton);
            builder.Register<MemoryRepository>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameSaveSystem>(Lifetime.Singleton).AsSelf();
            builder.RegisterEntryPoint<SaveLoader>(Lifetime.Singleton);

            // ── 진입점 ─────────────────────────────────────────────────
            builder.RegisterEntryPoint<GameEntryPoint>();
        }

        private static void SetupGlobalEngine(IObjectResolver resolver)
        {
            var engine = resolver.Resolve<GlobalEngine>();

            engine.Register<CultivationEngine, CultivationState>(
                resolver.Resolve<CultivationEngine>());

            engine.Register<SectEngine, SectState>(
                resolver.Resolve<SectEngine>());

            engine.Register<SessionEngine, SessionState>(
                resolver.Resolve<SessionEngine>());

            engine.Register<SkillEngine, SkillState>(
                resolver.Resolve<SkillEngine>());
        }
    }
}