using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Avatars;
using Ubiq.Rooms;
using Ubiq.XR;

public class NetworkPointer : MonoBehaviour
{
    // Enable ubiq networking function for pointer -> so that user can see the local guide point to somewhere with laser emitted from their hands
    // Local guide mode -> need to check if controller is pressed or not (to emit pointer)

    NetworkContext context;
    private string avatarID;
    private Pointer pointer;
    private AvatarManager avatarManager;
    public GameObject LeftHand;
    private HandController hc;
    private bool ispressed;
    private bool prevP;

    void Start()
    {
        context = NetworkScene.Register(this);
        avatarManager = AvatarManager.Find(this);
        hc = LeftHand.GetComponent<HandController>();


    }

    private void Update()
    {
        ispressed = hc.SecondaryButtonState; 
        pointer = avatarManager.LocalAvatar.GetComponentInChildren<Pointer>(); //get the laser gameobject
        YPressed(ispressed); 
        
    }

    void FixedUpdate()
    {
       
        if (pointer != null)
        {
            avatarID = avatarManager.LocalAvatar.Peer.uuid;
            
            var p = pointer.GetLineRenderer();

            //Send message if laser renderer state is changed
            if (p!=prevP)
            {
                prevP = p;
                context.SendJson(new Message(p, avatarID));
            }

            
        }
        
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
        // Parse the message
        var m = message.FromJson<Message>();

        var receivedAvatarP = m.pointerIsOn;
        var receivedAvatarID = m.avatarID;

        foreach (Ubiq.Avatars.Avatar a in avatarManager.Avatars)
        {
            if (a.Peer.uuid == receivedAvatarID)
            {
                a.GetComponentInChildren<Pointer>().SetLineRenderer(receivedAvatarP);
            }
            
        }
        
    }




    public void YPressed(bool pressed)
    {
        if (pointer != null)
        {
            if (pressed)
            {
                pointer.SetLineRenderer(true); // if controller is pressed then render the laser
            }
            else
            {
                pointer.SetLineRenderer(false);
            }
        }
    }
}

