using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkObject : MonoBehaviour
{
    private Button joinButton;

    private void Awake()
    {
        joinButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        
    }

    [PunRPC]
    public void ButtonScript()
    {
        print(gameObject.name + " attached to: " + transform.parent.parent.name);
    }
}
