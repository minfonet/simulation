using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HttpClient = System.Net.Http.HttpClient;

namespace DrivingSim;

public class BackendClient
{
    private static readonly HttpClient Client = new();
    private string _baseUrl = "http://localhost:8080";
    private Guid _sessionId;
    private string _token = "";

    public void ReadSessionId()
    {
        var args = OS.GetCmdlineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--session-id" && Guid.TryParse(args[i + 1], out var id))
                _sessionId = id;
        }

        var tokenArg = Array.IndexOf(args, "--token");
        if (tokenArg >= 0 && tokenArg + 1 < args.Length)
            _token = args[tokenArg + 1];

        var urlArg = Array.IndexOf(args, "--api-url");
        if (urlArg >= 0 && urlArg + 1 < args.Length)
            _baseUrl = args[urlArg + 1];

        if (_sessionId == Guid.Empty)
            GD.PrintErr("No --session-id argument provided. Telemetry will not be sent.");
    }

    public async void StartSession()
    {
        if (_sessionId == Guid.Empty) return;
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post, $"{_baseUrl}/api/trainee/sessions/{_sessionId}/start")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };

            if (!string.IsNullOrEmpty(_token))
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                GD.Print($"Session {_sessionId} started");
            else
                GD.PrintErr($"Failed to start session: {response.StatusCode}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Start session error: {e.Message}");
        }
    }

    public async void SendTelemetryBatch(List<VehicleController.TelemetryPoint> points)
    {
        if (_sessionId == Guid.Empty || points.Count == 0) return;
        try
        {
            var payload = new
            {
                sessionId = _sessionId.ToString(),
                points = points.ConvertAll(p => new
                {
                    timestamp = p.Timestamp.ToString("o"),
                    speed = p.Speed,
                    steeringAngle = p.SteeringAngle,
                    positionX = p.PositionX,
                    positionY = p.PositionY,
                    positionZ = p.PositionZ,
                    collision = p.Collision,
                }),
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/telemetry")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            if (!string.IsNullOrEmpty(_token))
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                GD.PrintErr($"Telemetry send failed: {response.StatusCode}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Telemetry error: {e.Message}");
        }
    }

    public async void FinishSession()
    {
        if (_sessionId == Guid.Empty) return;
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post, $"{_baseUrl}/api/trainee/sessions/{_sessionId}/finish")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };

            if (!string.IsNullOrEmpty(_token))
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                GD.Print($"Session {_sessionId} finished");
            else
                GD.PrintErr($"Failed to finish session: {response.StatusCode}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Finish session error: {e.Message}");
        }
    }
}
