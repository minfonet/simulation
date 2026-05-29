using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DrivingSim;

public partial class VehicleController : RigidBody3D
{
    [Export] public float EnginePower { get; set; } = 800f;
    [Export] public float SteeringTorque { get; set; } = 5000f;
    [Export] public float BrakeForce { get; set; } = 20000f;
    [Export] public float GripFactor { get; set; } = 0.8f;         // Lower = more drift
    [Export] public float DriftThreshold { get; set; } = 0.5f;     // Speed fraction to trigger drift
    [Export] public float OmegaMax { get; set; } = 3.0f;           // Max angular velocity (normal)
    [Export] public float OmegaMaxDrift { get; set; } = 6.0f;      // Max angular velocity (drift)
    [Export] public float WeightTransferPitch { get; set; } = 0.3f;// Pitch torque on accel/brake
    [Export] public float WeightTransferRoll { get; set; } = 0.2f; // Roll torque on steer
    [Export] public float GripRestoreSpeed { get; set; } = 2.0f;   // How fast grip returns
    [Export] public float DriftAngleThreshold { get; set; } = 0.3f;// Min angle (rad) to trigger drift
    [Export] public float LateralGripForce { get; set; } = 15f;   // Lateral grip resistance multiplier

    public float CurrentSpeed => LinearVelocity.Length();
    public float CurrentSteering => _steeringAngle;

    private float _steeringAngle;
    private bool _collidedThisFrame;
    private bool _sessionFinished;
    private int _frameCount;
    private float _driftFactor;  // 0-1, how much the car is currently drifting

    public struct TelemetryPoint
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        [JsonPropertyName("steeringAngle")]
        public double SteeringAngle { get; set; }

        [JsonPropertyName("positionX")]
        public double PositionX { get; set; }

        [JsonPropertyName("positionY")]
        public double PositionY { get; set; }

        [JsonPropertyName("positionZ")]
        public double PositionZ { get; set; }

        [JsonPropertyName("collision")]
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
        var basisRight = GlobalTransform.Basis.X;
        float speed = LinearVelocity.Length();
        float speedRatio = Mathf.Clamp(speed / 20.0f, 0f, 1f);

        bool accel = Input.IsActionPressed("move_forward");
        bool reverse = Input.IsActionPressed("move_backward");
        bool left = Input.IsActionPressed("move_left");
        bool steerRight = Input.IsActionPressed("move_right");
        bool braking = Input.IsActionPressed("brake");

        // === 1. SPEED-SENSITIVE STEERING ===
        float targetSteer = 0f;
        if (left) targetSteer = 0.5f;          // Positive = left turn (positive Y ang.vel. in Godot = CCW = left)
        else if (steerRight) targetSteer = -0.5f;

        float steerReduction = 1f - speedRatio * 0.5f; // Halve steering at top speed
        targetSteer *= steerReduction;
        _steeringAngle = Mathf.MoveToward(_steeringAngle, targetSteer, (float)delta * 5f);

        // === 2. ACCELERATION / BRAKING ===
        var flatForward = forward;
        flatForward.Y = 0f;
        if (flatForward.Length() > 0.001f)
            flatForward = flatForward.Normalized();

        if (accel)
            ApplyCentralForce(flatForward * EnginePower);
        if (reverse)
            ApplyCentralForce(-flatForward * EnginePower * 0.5f);
        if (braking && speed > 0.5f)
        {
            var brakeForce = -LinearVelocity.Normalized() * BrakeForce;
            brakeForce.Y = 0f; // Prevent upward brake component
            ApplyCentralForce(brakeForce);
            LinearVelocity *= 0.95f;
        }

        // === 3. DRIFT STATE DETECTION ===
        float driftAngle = 0f;
        if (speed > 0.5f)
        {
            var velDir = LinearVelocity / speed;
            driftAngle = Mathf.Abs(Mathf.Acos(Mathf.Clamp(forward.Dot(velDir), -1f, 1f)));
        }

        bool isDrifting = driftAngle > DriftAngleThreshold && speed > DriftThreshold * 20f;
        bool liftOff = !accel && !reverse && (left || steerRight) && speed > 3f;

        // === 4. GRIP CALCULATION ===
        float targetGrip = GripFactor;
        if (isDrifting) targetGrip *= 0.4f;
        if (liftOff) targetGrip *= 0.6f;

        // Smoothly interpolate drift factor toward target
        float speedDt = Mathf.Clamp((float)delta * GripRestoreSpeed, 0f, 1f);
        _driftFactor = Mathf.Lerp(_driftFactor, 1f - targetGrip / GripFactor, speedDt);

        float currentGrip = GripFactor * (1f - _driftFactor * 0.8f);

        // === 5. LATERAL GRIP FORCE ===
        // Resist sideways sliding proportional to grip (Y zeroed to prevent lift)
        var localVel = GlobalTransform.Basis.Inverse() * LinearVelocity;
        float lateralSpeed = localVel.X;
        Vector3 lateralForce = -basisRight * lateralSpeed * currentGrip * LateralGripForce;
        lateralForce.Y = 0f;
        ApplyCentralForce(lateralForce);

        // === 6. ANGULAR VELOCITY CONTROL (PID-style) ===
        float maxOmega = isDrifting ? OmegaMaxDrift : OmegaMax;
        float targetOmega = _steeringAngle * maxOmega * 2f;
        float omegaError = targetOmega - AngularVelocity.Y;
        float omegaTorque = omegaError * SteeringTorque * (float)delta;
        ApplyTorque(new Vector3(0, omegaTorque, 0));

        // Clamp angular velocity
        var angVel = AngularVelocity;
        angVel.Y = Mathf.Clamp(angVel.Y, -maxOmega, maxOmega);
        AngularVelocity = angVel;

        // === 6b. SPEED CAP (prevent absurd velocities) ===
        const float MaxSpeed = 50f; // ~180 km/h
        var vel = LinearVelocity;
        if (vel.Length() > MaxSpeed)
        {
            vel = vel.Normalized() * MaxSpeed;
            LinearVelocity = vel;
        }

        // === 7. WEIGHT TRANSFER (visual feedback) ===
        if (accel || reverse)
        {
            float pitchDir = accel ? 1f : -1f;
            ApplyTorque(new Vector3(pitchDir * WeightTransferPitch, 0, 0));
        }
        if (left || steerRight)
        {
            float rollDir = left ? -1f : 1f;
            ApplyTorque(new Vector3(0, 0, rollDir * WeightTransferRoll * speedRatio));
        }

        // === 7b. ANGULAR DAMPING (prevents pitching/rolling endlessly) ===
        var angVelDamped = AngularVelocity;
        angVelDamped.X *= 0.92f;
        angVelDamped.Z *= 0.92f;
        AngularVelocity = angVelDamped;

        // === 8. TELEMETRY (unchanged) ===
        CollectTelemetry();
        _frameCount++;

        // === 9. STEERING WHEEL ROTATION (unchanged) ===
        var steerWheel = GetNodeOrNull<Node3D>("Interior/SteeringWheel");
        if (steerWheel != null)
            steerWheel.Rotation = new Vector3(1.5708f, _steeringAngle * 4f, 0);
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
        if (!_sessionFinished)
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

    public void FinishAndQuit()
    {
        if (_sessionFinished) return;
        _sessionFinished = true;
        FlushTelemetry();
        _backend.FinishSession();
        GetTree().Quit();
    }
}
