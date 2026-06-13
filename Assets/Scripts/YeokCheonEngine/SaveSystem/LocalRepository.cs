// LocalRepository.cs
// 파일 I/O + XOR 암호화.
// 저장 경로: Application.persistentDataPath/save.dat
// 암호화 키: 0x5A (단순 XOR — 완전한 보안은 아니나 스크립트 키디 방어용)

using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YeokCheonEngine.SaveSystem
{
    public sealed class LocalRepository : ISaveRepository
    {
        private const byte XorKey = 0x5A;

        private readonly string _filePath;

        public LocalRepository()
        {
            _filePath = Path.Combine(Application.persistentDataPath, "save.dat");
        }

        public async UniTask<SaveData> LoadAsync()
        {
            // 파일 없음 → 새 데이터 반환.
            if (!File.Exists(_filePath))
                return CreateDefault();

            try
            {
                // 비동기 파일 읽기.
                var encrypted = await File.ReadAllBytesAsync(_filePath);

                // XOR 복호화.
                var json = Decrypt(encrypted);

                return JsonUtility.FromJson<SaveData>(json) ?? CreateDefault();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalRepository] 로드 실패: {e.Message}");
                return CreateDefault();
            }
        }

        public async UniTask SaveAsync(SaveData data)
        {
            try
            {
                var json      = JsonUtility.ToJson(data);
                var encrypted = Encrypt(json);

                // 디렉토리 없으면 생성.
                var dir = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

                await File.WriteAllBytesAsync(_filePath, encrypted);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalRepository] 저장 실패: {e.Message}");
            }
        }

        // JSON 문자열 → XOR 암호화 바이트 배열.
        private static byte[] Encrypt(string json)
        {
            var bytes  = Encoding.UTF8.GetBytes(json);
            var result = new byte[bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
                result[i] = (byte)(bytes[i] ^ XorKey);
            return result;
        }

        // XOR 복호화 → JSON 문자열. (XOR은 같은 키로 두 번 하면 원문)
        private static string Decrypt(byte[] encrypted)
        {
            var bytes = new byte[encrypted.Length];
            for (var i = 0; i < encrypted.Length; i++)
                bytes[i] = (byte)(encrypted[i] ^ XorKey);
            return Encoding.UTF8.GetString(bytes);
        }

        private static SaveData CreateDefault()
        {
            return new SaveData
            {
                LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }
}