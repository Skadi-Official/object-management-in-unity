using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
namespace ObjectManagement
{
    /// <summary>
    /// game也是需要被保存的对象，所以game本身也是一个PersistableObject
    /// </summary>
    public class Game : PersistableObject 
    {
        const int saveVersion = 2; // 存档版本标记
        public static Game Instance { get; private set; }
        public SpawnZone SpawnZoneOfLevel { get; set; }         // 生成区域，属于关卡不属于通用场景
        public float CreationSpeed { get; set; }
        public float DestructionSpeed { get; set; }
        [SerializeField]private ShapeFactory shapeFactory;
        [SerializeField]private PersistentStorage storage;
        [SerializeField]private KeyCode createKey = KeyCode.C;
        [SerializeField]private KeyCode newGameKey = KeyCode.N;
        [SerializeField]private KeyCode saveKey = KeyCode.S;
        [SerializeField]private KeyCode loadKey = KeyCode.L;
        [SerializeField]private KeyCode destroyKey = KeyCode.X;
        [SerializeField]private List<Shape> shapes = new List<Shape>();
        [SerializeField]private int levelCount;              // 总共的关卡数量
        
        private float creationProgress;     // 创建形状进度，满1就会执行一次创建
        private float destructionProgress;  // 摧毁形状进度，满1就会执行一次销毁
        private int loadedLevelBuildIndex;  // 当前加载场景的index

        private void OnEnable()
        {
            // 为了在重编译后恢复，我们可以在 OnEnable 方法中也设置这个属性。Unity 会在组件每次被启用时调用 OnEnable
            Instance = this;
        }

        private void Start () {
            Instance = this;
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
            
            StartCoroutine(LoadLevel(1));
        }
        private void Update()
        {
            HandleInput();

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
            
        }

        

        private void BeginNewGame()
        {
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
            writer.Write(loadedLevelBuildIndex);
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

            // 这样，当版本号 ≤ 0 时，我们就知道这是旧文件，那我们第一次读的version数据实际上就是count
            int count = version <= 0 ? -version : reader.ReadInt();
            StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
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
            Debug.Log("加载完成");
        }

        #endregion

        #region CreateShape

        void CreateShape () {
            Shape instance = shapeFactory.GetRandom();
            Transform t = instance.transform;
            t.localPosition = SpawnZoneOfLevel.SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * Random.Range(0.1f, 1f);
            instance.SetColor(Random.ColorHSV(
                hueMin: 0f, hueMax: 1f,
                saturationMin: 0.5f, saturationMax: 1f,
                valueMin: 0.25f, valueMax: 1f,
                alphaMin: 1f, alphaMax: 1f
            ));
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


