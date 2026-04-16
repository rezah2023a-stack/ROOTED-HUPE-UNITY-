#include <Adafruit_NeoPixel.h>

// ===== NeoPixel LED Strip Settings =====
#define LED_PIN    6
#define LED_COUNT  46

Adafruit_NeoPixel strip(LED_COUNT, LED_PIN, NEO_GRB + NEO_KHZ800);

// ===== Branch LED Settings =====
#define LED_CONTROL_PIN 9

// ===== Sensor Pin Settings =====
const int MOISTURE_PIN = A4;
const int LDR_PINS[]   = {A0, A1, A2, A3};
const int LDR_COUNT    = 4;

// ===== Threshold Values =====
const int MOISTURE_WET = 150;
const int LDR_BRIGHT   = 500;

// ===== Trigger Flags =====
bool moistureTriggered = false;
bool ldrTriggered[]    = {false, false, false, false};
bool springTriggered   = false;

void setup() {
  Serial.begin(9600);

  strip.begin();
  strip.setBrightness(50);
  setAllColor(150, 200, 255);
  strip.show();

  pinMode(LED_CONTROL_PIN, OUTPUT);
  digitalWrite(LED_CONTROL_PIN, LOW);
}

void loop() {
  // ===== Reset 신호 수신 =====
  if (Serial.available() > 0) {
    String received = Serial.readStringUntil('\n');
    received.trim();
    if (received == "Reset") {
      resetAll();
    }
  }

  int moistureValue = analogRead(MOISTURE_PIN);

  int ldrValues[LDR_COUNT];
  for (int i = 0; i < LDR_COUNT; i++) {
    ldrValues[i] = analogRead(LDR_PINS[i]);
  }

  // ===== Moisture → Rain =====
  if (!moistureTriggered && moistureValue >= MOISTURE_WET) {
    moistureTriggered = true;
    loadingFlow(0, 0, 255);
    Serial.println("Rain");
  }

  // ===== LDR → Spotlight =====
  for (int i = 0; i < LDR_COUNT; i++) {
    if (!ldrTriggered[i] && ldrValues[i] >= LDR_BRIGHT) {
      ldrTriggered[i] = true;
      loadingFlow(255, 255, 0);
      Serial.println("Spotlight" + String(i + 1));
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
      loadingFlow(255, 100, 150);
      Serial.println("Spring");
      digitalWrite(LED_CONTROL_PIN, HIGH);
    }
  }

  delay(100);
}

// ===== 리셋 함수 =====
void resetAll() {
  moistureTriggered = false;
  springTriggered = false;
  for (int i = 0; i < LDR_COUNT; i++) {
    ldrTriggered[i] = false;
  }
  digitalWrite(LED_CONTROL_PIN, LOW);
  setAllColor(150, 200, 255);
  strip.show();
}

// ===== Loading bar effect =====
void loadingFlow(int r, int g, int b) {
  strip.clear();
  strip.show();
  delay(200);

  for (int i = 0; i < LED_COUNT; i++) {
    strip.setPixelColor(i, strip.Color(r, g, b));
    strip.show();
    int waitTime = map(i, 0, LED_COUNT - 1, 200, 10);
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