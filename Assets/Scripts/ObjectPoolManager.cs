using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    #region 单例

    public static ObjectPoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("ObjectPoolManager");
                instance = go.AddComponent<ObjectPoolManager>();
            }
            return instance;
        }
    }
    private static ObjectPoolManager instance = null;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion

    #region 周期

    /// <summary>
    /// 循环检测对象池大小的时间间隔（秒）
    /// </summary>
    public float loopInterval { get; private set; } = 5f;
    /// <summary>
    /// 每次检测所减少的比例
    /// </summary>
    public float loopCheckReduceSacle { get; private set; } = 0.8f;

    private void Start()
    {
        StartCoroutine(LoopCheckPool());
    }

    /// <summary>
    /// 协程，定期执行检查函数
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoopCheckPool()
    {
        while (true)
        {
            yield return new WaitForSeconds(loopInterval);
            CheckPool();
        }
    }

    /// <summary>
    /// 检查所有对象池，缩小其大小
    /// </summary>
    private void CheckPool()
    {
        // Texture2D
        foreach (var key in texture2DQueueDict.Keys)
        {
            int countToReduce = (int)(texture2DQueueDict[key].Count * (1 - loopCheckReduceSacle));
            while (countToReduce > 0)
            {
                texture2DQueueDict[key].Dequeue();
            }
        }

        // RenderTexture
        foreach (var key in renderTextureQueueDict.Keys)
        {
            int countToReduce = (int)(renderTextureQueueDict[key].Count * (1 - loopCheckReduceSacle));
            while (countToReduce > 0)
            {
                // RenderTexture不被GC管理，需要手动Release https://docs.unity.cn/ScriptReference/RenderTexture.html
                renderTextureQueueDict[key].Dequeue().Release();
            }
        }
    }

    #endregion

    #region Texture

    /// <summary>
    /// 回收Texture类型的对象，自动判断具体类型
    /// </summary>
    /// <param name="tex">需要回收的Texture</param>
    /// <returns>是否回收成功（不是设定好的可回收的类型时失败）</returns>
    public bool ReleaseTexture(Texture tex)
    {
        if (tex is RenderTexture rt)
        {
            ReleaseRenderTexture(rt);
            return true;
        }
        else if (tex is Texture2D tex2d)
        {
            ReleaseTexture(tex2d);
            return true;
        }

        return false;
    }

    #endregion

    #region Texture2D

    /// <summary>
    /// Texture2D的对象池队列的字典，key是对应分辨率，value是队列
    /// </summary>
    private Dictionary<Vector2Int, Queue<Texture2D>> texture2DQueueDict = new();

    /// <summary>
    /// 从对象池获取一个Texture（使用完记得Release）
    /// </summary>
    /// <param name="width">Texture的宽度</param>
    /// <param name="height">Texture的高度</param>
    /// <returns>对象池中返回的Texture2D</returns>
	public Texture2D GetTexture2D(int width, int height)
    {
        Vector2Int indexKey = new Vector2Int(width, height);

        // 如果存在的话直接返回
        if (texture2DQueueDict.ContainsKey(indexKey) && texture2DQueueDict[indexKey].Count > 0)
        {
            return texture2DQueueDict[indexKey].Dequeue();
        }

        // 不存在的话创建一个新的
        return new Texture2D(width, height);
    }

    /// <summary>
    /// 回收一个Texture2D
    /// </summary>
    /// <param name="tex"></param>
	public void ReleaseTexture2D(Texture2D tex)
    {
        if (tex == null) return;

        Vector2Int indexKey = new Vector2Int(tex.width, tex.height);

        // 如果不存在这个分辨率的队列先创建一个
        if (!texture2DQueueDict.ContainsKey(indexKey))
        {
            texture2DQueueDict[indexKey] = new Queue<Texture2D>();
        }

        texture2DQueueDict[indexKey].Enqueue(tex);
    }

    #endregion

    #region RenderTexture

    /// <summary>
    /// RenderTexture的对象池队列的字典，key是对应分辨率，value是队列
    /// </summary>
    private Dictionary<Vector2Int, Queue<RenderTexture>> renderTextureQueueDict = new();

    /// <summary>
    /// 从对象池获取一个RenderTexture（使用完记得Release）
    /// </summary>
    /// <param name="width">Texture的宽度</param>
    /// <param name="height">Texture的高度</param>
    /// <returns>对象池中返回的Texture2D</returns>
	public RenderTexture GetRenderTexture(int width, int height)
    {
        Vector2Int indexKey = new Vector2Int(width, height);

        // 如果存在的话直接返回
        if (renderTextureQueueDict.ContainsKey(indexKey) && renderTextureQueueDict[indexKey].Count > 0)
        {
            return renderTextureQueueDict[indexKey].Dequeue();
        }

        // 不存在的话创建一个新的
        return new RenderTexture(width, height, 0);
    }

    /// <summary>
    /// 回收一个RenderTexture
    /// </summary>
    /// <param name="tex"></param>
	public void ReleaseRenderTexture(RenderTexture tex)
    {
        if (tex == null) return;

        Vector2Int indexKey = new Vector2Int(tex.width, tex.height);

        // 如果不存在这个分辨率的队列先创建一个
        if (!renderTextureQueueDict.ContainsKey(indexKey))
        {
            renderTextureQueueDict[indexKey] = new Queue<RenderTexture>();
        }

        renderTextureQueueDict[indexKey].Enqueue(tex);
    }

    #endregion

    #region UIBoundingBox

    private Queue<UIBoundingBox> uiBoundingBoxQueue = new();

    public UIBoundingBox GetUIBoundingBox()
    {
        if (uiBoundingBoxQueue.Count > 0)
        {
            return uiBoundingBoxQueue.Dequeue();
        }

        return Instantiate(ConfigScriptablObject.Instance.uiBoundingBox);
    }

    public void ReleaseUIBoundingBox(UIBoundingBox item)
    {
        uiBoundingBoxQueue.Enqueue(item);
    }

    #endregion

}
