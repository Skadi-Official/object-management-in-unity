using UnityEngine;
using System.IO;

namespace ObjectManagement
{
    /// <summary>
    /// GameDataWriter - 游戏存档数据写入器
    /// 核心职责：
    /// 作为所有“存档数据写入行为”的统一出口
    /// 基于 BinaryWriter 提供基础类型的二进制写入封装
    /// 负责将 Unity 常用结构类型序列化为可存储的数据格式
    /// 屏蔽具体序列化细节，保证上层逻辑（Game / Level / Shape）无需关心底层存储方式
    /// 支持确定性随机系统，通过写入 Random.State 实现随机序列的完整复现
    /// </summary>
    public class GameDataWriter
    {
        // 底层二进制写入器
        private BinaryWriter writer;

        public GameDataWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        #region 写入基础数据类型

        /// <summary>
        /// 写入 float 数据
        /// </summary>
        public void Write(float value)
        {
            writer.Write(value);
        }

        /// <summary>
        /// 写入 int 数据
        /// </summary>
        public void Write(int value)
        {
            writer.Write(value);
        }

        /// <summary>
        /// 写入 Vector3（按 x,y,z 顺序写入三个 float）
        /// </summary>
        public void Write(Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        /// <summary>
        /// 写入 Quaternion（按 x,y,z,w 顺序写入四个 float）
        /// </summary>
        public void Write(Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        /// <summary>
        /// 写入 Color（按 r,g,b,a 顺序写入四个 float）
        /// </summary>
        public void Write(Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        /// <summary>
        /// 写入 Unity 的随机状态 Random.State
        /// 通过 JsonUtility 转换为 JSON 字符串进行持久化，
        /// 用于保证：加载后随机序列可以 100% 精确复现
        /// </summary>
        public void Write(Random.State value)
        {
            //Debug.Log($"Write:: {JsonUtility.ToJson(value)}");
            writer.Write(JsonUtility.ToJson(value));
        }

        #endregion

        public void Write(ShapeInstance value)
        {
            writer.Write(value.IsValid ? value.Shape.SaveIndex : -1);
        }
    }
}
