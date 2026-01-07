
namespace ObjectManagement
{
    /// <summary>
    /// 对某一个shape的实例的引用
    /// </summary>
    [System.Serializable]
    public struct ShapeInstance
    {
        public ShapeInstance(Shape shape)
        {
            Shape = shape;
            instanceIdOrSaveIndex = shape.InstanceId;
        }

        public ShapeInstance(int saveIndex)
        {
            // 这里我们只拿到了一个saveIndex，此时不能直接使用
            Shape = null;
            instanceIdOrSaveIndex = saveIndex;
        }
        public Shape Shape { get; private set; }    // shape实例
        private int instanceIdOrSaveIndex;          // 记录实例的id
        // 当shape存在且实例标识一致时才返回有效
        public bool IsValid => Shape && Shape.InstanceId == instanceIdOrSaveIndex;

        /// <summary>
        /// 解析SaveIndex将其实际转换为instanceID
        /// </summary>
        public void Resolve()
        {
            if(instanceIdOrSaveIndex < 0) return;
            Shape = Game.Instance.GetShape(instanceIdOrSaveIndex);
            instanceIdOrSaveIndex = Shape.InstanceId;
        }
        
        // 用户自定义的隐式类型转换运算符，如果我们不加这个的话，当我们希望构造一个ShapeInstance时，只能
        // ShapeInstance instance = new ShapeInstance(shape);
        // 而加了implicit后我们可以直接写ShapeInstance instance = shape; 编译器会自动帮我们翻译
        // 如果我们用的是explicit，就必须写ShapeInstance instance = (ShapeInstance)shape;
        public static implicit operator ShapeInstance(Shape shape)
        {
            return new ShapeInstance(shape);
        }
    }
}