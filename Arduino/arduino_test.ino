#include <Adafruit_NeoPixel.h>

// ===== NeoPixel LED Strip Settings =====
#define LED_PIN    6
#define LED_COUNT  30

Adafruit_NeoPixel strip(LED_COUNT, LED_PIN, NEO_GRB + NEO_KHZ800);

// ===== Branch LED Settings =====
#define LED_CONTROL_PIN 9

// ===== Sensor Pin Settings =====
const int MOISTURE_PIN = A4;
const int LDR_PINS[]   = {A0, A1, A2, A3};
const int LDR_COUNT    = 4;

// ===== Threshold Values =====
const int MOISTURE_WET = 600;
const int LDR_BRIGHT   = 500;

// ===== Trigger Flags =====
bool moistureTriggered = false;
bool ldrTriggered[]    = {false, false, false, false};
bool springTriggered   = false;

void setup() {
  Serial.begin(9600);
  Serial.println("=== Sensor + LED Test ===");

  strip.begin();
  strip.setBrightness(50);
  setAllColor(150, 200, 255);
  strip.show();

  pinMode(LED_CONTROL_PIN, OUTPUT);
  digitalWrite(LED_CONTROL_PIN, LOW);
}

void loop() {
  int moistureValue = analogRead(MOISTURE_PIN);
  int ldrValues[LDR_COUNT];
  for (int i = 0; i < LDR_COUNT; i++) {
    ldrValues[i] = analogRead(LDR_PINS[i]);
  }

  // ===== Serial Output =====
  Serial.println("=============================");
  Serial.print("Moisture: ");
  Serial.print(moistureValue);
  Serial.println(moistureValue >= MOISTURE_WET ? " → WET 💧" : " → DRY");

  for (int i = 0; i < LDR_COUNT; i++) {
    Serial.print("LDR");
    Serial.print(i + 1);
    Serial.print(": ");
    Serial.print(ldrValues[i]);
    Serial.println(ldrValues[i] >= LDR_BRIGHT ? " → BRIGHT ☀️" : " → DARK");
  }

  // ===== Moisture → Rain (first time only) =====
  if (!moistureTriggered && moistureValue >= MOISTURE_WET) {
    moistureTriggered = true;
    Serial.println(">>> Moisture triggered! Loading Rain...");
    loadingFlow(100, 150, 255);
  }

  // ===== Each LDR → Spotlight (first time only) =====
  for (int i = 0; i < LDR_COUNT; i++) {
    if (!ldrTriggered[i] && ldrValues[i] >= LDR_BRIGHT) {
      ldrTriggered[i] = true;
      Serial.print(">>> LDR");
      Serial.print(i + 1);
      Serial.println(" triggered! Loading Spotlight...");
      loadingFlow(255, 255, 200);
    }
  }

  // ===== All triggered → Spring =====
  if (!springTriggered) {
    bool allDone = moistureTriggered;
    for (int i = 0; i < LDR_COUNT; i++) {
      if (!ldrTriggered[i]) {
        allDone = false;
        break;
      }
    }

    if (allDone) {
      springTriggered = true;
      Serial.println(">>> All triggered! Loading Spring...");
      loadingFlow(255, 100, 150);
      digitalWrite(LED_CONTROL_PIN, HIGH);
      Serial.println("State: SPRING 🌸 Branch LED ON!");
    }
  }

  delay(100);
}

// ===== Loading bar effect with accelerating speed =====
void loadingFlow(int r, int g, int b) {
  // Clear all LEDs first
  strip.clear();
  strip.show();
  delay(200);

  for (int i = 0; i < LED_COUNT; i++) {
    strip.setPixelColor(i, strip.Color(r, g, b));
    strip.show();
    int waitTime = map(i, 0, LED_COUNT - 1, 150, 10);
    delay(waitTime);
  }
}

// ===== Set All LEDs to One Color =====
void setAllColor(int r, int g, int b) {
  for (int i = 0; i < LED_COUNT; i++) {
    strip.setPixelColor(i, strip.Color(r, g, b));
  }
  strip.show();
}