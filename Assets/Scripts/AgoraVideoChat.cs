using System.Collections.Generic;
using UnityEngine; 
using agora_gaming_rtc;

public class AgoraVideoChat : Photon.MonoBehaviour
{
    [SerializeField]
    private string appID = "57481146914f4cddaa220d6f7a045063";
    [SerializeField]
    private string channel = "unity3d";
    private string originalChannel;
    private IRtcEngine mRtcEngine;

    [Header("Misc.")]
    [SerializeField]
    private uint myUID = 0;
    [SerializeField]
    private int currentUserCount = 0;


    [Header("Player Video Panel")]
    [SerializeField]
    public GameObject userVideoPrefab;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private float spaceBetweenUserVideos = 150f;

    [Space]
    public List<GameObject> playerVideoList;

    void Start()
    {
        if (!photonView.isMine)
            return;

        if(mRtcEngine != null)
        {
            IRtcEngine.Destroy();
        }

        originalChannel = channel;

        mRtcEngine = IRtcEngine.GetEngine(appID);

        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        mRtcEngine.OnUserJoined = OnUserJoinedHandler;
        mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
        mRtcEngine.OnUserOffline = OnUserOfflineHandler;

        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();

        mRtcEngine.JoinChannel(channel, null, 0);
    }


    uint test;
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            CreateUserVideoSurface((uint)currentUserCount + 1, false);
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            RemoveUserVideoSurface(test);
        }
    }

    public string GetLocalChannel()
    {
        return channel;
    }

    public void JoinRemoteChannel(string remoteChannelName)
    {
        if (!photonView.isMine)
            return;

        mRtcEngine.LeaveChannel();

        mRtcEngine.JoinChannel(remoteChannelName, null, myUID);
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();

        channel = remoteChannelName;
    }

    // Local Client Joins Channel.
    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        myUID = uid;

        CreateUserVideoSurface(uid, true); 
    }

    // Remote Client Joins Channel.
    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        CreateUserVideoSurface(uid, false);
    }

    // User Leaves Channel.
    void OnLeaveChannelHandler(RtcStats stats)
    {
        if (!photonView.isMine)
            return;

        currentUserCount--;
    }

    // Remote User Leaves the Channel.
    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        if (!photonView.isMine)
            return;

        RemoveUserVideoSurface(uid);
    }

    // Create new image plane to display users in party
    void CreateUserVideoSurface(uint uid, bool isLocalUser)
    {
        // Get the next position for newly created VideoSurface
        float spawnY = currentUserCount * spaceBetweenUserVideos;
        Vector3 spawnPosition = new Vector3(0, -spawnY, 0);

        // Create Gameobject holding video surface
        GameObject newUserVideo = Instantiate(userVideoPrefab, spawnPosition, spawnPoint.rotation);
        newUserVideo.name = uid.ToString();
        if(newUserVideo == null)
        {
            Debug.LogError("CreateUserVideoSurface() - newUserVideoIsNull");
            return;
        }

        playerVideoList.Add(newUserVideo);

        // Update our VideoSurface to reflect new users
        VideoSurface newVideoSurface = newUserVideo.GetComponent<VideoSurface>();
        if(newVideoSurface == null)
        {
            Debug.LogError("CreateUserVideoSurface() - VideoSurface component is null on newly joined user");
        }
        if (isLocalUser == false)
        {
            newVideoSurface.SetForUser(uid);
        }
        newVideoSurface.SetGameFps(30);

        // Update our "Content" container that holds all the image planes
        content.sizeDelta = new Vector2(0, currentUserCount * spaceBetweenUserVideos + 140);

        newUserVideo.transform.SetParent(spawnPoint, false);
        newUserVideo.transform.rotation = Quaternion.Euler(Vector3.right * -180);

        currentUserCount++;        
    }

    void RemoveUserVideoSurface(uint deletedUID)
    {
        currentUserCount--;

        foreach (GameObject player in playerVideoList)
        {
            if (player.name == deletedUID.ToString())
            {
                // remove videoview from list
                playerVideoList.Remove(player);
                // delete it
                Destroy(player.gameObject);
                break;
            }
        }

        // update positions of new players
        for (int i = 0; i < playerVideoList.Count; i++)
        {
            print(i + " old position: " + playerVideoList[i].GetComponent<RectTransform>().anchoredPosition);
            playerVideoList[i].GetComponent<RectTransform>().anchoredPosition = Vector2.down * 150 * i;
            print(i + " new position: " + playerVideoList[i].GetComponent<RectTransform>().anchoredPosition);
        }

        Vector2 oldContent = content.sizeDelta;
        content.sizeDelta = oldContent + Vector2.down * 150;
        content.anchoredPosition = Vector2.zero;
    }

    private void OnApplicationQuit()
    {
        currentUserCount--;
        if(mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine = null;
            IRtcEngine.Destroy();
        }
    }
}