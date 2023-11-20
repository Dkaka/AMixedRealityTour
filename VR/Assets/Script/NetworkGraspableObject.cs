using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.XR;
using UnityEngine.UIElements;

public class NetworkGraspableObject : MonoBehaviour, INetworkSpawnable, IGraspable
{
    // Attach this script to objects to enable Ubiq networking function and make it graspable 

    public NetworkId NetworkId { get; set; }
    
    private Hand controller;
    NetworkContext context;
    private bool owner;


    void Start()
    {
        context = NetworkScene.Register(this);
    }


    void FixedUpdate()
    {
        if (owner)
        {
            context.SendJson(new Message(transform));
        }

    }

    private void LateUpdate()
    {
        if (controller)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
    }


    private struct Message
    {
        public TransformMessage transform;

        public Message(Transform transform)
        {
            this.transform = new TransformMessage(transform);
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.transform.position;
        transform.localRotation = m.transform.rotation;

    }


    void IGraspable.Grasp(Hand controller)
    {
        owner = true;
        this.controller = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        owner = false;
        this.controller = null;
    }
}
