﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using agora_utilities;
using UnityEngine.UI;
using Photon;

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
    public int currentUsers = 0;
    

    public Vector3 videoFramePosition;
    public float newVideoFrameOffsetAmount = 140;

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

    // local client joins
    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        //print("local user joined - channel: " + channelName + " - uid: " + uid + " - elapsed: " + elapsed);
        myUID = uid;

        CreateUserVideoSurface(uid, videoFramePosition + (Vector3.right * currentUsers), true);

        //print("userCount: " + currentUsers);
    }

    // remote client joins
    void OnUserJoinedHandler(uint uid, int elapsed)
    {
        if (!photonView.isMine)
            return;

        //print("remote user joined - uid: " + uid + " - elapsed: " + elapsed);

        VideoSurface surface = CreateUserVideoSurface(uid, videoFramePosition + (Vector3.right * currentUsers), false);

        //print("userCount: " + currentUsers);
    }

    // user leaves
    void OnLeaveChannelHandler(RtcStats stats)
    {
        if (!photonView.isMine)
            return;

        //print("User left");
        currentUsers--;
    }

    // when remote user leaves the channel
    void OnUserOfflineHandler(uint uid, USER_OFFLINE_REASON reason)
    {
        if (!photonView.isMine)
            return;

        //print("remote user offline - uid: " + uid + " - reason: " + reason);

        currentUsers--;
        //print("userCount: " + currentUsers);

        Destroy(GameObject.Find(uid.ToString()));
    }

    VideoSurface CreateUserVideoSurface(uint uid, Vector3 spawnPosition, bool isLocalUser)
    {

        GameObject newUserVideo = new GameObject(uid.ToString(), typeof(RawImage), typeof(VideoSurface));
        if(newUserVideo == null)
        {
            print("VideoSurface() - new user video <GAMEOBJECT> couldn't be created: " + gameObject.name);
            return null;
        }

        newUserVideo.transform.parent = transform.GetChild(0);
        newUserVideo.GetComponent<RectTransform>().anchoredPosition = spawnPosition + (Vector3.right * currentUsers * newVideoFrameOffsetAmount);
        newUserVideo.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120);
        newUserVideo.transform.rotation = Quaternion.Euler(Vector3.right * -180);
        newUserVideo.transform.localScale = Vector3.one;

        VideoSurface newVideoSurface = newUserVideo.GetComponent<VideoSurface>();
        if (newVideoSurface == null)
        {
            //print("VideoSurface() - new user video <VIDEOSURFACE> couldn't be created: " + gameObject.name);
            return null;
        }

        if (isLocalUser == false)
        {
            newVideoSurface.SetForUser(uid);
        }

        //print(gameObject.name + " creating new video surface for: " + uid);

        newVideoSurface.SetGameFps(30);

        currentUsers++;

        return newVideoSurface;
    }

    private void OnApplicationQuit()
    {
        currentUsers--;
        if(mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            mRtcEngine = null;
            IRtcEngine.Destroy();
        }
    }
}