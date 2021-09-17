#include <display_service.h>

DisplayService::DisplayService() { 
    driver = Adafruit_SSD1306( 
        constants::screen_width, constants::screen_height, &Wire, -1);
} 

void DisplayService::initialize() {
    // Initialize OLED with SSD1306 driver; 0x3C is I2C address
    if (!driver.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
        Serial.println("SSD1306 allocation failed");

        for (;;)
            ;  // Don't proceed, loop forever
    }

    driver.clearDisplay();
    driver.setTextSize(1);
    driver.setTextColor(WHITE);

    std::array<float, 5> values = { 0, 0, 0, 0, 0 };
    display_servo_values(values);
}

void DisplayService::display_servo_values(std::array<float, 5> values) {
    driver.clearDisplay();
    driver.display();
    driver.setCursor(0, 0);

    for (int i = 0; i < 5; i++) {
        driver.print("TH");
        driver.print(i);
        driver.print(": ");
        driver.print(values[i]);
        driver.println();
    }

    driver.display();
}