using UnityEngine;

namespace NucleonPerformance
{
    [RequireComponent(typeof(Rigidbody))]
    public class Nucleon : MonoBehaviour {

        [SerializeField]
        private float attractionForce = 0f;

        private Rigidbody body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        // Use this for initialization
        void Start () {

        }
	
	    // Update is called once per frame
	    void Update () {
		
	    }

        void FixedUpdate()
        {
            body.AddForce(transform.localPosition * -attractionForce);    
        }
    }
}
