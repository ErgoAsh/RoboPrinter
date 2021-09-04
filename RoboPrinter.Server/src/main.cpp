#include <Wire.h>

#include "servo_control.h"
#include "BluetoothSerial.h"

// Check if Bluetooth configurations are enabled in the SDK 
// If not, then you have to recompile the SDK 
#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

void processIncomingByte(const byte input_byte);
void callback(esp_spp_cb_event_t event, esp_spp_cb_param_t *param);

namespace constants {
    // Maximum length of input of Bluetooth transmission
    static const unsigned int bluetooth_max_input = 16;
}

static auto servo = ServoControl();
static BluetoothSerial SerialBT = BluetoothSerial();

void setup() {
    Serial.begin(115200);   // Serial monitor for USB debugging

    SerialBT.setPin("1234");
    SerialBT.register_callback(callback);

    if (!SerialBT.begin("RoboPrinter.Board")) {
        Serial.println("An error occurred initializing Bluetooth");
    } else {
        Serial.println("Bluetooth initialized");
    }

    servo.initialize();

    delay(10);
}

void loop() {
    if (SerialBT.available() > 0) {
        processIncomingByte(SerialBT.read());
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
                SerialBT.println("T");
                Serial.println("T");
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

void callback(esp_spp_cb_event_t event, esp_spp_cb_param_t *param) {
    if (event == ESP_SPP_SRV_OPEN_EVT){
        Serial.println("Client Connected");
        SerialBT.println("M_BT_CONNECTED");
    }
}