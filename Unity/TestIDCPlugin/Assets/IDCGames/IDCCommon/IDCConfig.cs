using System;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
#endif

using UnityEngine;
public class IDCConfig : ScriptableObject {
    public string email;
    public string GameID;
    public string GameSecret;
#if UNITY_EDITOR
    [HideInInspector]
    public ReleasesData releasesData;

    public static IDCConfig instance;
    public static IDCConfig Get() {
        if (instance == null ) {
            string[] configs = AssetDatabase.FindAssets("t:IDCConfig");
            if (configs.Length != 0) {
                instance = AssetDatabase.LoadAssetAtPath<IDCConfig>( AssetDatabase.GUIDToAssetPath(configs[0]));
            } else {
                instance = ScriptableObject.CreateInstance<IDCConfig>();
                AssetDatabase.CreateAsset(instance, "Assets/Plugins/IDCGames/IDCConfig.asset");
                AssetDatabase.SaveAssets();
            }
        }
        return instance;
    }

    public delegate void callback<T>(T data);

    [System.Serializable]
    public class ReleaseData {
        public int id;
        public string name;
        public string exe;
        public string size;
        public string published;
        public bool active;
        public string splash;
        public string ico;
        public Texture2D iconTexture { get; private set; }
        UnityWebRequest iconRequest;
        public int sync_status;

        public bool needUpdate;
        public string accessUntil;
        public string ftpUser="";
        public string ftpPass="";

        public EditorCoroutine checkPublishingCoroutine { get; set; }


        public Texture2D GetIconTexture() {
            if (iconTexture== null && iconRequest == null) {
                iconRequest = UnityWebRequestTexture.GetTexture(splash);
                iconRequest.SendWebRequest().completed += (op) => {
                    iconTexture = DownloadHandlerTexture.GetContent(iconRequest);
                };
            } 
            return iconTexture;
        }

    }

    [System.Serializable]
    public class ReleasesData {
        public ReleaseData[] data;
    }
    [System.Serializable]
    public class NewRelease {
        public ReleaseData data;
    }
    public static void GetReleases(callback<ReleasesData> callback=null) {
        EditorUtility.DisplayProgressBar("IDC Games", "Getting releass", 0);
        Get();
        UnityWebRequest www = UnityWebRequest.Get("https://admin.idcgames.com/api/release");
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();        
        operation.completed += (op) => {
            EditorUtility.ClearProgressBar();
            var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            var data = JsonUtility.FromJson<ReleasesData>(resp);
            instance.releasesData = data;
            callback?.Invoke(data); 
        };
    }
    public class SuccessResponse {
        public bool success;
        public string message;
    }

    public class FtpAccessSuccessResponse: SuccessResponse {
        public DateTime accessUntil;
    }
    public static void GetFTPAccess(int release, callback<FtpAccessSuccessResponse> callback) {
        EditorUtility.DisplayProgressBar("IDC Games", "Grating FTP access", 0);
        Get();
        WWWForm form = new WWWForm();
        UnityWebRequest www = UnityWebRequest.Post($"https://admin.idcgames.com/api/release/ftp/{release}?rel_dir=common&hours=1&email={instance.email}","{}");
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();
        operation.completed += (op) => {
            EditorUtility.ClearProgressBar();
            var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            var data = JsonUtility.FromJson<FtpAccessSuccessResponse>(resp);
            if (data.success) {
                data.accessUntil = DateTime.Now.AddHours(1);
                Debug.Log($"release({release})/ftp -> RES: {resp}");
                callback?.Invoke(data);
            } else {

            }

        };
    }

    public static void CreateNewRelease(string name, string exe, callback<NewRelease> callback=null) {
        EditorUtility.DisplayProgressBar("IDC Games", "Creating New Release", 0);
        Get();
        WWWForm form = new WWWForm();
        form.AddField("rel_name", name);
        form.AddField("exe", exe);

        UnityWebRequest www = UnityWebRequest.Post($"https://admin.idcgames.com/api/release", form);
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();
        operation.completed += (op) => {
            EditorUtility.ClearProgressBar();
            var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            Debug.Log($"api/release -> RES: {resp}");
            var data = JsonUtility.FromJson<NewRelease>(resp);
            callback?.Invoke(data);
        };

    }

    public static void UpdateRelease(int release, string name, string exe, callback<SuccessResponse> callback) {
        EditorUtility.DisplayProgressBar("IDC Games", "Updating data", 0);
        Get();

        WWWForm form = new WWWForm();
        form.AddField("rel_name", name);
        form.AddField("exe", exe);

        UnityWebRequest www = UnityWebRequest.Post($"https://admin.idcgames.com/api/release/update/{release}", form);
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();
        operation.completed += (op) => {
            EditorUtility.ClearProgressBar();
            var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            Debug.Log($"release({release})/update -> RES: {resp}");
            var data = JsonUtility.FromJson<SuccessResponse>(resp);
            callback?.Invoke(data);
        };
    }
    public static void PublishRelease(int release, callback<SuccessResponse> callback = null)
    {
        EditorUtility.DisplayProgressBar("IDC Games", "Publishing Release", 0);
        Get();
        UnityWebRequest www = UnityWebRequest.Post($"https://admin.idcgames.com/api/release/publish/{release}", "{}");
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();
        operation.completed += (op) => {
            EditorUtility.ClearProgressBar();
            var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            var data = JsonUtility.FromJson<SuccessResponse>(resp);
            if (data.success) callback?.Invoke(data);
        };
    }
    public class CheckPublishStatusResponse
    {
        public int status;
    }
    public static IEnumerator CheckPublishStatus(int release, callback<CheckPublishStatusResponse> callback = null) {
        UnityWebRequest www = UnityWebRequest.Get($"https://admin.idcgames.com/api/release/check_sync/{release}");
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        yield return www.SendWebRequest();
        var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
        var data = JsonUtility.FromJson<CheckPublishStatusResponse>(resp);
        callback?.Invoke(data);
    }

    public class ActivateResponse : SuccessResponse
    {
    }

    public static void ActivateRelease(int release, bool activate, callback<ActivateResponse> callback = null) {
        EditorUtility.DisplayProgressBar("IDC Games", "Activate Release", 0);
        Get();
        UnityWebRequest www = UnityWebRequest.Post($"https://admin.idcgames.com/api/release/activate/{release}?active={(activate ? 1:0)}","{}");
        www.SetRequestHeader("GameID", instance.GameID);
        www.SetRequestHeader("GameSecret", instance.GameSecret);
        UnityWebRequestAsyncOperation operation = www.SendWebRequest();
        operation.completed += (op) => {
            EditorUtility.ClearProgressBar();
            var resp = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            var data = JsonUtility.FromJson<ActivateResponse>(resp);
            if (data.success) callback?.Invoke(data);
        };
    }
#endif
}
