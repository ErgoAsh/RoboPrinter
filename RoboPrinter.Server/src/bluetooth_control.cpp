#include "bluetooth_control.h"

#include <Arduino.h>

CustomServerCallbacks::CustomServerCallbacks(BluetoothControl* control) {
    _control = control;
}

void CustomServerCallbacks::onConnect(BLEServer* ble_server) {
    _control->central_connected = true;
}

void CustomServerCallbacks::onDisconnect(BLEServer* ble_server) {
    _control->central_connected = false;
}

void CustomServerCallbacks::onMtuChanged(BLEServer* pServer,
                                         esp_ble_gatts_cb_param_t* param) {
    Serial.print("New MTU length:");
    Serial.print(param->mtu.mtu);
    Serial.println();
}

CustomResponseCallbacks::CustomResponseCallbacks(ServoControl* servo_control) {
    _servo_control = servo_control;
}

void CustomResponseCallbacks::onWrite(BLECharacteristic* characteristic,
                                      esp_ble_gatts_cb_param_t* param) {
    auto input_data = characteristic->getValue().c_str();
    auto length = characteristic->getValue().length();

    Serial.print("Received message: ");
    Serial.print(input_data);
    Serial.println();

    _servo_control->process_data(characteristic->getValue(), length);
}

BluetoothControl::BluetoothControl(ServoControl* servo_control) {
    _servo_control = servo_control;
}

void BluetoothControl::initialize() {
    // Initialize BLE
    BLEDevice::init("RoboPrinter.Board");
    ble_server = BLEDevice::createServer();
    ble_server->setCallbacks(new CustomServerCallbacks(this));
    BLEService* ble_service =
        ble_server->createService(constants::service_uuid);

    // Sensor reading characteristic
    uint32_t sensor_flags =
        BLECharacteristic::PROPERTY_READ | BLECharacteristic::PROPERTY_NOTIFY;
    sensor_characteristic =
        ble_service->createCharacteristic(constants::sensor_uuid, sensor_flags);
    sensor_characteristic->addDescriptor(new BLE2902());

    // Servo position characteristic
    uint32_t position_flags = BLECharacteristic::PROPERTY_WRITE;
    position_characteristic = ble_service->createCharacteristic(
        constants::position_uuid, position_flags);
    position_characteristic->setCallbacks(
        new CustomResponseCallbacks(_servo_control));

    ble_service->start();

    // Turn on BLE connection advertisement
    BLEAdvertising* pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(constants::service_uuid);
    pAdvertising->setScanResponse(false);
    pAdvertising->setMinPreferred(0x06);
    pAdvertising->setMinPreferred(0x12);
    BLEDevice::startAdvertising();
    Serial.println("BLE setup done. Advertising the device...");
}

void BluetoothControl::on_loop() {
    // Disconnecting
    if (!central_connected && old_central_connected) {
        delay(500);  // give the bluetooth stack the chance to get
                     // things ready
        ble_server->startAdvertising();  // restart advertising
        Serial.println("BLE device disconnected. Advertising the device...");
        old_central_connected = central_connected;
    }
    // Connecting
    if (central_connected && !old_central_connected) {
        Serial.println("BLE device connected.");
        old_central_connected = central_connected;
    }
}