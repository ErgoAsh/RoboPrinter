#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <BLE2902.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <Wire.h>

#include "servo_control.h"

namespace constants {
static const short screen_width = 128;
static const short screen_height = 64;
static const char* service_uuid = "e31ba033-e901-4566-afb2-8c545c554c8b";
static const char* sensor_uuid = "d8593ea1-0153-4397-9f2e-13703b529215";
static const char* position_uuid = "944eedc2-3725-425e-9817-0ab0bfda64fe";
static const char* time_response_uuid = "9539242d-efbb-414e-a10a-2a06e7445c67";
}  // namespace constants

static auto servo_driver = ServoControl();
static auto display_driver = Adafruit_SSD1306(
    constants::screen_width, constants::screen_height, &Wire, -1);

static BLEServer* ble_server = NULL;
static BLECharacteristic* sensor_characteristic = NULL;
static BLECharacteristic* position_characteristic = NULL;
static BLECharacteristic* response_time_characteristic = NULL;

static bool central_connected = false;
static bool old_central_connected = false;

// TODO move to bluetooth class
class CustomServerCallbacks : public BLEServerCallbacks {
    void onConnect(BLEServer* ble_server) override {
        central_connected = true;
    };

    void onDisconnect(BLEServer* ble_server) override {
        central_connected = false;
    }

    void onMtuChanged(BLEServer* pServer, esp_ble_gatts_cb_param_t* param) {
        Serial.print("New MTU length:");
        Serial.print(param->mtu.mtu);
        Serial.println();
    }
};

class CustomResponseCallbacks : public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic* characteristic) override {
        auto input_data = characteristic->getValue().c_str();
        auto length = characteristic->getValue().length();
        Serial.print("Received message: ");
        Serial.print(input_data);
        Serial.println();

        if (input_data[0] == 'T') {
            response_time_characteristic->setValue("T");
            response_time_characteristic->notify();
        }

        servo_driver.process_serial_data(input_data, length);
    }
};

void setup() {
    // Initialize serial monitor for USB debugging
    Serial.begin(115200);

    // Initialize OLED with SSD1306 driver; 0x3C is I2C address
    if (!display_driver.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
        Serial.println("SSD1306 allocation failed");

        for (;;)
            ;  // Don't proceed, loop forever
    }

    // TODO move to display class
    display_driver.clearDisplay();
    display_driver.setTextSize(1);
    display_driver.setTextColor(WHITE);
    display_driver.display();

    // Initialize BLE
    BLEDevice::init("RoboPrinter.Board");
    ble_server = BLEDevice::createServer();
    ble_server->setCallbacks(new CustomServerCallbacks());
    BLEService* ble_service =
        ble_server->createService(constants::service_uuid);

    // Write sensor reading characteristic
    uint32_t sensor_flags = BLECharacteristic::PROPERTY_READ;
    sensor_characteristic =
        ble_service->createCharacteristic(constants::sensor_uuid, sensor_flags);

    // Latency test characteristic
    uint32_t latency_flags = BLECharacteristic::PROPERTY_WRITE | BLECharacteristic::PROPERTY_INDICATE;
    sensor_characteristic =
        ble_service->createCharacteristic(constants::sensor_uuid, sensor_flags);

    // Read servo position characteristic
    uint32_t position_flags = BLECharacteristic::PROPERTY_WRITE_NR;
    position_characteristic = ble_service->createCharacteristic(
        constants::position_uuid, position_flags);
    position_characteristic->setCallbacks(new CustomResponseCallbacks());

    ble_service->start();
    // Turn on BLE connection advertisement
    BLEAdvertising* pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(constants::service_uuid);
    pAdvertising->setScanResponse(false);
    pAdvertising->setMinPreferred(0x06);
    pAdvertising->setMinPreferred(0x12);
    BLEDevice::startAdvertising();
    Serial.println("BLE setup done. Advertising the device...");

    servo_driver.initialize();

    delay(10);
}

void loop() {

    display_driver.clearDisplay();
    display_driver.print("TH0: ");
    display_driver.println(analogRead(GPIO_NUM_36));

    if (central_connected) {
        static int val = 0;

        // TODO use sensor feedback
        sensor_characteristic->setValue(val);
        val++;

        delay(1000);
    }

    // disconnecting
    if (!central_connected && old_central_connected) {
        delay(500);  // give the bluetooth stack the chance to get
                     // things ready
        ble_server->startAdvertising();  // restart advertising
        Serial.println("BLE device disconnected. Advertising the device...");
        old_central_connected = central_connected;
    }
    // connecting
    if (central_connected && !old_central_connected) {
        Serial.println("BLE device connected.");
        old_central_connected = central_connected;
    }

    delay(100);
}