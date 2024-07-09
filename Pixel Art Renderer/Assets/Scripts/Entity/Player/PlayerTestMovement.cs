using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _deceleration = 5f;
    [SerializeField] private float _maxSpeed = 15f;
    [SerializeField] private float _rotationSpeed = 15f;

    private Camera _camera;
    private Transform _modelPivot;
    private Rigidbody _rigidbody;
    private Vector3 _inputDirection;

    private Quaternion _targetAngle;

    private Animator _animator;

    private void Start()
    {
        _modelPivot = transform.GetChild(0);
        _animator = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        _camera = Camera.main;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true; 
    }
    private void Update()
    {
        _inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        _animator.SetFloat("Speed", Mathf.Abs(_rigidbody.velocity.x) + Mathf.Abs(_rigidbody.velocity.y));
    }
    private void FixedUpdate()
    {
        Rotate();
        Move();
    }
    private void Rotate()
    {
        if (Input.GetMouseButton(1))
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, _camera.transform.eulerAngles.y, transform.eulerAngles.z);
            if (_inputDirection == Vector3.zero)
                _targetAngle = transform.rotation;
        }
        _inputDirection = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * _inputDirection;

        if (_inputDirection != Vector3.zero)
            _targetAngle = Quaternion.LookRotation(_inputDirection, Vector3.up);

        _modelPivot.rotation = Quaternion.LerpUnclamped(_modelPivot.rotation, _targetAngle, _rotationSpeed * Time.deltaTime);
    }
    private void Move()
    {
        Vector3 currentVelocity = _rigidbody.velocity;
        Vector3 desiredVelocity = _inputDirection * _maxSpeed;

        Vector3 accelerationVector = (desiredVelocity - currentVelocity) * _acceleration * Time.fixedDeltaTime;
        _rigidbody.AddForce(accelerationVector, ForceMode.VelocityChange);

        if (_inputDirection == Vector3.zero)
        {
            Vector3 decelerationVector = _deceleration * Time.fixedDeltaTime * -currentVelocity.normalized;
            _rigidbody.AddForce(decelerationVector, ForceMode.VelocityChange);
        }

        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, _maxSpeed);
    }
}
