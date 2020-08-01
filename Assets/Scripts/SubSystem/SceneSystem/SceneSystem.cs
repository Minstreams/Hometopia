using System.Collections;using System.Collections.Generic;
using UnityEngine;
using GameSystem.Setting;
using UnityEngine.SceneManagement;

namespace GameSystem
{
    /// <summary>
    /// 场景系统，用于加载卸载场景
    /// </summary>
    public class SceneSystem : SubSystem<SceneSystemSetting>
    {
        /// <summary>
        /// 场景栈
        /// </summary>
        private static Stack<string> sceneStack = new Stack<string>();
        /// <summary>
        /// 场景加载进度事件，用于在加载场景中播放进度加载效果
        /// </summary>
        public static event System.Action<float> InLoadingProgress;
        private static AsyncOperation progress;
        /// <summary>
        /// 场景加载结束事件，用于退出加载效果并延迟一段时间，返回延迟秒数
        /// </summary>
        public static event System.Func<float> OnLoaded;
        /// <summary>
        /// 场景卸载事件，用于卸载前延迟一段时间，并返回延迟秒数
        /// </summary>
        public static event System.Func<float> OnUnLoaded;

        //API---------------------------------
        /// <summary>
        /// 获取委托列表最大返回值，为空则返回0
        /// </summary>
        private static float GetMaxReturn(System.Func<float> func)
        {
            if (func == null) return 0;
            System.Delegate[] list = func.GetInvocationList();
            float max = 0;

            foreach (System.Delegate d in list)
            {
                System.Func<float> f = d as System.Func<float>;
                float temp = f();
                if (temp > max) max = temp;
            }

            return max;
        }
        private static IEnumerator YieldPushScene(string sceneName, bool loadLoadingScene)
        {
            //加载新场景
            progress = SceneManager.LoadSceneAsync(sceneName, sceneStack.Count == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive);

            if (loadLoadingScene)
                //加载Loading场景
                SceneManager.LoadScene(Setting.loadingScene, LoadSceneMode.Additive);

            //加载进度效果
            while (!progress.isDone)
            {
                if (InLoadingProgress != null) InLoadingProgress(progress.progress);
                yield return 0;
            }

            //加载后延迟
            yield return new WaitForSeconds(GetMaxReturn(OnLoaded));

            if (loadLoadingScene)
                //卸载Loading场景
                SceneManager.UnloadSceneAsync(Setting.loadingScene);

            sceneStack.Push(sceneName);
        }
        private static IEnumerator YieldPopScene(float delay)
        {
            yield return new WaitForSeconds(delay);
            string toPop = sceneStack.Pop();
            if (sceneStack.Count > 0)
                SceneManager.UnloadSceneAsync(toPop);
        }
        private static IEnumerator YieldPopAndPushScene(float delay, string sceneName, bool loadLoadingScene)
        {
            yield return YieldPopScene(delay);
            yield return YieldPushScene(sceneName, loadLoadingScene);
        }
        /// <summary>
        /// 新场景入栈
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="loadLoadingScene">是否显示加载场景</param>
        public static void PushScene(string sceneName, bool loadLoadingScene = false)
        {
            Debug.Log("Push场景 " + sceneName);

            StartCoroutine(YieldPushScene(sceneName, loadLoadingScene));
        }
        /// <summary>
        /// 场景出栈,返回延迟秒数
        /// </summary>
        public static float PopScene()
        {
            if (sceneStack.Count == 0)
            {
                Debug.LogError("场景栈空了");
                return 0;
            }
            float delay = GetMaxReturn(OnUnLoaded);
            StartCoroutine(YieldPopScene(delay));
            return delay;
        }
        /// <summary>
        /// 出栈并入栈
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <param name="loadLoadingScene">是否显示加载场景</param>
        public static float PopAndPushScene(string sceneName, bool loadLoadingScene = false)
        {
            if (sceneStack.Count == 0)
            {
                Debug.LogError("场景栈空了");
                return 0;
            }
            float delay = GetMaxReturn(OnUnLoaded);
            StartCoroutine(YieldPopAndPushScene(delay, sceneName, loadLoadingScene));
            return delay;
        }
        /// <summary>
        /// 最简单的加载场景方法
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        //[RuntimeInitializeOnLoadMethod]
        //private static void RuntimeInit()
        //{
        //    //用于控制Action初始化
        //}
    }
}