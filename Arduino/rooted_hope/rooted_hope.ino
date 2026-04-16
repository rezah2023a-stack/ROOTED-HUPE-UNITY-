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

// ===== Trigger Flags (fire once only) =====
bool moistureTriggered = false;
bool ldrTriggered[]    = {false, false, false, false};
bool springTriggered   = false;

void setup() {
  Serial.begin(9600);

  strip.begin();
  strip.setBrightness(50);
  setAllColor(150, 200, 255); // winter blue on start
  strip.show();

  pinMode(LED_CONTROL_PIN, OUTPUT);
  digitalWrite(LED_CONTROL_PIN, LOW); // OFF at start
}

void loop() {
  int moistureValue = analogRead(MOISTURE_PIN);

  // Read all 4 LDR values
  int ldrValues[LDR_COUNT];
  for (int i = 0; i < LDR_COUNT; i++) {
    ldrValues[i] = analogRead(LDR_PINS[i]);
  }

  // ===== Moisture → Rain (first time only) =====
  if (!moistureTriggered && moistureValue >= MOISTURE_WET) {
    moistureTriggered = true;
    loadingFlow(0, 0, 255); // blue flow
    Serial.println("Rain");
  }

  // ===== Each LDR → Spotlight (first time only) =====
  for (int i = 0; i < LDR_COUNT; i++) {
    if (!ldrTriggered[i] && ldrValues[i] >= LDR_BRIGHT) {
      ldrTriggered[i] = true;
      loadingFlow(255, 255, 0); // yellow flow
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
      loadingFlow(255, 100, 150); // pink flow
      Serial.println("Spring");
      digitalWrite(LED_CONTROL_PIN, HIGH); // Branch LEDs ON!
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

    // Start slow, get faster toward the end
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