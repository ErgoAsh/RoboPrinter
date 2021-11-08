#include "board_server_callbacks.h"
#include <Arduino.h>

BoardServerCallbacks::BoardServerCallbacks(BluetoothService* service) {
    _service = service;
}

void BoardServerCallbacks::onConnect(BLEServer* ble_server) {
    _service->central_connected = true;
}

void BoardServerCallbacks::onDisconnect(BLEServer* ble_server) {
    _service->central_connected = false;
}

void BoardServerCallbacks::onMtuChanged(BLEServer* pServer,
                                         esp_ble_gatts_cb_param_t* param) {
    Serial.print("New MTU length:");
    Serial.print(param->mtu.mtu);
    Serial.println();
}