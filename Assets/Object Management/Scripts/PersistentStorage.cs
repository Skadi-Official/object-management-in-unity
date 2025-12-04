using UnityEngine;
using System.IO;

namespace ObjectManagement
{
    /// <summary>
    /// PersistentStorage - 持久化存储管理器
    /// 核心职责：
    /// 统一管理游戏存档文件在磁盘中的存储路径
    /// 负责创建、覆盖、打开存档文件（真实的文件 IO 层）
    /// 作为“磁盘 ↔ 存档系统”的桥梁，连接：
    ///     - PersistableObject（谁负责提供存档数据）
    ///     - GameDataWriter（如何写入）
    ///     - GameDataReader（如何读取）
    /// 负责写入和读取“存档版本号”，支持多版本存档兼容
    /// 架构意义：
    /// 将“文件系统操作”与“游戏数据逻辑”彻底解耦
    /// 使存档系统可以自由更换存储介质（文件、云端、加密流等）
    /// 作为所有 Save / Load 行为的唯一入口，避免多点读写导致数据损坏
    /// </summary>
    public class PersistentStorage : MonoBehaviour
    {
        // 存档文件在本地磁盘上的完整路径
        private string savePath;

        private void Awake()
        {
            // Application.persistentDataPath 是 Unity 提供的跨平台安全存储目录
            savePath = Path.Combine(Application.persistentDataPath, "saveFile");
        }

        /// <summary>
        /// 保存游戏数据到磁盘
        /// </summary>
        /// <param name="o">要保存的对象（必须继承自 PersistableObject）</param>
        /// <param name="version">当前存档版本号</param>
        public void Save(PersistableObject o, int version)
        {
            // FileMode.Create：若文件不存在则创建，若已存在则直接覆盖
            using var writer = new BinaryWriter(File.Open(savePath, FileMode.Create));

            // 写入“负数版本号”作为存档头，用于区分数据版本
            writer.Write(-version);

            // 由对象自己负责写入其内部数据（控制反转）
            o.Save(new GameDataWriter(writer));
        }

        /// <summary>
        /// 从磁盘加载游戏数据
        /// </summary>
        /// <param name="o">要恢复数据的对象</param>
        public void Load(PersistableObject o)
        {
            byte[] data = File.ReadAllBytes(savePath);
            var reader = new BinaryReader(new MemoryStream(data));
            o.Load(new GameDataReader(reader, -reader.ReadInt32()));
        }
    }
}