using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class guest : MonoBehaviour
{
    public GuestAuthConfig config;

    private const string CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public Button guestBtn;
    public Button tapToBeginBtn;

    public GameObject ui_guestBtn;
    public GameObject ui_logoutBtn;
    public GameObject ui_tapToBeginBtn;

    [System.Serializable]
    class GrantResponse
    {
        public GrantData data;
    }

    [System.Serializable]
    class GrantData
    {
        public string access_token;
        public string open_id;
    }

    bool newGuest;

    void Start()
    {
        ui_tapToBeginBtn.SetActive(false);

        if (loadGuest())
        {
            Debug.Log("Guest already exists. UID = " + SessionManager.Instance.uid);
            grant();
            ui_guestBtn.SetActive(false);
            ui_logoutBtn.SetActive(true);
        }
        else
        {
            ui_guestBtn.SetActive(true);
            ui_logoutBtn.SetActive(false);
        }

        guestBtn.onClick.AddListener(onClick);
        tapToBeginBtn.onClick.AddListener(tapToBegin);
    }

    string createPassword()
    {
        StringBuilder password = new StringBuilder(64);

        for (int i = 0; i < 64; i++)
        {
            int index = Random.Range(0, CHARSET.Length);
            password.Append(CHARSET[index]);
        }

        return password.ToString();
    }

    string HmacSha256Hex(string hexKey, string message)
    {
        byte[] keyBytes = new byte[hexKey.Length / 2];
        for (int i = 0; i < keyBytes.Length; i++)
            keyBytes[i] = byte.Parse(hexKey.Substring(i * 2, 2), NumberStyles.HexNumber);

        using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            StringBuilder sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
    }

    bool loadGuest()
    {
        if (!PlayerPrefs.HasKey("uid") || !PlayerPrefs.HasKey("password"))
            return false;

        if (!long.TryParse(PlayerPrefs.GetString("uid"), out long uid))
            return false;

        SessionManager.Instance.uid = uid;
        SessionManager.Instance.guestPassword = PlayerPrefs.GetString("password");

        return true;
    }


    void register()
    {
        SessionManager.Instance.guestPassword = createPassword();
        StartCoroutine(RegisterCoroutine());
    }

    IEnumerator RegisterCoroutine()
    {
        string body =
            "password=" + SessionManager.Instance.guestPassword +
            "&client_type=" + config.client_type +
            "&source=" + config.source +
            "&app_id=" + config.app_id;

        string signature = HmacSha256Hex(config.client_secret, body);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        UnityWebRequest req = new UnityWebRequest(config.registerUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();

        req.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        req.SetRequestHeader("User-Agent", "AfterDuskSDK/1.0(Android 12; en; US)");
        req.SetRequestHeader("Authorization", "Signature " + signature);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("/oauth/guest:register failed");
            Debug.LogError("HTTP Code: " + req.responseCode);
            Debug.LogError("Response: " + req.downloadHandler.text);
            yield break;
        }

        Debug.Log("/oauth/guest:register response: " + req.downloadHandler.text);

        string response = req.downloadHandler.text;
        int uidIndex = response.IndexOf("\"uid\":");
        if (uidIndex == -1)
        {
            Debug.LogError("UID not found in response");
            yield break;
        }

        string uidStr = "";
        for (int i = uidIndex + 6; i < response.Length; i++)
        {
            char c = response[i];
            if (char.IsDigit(c)) uidStr += c;
            else break;
        }

        SessionManager.Instance.uid = long.Parse(uidStr);
        Debug.Log("Guest UID: " + SessionManager.Instance.uid);

        newGuest = true;

        grant();
    }

    bool grantInProgress = false;

    void grant()
    {
        if (grantInProgress) return;
        grantInProgress = true;
        StartCoroutine(GrantCoroutine());
    }

    IEnumerator GrantCoroutine()
    {
        string jsonPayload =
            "{"
            + "\"client_id\":" + config.app_id + ","
            + "\"client_secret\":\"" + config.client_secret + "\","
            + "\"client_type\":" + config.client_type + ","
            + "\"password\":\"" + SessionManager.Instance.guestPassword + "\","
            + "\"response_type\":\"token\","
            + "\"uid\":" + SessionManager.Instance.uid
            + "}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest req = new UnityWebRequest(config.grantUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("/oauth/guest/token:grant failed: " + req.error);
            yield break;
        }

        Debug.Log("/oauth/guest/token:grant response: " + req.downloadHandler.text);

        GrantResponse gr = JsonUtility.FromJson<GrantResponse>(req.downloadHandler.text);

        if (gr == null || gr.data == null ||
            string.IsNullOrEmpty(gr.data.access_token) ||
            string.IsNullOrEmpty(gr.data.open_id))
        {
            Debug.LogError("Grant response invalid, resetting session");
            yield break;
        }

        SessionManager.Instance.accessToken = gr.data.access_token;
        SessionManager.Instance.openId = gr.data.open_id;

        if (!PlayerPrefs.HasKey("uid") || !PlayerPrefs.HasKey("password"))
            SaveGuestCredentials();

        PlayerPrefs.SetString("accessToken", SessionManager.Instance.accessToken);
        PlayerPrefs.SetString("openId", SessionManager.Instance.openId);

        ui_guestBtn.SetActive(false);
        ui_logoutBtn.SetActive(true);

        ui_tapToBeginBtn.SetActive(true);

        grantInProgress = false;

        if (!newGuest)
            SceneManager.LoadSceneAsync("MajorLogin");
    }

    void SaveGuestCredentials()
    {
        PlayerPrefs.SetString("uid", SessionManager.Instance.uid.ToString());
        PlayerPrefs.SetString("password", SessionManager.Instance.guestPassword);
        PlayerPrefs.SetString("accessToken", SessionManager.Instance.accessToken);
        PlayerPrefs.SetString("openId", SessionManager.Instance.openId);
        PlayerPrefs.Save();
    }

    public void logout()
    {
        SessionManager.Instance.accessToken = null;
        SessionManager.Instance.openId = null;
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("openId");

        ui_tapToBeginBtn.SetActive(false);
        ui_guestBtn.SetActive(true);
        ui_logoutBtn.SetActive(false);
    }

    void onClick()
    {
        if (loadGuest())
        {
            Debug.Log("Using existing guest");
            grant();
        }
        else
        {
            register();
        }
    }

    void tapToBegin()
    {
        SceneManager.LoadSceneAsync("MajorRegister");
    }
}