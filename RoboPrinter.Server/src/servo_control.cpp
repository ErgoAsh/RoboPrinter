#include "servo_control.h"

#include <Arduino.h>

#include <stdlib.h>
#include <cstring>
#include <string>

ServoControl::ServoControl(DisplayControl* display_control) { 
    _display_control = display_control;
    _pwm_driver = Adafruit_PWMServoDriver(); 
}

void ServoControl::initialize() {
    _pwm_driver.begin();
    _pwm_driver.setOscillatorFrequency(27000000); // TODO adjust values
    _pwm_driver.setPWMFreq(
        constants::servo_freq);  // Analog servos run at ~50 Hz updates
}

void ServoControl::process_data(const std::string& data, const short length) {
    if (data.empty() || length != 20) {
        Serial.println("Data parsing issue");
        return;
    }

    uint32_t values[5];
    for (int i = 0; i < length; i += 4) {
        uint32_t number;  // float binary representation (IEEE 754)
                          // 4 bytes per one float (32 bits in total)

        for (int j = i; j < i + 4; j++) {
            number = (number << 8) + data[j];  // Append one byte to the number
        }
        values[i] = number;

        float position;
        std::memcpy(&position, &number, sizeof(position));

        set_servo_position(i / 4, position);

        Serial.print(i / 4);
        Serial.print(": ");
        Serial.print(position);
        Serial.println();
    }
    Serial.println();
    
    _display_control->display_servo_values(values, 5);
}

void ServoControl::set_servo_position(const short servo_number,
                                      const float position) {
    if (servo_number < 0 ||
        servo_number > 5)  // TODO save & retrive value from somewhere
        return;            // TODO Return bool or throw error

    if (position < 0 || position > 180) return;

    _pwm_driver.setPWM(
        servo_number, 0,  // TODO save servo constants in servo structure
        map(position, 0, 180, constants::servo_min, constants::servo_max));
}