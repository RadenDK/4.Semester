#include <WiFi.h>
#include <HTTPClient.h>
#include <WebServer.h>
#include <Preferences.h>
#include <ESPmDNS.h>  // Include mDNS library for hostname resolution

// Define pin numbers
const int buttonPin = 0;   // Pin for mode-switch button (GPIO 0 is typically the BOOT button)
const int ledPin = 2;      // LED to indicate AP mode or request being made
const int sensorPin = 23;  // GPIO 23 for wire touch detection

// Access Point credentials
const char* apSSID = "ESP32_Config";
const char* apPassword = "12345678";

// Web server for AP mode
WebServer server(80);

// Preferences for storing settings
Preferences preferences;

bool apMode = false;  // Track if ESP32 is in AP mode
unsigned long previousMillis = 0;  // For non-blocking LED flashing
const long interval = 100;         // Interval for LED flashing in AP mode

// WiFi credentials and other settings
String ssid = "";
String password = "";
String tableId = "";
String companyId = "";
String departmentId = "";
String side = "";

void setup() {
  pinMode(buttonPin, INPUT_PULLUP);  // Initialize button for AP mode
  pinMode(ledPin, OUTPUT);
  pinMode(sensorPin, INPUT);

  // Start with LED off
  digitalWrite(ledPin, LOW);

  // Initialize Serial monitor
  Serial.begin(115200);

  // Load stored configuration
  preferences.begin("config", false);
  
  // Check if Wi-Fi credentials are saved
  ssid = preferences.getString("ssid", "");
  password = preferences.getString("password", "");
  tableId = preferences.getString("tableId", "");
  companyId = preferences.getString("companyId", "");
  departmentId = preferences.getString("departmentId", "");
  side = preferences.getString("side", "");
  
  // Debugging: Print loaded settings
  Serial.println("Loaded Settings:");
  Serial.print("SSID: "); Serial.println(ssid);
  Serial.print("Password: "); Serial.println(password);
  Serial.print("Table ID: "); Serial.println(tableId);
  Serial.print("Company ID: "); Serial.println(companyId);
  Serial.print("Department ID: "); Serial.println(departmentId);
  Serial.print("Side: "); Serial.println(side);

  // If valid settings are saved, connect to Wi-Fi; otherwise enter AP mode
  if (ssid == "" || password == "" || tableId == "" || companyId == "" || departmentId == "" || side == "") {
    Serial.println("No valid settings found, entering AP mode.");
    enterAPMode();
  } else {
    connectToWiFi();
  }

  // Set up button interrupt for AP mode
  attachInterrupt(digitalPinToInterrupt(buttonPin), toggleAPMode, FALLING);
}

void enterAPMode() {
  apMode = true;  // Activate AP mode

  // Start the ESP32 in access point mode
  WiFi.softAP(apSSID, apPassword);
  IPAddress IP = WiFi.softAPIP();
  Serial.print("AP IP Address: ");
  Serial.println(IP);

  // Set up mDNS (hostname will be 'esp32config.local')
  if (!MDNS.begin("esp32config")) {
    Serial.println("Error setting up mDNS responder!");
  } else {
    Serial.println("mDNS responder started: esp32config.local");
  }

  // Set up web server routes for configuration
  server.on("/", handleRoot);
  server.on("/save", handleSave);

  server.begin();  // Start the web server
  Serial.println("Web server started in AP mode.");
}

void connectToWiFi() {
  Serial.println("Connecting to WiFi...");
  WiFi.begin(ssid.c_str(), password.c_str());

  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED && attempts < 20) {  // Retry 20 times
    delay(500);
    Serial.print(".");
    attempts++;
  }

  if (WiFi.status() == WL_CONNECTED) {
    Serial.println();
    Serial.println("Connected to WiFi!");
    Serial.print("IP Address: ");
    Serial.println(WiFi.localIP());
    apMode = false;
  } else {
    Serial.println();
    Serial.println("Failed to connect to WiFi, entering AP mode.");
    enterAPMode();  // Fallback to AP mode if connection fails
  }
}

