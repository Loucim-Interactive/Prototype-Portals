using UnityEngine;

namespace Testing {
    public class CarScript : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float force = 1f;
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Space)) {
                rb.AddForce(force * transform.forward, ForceMode.Acceleration);
                //Debug.Log("Acccel");
            }
            
            if (Input.GetKey(KeyCode.LeftShift)) {
                rb.AddForce(force * -transform.forward, ForceMode.Acceleration);
                //Debug.Log("Deccell");
            }
        }
    }
}
