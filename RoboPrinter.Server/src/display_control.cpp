#include "display_control.h"

DisplayControl::DisplayControl() {
    driver = Adafruit_SSD1306(
        constants::screen_width, constants::screen_height, &Wire, -1);
} 

void DisplayControl::initialize() {
    // Initialize OLED with SSD1306 driver; 0x3C is I2C address
    if (!driver.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
        Serial.println("SSD1306 allocation failed");

        for (;;)
            ;  // Don't proceed, loop forever
    }

    driver.clearDisplay();
    driver.setTextSize(1);
    driver.setTextColor(WHITE);
}

void DisplayControl::display_servo_values(uint32_t* values, uint8_t length) {
    driver.clearDisplay();
    driver.setCursor(0, 0);

    for (int i = 0; i < length; i++) {
        driver.print("TH");
        driver.print(i);
        driver.print(": ");
        driver.println(values[i]);
    }

    driver.display();
}