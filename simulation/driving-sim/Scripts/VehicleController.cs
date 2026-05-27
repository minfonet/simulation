using Godot;
using System;
using System.Collections.Generic;

namespace DrivingSim;

public partial class VehicleController : RigidBody3D
{
    [Export] public float EnginePower { get; set; } = 800f;
    [Export] public float SteeringTorque { get; set; } = 120f;
    [Export] public float BrakeForce { get; set; } = 400f;

    private float _steeringAngle;
    private bool _collidedThisFrame;
    private int _frameCount;

    public struct TelemetryPoint
    {
        public DateTime Timestamp { get; set; }
        public double Speed { get; set; }
        public double SteeringAngle { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public bool Collision { get; set; }
    }

    private readonly List<TelemetryPoint> _pendingPoints = new();
    private const int BatchIntervalFrames = 10;
    private BackendClient _backend;

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 1;
        BodyEntered += OnBodyEntered;
        _backend = new BackendClient();
        _backend.ReadSessionId();
        _backend.StartSession();
    }

    public override void _PhysicsProcess(double delta)
    {
        _collidedThisFrame = false;
        var forward = -GlobalTransform.Basis.Z;

        if (Input.IsActionPressed("move_forward"))
            ApplyCentralForce(forward * EnginePower);

        if (Input.IsActionPressed("move_backward"))
            ApplyCentralForce(-forward * EnginePower * 0.5f);

        if (Input.IsActionPressed("move_left"))
        {
            ApplyTorque(new Vector3(0, -SteeringTorque, 0));
            _steeringAngle = Mathf.MoveToward(_steeringAngle, -0.5f, 0.05f);
        }
        else if (Input.IsActionPressed("move_right"))
        {
            ApplyTorque(new Vector3(0, SteeringTorque, 0));
            _steeringAngle = Mathf.MoveToward(_steeringAngle, 0.5f, 0.05f);
        }
        else
        {
            _steeringAngle = Mathf.MoveToward(_steeringAngle, 0, 0.05f);
        }

        if (Input.IsActionPressed("brake"))
        {
            var brakeDir = -LinearVelocity.Normalized();
            ApplyCentralForce(brakeDir * BrakeForce);
            LinearVelocity *= 0.95f;
        }

        CollectTelemetry();
        _frameCount++;
    }

    private void CollectTelemetry()
    {
        var speed = LinearVelocity.Length();
        var pos = Position;

        _pendingPoints.Add(new TelemetryPoint
        {
            Timestamp = DateTime.UtcNow,
            Speed = speed,
            SteeringAngle = _steeringAngle,
            PositionX = pos.X,
            PositionY = pos.Y,
            PositionZ = pos.Z,
            Collision = _collidedThisFrame,
        });

        if (_frameCount % BatchIntervalFrames == 0 && _pendingPoints.Count > 0)
        {
            var batch = new List<TelemetryPoint>(_pendingPoints);
            _pendingPoints.Clear();
            _backend.SendTelemetryBatch(batch);
        }
    }

    private void OnBodyEntered(Node body)
    {
        _collidedThisFrame = true;
    }

    public override void _ExitTree()
    {
        FlushTelemetry();
        _backend.FinishSession();
    }

    public void FlushTelemetry()
    {
        if (_pendingPoints.Count > 0)
        {
            _backend.SendTelemetryBatch(new List<TelemetryPoint>(_pendingPoints));
            _pendingPoints.Clear();
        }
    }
}
