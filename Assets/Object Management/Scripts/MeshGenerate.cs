using UnityEngine;

[ExecuteAlways]
public class MeshGenerate : MonoBehaviour
{
    //顶点数据
    [SerializeField]
    private Vector3[] vertices;
    //三角形索引
    [SerializeField]
    private int[] triangles;
    //uv坐标数据
    [SerializeField]
    private Vector2[] uvs;

    void Start()
    {
        //画三角形需要三个顶点，定义三个顶点坐标，这里的坐标是相对于物体的坐标，也就是LocalPosirion
        vertices = new Vector3[]{
            // 顶点1
            new Vector3(-2.0f, 5.0f, -2.0f),//[0]
            // 顶点2
            new Vector3(-2.0f, 0.0f, -2.0f),//[1]
            // 顶点3
            new Vector3(2.0f, 0.0f, -2.0f),//[2]
        };

        //定义顶点顺序，因为要绘制正面，所以按顺时针排序，记得是遵循左手坐法则，不理解左手法则的一定要理解
        triangles =new int[]{
             2,1,0,
        };

        uvs = new Vector2[]{
            // 顶点1的uv，对应上面的vertices[0]
            new Vector2(0.5f, 1.0f),
            // 顶点2的uv，对应上面的vertices[1]
            new Vector2(0.0f, 0.0f),
            // 顶点3的uv，对应上面的vertices[2]
            new Vector2(1.0f, 0.0f),
        };

        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }
    void Generate()
    {
        // 新建一个Mesh
        Mesh mesh = new Mesh();
        // 用构建的数据初始化Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        // 法线是根据顶点数据计算出来的,所以在修改完顶点后,需要更新一下法线
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        // 将构建好的Mesh替换上，节点上需要挂载MeshFilter组件和MeshRenderer组件
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDrawGizmos()
    {
        //这里是把法线和切线在Scene窗口上绘制出来
        var mesh = gameObject.GetComponent<MeshFilter>().mesh;
        if(mesh == null)
        {
            return;
        }
        var normals = mesh.normals;
        var vers = mesh.vertices;
        var tan = mesh.tangents;
        for (int i = 0; i < vers.Length; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(vers[i], vers[i] + normals[i]);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(vers[i], vers[i] + new Vector3(tan[i].x, tan[i].y, tan[i].z));
        }
    }
}
