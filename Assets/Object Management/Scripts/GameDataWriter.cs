using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace ObjectManagement
{
    public class GameDataWriter
    {
        private BinaryWriter writer;

        public GameDataWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        #region 写入数据

        public void Write(float value)
        {
            writer.Write(value);
        }
        
        public void Write(int value)
        {
            writer.Write(value);
        }

        public void Write(Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public void Write(Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public void Write(Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }
        
        #endregion
    }
}

