using System.Collections.Generic;
using UnityEngine; 
using agora_gaming_rtc;

public class AgoraVideoChat : Photon.MonoBehaviour
{
    [Header("Agora Properties")]
    [SerializeField]
    private string appID = "57481146914f4cddaa220d6f7a045063";
    [SerializeField]
    private string channel = "unity3d";
    private string originalChannel;
    private IRtcEngine mRtcEngine;
    private uint myUID = 0;
    private int currentUserCount = 0;

    [Header("Player Video Panel Properties")]
    [SerializeField]
    public GameObject userVideoPrefab;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private RectTransform content;
    [SerializeField]
    private float spaceBetweenUserVideos = 150f;
    public List<GameObject> playerVideoList;

    public delegate void AgoraCustomEvent();
    public static event AgoraCustomEvent PlayerChatIsEmpty;
    public static event AgoraCustomEvent PlayerChatIsPopulated;

    void Start()
    {
        if (!photonView.isMine)
            return;

        // Setup Agora Engine and Callbacks.
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

    public string GetCurrentChannel() => channel;

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

    /// <summary>
    /// Resets player Agora video chat party, and joins their original channel.
    /// </summary>
    public void JoinOriginalChannel()
    {
        if (!photonView.isMine)
            return;

        currentUserCount = 0;

        if(channel == originalChannel)
        {
            channel = myUID.ToString();
        }
        else if(channel == myUID.ToString())
        {
            channel = originalChannel;
        }

        JoinRemoteChannel(channel);
    }

    #region Agora Callbacks
    // Local Client Joins Channel.
    private void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        myUID = uid;

        CreateUserVideoSurface(uid, true);
    }

    // Remote Client Joins Channel.
    private void OnUserJoinedHandler(uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        CreateUserVideoSurface(uid, false);
    }

    // Local user leaves channel.
    private void OnLeaveChannelHandler(RtcStats stats)
    {
        if (!photonView.isMine)
            return;

        foreach (GameObject player in playerVideoList)
        {
            Destroy(player.gameObject);
        }
        playerVideoList.Clear();

        currentUserCount--;
    }

    // Remote User Leaves the Channel.
    private void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        if (!photonView.isMine)
            return;

        if (playerVideoList.Count <= 1)
        {
            PlayerChatIsEmpty();
        }

        RemoveUserVideoSurface(uid);
    }
    #endregion

    // Create new image plane to display users in party
    private void CreateUserVideoSurface(uint uid, bool isLocalUser)
    {
        // Avoid duplicating Local player video screen
        for (int i = 0; i < playerVideoList.Count; i++)
        {
            if (playerVideoList[i].name == uid.ToString())
            {
                return;
            }
        }

        // Get the next position for newly created VideoSurface
        float spawnY = currentUserCount * spaceBetweenUserVideos;
        Vector3 spawnPosition = new Vector3(0, -spawnY, 0);

        // Create Gameobject holding video surface and update properties
        GameObject newUserVideo = Instantiate(userVideoPrefab, spawnPosition, spawnPoint.rotation);
        if (newUserVideo == null)
        {
            Debug.LogError("CreateUserVideoSurface() - newUserVideoIsNull");
            return;
        }
        newUserVideo.name = uid.ToString();
        newUserVideo.transform.SetParent(spawnPoint, false);
        newUserVideo.transform.rotation = Quaternion.Euler(Vector3.right * -180);

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

        currentUserCount++;

        UpdatePlayerVideoPostions();
        if (playerVideoList.Count > 1)
        {
            PlayerChatIsPopulated();
        }
        else
        {
            PlayerChatIsEmpty();
        }
    }

    private void RemoveUserVideoSurface(uint deletedUID)
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
        UpdatePlayerVideoPostions();

        Vector2 oldContent = content.sizeDelta;
        content.sizeDelta = oldContent + Vector2.down * 150;
        content.anchoredPosition = Vector2.zero;
    }

    private void UpdatePlayerVideoPostions()
    {
        for (int i = 0; i < playerVideoList.Count; i++)
        {
            playerVideoList[i].GetComponent<RectTransform>().anchoredPosition = Vector2.down * 150 * i;
        }
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