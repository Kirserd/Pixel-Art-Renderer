using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CameraState
{
    PLAYER,
    TARGET,
    FREE
};

public enum CameraMovementType
{
    INTERPOLATION,
    LINEAR,
    TELEPORT
};

public class IsometricCameraMovement : MonoBehaviour
{

    #region MainFields
    public static IsometricCameraMovement Current { get; private set; }

    public CameraState State;
    public CameraMovementType MovementType;

    [Space(15)]

    [Header("View")]
    [SerializeField] private float _cameraHeight = 18f;
    [SerializeField] private float _targetAngle = 45f;
    [SerializeField] private float _currentAngle = 0f;
    [SerializeField] private float _dragMouseSensitivity = 2f;
    [SerializeField] private float _rotationSpeed = 5f;
    [Space(5)]
    [Header("Movement")]
    [SerializeField] private float _linearSpeed = 1;
    [SerializeField] private float _interpolateSpeed = 8f;
    [Space(5)]
    [Header("Zoom")]
    [SerializeField] private float _currentZoom = 0f;
    [SerializeField] private float _zoomMouseSensitivity = 2f;
    [SerializeField] private float _zoomSpeed = 5f;
    [Space(5)]
    [Header("Optional")]
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _player;
    public Transform Target
    {
        get => _target;
        set
        {
            if (value != null)
                _target = value;
        }
    }
    public Transform Player
    {
        get => _player;
        set
        {
            if (value != null)
                _player = value;
        }
    }

    public float LinearSpeed
    {
        get => _linearSpeed;
        set
        {
            if (value > 0)
                _linearSpeed = value;
            else
                Debug.LogAssertion("Linear speed can not be less equal to 0");
        }
    }
    public float InterpolateSpeed
    {
        get
        {
            return _interpolateSpeed;
        }
        set
        {
            if (value > 0)
                _interpolateSpeed = value;
            else
                Debug.LogAssertion("Interpolation speed can not be less or equal to 0");
        }
    }

    private float _maxZoom = 8;
    private float _minZoom = 16;

    private List<Camera> _cameras = new();

    #endregion

    #region Subscriptions
    private void Awake() => Current = this;
    #endregion

    private void Start()
    {
        RefreshMainLinks();
        RefreshPlayerLink();
    }
    private void RefreshMainLinks()
    {
        for(int i=0; i < transform.childCount; i++)
            _cameras.Add(transform.GetChild(i).GetComponent<Camera>());
    }
    private void RefreshPlayerLink()
        => Player = GameObject.FindGameObjectWithTag("Player").transform;

    private void LateUpdate()
    {
        Rotate();
        Zoom();
        switch (State)
        {
            case CameraState.PLAYER:
                Move(Player.position);
                break;
            case CameraState.TARGET:
                Move(Target.position);
                break;
            default:
                break;
        }
    }
    private void Move(Vector3 Destination)
    {
        switch (MovementType)
        {
            case CameraMovementType.INTERPOLATION:
                Interpolate();
                break;
            case CameraMovementType.LINEAR:
                Linear();
                break;
            default:
                Teleport();
                break;
        }

        void Interpolate() => transform.position = Vector3.Lerp(transform.position, Destination, InterpolateSpeed * Time.deltaTime);

        void Linear() => transform.position += LinearSpeed * ( Destination - transform.position).normalized;

        void Teleport() => transform.position = Destination;
    }
    private void Rotate()
    {
        float mouseX = Input.GetAxis("Mouse X");

        if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
            _targetAngle += mouseX * _dragMouseSensitivity;
        else
            _targetAngle = Mathf.Round(_targetAngle / 45) * 45;

        if (_targetAngle < 0)
            _targetAngle += 360;
        if (_targetAngle > 360)
            _targetAngle -= 360;

        _currentAngle = Mathf.LerpAngle(transform.eulerAngles.y, _targetAngle, _rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(_cameraHeight, _currentAngle, 0);
    }
    private void Zoom()
    {
        float input = Input.mouseScrollDelta.y * _zoomMouseSensitivity;
        if (input != 0)
        {
            if (_currentZoom + input >= 1)
                _currentZoom = 1;
            else if (_currentZoom + input <= 0)
                _currentZoom = 0;
            else
                _currentZoom += input;
        }

        var size = Mathf.Lerp(
            _cameras[0].orthographicSize,
            Mathf.Lerp(_minZoom, _maxZoom, _currentZoom),
            _zoomSpeed * Time.deltaTime);

        foreach (var camera in _cameras )
            camera.orthographicSize = size;
    }

    public void Shake(float amount = 0.5f, float time = 0.5f, float frequency = 0.04f) => StartCoroutine(Shaker(amount, time, frequency));
    private IEnumerator Shaker(float amount, float time, float frequency)
    {
        float timer = 0;
        while (time != 0 ? timer < time : amount > 0.01f)
        {
            transform.position += new Vector3(Random.Range(-amount, amount), Random.Range(-amount, amount), 0);
            yield return new WaitForSeconds(frequency);
            amount *= 0.8f;
            timer += frequency;
        }
    }
}
