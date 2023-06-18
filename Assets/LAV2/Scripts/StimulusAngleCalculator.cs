using UnityEngine;
using System.Collections;

public static class FloatExtensions
    {

        internal static float Rescale(this float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            //y = ax + b
            float a = (outputMax - outputMin) / (inputMax - inputMin);
            float b = outputMin - a * inputMin;
            return a * value + b;
        }
    }
    
    public class StimulusAngleCalculator : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        [Range(0.0f, 1.0f)]
        public float sensibilityX = 4e-5f;
        [Range(0.0f, 1.0f)]
        public float sensibilityZ = 0.002f;
        public float refreshRate = 0.2f;
        public float turningSpeed = 10.0f;
        
        [SerializeField] private Vector3 _previousPosition;
        [SerializeField] private Vector3 _linearVelocity;
        [SerializeField] private Vector3 _previousLinearVelocity;
        [SerializeField] private Vector3 _linearAcceleration;
        

        [SerializeField] private float _xAngle = 0;
        [SerializeField] private float _zAngle = 0;
        
        private readonly float _maxAngle = 45f;
        
        [SerializeField] private Vector3 _centrifugalForce;

        public GameObject stimulusRepr;
        public float speedThreshold = 5.0f;
        private float formulaOneAccForce = 4000f;

        private void Start()
        {
 
            _rigidbody = GetComponent<Rigidbody>();
        }

        private Vector3 GetCentrifugalForce()
        {
                // Calculate the direction of the centripetal acceleration
                var position = transform.localPosition;
                
                //Calculate the direction of the outside curve of the vehicle
                Vector3 direction = Vector3.Cross(-transform.up, position - _previousPosition).normalized;
                
                // Calculate the distance between the start and end points
                float distance = Vector3.Distance(_previousPosition, position);

                // Calculate the radius using the distance and the direction of the centripetal acceleration
                float radius = distance / (2 * Mathf.Sin(Mathf.Acos(Vector3.Dot(transform.forward, (position - _previousPosition).normalized)) / 2));

                float velocity = _linearVelocity.magnitude;
                
                float centrifugalForce = (_rigidbody.mass * velocity * velocity) / radius;

                return direction * (centrifugalForce * sensibilityX);

        }
        
        private void FixedUpdate()
        {
            _linearVelocity = (transform.localPosition - _previousPosition) / Time.deltaTime;
            _linearAcceleration = (_linearVelocity - _previousLinearVelocity) / Time.deltaTime;
            _centrifugalForce = Vector3.zero;
            
            if (_linearVelocity.magnitude > speedThreshold)
                _centrifugalForce = GetCentrifugalForce();

            //Acceleration on x axis when driving along x axis
            float xAccValue = _linearAcceleration.x *
                              Vector3.Dot(Vector3.right, transform.forward);

            //Acceleration on z axis when driving along z axis
            float zAccValue = _linearAcceleration.z *
                              Vector3.Dot(Vector3.forward, transform.forward);


            float accForce = (xAccValue + zAccValue) * _rigidbody.mass * sensibilityZ;
            accForce.Rescale(-formulaOneAccForce, formulaOneAccForce, -_maxAngle, _maxAngle);

            float localX = transform.localEulerAngles.x;
            localX = localX > 180f ? localX - 360f : localX;
            
            float localZ = transform.localEulerAngles.z;
            localZ = localZ > 180f ? localZ - 360f : localZ;

            _xAngle = _centrifugalForce.x.Rescale(-100f, 100f, -_maxAngle, _maxAngle);
            _xAngle = Mathf.Clamp(_xAngle + localX, -_maxAngle, _maxAngle);
            
            _zAngle = Mathf.Clamp(accForce + localZ, -_maxAngle, _maxAngle);

            Quaternion targetAngle = Quaternion.Euler(new Vector3(_xAngle, stimulusRepr.transform.rotation.z, _zAngle));
            
            stimulusRepr.transform.rotation = Quaternion.Lerp(stimulusRepr.transform.rotation, targetAngle,
                turningSpeed * Time.deltaTime);
            
            Debug.DrawLine(transform.position, transform.position + _centrifugalForce);

            StartCoroutine(RefreshLastValues());
        }

        IEnumerator RefreshLastValues()
        {
            _previousPosition = transform.localPosition;
            _previousLinearVelocity = _linearVelocity;
            yield return new WaitForSeconds(refreshRate);
        }
    }