using UnityEngine;

namespace CameraDoorScript {
    public class CameraOpenDoor : MonoBehaviour {
        
        public GameObject Door; 
        public float distanceOpen = 2f;
        public GameObject text;

        // Use only ONE Update function
        void Update() {
            RaycastHit hit;
            // Use the lowercase 'd' here to match line 7
            if (Physics.Raycast(transform.position, transform.forward, out hit, distanceOpen)) {
                if (hit.transform.GetComponent<DoorScript.Door>()) {
                    text.SetActive(true);
                    if (Input.GetKeyDown(KeyCode.E)) {
                        hit.transform.GetComponent<DoorScript.Door>().OpenDoor();
                    }
                } else {
                    text.SetActive(false);
                }
            } else {
                text.SetActive(false);
            }
        }
    }
}
