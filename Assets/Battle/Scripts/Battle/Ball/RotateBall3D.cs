using UnityEngine;

namespace Battle.Scripts.Battle.Ball
{
    /// <summary>
    /// Rotates 3D sphere (ball) in 2D environment.
    /// </summary>
    /// <remarks>
    /// Check Rotate a 3D ball using 2D Physics
    /// http://kwarp.blogspot.com/2015/07/unity-rotate-3d-ball-using-2d-physics.html <br />
    /// And Rolling Animated Sphere
    /// https://catlikecoding.com/unity/tutorials/movement/rolling/
    /// </remarks>
    public class RotateBall3D : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Transform _ball;
        [SerializeField, Min(0.1f)] private float _ballRadius = 0.5f;

        private Vector3 _lastPosition;

        private void OnEnable()
        {
            _lastPosition = _ball.position;
        }

        void FixedUpdate()
        {
            RotateBall();
            _lastPosition = transform.position;
        }

        private void RotateBall()
        {
            if (Mathf.Approximately(_lastPosition.x, _ball.position.x) && Mathf.Approximately(_lastPosition.y, _ball.position.y))
            {
                // no distance travelled, nothing to do
                return;
            }
            // distance the ball traveled between the last Update and this one
            // we subtract the vectors in this order to use it as a direction vector later
            var currentToLast = _lastPosition - _ball.position;

            // segment length that the ball rolled along its surface
            var segment = currentToLast.magnitude;


            // to build a Quaternion to hold the desired rotation,
            // we need a Vector3 "axis" and an angle "theta"
            // first, the axis:

            // define the down direction vector for the ball
            // the ball rolls in the x and y directions,
            // and positive z points to the ground
            // Important: 	this is DIFFERENT from Vector3.down because Vector3 assumes
            //				a 3D world space where negative y is down
            var ballDown = new Vector3(0, 0, 1);

            // use Cross Product to find the axis of rotation
            // https://www.mathsisfun.com/algebra/vectors-cross-product.html
            var axis = Vector3.Cross(ballDown, currentToLast);

            // Cross Product will fail if both vectors are parallel or perpendicular
            if (axis == Vector3.zero)
            {
                // this should never happen because currentToLast.z is always 0
                // but who knows where this code will be copy-pasted to...
                return;
            }

            // next, the angle theta:

            // arc length formula
            // s = r * theta
            // theta = s / r
            // http://www.mathopenref.com/arclength.html
            var theta = segment / _ballRadius; // in radians
            var thetaDegrees = theta * 180 / Mathf.PI;

            // create the rotation Quaternion
            var q = Quaternion.AngleAxis(thetaDegrees, axis);

            // apply rotation to the transform
            // q must come first in multiplication order!!!
            // Quaternion multiplication is Non-Commutative!
            _ball.rotation = q * _ball.rotation;
        }
    }
}