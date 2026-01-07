using UnityEngine;
using System.IO;
using Random = UnityEngine.Random;

namespace ObjectManagement
{
    /// <summary>
    /// GameDataReader - 游戏存档数据读取器
    /// 核心职责：
    /// 作为所有“存档数据读取行为”的统一入口
    /// 基于 BinaryReader 提供基础类型的反序列化封装
    /// 负责将二进制数据还原为 Unity 常用结构类型（Vector3 / Quaternion / Color 等）
    /// 负责恢复 Random.State，以保证加载后随机序列的 100% 可复现
    /// 通过 Version 支持多版本存档的向后兼容
    /// </summary>
    public class GameDataReader
    {
        // 底层二进制读取器
        private BinaryReader reader;

        // 当前存档版本号（用于兼容旧版本存档）
        public int Version { get; }

        public GameDataReader(BinaryReader reader, int version)
        {
            this.reader = reader;
            this.Version = version;
        }

        #region 读取基础数据类型

        /// <summary>
        /// 读取 int 数据
        /// </summary>
        public int ReadInt()
        {
            int value = reader.ReadInt32();
            return value;
        }

        /// <summary>
        /// 读取浮点数据
        /// </summary>
        public float ReadFloat()
        {
            var value = reader.ReadSingle();
            return value;
        }

        /// <summary>
        /// 读取 Vector3（按 x,y,z 顺序读取三个 float）
        /// </summary>
        public Vector3 ReadVector3()
        {
            Vector3 value;
            value.x = reader.ReadSingle();
            value.y = reader.ReadSingle();
            value.z = reader.ReadSingle();
            return value;
        }

        /// <summary>
        /// 读取 Quaternion（按 x,y,z,w 顺序读取四个 float）
        /// </summary>
        public Quaternion ReadQuaternion()
        {
            Quaternion value;
            value.x = reader.ReadSingle();
            value.y = reader.ReadSingle();
            value.z = reader.ReadSingle();
            value.w = reader.ReadSingle();
            return value;
        }

        /// <summary>
        /// 读取 Color（按 r,g,b,a 顺序读取四个 float）
        /// </summary>
        public Color ReadColor()
        {
            Color value;
            value.r = reader.ReadSingle();
            value.g = reader.ReadSingle();
            value.b = reader.ReadSingle();
            value.a = reader.ReadSingle();
            return value;
        }

        /// <summary>
        /// 从存档文件中读取随机状态的 JSON 字符串，
        /// 并反序列化为 Unity 的 Random.State 结构体，
        /// 用于精确恢复随机序列的执行进度
        /// </summary>
        public Random.State ReadRandomState()
        {
            return JsonUtility.FromJson<Random.State>(reader.ReadString());
        }

        #endregion
        public ShapeInstance ReadShapeInstance()
        {
            return new ShapeInstance(reader.ReadInt32());
        }
    }
}
