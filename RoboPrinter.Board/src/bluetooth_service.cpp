#include <bluetooth_service.h>

#include <Arduino.h>

BluetoothService::BluetoothService(ServoService* servo_service) {
    _servo_service = servo_service;
}

void BluetoothService::initialize() {
    // Initialize BLE
    BLEDevice::init("RoboPrinter.Board");
    ble_server = BLEDevice::createServer();
    ble_server->setCallbacks(new BoardServerCallbacks(this));
    BLEService* ble_service =
        ble_server->createService(constants::service_uuid);

    // Sensor reading characteristic ==========================================
    uint32_t sensor_flags =
        BLECharacteristic::PROPERTY_READ | BLECharacteristic::PROPERTY_NOTIFY;
    sensor_characteristic =
        ble_service->createCharacteristic(constants::sensor_uuid, sensor_flags);
    sensor_characteristic->addDescriptor(new BLE2902());

    // Servo position characteristic ==========================================
    uint32_t position_flags = 
        BLECharacteristic::PROPERTY_WRITE;
    position_characteristic = ble_service->createCharacteristic(
        constants::position_uuid, position_flags);
    position_characteristic->setCallbacks(
        new BoardWriteCallbacks(_servo_service));

    // Latency test characteristic ============================================
    uint32_t latency_test_flags = 
        BLECharacteristic::PROPERTY_READ;
    position_characteristic = ble_service->createCharacteristic(
        constants::latency_test_uuid, latency_test_flags);
    position_characteristic->setCallbacks(
        new BoardWriteCallbacks(_servo_service));

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

void BluetoothService::on_loop() {
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