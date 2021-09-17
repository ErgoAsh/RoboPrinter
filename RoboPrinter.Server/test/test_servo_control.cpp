#include <Arduino.h>
#include <servo_service.h>
#include <unity.h>
#include <iostream>

static ServoService service = ServoService(nullptr);
static std::string correct_data;
static std::array<float, 5> empty_array = { };

void test_null_data_parse() {
    TEST_ASSERT(empty_array == service.parse_data(nullptr, 20));
}

void test_empty_data_parse() {
    const std::string data = {};
    TEST_ASSERT(empty_array == service.parse_data(data, 20));
}

void test_invalid_length_parse() {
    TEST_ASSERT(empty_array == service.parse_data(correct_data, 5));
}

void test_valid_parse() {
    std::array<float, 5> array = { 2137.0f, 2137.0f, 2137.0f, 2137.0f, 2137.0f };
    TEST_ASSERT(array == service.parse_data(correct_data, 20));
}
 
void setup() {
    delay(2000);

    uint32_t number = 0x45059000;
    for (int i = 0; i < 5; i++) {
        for (int j = 0; j < 4; j++) {
            uint32_t mask = 0xFF << j * 8;
            uint32_t content = content & mask;
            char symbol = content >> j * 8;
            correct_data.push_back(symbol);
        }
    }
    std::cout << correct_data;

    UNITY_BEGIN();
    RUN_TEST(test_null_data_parse);
    RUN_TEST(test_empty_data_parse);
    RUN_TEST(test_invalid_length_parse);
    RUN_TEST(test_valid_parse);
    UNITY_END();
}

void loop() {

}