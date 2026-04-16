#include <Adafruit_NeoPixel.h>

// ===== NeoPixel LED Strip Settings =====
#define LED_PIN    6
#define LED_COUNT  50

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

void setup() {
  Serial.begin(9600);
  Serial.println("=== Full Test Start ===");

  // NeoPixel setup
  strip.begin();
  strip.setBrightness(50);
  setAllColor(150, 200, 255); // winter blue
  strip.show();

  // Branch LED setup
  pinMode(LED_CONTROL_PIN, OUTPUT);
  digitalWrite(LED_CONTROL_PIN, LOW);

  // Branch LED blink test
  Serial.println("Branch LED blink test...");
  for (int i = 0; i < 3; i++) {
    digitalWrite(LED_CONTROL_PIN, HIGH);
    delay(500);
    digitalWrite(LED_CONTROL_PIN, LOW);
    delay(500);
  }
  Serial.println("Branch LED test done!");

  // NeoPixel color test
  Serial.println("NeoPixel color test...");
  setAllColor(255, 0, 0); delay(500);   // red
  setAllColor(0, 255, 0); delay(500);   // green
  setAllColor(0, 0, 255); delay(500);   // blue
  setAllColor(150, 200, 255);           // back to winter blue
  strip.show();
  Serial.println("NeoPixel test done!");

  Serial.println("=== Sensor Test Start ===");
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

  // ===== LED Strip State =====
  bool isWet = moistureValue >= MOISTURE_WET;
  int brightCount = 0;
  for (int i = 0; i < LDR_COUNT; i++) {
    if (ldrValues[i] >= LDR_BRIGHT) brightCount++;
  }

  if (isWet && brightCount == LDR_COUNT) {
    Serial.println("State: SPRING 🌸");
    loadingFlow(255, 100, 150); // pink
    digitalWrite(LED_CONTROL_PIN, HIGH); // Branch LED ON!
  }
  else if (isWet) {
    Serial.println("State: RAIN 🌧️");
    loadingFlow(0, 0, 255); // blue
    digitalWrite(LED_CONTROL_PIN, LOW);
  }
  else if (brightCount > 0) {
    Serial.print("State: ");
    Serial.print(brightCount);
    Serial.println("/4 LDRs bright 💡");
    loadingFlow(255, 255, 0); // yellow
    digitalWrite(LED_CONTROL_PIN, LOW);
  }
  else {
    Serial.println("State: WINTER ❄️");
    setAllColor(150, 200, 255); // icy blue
    digitalWrite(LED_CONTROL_PIN, LOW);
  }

  delay(500);
}

// ===== Loading bar effect with accelerating speed =====
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