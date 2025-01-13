#include "MqttHandler.h"
#include "LcdHandler.h"
#include <WiFi.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include "ConfigMode.h"  // Include ConfigMode to access settings

namespace MqttHandler {
    // MQTT settings
    const String mqttServer = "foosballproleague.live";
    const int mqttPort = 1883;
    const String mqttTopic = "test/topic";

    bool firstMessage = true;

    WiFiClient espClient;
    PubSubClient mqttClient(espClient);

    void connectToMQTT() {
        while (!mqttClient.connected()) {
            Serial.println("Connecting to MQTT broker...");
            // Convert String to C-style strings using c_str()
            if (mqttClient.connect("ESP32Client", ConfigMode::getMqttUsername().c_str(), ConfigMode::getMqttPassword().c_str())) {
                Serial.println("Connected to MQTT!");
                mqttClient.subscribe(mqttTopic.c_str());
            } else {
                Serial.print("MQTT connection failed: ");
                Serial.println(mqttClient.state());
                delay(5000);
            }
        }
    }

    void mqttCallback(char* topic, byte* payload, unsigned int length) {

        if (firstMessage) {
            firstMessage = false;
            return; // Discard the very first message received
        } 

        // Convert payload to a String
        String message = "";
        for (int i = 0; i < length; i++) {
            message += (char)payload[i];
        }
        Serial.println("Message received: " + message);

        // Parse the JSON message
        StaticJsonDocument<200> doc;
        DeserializationError error = deserializeJson(doc, message);

        if (error) {
            Serial.print("JSON parse failed: ");
            Serial.println(error.c_str());
            return;
        }

        // Extract scores from JSON
        if (doc.containsKey("RedScore") && doc.containsKey("BlueScore")) {
            int redScore = doc["RedScore"];
            int blueScore = doc["BlueScore"];

            Serial.print("Red Score: ");
            Serial.println(redScore);
            Serial.print("Blue Score: ");
            Serial.println(blueScore);

            // Update the LCD with the scores
            LcdHandler::UpdateLcdSetScores(redScore, blueScore);
        } else {
            Serial.println("Invalid JSON format. Missing keys 'RedScore' or 'BlueScore'.");
        }
    }

    void initialize() {
        // Convert mqttServer to a C-string before passing to setServer
        mqttClient.setServer(mqttServer.c_str(), mqttPort);
        mqttClient.setCallback(mqttCallback);
        connectToMQTT();
    }

    void loop() {
        if (!mqttClient.connected()) {
            connectToMQTT();
        }
        mqttClient.loop();
    }
}
