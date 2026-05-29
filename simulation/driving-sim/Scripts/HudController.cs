using Godot;
using System;

namespace DrivingSim;

public partial class HudController : CanvasLayer
{
    private Label _speedLabel;
    private Label _steeringLabel;
    private Label _controlsHint;
    private Button _finishButton;
    private VehicleController _car;

    public override void _Ready()
    {
        _speedLabel = GetNode<Label>("SpeedLabel");
        _steeringLabel = GetNode<Label>("SteeringLabel");
        _controlsHint = GetNode<Label>("ControlsHint");
        _finishButton = GetNode<Button>("FinishButton");
        _car = GetNode<VehicleController>("../Car");

        _finishButton.Pressed += OnFinishPressed;
    }

    public override void _Process(double delta)
    {
        // Speed: m/s → km/h (multiply by 3.6)
        float speedKmh = _car.CurrentSpeed * 3.6f;
        _speedLabel.Text = $"Speed: {speedKmh:F0} km/h";

        // Steering: visual bar [-0.5 ... 0 ... 0.5] → 0-10 scale
        float steer = _car.CurrentSteering;
        int barPos = Mathf.Clamp((int)((steer + 0.5f) * 10f), 0, 10);
        string bar = new string('-', barPos) + "O" + new string('-', 10 - barPos);
        _steeringLabel.Text = $"STEERING: {bar}";
    }

    private void OnFinishPressed()
    {
        _car.FinishAndQuit();
    }
}
