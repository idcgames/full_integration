using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class IDCLibrary : UnityEngine.MonoBehaviour {
    static IDCLibrary Instance;
    protected virtual void Awake() {
        Instance = this;
    }
    protected virtual void OnLogMessage(string message) { Debug.Log($"[IDC] LOG: {message}"); }
    public delegate void UserDataCallback(bool bSuccess, IDCUserData userData);
    public delegate void PurchaseCallback(PurchaseData purchaseData);
    public delegate void ErrorCallback(ErrorResponse errorData);
    public delegate void PurchaseRequestCallback(PurchaseRequestData purchaseRequestData);
    public delegate void PurchaseCloseCallback(PurchaseCloseData purchaseCloseData);

    static UserDataCallback OnUserDataCallback;
    static PurchaseCallback OnPurchaseCallback;
    static PurchaseCloseCallback OnPurchaseCloseCallback;
    static PurchaseRequestCallback OnPurchaseRequestCallback;
    static ErrorCallback OnErrorCallback;

    protected void OnNotifyMessage(Command command, string data) {
       
    }

    private static void ProcessLogMessage(IntPtr data, int len) {
        if (len > 0) {
            var arr = new byte[len];
            Marshal.Copy(data, arr, 0, len);
            string logMessage = Encoding.ASCII.GetString(arr);
            Instance.OnLogMessage(logMessage);
        }
    }
    private static void ProcessNotifyMessage(Command command, IntPtr data, int len) {
        
        if (len > 0) {
            var arr = new byte[len];
            Marshal.Copy(data, arr, 0, len);
            string commandData = Encoding.Unicode.GetString(arr);
            switch (command) {
                case Command.UserData:
                    OnUserDataCallback?.Invoke(true, JsonUtility.FromJson<UserDataResponse>(commandData).extraParams);
                    break;
                case Command.BadUserInitData:
                    OnUserDataCallback?.Invoke(true, JsonUtility.FromJson<UserDataResponse>(commandData).extraParams);
                    break;
                case Command.PurchaseNotification:
                    OnPurchaseCallback?.Invoke(JsonUtility.FromJson<PurchaseData>(commandData));
                    break;
                case Command.NewPurchaseCode:
                    OnPurchaseRequestCallback?.Invoke(JsonUtility.FromJson<PurchaseRequestData>(commandData));
                    OnPurchaseRequestCallback = null;
                    break;
                case Command.PurchaseOk:
                    OnPurchaseCloseCallback?.Invoke(JsonUtility.FromJson<PurchaseCloseData>(commandData));
                    OnPurchaseCloseCallback = null;
                    break;
                case Command.PurchaseClosed:
                    OnPurchaseRequestCallback = null;
                    OnPurchaseCloseCallback = null;
                    break;
                case Command.Error:
                    OnErrorCallback?.Invoke(JsonUtility.FromJson<ErrorResponse>(commandData));
                    break;
                default:
                    Debug.Log($"Unregister Command <{command}> data {data}");
                    break;
            }
        }
    }
    IEnumerator ProcessMessages() {
        while (true) {
            _PullMessage();
            yield return null; 
        };
    }
    Coroutine corutine;
    public int InitServices(string GameID, string GameSecret, UserDataCallback onUserData = null, PurchaseCallback onPurchase = null, ErrorCallback onError=null) {
        OnUserDataCallback = onUserData;
        OnPurchaseCallback = onPurchase;
        OnErrorCallback = onError;
        int iRet = _InitIDCLib(GameID, GameSecret, System.Diagnostics.Process.GetCurrentProcess().Id, ProcessNotifyMessage, ProcessLogMessage, true);
        if (corutine != null) { StopCoroutine(corutine); corutine = null; }
        if (iRet==0 ) corutine = StartCoroutine(ProcessMessages());
        return iRet;
    }
    public int EndServices() {        
        if (corutine != null) { StopCoroutine(corutine); corutine = null; }
        return _EndIDCLib();
    }
    public int Purchase(string transactionID, bool sandbox=false, PurchaseRequestCallback purchaseRequestCallback=null) {
        if (OnPurchaseRequestCallback != null) return -1;
        OnPurchaseRequestCallback= purchaseRequestCallback;
        return _OpenShop(transactionID, "", 0, sandbox);
    }
    public int ClosePurchase(string transactionID, string idcPaymentId, PurchaseCloseCallback purchaseCloseCallback=null) {
        if (OnPurchaseCloseCallback != null) return -1;
        OnPurchaseCloseCallback = purchaseCloseCallback;
        return _ClosePurchase(transactionID, idcPaymentId);
    }
    private void OnDestroy() {
        _EndIDCLib();
    }
    public enum Command : uint {
        Error = 0xfbb65363,
        InitLib = 0x89d4951f,
        EndLib = 0x87a3628f,
        PurchaseInit = 0xffeecce6,
        PurchaseEnd = 0xa9bbb04b,
        UserData = 0x3275c4a, //DTINF
        BadUserInitData = 0xaba83a28, //BDINT
        PurchaseNotification = 0x733f2850, //PPNOT
        NewPurchaseCode = 0xfa4967c3, //NWPRC
        PurchaseOk = 0x8218d08c, //OKPRC
        PurchaseError = 0xbb485d88, //BDPRC
        BadPurchaseCode = 0x6a6ac964, // BDCPR
        PurchaseClosed = 0x6c1ccf80, // PRCHC
        Log = 0x10018888
    }
    [DllImport("IDCLib2.dll", CharSet = CharSet.Unicode)]
    private static extern int _InitIDCLib(string gameId, string token, int ProcessId, NotifyCallBack notifyCallBack, LogCallBack logCallBack, bool useQueue);
    [DllImport("IDCLib2.dll", CharSet = CharSet.Unicode)]
    private static extern int _EndIDCLib();
    [DllImport("IDCLib2.dll", CharSet = CharSet.Unicode)]
    private static extern int _OpenShop(string transactionId, string extra, int userLevel, bool sandbox);
    [DllImport("IDCLib2.dll", CharSet = CharSet.Unicode)]
    private static extern int _ClosePurchase(string transactionId, string idcPaymentId);
    [DllImport("IDCLib2.dll", CharSet = CharSet.Unicode)]
    private static extern int _PullMessage();

    public delegate void NotifyCallBack(Command command, IntPtr data, int len);
    public delegate void LogCallBack(IntPtr data, int len);

    [System.Serializable]
    public struct IDCUserData {
        public int userID;
        public string nick;
        public string email;
        public string language;
        public string country;
        public string currency;
        public string tokenUserGameId;
    }
    public struct UserDataResponse {
        public int status;
        public string description;
        public IDCUserData extraParams;
    }
    public struct PurchaseData {
        public string dllTransaction;
        public string idcpaymentTransaction;
    }
    public struct PurchaseRequestData{
        public string transactionId;
    }
    public struct PurchaseCloseData
    {
        public int status;
        public string description;
    }
    public struct ErrorResponse
    {
        public int status;
        public string description;
    }

}
