using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Avatars;
using Unity.VisualScripting;

public class SetMapMaker : MonoBehaviour
{
    // To set the position of the marker on map as the position of the users

    private GameObject Player;
    private AvatarManager avatarManager;
    public GameObject mapMarkerPrefab;
    private List<GameObject> tempMarker = new List<GameObject>();
    private List<Ubiq.Avatars.Avatar> avatarList = new List<Ubiq.Avatars.Avatar>();

    void Start()
    {
        Player = GameObject.Find("Player");
        avatarManager = AvatarManager.Find(this);
    }

    void Update()
    {
        if (UserMode.Instance.userMode) // Marker can only be set and is only viewable in VR mode
        {
            var dropCamera = Player.GetComponent<DropCamera>();
            if (!dropCamera.isOnGround)
            {
                foreach (Ubiq.Avatars.Avatar avatar in avatarManager.Avatars)
                {
                    if (!avatar.IsLocal)
                    {
                        var avatarPosition = avatar.gameObject.GetComponent<GetAvatarPosition>().position;
                        var markerPosition = new Vector3(avatarPosition.x, 200, avatarPosition.z);
                        // if to up position, set marker where avatar is not local avatar
                        if (!avatarList.Contains(avatar)) // marker has not been set before
                        {
                            GameObject temp = Instantiate(mapMarkerPrefab, markerPosition, Quaternion.identity);
                            tempMarker.Add(temp);
                            avatarList.Add(avatar);
                        }
                        else
                        {
                            var idx = avatarList.IndexOf(avatar);
                            tempMarker[idx].transform.position = markerPosition;
                        }
                    }
                }
            }
            else
            {
                // if in up position and there are map markers, destroy all of them
                if (!tempMarker.Equals(null))
                {
                    foreach (GameObject temp in tempMarker)
                    {
                        Destroy(temp);
                    }
                    avatarList.Clear();
                    tempMarker.Clear();
                }
            }







            
        
        }
    }
}
