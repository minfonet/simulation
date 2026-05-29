using Godot;
using System;

namespace DrivingSim;

public partial class FollowCamera : Node3D
{
    [Export] public float Distance = 6.0f;
    [Export] public float Height = 3.0f;
    [Export] public float SmoothSpeed = 5.0f;
    [Export] public float LookAheadDistance = 2.0f;
    [Export] public float LeanAmount = 1.5f;

    private VehicleController _car;
    private Camera3D _camera;
    private Camera3D _cockpitCamera;
    private Vector3 _desiredPosition;
    private bool _isThirdPerson;
    private bool _cWasPressed;

    public override void _Ready()
    {
        _car = GetNode<VehicleController>("../Car");
        _camera = GetNode<Camera3D>("Camera");
        _cockpitCamera = GetNode<Camera3D>("../Car/CameraCockpit");

        // Start in cockpit view (cockpit is already current=true in scene)
        _isThirdPerson = false;
        _camera.Current = false;

        // Initialize position near the car for smooth first lerp
        GlobalPosition = _car.GlobalPosition + Vector3.Up * Height;
    }

    public override void _Process(double delta)
    {
        // Handle C key toggle (manual edge detection)
        bool cDown = Input.IsKeyPressed(Key.C);
        if (cDown && !_cWasPressed)
        {
            _isThirdPerson = !_isThirdPerson;
            _camera.Current = _isThirdPerson;
            _cockpitCamera.Current = !_isThirdPerson;
        }
        _cWasPressed = cDown;

        if (!_isThirdPerson) return;

        var carPos = _car.GlobalTransform.Origin;
        var carBack = -_car.GlobalTransform.Basis.Z;

        // Base position behind and above the car
        _desiredPosition = carPos + carBack * Distance + Vector3.Up * Height;

        // Drift lean: when velocity direction diverges from forward, offset laterally
        float speed = _car.LinearVelocity.Length();
        if (speed > 1.0f)
        {
            var velDir = _car.LinearVelocity / speed;
            var forward = carBack;
            var cross = forward.Cross(velDir);
            float driftAngle = Mathf.Abs(forward.AngleTo(velDir));

            if (driftAngle > 0.1f)
            {
                var leanDir = _car.GlobalTransform.Basis.X * Mathf.Sign(cross.Y);
                _desiredPosition += leanDir * driftAngle * LeanAmount;
            }
        }

        // Smooth follow via lerp
        GlobalPosition = GlobalPosition.Lerp(_desiredPosition, (float)delta * SmoothSpeed);

        // Look at car with look-ahead (in velocity direction)
        Vector3 lookTarget;
        if (speed > 0.5f)
        {
            var velDir = _car.LinearVelocity / speed;
            lookTarget = carPos + velDir * LookAheadDistance;
        }
        else
        {
            lookTarget = carPos;
        }

        // Safety: prevent LookAt from failing (zero direction or parallel to up vector)
        var camPos = _camera.GlobalPosition;
        var lookDir = lookTarget - camPos;
        if (lookDir.Length() < 0.001f)
        {
            // Camera at same position as target — look forward from car
            lookTarget = carPos + carBack * LookAheadDistance;
        }
        else
        {
            lookDir = lookDir.Normalized();
            if (Mathf.Abs(lookDir.Dot(Vector3.Up)) > 0.999f)
                lookTarget += Vector3.Right * 0.01f;
        }

        _camera.LookAt(lookTarget);
    }
}
