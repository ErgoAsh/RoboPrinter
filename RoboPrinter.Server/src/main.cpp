#include <servo_service.h>
#include <bluetooth_service.h>
#include <display_service.h>

static auto display_service = DisplayService();
static auto servo_service = ServoService(&display_service);
static auto bluetooth_service = BluetoothService(&servo_service);

#ifndef UNIT_TEST
void setup() {
    // Initialize serial monitor for USB debugging
    Serial.begin(115200);

    display_service.initialize();
    servo_service.initialize();
    bluetooth_service.initialize();

    delay(10);
}

void loop() {
    bluetooth_service.on_loop();

    delay(100);
}
#endif