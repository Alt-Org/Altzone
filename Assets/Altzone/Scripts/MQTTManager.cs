using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Altzone.Scripts;
using MQTTnet;
using MQTTnet.Client;
using UnityEngine;

public class MQTTManager : MonoBehaviour
{
    public static MQTTManager Instance { get; private set; }
    public IMqttClient Client => _client; 

    private IMqttClient _client = null;

    public delegate void MQTTConnectionEstablished(bool established);
    public static event MQTTConnectionEstablished OnMQTTConnectionEstablished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private async void OnDestroy()
    {
        if (_client != null && _client.IsConnected)
        {
            await _client.DisconnectAsync();
        }
    }

    public async void StartMQTT()
    {
        await StartMQTTAsync();
    }

    private async Task StartMQTTAsync()
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var topic = $"/matchmaking/invites/player/unity-test-{69}";

        _client.ApplicationMessageReceivedAsync += (e) =>
        {
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            Debug.Log($"MQTT message received. Topic: {e.ApplicationMessage.Topic}");
            Debug.Log($"Payload: {message}");
            return Task.CompletedTask;
        };

        _client.ConnectedAsync += e =>
        {
            Debug.Log("MQTT connected");
            OnMQTTConnectionEstablished?.Invoke(true);
            return Task.CompletedTask;
        };

        _client.DisconnectedAsync += e =>
        {
            Debug.LogWarning($"MQTT disconnected: {e.Reason}");
            OnMQTTConnectionEstablished?.Invoke(false);
            return Task.CompletedTask;
        };

        var options = new MqttClientOptionsBuilder()
            .WithWebSocketServer(o => o.WithUri("ws://notifications.altzone.fi"))
            .WithCredentials("subscriber", "QNecDttbY92MzfURzPzOjYvICnBkmAXI")
            .WithClientId(ServerManager.Instance.Player._id)
            .WithCleanSession()
            .Build();

        try
        {
            Debug.Log($"Connecting to ws://notifications.altzone.fi");
            await _client.ConnectAsync(options);

            Debug.Log($"Subscribing to {topic}");
            await _client.SubscribeAsync(topic);
        }
        catch (Exception ex)
        {
            Debug.LogError($"MQTT connection failed: {ex}");
        }
    }

    [Serializable]
    private class MqttTestMessage
    {
        public string type;
        public MqttTestPayload payload;
    }

    [Serializable]
    private class MqttTestPayload
    {
        public bool test;
        public string source;
        public string ts;
    }
}
