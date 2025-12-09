using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ObjectManagement
{
    /// <summary>
    /// game也是需要被保存的对象，所以game本身也是一个PersistableObject
    /// </summary>
    public class Game : PersistableObject 
    {
        const int saveVersion = 4; // 存档版本标记
        //public static Game Instance { get; private set; } 由于我们把生成点全部解耦到了关卡自己，game只需要去获取，不需要关卡来设置game的生成点变量
        public SpawnZone SpawnZoneOfLevel { get; set; }         // 生成区域，属于关卡不属于通用场景
        public float CreationSpeed { get; set; }        // 创建速度
        public float DestructionSpeed { get; set; }     // 销毁速度
        [SerializeField]private Slider creationSpeedSlider;             // 控制创建速度的拖动条 
        [SerializeField]private Slider destructionSpeedSlider;          // 控制销毁速度的拖动条
        [SerializeField]private ShapeFactory shapeFactory;              // 创建物体的工厂类
        [SerializeField]private PersistentStorage storage;              // 持久化存储管理器
        [SerializeField]private KeyCode createKey = KeyCode.C;          // 创建物体的按键
        [SerializeField]private KeyCode newGameKey = KeyCode.N;         // 新游戏按键
        [SerializeField]private KeyCode saveKey = KeyCode.S;            // 保存按键
        [SerializeField]private KeyCode loadKey = KeyCode.L;            // 加载按键
        [SerializeField]private KeyCode destroyKey = KeyCode.X;         // 销毁物体按键
        [SerializeField]private List<Shape> shapes = new List<Shape>(); // 场景中所有生成物体的引用
        [SerializeField]private int levelCount;                         // 总共的关卡数量
        [SerializeField] private bool reseedOnLoad;                     // 加载时是否重新设定随机种子
        
        private float creationProgress;         // 创建形状进度，满1就会执行一次创建
        private float destructionProgress;      // 销毁形状进度，满1就会执行一次销毁
        private int loadedLevelBuildIndex;      // 当前加载场景的index
        private Random.State mainRandomState;   // 主随机流状态

        // private void OnEnable()
        // {
        //     // 为了在重编译后恢复，我们可以在 OnEnable 方法中也设置这个属性。Unity 会在组件每次被启用时调用 OnEnable
        //     Instance = this;
        // }

        private void Start () {
            mainRandomState = Random.state; // 这里我们保存一次随机序列，此时我们对随机序列没有做任何处理，将这个结果作为主随机序列
            //Debug.Log($"Start::{JsonUtility.ToJson(mainRandomState)}");
            shapes = new List<Shape>();

            if (Application.isEditor)
            {
                // 防止重复加载场景，如果已经加载了，直接将其激活并返回
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene loadedScene = SceneManager.GetSceneAt(i);
                    if (loadedScene.name.Contains("Level "))
                    {
                        SceneManager.SetActiveScene(loadedScene);
                        loadedLevelBuildIndex = loadedScene.buildIndex;
                        return;
                    }
                }
            }
            BeginNewGame();
            StartCoroutine(LoadLevel(1));
        }
        private void Update()
        {
            HandleInput();
            
        }

        private void FixedUpdate()
        {
            #region CreateAndDestroyShape

            creationProgress += Time.deltaTime * CreationSpeed;
            destructionProgress += Time.deltaTime * DestructionSpeed;
            while (creationProgress >= 1f)
            {
                creationProgress -= 1f;
                CreateShape();
            }
            while (destructionProgress >= 1f)
            {
                destructionProgress -= 1f;
                DestroyShape();
            }

            #endregion

            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].GameUpdate();
            }
        }

        private void BeginNewGame()
        {
            // 1. 切到主随机流
            Random.state = mainRandomState;
            // 2. 主随机流生成一个新种子
            int seed = Random.Range(0, int.MaxValue);
            // 3. 推进主随机流
            mainRandomState = Random.state;
            //Debug.Log($"BeginNewGame::{JsonUtility.ToJson(mainRandomState)}");
            // 4. 用新种子初始化本局游戏的随机流
            Random.InitState(seed);
            // 重置生成进度和速度
            creationSpeedSlider.value = CreationSpeed = 0;
            creationProgress = 0;
            destructionSpeedSlider.value = DestructionSpeed = 0;
            destructionProgress = 0;
            if (shapes == null) return;

            foreach (var obj in shapes)
            {
                //Destroy(obj.gameObject);
                shapeFactory.Reclaim(obj);
            }
            shapes.Clear();
        }

        #region SaveAndLoadData

        public override void Save(GameDataWriter writer) {
            writer.Write(shapes.Count);
            writer.Write(Random.state);
            writer.Write(CreationSpeed);
            writer.Write(creationProgress);
            writer.Write(DestructionSpeed);
            writer.Write(destructionProgress);
            writer.Write(loadedLevelBuildIndex);
            GameLevel.Current.Save(writer);
            for (int i = 0; i < shapes.Count; i++)
            {
                // 实际写入形状编号到存档文件里面
                writer.Write(shapes[i].ShapeID);
                writer.Write(shapes[i].MaterialID);
                shapes[i].Save(writer);
            }
            Debug.Log("保存完成");
        }

        public override void Load (GameDataReader reader) {
//            Debug.Log("public override void Load (GameDataReader reader)");
            int version = reader.Version;
//            Debug.Log("version = " + version);
            if (version > saveVersion) {
                Debug.LogError("Unsupported future save version " + version);
                return;
            }
            StartCoroutine(LoadGame(reader));
            Debug.Log("加载完成");
        }

        IEnumerator LoadGame(GameDataReader reader)
        {
            int version = reader.Version;
            // 这样，当版本号 ≤ 0 时，我们就知道这是旧文件，那我们第一次读的version数据实际上就是count
            int count = version <= 0 ? -version : reader.ReadInt();

            if (version >= 3)
            {
                Random.State state = reader.ReadRandomState();
                Debug.Log($"Load::{JsonUtility.ToJson(Random.state)}");
                if (!reseedOnLoad)
                {
                    Random.state = state;
                }
                // 读取创建的速度和进度以及销毁的速度和进度
                creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
                destructionSpeedSlider.value = creationProgress = reader.ReadFloat();
                DestructionSpeed = reader.ReadFloat();
                destructionProgress = reader.ReadFloat();
            }
            
            yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
            if (version >= 3)
            {
                GameLevel.Current.Load(reader);
            }
            // 正式开始创建时也要先读取形状数据，如果版本号大于0，说明我们写入过形状数据，要读取一次，否则直接设置为0
            for (int i = 0; i < count; i++)
            {
                int shapeID = version > 0 ? reader.ReadInt() : 0;
                int materialID = version > 0 ? reader.ReadInt() : 0;
                //Debug.Log($"{i}: {shapeID}");
                Shape instance = shapeFactory.Get(shapeID, materialID);
                instance.Load(reader);
                shapes.Add(instance);
            }
        }
        
        #endregion

        #region CreateShape

        void CreateShape () {
            Shape instance = shapeFactory.GetRandom();
            GameLevel.Current.ConfigureSpawn(instance);
            shapes.Add(instance);
        }

        #endregion
        
        #region DestroyShape

        private void DestroyShape()
        {
            if (shapes.Count == 0) return;
            int index = Random.Range(0, shapes.Count);
            //Destroy(shapes[index].gameObject);
            shapeFactory.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
        }

        #endregion

        #region LoadLevel

        /// <summary>
        /// 加载指定的场景并添加到当前场景
        /// </summary>
        /// <param name="levelBuildIndex">场景在build时的序号</param>
        /// <returns></returns>
        private IEnumerator LoadLevel(int levelBuildIndex)
        {
            // 这里的异步加载需要消耗时间并且不是阻塞式的，update依然会执行，我们要防止用户在加载完成之前的操作被读取并处理
            enabled = false;

            if (loadedLevelBuildIndex > 0)
            {
                yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
            }
            
            // 这里的第二个参数是为了声明加载的场景不是替换而是加在当前已经打开的场景
            yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
            // 并且我们还要切换当前的ActiveScene
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
            loadedLevelBuildIndex = levelBuildIndex;
            enabled = true;
        }

        #endregion

        #region HandleInput

        private void HandleInput()
        {
            if (Input.GetKeyDown(createKey))
            {
                CreateShape();
            }
            else if (Input.GetKeyDown(newGameKey))
            {
                BeginNewGame();
                StartCoroutine(LoadLevel(loadedLevelBuildIndex));
            }
            else if (Input.GetKeyDown(saveKey)) {
                storage.Save(this, saveVersion);
            }
            else if (Input.GetKeyDown(loadKey)) {
                BeginNewGame();
                storage.Load(this);
            }
            else if (Input.GetKeyDown(destroyKey))
            {
                DestroyShape();
            }
            else
            {
                for (int i = 1; i <= levelCount; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    {
                        BeginNewGame();
                        StartCoroutine(LoadLevel(i));
                        return;
                    }
                }
            }
        }

        #endregion
    }
}