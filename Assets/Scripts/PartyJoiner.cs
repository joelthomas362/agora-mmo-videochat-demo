using UnityEngine;
using UnityEngine.UI;

public class PartyJoiner : Photon.MonoBehaviour
{
    [Header("Local Player Stats")]
    [SerializeField]
    private Button inviteButton;
    [SerializeField]
    private GameObject joinButton;
    [SerializeField]
    private GameObject leaveButton;

    [Header("Remote Player Stats")]
    [SerializeField]
    private int remotePlayerViewID;
    [SerializeField]
    private string remoteInviteChannelName;

    private AgoraVideoChat agoraVideo;

    private void Awake()
    {
        agoraVideo = GetComponent<AgoraVideoChat>();
    }

    void Start()
    {
        if(!photonView.isMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        inviteButton.interactable = false;
        joinButton.SetActive(false);
        leaveButton.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.isMine || !other.CompareTag("Player"))
        {
            return;
        }

        PhotonView otherPlayerPhotonView = other.GetComponent<PhotonView>();
        if(otherPlayerPhotonView == null)
        {
            return;
        }

        remotePlayerViewID = otherPlayerPhotonView.viewID;
        inviteButton.interactable = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if(!photonView.isMine || !other.CompareTag("Player"))
        {
            return;
        }

        remotePlayerViewID = -1;    

        inviteButton.interactable = false;
        joinButton.SetActive(false);
    }

    public void OnInviteButtonPress()
    {
        PhotonView.Find(remotePlayerViewID).RPC("InvitePlayerToPartyChannel", PhotonTargets.All, remotePlayerViewID, agoraVideo.GetLocalChannel());
    }

    public void OnJoinButtonPress()
    {
        if (remotePlayerViewID != -1 && photonView.isMine)
        {
            agoraVideo.JoinRemoteChannel(remoteInviteChannelName);
            joinButton.SetActive(false);
            leaveButton.SetActive(true);
        }
    }

    public void OnLeaveButtonPress()
    {
        agoraVideo.JoinOriginalChannel();
        leaveButton.SetActive(false);
    }

    [PunRPC]
    public void InvitePlayerToPartyChannel(int invitedID, string channelName)
    {
        if (invitedID == photonView.viewID && photonView.isMine)
        {
            joinButton.SetActive(true);
            remoteInviteChannelName = channelName;
        }
    }

    [PunRPC]
    public void WithdrawInvite(int canceledID)
    {
        if(canceledID == photonView.viewID && photonView.isMine)
        {
            joinButton.SetActive(false);
        }
    }
}