#include <Arduino.h>
#include <servo_service.h>
#include <stdlib.h>

#include <array>
#include <cstring>
#include <string>

ServoService::ServoService(DisplayService* display_service) {
    _display_service = display_service;
    _pwm_driver = Adafruit_PWMServoDriver();
}

void ServoService::initialize() {
    _pwm_driver.begin();
    _pwm_driver.setOscillatorFrequency(27000000);  // TODO adjust values
    _pwm_driver.setPWMFreq(
        constants::servo_freq);  // Analog servos run at ~50 Hz updates
}

std::array<float, 5> ServoService::parse_data(const std::string& data,
                                              const short length) {
    if (data.empty() || length != 20) {
        Serial.println("Data parsing issue");
        return std::array<float, 5>();
    }

    std::array<float, 5> values;
    for (size_t i = 0; i < length; i += 4) {
        uint32_t number = 0;  // float binary representation (IEEE 754)
                              // 4 bytes per one float (32 bits in total)

        for (size_t j = i; j < i + 4; j++) {
            // Shift existing number by 8 bits and append one byte using AND
            // operation (since bits does not overlap, nothing will be
            // overwritten)
            number = (number << 8) | data[j];
        }

        float position;
        std::memcpy(&position, &number, sizeof(number));
        values[i / 4] = position;

        Serial.print(i / 4);
        Serial.print(": ");
        Serial.print(position);
        Serial.print(": ");
        Serial.print(number);
        Serial.println();
    }
    Serial.println();
    return values;
}

void ServoService::process_data(const std::string& data, const short length) {
    std::array<float, 5> values = parse_data(data, length);

    for (int i = 0; i < 5; i++) {
        set_servo_position(i / 4, values[i]);
    }

    if (_display_service != nullptr) {
        _display_service->display_servo_values(values);
    }
}

void ServoService::set_servo_position(const short servo_number,
                                      const float position) {
    if (servo_number < 0 ||
        servo_number > 5)  // TODO save & retrive value from somewhere
        return;            // TODO Return bool or throw error

    if (position < 0 || position > 180) return;

    _pwm_driver.setPWM(
        servo_number, 0,  // TODO save servo constants in servo structure
        map(position, 0, 180, constants::servo_min, constants::servo_max));
}