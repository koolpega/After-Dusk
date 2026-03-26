using UnityEngine;

[CreateAssetMenu(fileName = "GuestAuthConfig", menuName = "AfterDusk/Guest Auth Config")]
public class GuestAuthConfig : ScriptableObject
{
    public int app_id;
    public int client_type;
    public int source;
    public string client_secret;

    public string registerUrl;
    public string grantUrl;
    public string MajorRegisterUrl;
    public string MajorLoginUrl;
    public string AES_KEY;
    public string AES_IV;
}