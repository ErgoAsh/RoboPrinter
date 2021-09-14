#include "servo_control.h"
#include "bluetooth_control.h"
#include "display_control.h"

static auto display_control = DisplayControl();
static auto servo_control = ServoControl(&display_control);
static auto bluetooth_control = BluetoothControl(&servo_control);

void setup() {
    // Initialize serial monitor for USB debugging
    Serial.begin(115200);

    display_control.initialize();
    servo_control.initialize();
    bluetooth_control.initialize();

    delay(10);
}

void loop() {
    bluetooth_control.on_loop();

    delay(100);
}