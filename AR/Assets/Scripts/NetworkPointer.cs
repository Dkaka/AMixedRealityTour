using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Avatars;
using Ubiq.Rooms;
using Ubiq.XR;

public class NetworkPointer : MonoBehaviour
{
    NetworkContext context;
    private string avatarID;
    private Pointer pointer;
    private AvatarManager avatarManager;
    public GameObject LeftHand;
    private HandController hc;
    private bool ispressed;
    private bool prevP;

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
        avatarManager = AvatarManager.Find(this);

    }

    private void Update()
    {

    }


    private struct Message
    {
        public bool pointerIsOn;
        public string avatarID;

        public Message(bool pointerIsOn, string avatarID)
        {
            this.pointerIsOn = pointerIsOn;
            this.avatarID = avatarID;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();

        var receivedAvatarP = m.pointerIsOn;
        var receivedAvatarID = m.avatarID;

        //Debug.Log(receivedAvatarID);
        foreach (Ubiq.Avatars.Avatar a in avatarManager.Avatars)
        {
            if (a.Peer.uuid == receivedAvatarID)
            {
                a.GetComponentInChildren<Pointer>().SetLineRenderer(receivedAvatarP);
            }

        }

    }

}

