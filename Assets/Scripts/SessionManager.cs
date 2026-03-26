using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;

    public long uid;
    public string guestPassword;

    public string accessToken;
    public string openId;

    public long accountId;

    public string jwt;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}