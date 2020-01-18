#include "HX711.h"                                                                        // подключаем библиотеку для работы с АЦП и тензодатчиками

HX711 scale;                                                                              // создаём объект scale для работы с тензодатчиком

uint8_t DOUT_PIN = 9;                                                                     // указываем вывод DOUT, к которому подключен HX711
uint8_t SCK_PIN  = 8;                                                                     // указываем вывод SCK , к которому подключен HX711

float     weight_of_standard = 500;                                                     // указываем эталонный вес
float     conversion_rate    = 0.035274;                                                  // указываем коэффициент для перевода из унций в граммы
const int z                  = 10;                                                        // указываем количество измерений, по которым будет найдено среднее значение
float     calibration_value[z];                                                           // создаём массив для хранения считанных значений
float     calibration_factor = 0;                                                         // создаём переменную для значения калибровочного коэффициента

void setup() {
  
Serial.begin(9600); 
  Serial.println("!!!!!!!");
                                                                     // инициируем работу с последовательным портом на скорости 57600 бод
  scale.begin(DOUT_PIN, SCK_PIN);  
  while (true){
    Serial.println(scale.is_ready());
    delay(100);
    break;
  }
                                                                     
  //scale.begin(DOUT_PIN, SCK_PIN);                                                         // инициируем работу с платой HX711, указав номера выводов Arduino, к которым подключена плата
  scale.set_scale();                                                                      // не калибруем полученные значения
  scale.tare();                                                                           // обнуляем вес на весах (тарируем)
  
  Serial.println("You have 10 seconds to set your known load");                           // выводим в монитор порта текст о том, что у вас есть 10 секунд для установки эталонного веса на весы
  Serial.println(scale.is_ready());
  delay(10000);                                                                           // ждём 10 секунд
  Serial.print("calibration factor: ");                                                   // выводим текст в монитор поседовательного порта
  for (int i = 0; i < z; i++) {                                                           // запускаем цикл, в котором
    calibration_value[i] = scale.get_units(1) / (weight_of_standard / conversion_rate);   // считываем значение с тензодатчика и переводим его в граммы
    calibration_factor += calibration_value[i];                                           // суммируем все значения
  }
  calibration_factor = calibration_factor / z;                                            // делим сумму на количество измерений
  Serial.println(calibration_factor);                                                     // выводим в монитор порта значение корректирующего коэффициента
}

void loop() {}
