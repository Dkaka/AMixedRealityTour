using System.Collections.Generic;
using UnityEngine;
using Ubiq.Rooms;
using Ubiq.Messaging;


public class AutoRoomEnter : MonoBehaviour
{
    private NetworkScene scene;
    private RoomClient roomClient;
    private string roomName;
    public bool joinRoom;
    private bool joined = false;

    void Start()
    {
        roomName = "EarthTrip";
        scene = NetworkScene.Find(this);
        roomClient = scene.GetComponent<RoomClient>();
    }

    private void Update()
    {
        if (joinRoom && joined==false)
        {
            //isIn = true;
            roomClient.OnRooms.AddListener(TryJoin);
            roomClient.DiscoverRooms();
            joined = true;
        }
        
    }

    private void TryJoin(List<IRoom> rooms, RoomsDiscoveredRequest request)
    {
        foreach (IRoom room in rooms)
        {
            if (room.Name.Contains("EarthTrip"))
            {
                roomClient.Join(room.JoinCode);
                
                return;
            }
        }

        // If room does not exist, create a new room
        roomClient.Join(name: roomName, publish: true);
    }

}
