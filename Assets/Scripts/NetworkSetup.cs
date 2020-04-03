using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSetup : Photon.MonoBehaviour
{
    public List<GameObject> localGameObjects;
    public List<Behaviour> localComponents;

    private PhotonView myPhotonView;

    // Start is called before the first frame update
    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();

        if (PhotonNetwork.connected == true && !myPhotonView.isMine)
        {
            foreach (GameObject localObject in localGameObjects)
            {
                localObject.SetActive(false);
            }

            foreach (Behaviour localComponent in localComponents)
            {
                localComponent.enabled = false;
            }
        }
    }
}
