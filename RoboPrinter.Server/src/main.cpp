#include <Wire.h>

#include "servo_control.h"

void processIncomingByte(const byte input_byte);

namespace constants {

// Maximum length of input of Bluetooth transmission
static const unsigned int bluetooth_max_input = 16;

}  // namespace constants

static auto servo = ServoControl();

void setup() {
    Serial.begin(9600);   // Serial monitor for USB debugging
    Serial1.begin(9600);  // Arduino Nano Every UART

    servo.initialize();

    delay(10);
}

void loop() {
    if (Serial1.available() > 0) {
        processIncomingByte(Serial1.read());
    }
}

void processIncomingByte(const byte input_byte) {
    static char input_line[constants::bluetooth_max_input];
    static unsigned int input_position = 0;

    switch (input_byte) {
        case '\n':                           // end of text
            input_line[input_position] = 0;  // terminating null byte

            // terminator reached process input_line here
            if (input_line[0] == 'T') {
                Serial1.println("T\n");
                // TODO move to a separate function?
            } else {
                servo.process_serial_data(input_line, input_position + 1);
            }

            // reset buffer for next time
            input_position = 0;
            break;

        case '\r':  // discard carriage return
            break;

        default:
            // keep adding if not full; allow for terminating null byte
            if (input_position < (constants::bluetooth_max_input - 1)) {
                input_line[input_position++] = input_byte;
            }
            break;
    }
}