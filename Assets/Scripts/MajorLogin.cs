using System;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Google.Protobuf;

public class MajorLogin : MonoBehaviour
{
    public GuestAuthConfig config;

    public Button tapToBeginBtn;

    void Start()
    {
        tapToBeginBtn.onClick.AddListener(tapToBegin);
    }

    IEnumerator MajorLoginTask()
    {
        if (SessionManager.Instance == null ||
            string.IsNullOrEmpty(SessionManager.Instance.accessToken) ||
            string.IsNullOrEmpty(SessionManager.Instance.openId))
        {
            Debug.LogError("Session not ready for MajorLogin");
            yield break;
        }

        MajorLoginReq req = new MajorLoginReq
        {
            Uid = SessionManager.Instance.uid,
            AccessToken = SessionManager.Instance.accessToken,
            OpenId = SessionManager.Instance.openId,
            OpenIdType = "guest",

            EventTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            GameName = Application.productName,

            PlatformId = Application.platform == RuntimePlatform.Android ? 1 : 3,
            PlatformSdkId = Application.platform == RuntimePlatform.Android ? 1 : 0,

            ClientVersion = Application.version,
            ClientUsingVersion = Application.version,
            ClientVersionCode = Application.version,

            ReleaseChannel = Application.isEditor ? "editor" : "production",
            ChannelType = Application.isEditor ? 99 : 1,

            SystemSoftware = SystemInfo.operatingSystem,
            SystemHardware = SystemInfo.deviceModel,
            DeviceType = Application.platform.ToString().ToLower(),
            DeviceModel = SystemInfo.deviceModel,

            TelecomOperator = Application.platform == RuntimePlatform.Android
        ? SystemInfo.operatingSystem
        : "unknown",

            NetworkType = Application.internetReachability.ToString(),
            NetworkTypeA = Application.internetReachability.ToString(),
            NetworkOperatorA = "unknown",

            ScreenWidth = Screen.width,
            ScreenHeight = Screen.height,
            ScreenDpi = Screen.dpi > 0 ? Screen.dpi.ToString("F1") : "unknown",

            ProcessorDetails = SystemInfo.processorType,
            CpuType = SystemInfo.processorCount,
            CpuArchitecture = SystemInfo.processorType,
            CpuArchitectureFlag = SystemInfo.processorCount > 4 ? 1 : 0,

            GpuRenderer = SystemInfo.graphicsDeviceName,
            GpuVersion = SystemInfo.graphicsDeviceVersion,
            GraphicsApi = SystemInfo.graphicsDeviceType.ToString(),

            Memory = SystemInfo.systemMemorySize,
            Language = Application.systemLanguage.ToString().ToLower(),

            LibraryPath = Application.dataPath,
            LibraryToken = SystemInfo.deviceUniqueIdentifier,

            SupportedAstcBitset = 0,

            LoginBy = 1,
            LoginOpenIdType = 1,
            AndroidEngineInitFlag = 1,
            IfPush = 0,
            IsVpn = 0,
            LoadingTime = 0,

            OriginPlatformType = Application.platform.ToString().ToLower(),
            PrimaryPlatformType = Application.platform.ToString().ToLower(),
        };

        byte[] plain = req.ToByteArray();
        byte[] cipher = EncryptAes(plain);

        UnityWebRequest uwr = new UnityWebRequest(config.MajorLoginUrl, "POST");
        uwr.uploadHandler = new UploadHandlerRaw(cipher);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/octet-stream");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("MajorLogin failed");
            Debug.LogError(uwr.responseCode + " " + uwr.downloadHandler.text);
            yield break;
        }

        byte[] respBytes = HexToBytes(uwr.downloadHandler.text.Trim());

        MajorLoginRes resp = new MajorLoginRes();
        resp.MergeFrom(respBytes);

        SessionManager.Instance.accountId = resp.AccountId;
        SessionManager.Instance.jwt = resp.Jwt;

        PlayerPrefs.SetString("jwt", SessionManager.Instance.jwt);

        Debug.Log("MajorLogin OK");
        Debug.Log("AccountID: " + resp.AccountId);
        Debug.Log("JWT: " + resp.Jwt);

        SceneManager.LoadSceneAsync("main");
    }

    byte[] EncryptAes(byte[] plain)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.ASCII.GetBytes(config.AES_KEY);
            aes.IV = Encoding.ASCII.GetBytes(config.AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform enc = aes.CreateEncryptor())
                return enc.TransformFinalBlock(plain, 0, plain.Length);
        }
    }

    byte[] HexToBytes(string hex)
    {
        int len = hex.Length / 2;
        byte[] data = new byte[len];
        for (int i = 0; i < len; i++)
            data[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return data;
    }

    public void logout()
    {
        SessionManager.Instance.accessToken = null;
        SessionManager.Instance.openId = null;
        SessionManager.Instance.jwt = null;
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("openId");
        PlayerPrefs.DeleteKey("jwt");

        SceneManager.LoadSceneAsync("login");
    }

    void tapToBegin()
    {
        StartCoroutine(MajorLoginTask());
    }
}