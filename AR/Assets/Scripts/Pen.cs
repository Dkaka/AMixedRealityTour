using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;
using Ubiq.Spawning;
using System.Security.Cryptography;

// Adds simple networking to the 3d pen. The approach used is to draw locally
// when a remote user tells us they are drawing, and stop drawing locally when
// a remote user tells us they are not.
public class Pen : MonoBehaviour, IGraspable, IUseable, INetworkSpawnable
{
    private NetworkContext context;
    public bool owner;
    public Hand controller;
    private Transform nib;
    private Material drawingMaterial;
    private GameObject currentDrawing;
    //public bool owned;
    //public bool justowned;
    public NetworkId NetworkId { get; set; }
    

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isDrawing;
        //public bool owned;

        //public Message(Transform transform, bool isDrawing, bool owned)
        public Message(Transform transform, bool isDrawing)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing; 
            //this.owned = owned;
        }
    }

    private void Start()
    {
        nib = transform.Find("Grip/Nib");
        context = NetworkScene.Register(this);
        var shader = Shader.Find("Particles/Standard Unlit");
        drawingMaterial = new Material(shader);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
        //owned = data.owned;

        // new
        // Also start drawing locally when a remote user starts
        if (data.isDrawing && !currentDrawing)
        {
            BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing)
        {
            EndDrawing();
        }
    }

    private void FixedUpdate()
    {
        

        if (owner)
        {
            context.SendJson(new Message(transform, isDrawing: currentDrawing));
            //context.SendJson(new Message(transform, isDrawing: currentDrawing, owned));
        }
        //else if (justowned)
        //{
        //    context.SendJson(new Message(transform, isDrawing: currentDrawing, owned));
        //    justowned = false;
        //}
        
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

    private void LateUpdate()
    {
        
        if (controller)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
        

    }

    void IGraspable.Grasp(Hand controller)
    {
        owner = true;
        //owned = true;
        this.controller = controller;
        //this.GetComponent<Rigidbody>().isKinematic = true;
    }

    void IGraspable.Release(Hand controller)
    {
        //justowned = true;
        owner = false;
        //owned = false;
        this.controller = null;
        //this.GetComponent <Rigidbody>().isKinematic = false;
    }


    void IUseable.Use(Hand controller)
    {
        BeginDrawing();
    }

    void IUseable.UnUse(Hand controller)
    {
        EndDrawing();
    }

    public void BeginDrawing()
    {
        currentDrawing = new GameObject("Drawing");
        var trail = currentDrawing.AddComponent<TrailRenderer>();
        trail.time = Mathf.Infinity;
        trail.material = drawingMaterial;
        trail.startWidth = .05f;
        trail.endWidth = .05f;
        trail.minVertexDistance = .02f;

        currentDrawing.transform.parent = nib.transform;
        currentDrawing.transform.localPosition = Vector3.zero;
        currentDrawing.transform.localRotation = Quaternion.identity;
    }

    public void EndDrawing()
    {
        var trail = currentDrawing.GetComponent<TrailRenderer>();
        currentDrawing.transform.parent = null;
        currentDrawing.GetComponent<TrailRenderer>().emitting = false;
        currentDrawing = null;
    }

    

}