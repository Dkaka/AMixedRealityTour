using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.XR;
using UnityEngine.UIElements;

public class NetworkGraspableObject : MonoBehaviour, INetworkSpawnable, IGraspable
{
    public NetworkId NetworkId { get; set; }
    
    private Hand controller;
    NetworkContext context;
    private bool owner;
    //public bool owned;
    //public bool justowned;

    void Start()
    {
        context = NetworkScene.Register(this);
    }


    void FixedUpdate()
    {
        if (owner)
        {
            context.SendJson(new Message(transform));
            //context.SendJson(new Message(transform, owned));
        }
        //else if (justowned)
        //{
        //    context.SendJson(new Message(transform, owned));
        //    justowned = false;
        //}
        
    }

    private void LateUpdate()
    {
        if (controller)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
    }

    //private void Update()
    //{
    //    if (owned)
    //    {
    //        this.GetComponent<Rigidbody>().isKinematic = true;
    //    }
    //    else
    //    {
    //        this.GetComponent<Rigidbody>().isKinematic = false;
    //    }
    //}

    private struct Message
    {
        public TransformMessage transform;
        //public bool owned;

        //public Message(Transform transform, bool owned)
        public Message(Transform transform)
        {
            this.transform = new TransformMessage(transform);
            //this.owned = owned;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.transform.position;
        transform.localRotation = m.transform.rotation;
        //owned = m.owned;

    }


    void IGraspable.Grasp(Hand controller)
    {
        //owned = true;
        owner = true;
        this.controller = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        //justowned = true;
        //owned = false;
        owner = false;
        this.controller = null;
    }
}
