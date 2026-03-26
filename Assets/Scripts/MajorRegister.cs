using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using Google.Protobuf;

public class MajorRegister : MonoBehaviour
{
    public GuestAuthConfig config;

    public TMP_InputField nickname;
    public Button MajorRegisterBtn;

    private const int PLATFORM = 4;
    private const int CLIENT_TYPE = 2;
    private const string LANG = "en";

    void Start()
    {
        if (SessionManager.Instance == null)
        {
            Debug.LogError("SessionManager not initialized!");
            return;
        }

        MajorRegisterBtn.onClick.AddListener(onClick);
    }

    IEnumerator MajorRegisterTask()
    {
        MajorRegisterReq req = new MajorRegisterReq
        {
            Nickname = nickname.text,
            AccessToken = SessionManager.Instance.accessToken,
            OpenId = SessionManager.Instance.openId,
            Platform = PLATFORM,
            PlatformRegisterInfo = Google.Protobuf.ByteString.CopyFrom(
                BuildPlatformRegisterInfo(SessionManager.Instance.openId)
            ),
            Lang = LANG,
            ClientType = CLIENT_TYPE,
            Uid = SessionManager.Instance.uid
        };

        byte[] plainProto = req.ToByteArray();
        Debug.Log("MajorRegister proto size: " + plainProto.Length);

        byte[] cipherBytes = EncryptAes(plainProto);

        UnityWebRequest uwr = new UnityWebRequest(config.MajorRegisterUrl, "POST");
        uwr.uploadHandler = new UploadHandlerRaw(cipherBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/octet-stream");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("MajorRegister failed");
            Debug.LogError("HTTP " + uwr.responseCode);
            Debug.LogError(uwr.downloadHandler.text);
            yield break;
        }

        byte[] respBytes;
        try
        {
            respBytes = HexToBytes(uwr.downloadHandler.text.Trim());
        }
        catch
        {
            Debug.LogError("Invalid hex response");
            yield break;
        }

        MajorRegisterRes resp = new MajorRegisterRes();
        resp.MergeFrom(respBytes);

        SessionManager.Instance.accountId = resp.AccountId;

        Debug.Log("MajorRegister SUCCESS");
        Debug.Log("Account ID: " + resp.AccountId);

        PlayerPrefs.SetString("accountId", SessionManager.Instance.accountId.ToString());

        SceneManager.LoadSceneAsync("MajorLogin");
    }

    byte[] BuildPlatformRegisterInfo(string openId)
    {
        byte[] data = Encoding.UTF8.GetBytes(openId);

        byte[] xorKey = new byte[]
        {
            0x30,0x30,0x30,0x32,0x30,0x31,0x37,0x30,
            0x30,0x30,0x30,0x30,0x32,0x30,0x31,0x37,
            0x30,0x30,0x30,0x30,0x30,0x32,0x30,0x31,
            0x37,0x30,0x30,0x30,0x30,0x30,0x32,0x30
        };

        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
            result[i] = (byte)(data[i] ^ xorKey[i % xorKey.Length]);

        return result;
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
        byte[] bytes = new byte[len];
        for (int i = 0; i < len; i++)
            bytes[i] = System.Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }

    void onClick()
    {
        StartCoroutine(MajorRegisterTask());
    }
}