void toggleAPMode() {
  if (!apMode) {
    enterAPMode();
  } else {
    apMode = false;
    digitalWrite(ledPin, LOW);
    WiFi.softAPdisconnect(true);
    server.stop();
    ESP.restart();
  }
}

void handleRoot() {
  String html = "<html><body><h1>ESP32 Configuration</h1>";
  html += "<form action='/save' method='POST'>";
  html += "WiFi SSID: <input type='text' name='ssid'><br>";
  html += "WiFi Password: <input type='password' name='password'><br>";
  html += "Table ID: <input type='text' name='tableId'><br>";
  html += "Company ID: <input type='text' name='companyId'><br>";
  html += "Department ID: <input type='text' name='departmentId'><br>";
  html += "Side (red/blue): <input type='text' name='side'><br>";
  html += "<input type='submit' value='Save'>";
  html += "</form></body></html>";

  server.send(200, "text/html", html);
}

void handleSave() {
  if (server.hasArg("ssid") && server.hasArg("password") && server.hasArg("tableId") &&
      server.hasArg("companyId") && server.hasArg("departmentId") && server.hasArg("side")) {

    ssid = server.arg("ssid");
    password = server.arg("password");
    tableId = server.arg("tableId");
    companyId = server.arg("companyId");
    departmentId = server.arg("departmentId");
    side = server.arg("side");

    // Store the configuration in Preferences
    preferences.putString("ssid", ssid);
    preferences.putString("password", password);
    preferences.putString("tableId", tableId);
    preferences.putString("companyId", companyId);
    preferences.putString("departmentId", departmentId);
    preferences.putString("side", side);

    Serial.println("Settings saved.");
    server.send(200, "text/html", "Settings saved! Rebooting ESP32 to apply.");
    
    // Reboot the ESP32 after saving the settings
    ESP.restart();
  } else {
    server.send(200, "text/html", "Failed to save settings. All fields are required.");
  }
}

void sendPostRequest() {
  if (WiFi.status() == WL_CONNECTED) {
    HTTPClient http;

    // Set up the request URL
    http.begin("https://api.foosballproleague.live/test");

    // Set content type header
    http.addHeader("Content-Type", "application/json");

    // Prepare JSON data
    String jsonPayload = "{\"tableId\":\"" + tableId + "\", \"companyId\":\"" + companyId + "\", \"departmentId\":\"" + departmentId + "\", \"side\":\"" + side + "\"}";

    // Send HTTP POST request
    int httpResponseCode = http.POST(jsonPayload);

    if (httpResponseCode > 0) {
      String response = http.getString();
      Serial.println("HTTP Response code: " + String(httpResponseCode));
      Serial.println("Response: " + response);
    } else {
      Serial.println("Error on sending POST: " + String(httpResponseCode));
    }

    http.end();  // Free resources
  } else {
    Serial.println("WiFi not connected");
  }
}

void loop() {
  if (apMode) {
    server.handleClient();  // Handle incoming web requests in AP mode

    // Flash LED rapidly in AP mode
    unsigned long currentMillis = millis();
    if (currentMillis - previousMillis >= interval) {
      previousMillis = currentMillis;
      int state = digitalRead(ledPin);
      digitalWrite(ledPin, !state);  // Toggle LED state
    }
  } else {
    // In normal mode, monitor sensor input and handle LED
    int sensorState = digitalRead(sensorPin);
    if (sensorState == HIGH) {  // Replace with actual sensor logic
      digitalWrite(ledPin, HIGH);  // Turn on LED when a request is being made
      sendPostRequest();  // Send POST request
      delay(1000);  // Simulate request delay
      digitalWrite(ledPin, LOW);  // Turn off LED after request
    }
  }
}
