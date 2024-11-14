// sensorHandler.ino

#include "sensorHandler.h"
#include "LedHandler.h"      // To handle LED interaction
#include "ApiRequester.h"    // To send HTTP requests
#include <MFRC522.h>
#include <SPI.h>


namespace sensorHandler {
    const int numSensors = 4;
    const int sensorPins[numSensors] = {18, 19, 22, 23}; // Sensor pins
    unsigned long lastTriggerTimes[numSensors] = {0};   // Time of the last trigger for each sensor
    const unsigned long debounceDelay = 500;            // Debounce delay in milliseconds

    // RFID pins
    const int RFID_RST_PIN = 23;   // Reset pin for RFID module
    const int RFID_SS_PIN = 26;    // Slave Select pin for RFID module

    MFRC522 mfrc522(RFID_SS_PIN, RFID_RST_PIN);  // Create instance of MFRC522 class

    String userId = "";  // This will store the userId inputted via Serial Monitor



    void initializeSensors() {
        for (int i = 0; i < numSensors; i++) {
            pinMode(sensorPins[i], INPUT_PULLDOWN);  // Enable internal pull-down resistor
        }
        LedHandler::initializeLed();         // Initialize the LED
        Serial.begin(9600);                  // Initialize Serial for debug output
        Serial.println("Sensors and LED initialized.");

        //Initialize RFID reader
        SPI.begin();
        mfrc522.PCD_Init();
        Serial.println("RFID Reader Initialized");
    }

    void monitorSensors() {
        unsigned long currentTime = millis();

        // Check for RFID card data
        if(mfrc522.PICC_IsNewCardPresent()){
          if (mfrc522.PICC_ReadCardSerial()) {
                // Read the RFID card serial number
                userId = "";  // Clear previous userId
                for (byte i = 0; i < mfrc522.uid.size; i++) {
                    userId += String(mfrc522.uid.uidByte[i], HEX); // Convert byte to string
                }

                // Print the userId (RFID card ID)
                Serial.print("RFID Card Detected with ID: ");
                Serial.println(userId);

                // Send the userId to the API for login
                ApiRequester::sendLoginOnTable(userId);
                LedHandler::shortFlash();  // Flash the LED as feedback

                // Halt the card to stop reading it
                mfrc522.PICC_HaltA();
            }
          }
        }



        for (int i = 0; i < numSensors; i++) {
            int sensorState = digitalRead(sensorPins[i]);

            if (sensorState == HIGH && (currentTime - lastTriggerTimes[i] >= debounceDelay)) {
                lastTriggerTimes[i] = currentTime;
                Serial.print("Sensor on pin ");
                Serial.print(sensorPins[i]);
                Serial.println(" triggered, sending HTTP request...");
                LedHandler::shortFlash();

                // Trigger the corresponding API request based on the pin
                switch(sensorPins[i]) {
                    case 18:
                        ApiRequester::sendRegisterGoal();
                        break;
                    case 19:
                        ApiRequester::sendInterruptMatch();
                        break;
                    case 22:
                        ApiRequester::sendStartMatch();
                        break;
                    case 23:
                        ApiRequester::sendLoginOnTable();
                        break;
                    default:
                        Serial.println("Unknown sensor pin!");
                        break;
                }
                Serial.println("HTTP request sent.");
            }
        }
    }
}
