using MQTTnet;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Services
{
    public class MQTTService : IMQTTService
    {
        private readonly IMqttClient _mqttClient;
        private readonly string _mqttServer;
        private readonly int _mqttPort;
        private readonly string _mqttUsername;
        private readonly string _mqttPassword;
        private readonly string _mqttTopic;


        public MQTTService(IConfiguration configuration)
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();
            _mqttServer = configuration["MQTT:Server"];
            _mqttPort = int.Parse(configuration["MQTT:Port"]);
            _mqttUsername = configuration["MQTT:Username"];
            _mqttPassword = configuration["MQTT:Password"];
            _mqttTopic = configuration["MQTT:Topic"];
        }

        public async Task ConnectAsync()
        {
            MqttClientOptions options = new MqttClientOptionsBuilder()
                .WithClientId("FoosballProLeagueClient")
                .WithTcpServer(_mqttServer, _mqttPort)
                .WithCredentials(_mqttUsername, _mqttPassword)
                .WithCleanSession()
                .Build();

            await _mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        public async Task PublishMessageAsync(string message)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            MqttApplicationMessage mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_mqttTopic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag()
                .Build();

            await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
        }
    }
}