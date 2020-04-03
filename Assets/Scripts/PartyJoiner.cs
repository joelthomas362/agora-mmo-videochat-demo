using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PartyJoiner : MonoBehaviour
{

    private PhotonView photonView;
    // when the invite is made, send them your channelName
    // when two players separate, nullify/disable everything
    // when the other player presses join, have them join the special channel

    //[SerializeField]
    //private Canvas playerCanvas;

    [SerializeField]
    private Button inviteButton;
    [SerializeField]
    private GameObject joinButton;
    [SerializeField]
    private int remotePlayerID;
    [SerializeField]
    private int remoteButtonID;

    [SerializeField]
    private string remoteInviteChannelName;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if(!photonView.isMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        inviteButton.interactable = false;
        joinButton.gameObject.SetActive(false);
        //joinButton.interactable = false;
    }

    [PunRPC]
    public void InvitePlayerToPartyChannel(string channelName)
    {
        if(photonView.isMine)
        {
            print(gameObject.name);
            remoteInviteChannelName = channelName;
            joinButton.SetActive(true);
            print("I've been invited to join channel: " + remoteInviteChannelName);
        }

        EnableJoinButton();
    }

    public void EnableJoinButton()
    {
        joinButton.SetActive(true);
    }

    [PunRPC]
    public void AllTest()
    {
        print("AllTest() " + gameObject.name);
    }

    [PunRPC]
    public void OtherTest()
    {
        print("OtherTest() " + gameObject.name);
    }

    [PunRPC]
    public void TargetPlayer(string nickName)
    {
        print("MyPlayer: " + gameObject.name + "OtherPlayer: " + nickName);
    }

    // this button press will always be local because the remote clients canvases are disabled
    public void OnInviteButtonPress()
    {
        if(remotePlayerID != -1)
        {
            //print("attempting to access remotePlayerID: " + remotePlayerID);
            //photonView.RPC("InvitePlayerToPartyChannel", PhotonPlayer.Find(remotePlayerID), GetComponent<AgoraVideoChat>().GetRemoteChannel());

            photonView.RPC("AllTest", PhotonTargets.All);
            photonView.RPC("OtherTest", PhotonTargets.Others);
            photonView.RPC("TargetPlayer", PhotonPlayer.Find(remotePlayerID), PhotonPlayer.Find(remotePlayerID).NickName);
            


            //photonView.RPC("InvitePlayerToPartyChannel", PhotonTargets.All, GetComponent<AgoraVideoChat>().GetRemoteChannel());
            
            //print("trying to access " + PhotonPlayer.Find(remotePlayerID).ToStringFull());

        }
    }

    public void OnJoinButtonPress()
    {
        print("I'm going to join players channel: " + remoteInviteChannelName);
        GetComponent<AgoraVideoChat>().JoinRemoteChannel(remoteInviteChannelName);
    }

    // this scripts fire everywhere
    // each of these objects are now essentially in the scene, and you have to sort them out as such.
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.isMine)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            //print("I bumped into: " + other.name);   
            remotePlayerID = PhotonView.Get(other.gameObject).ownerId;
            remoteButtonID = PhotonView.Get(other.transform.GetChild(0).GetChild(1)).ownerId;

            inviteButton.interactable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(!photonView.isMine)
        {
            return;
        }

        if(other.CompareTag("Player"))
        {
            remotePlayerID = -1;    

            inviteButton.interactable = false;
        }
    }
}
