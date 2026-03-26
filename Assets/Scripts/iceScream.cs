using UnityEngine;
using UnityEngine.Video;

public class iceScream : MonoBehaviour
{
    public Vector3 teleportPosition = new Vector3(37.5f, 150f, 75f);
    public VideoPlayer iceScreamVideo;
    public GameObject canvas;
    private MeshCollider trigger;

    void Awake()
    {
        trigger = GetComponent<MeshCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            iceScreamVideo.Play();
            CharacterController cc = other.GetComponent<CharacterController>();
            cc.enabled = false;
            trigger.isTrigger = false;
            Destroy(canvas, 2.45f);
            other.transform.position = teleportPosition;
            cc.enabled = true;
            return;
        }
    }
}