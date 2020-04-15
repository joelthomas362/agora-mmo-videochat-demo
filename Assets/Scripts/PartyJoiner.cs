using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon;



public class PartyJoiner : Photon.MonoBehaviour
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
    private string remoteInviteChannelName;

    private int myID;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        myID = photonView.ownerId;

        if(!photonView.isMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        inviteButton.interactable = false;
        joinButton.gameObject.SetActive(false);
        //joinButton.interactable = false;
    }

    [PunRPC]
    public void InvitePlayerToPartyChannel(int invitedID)
    {
        // display a little ball  over their head
        if(invitedID == photonView.viewID && photonView.isMine)
        {
            print("THATS ME!");
        }
    }

    [PunRPC]
    public void WithDrawInvite()
    {

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
            PhotonView otherPlayerPhotonView = other.GetComponent<PhotonView>();
            if(otherPlayerPhotonView == null)
            {
                return;
            }

            PhotonView.Find(otherPlayerPhotonView.viewID).RPC("InvitePlayerToPartyChannel", PhotonTargets.All, otherPlayerPhotonView.viewID);
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
