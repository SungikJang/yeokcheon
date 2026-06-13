// ISaveRepository.cs
// 저장소 인터페이스. MemoryRepository와 LocalRepository가 구현.

using Cysharp.Threading.Tasks;

namespace YeokCheonEngine.SaveSystem
{
    public interface ISaveRepository
    {
        UniTask<SaveData> LoadAsync();
        UniTask SaveAsync(SaveData data);
    }
}