#include <Wire.h>
#include "HX711.h"                                            // подключаем библиотеку для работы с тензодатчиком
#include "SevSeg.h"



#define DT  A0                                                // Указываем номер вывода, к которому подключен вывод DT  датчика
#define SCK A1                                                // Указываем номер вывода, к которому подключен вывод SCK датчика
#define SLAVE_ADDRESS 0x08

#define pinArduino 12
#define pinRaspberry 13

HX711 scale;                                                  // создаём объект scale

float calibration_factor = 6.66;                            // вводим калибровочный коэффициент
float adjustWeigth = 0.0;
float units = 0.0;                                                  // задаём переменную для измерений в граммах
int weight = 0;                                                 // задаём переменную для измерений в унциях
int min_weight = 50;

int duration;

byte system_status = 0;

long start_checking_raspberry = 0;

/* 
  0 Ожидание
  1 Товар установлен
  2 Отправлен запрос на RPI о распознавании
  3 Получен ответ с RPI о распознавании
  4 Отправлены итоги распознавания на кассу
  5 Касса передала правильный ответ
  6 Правильный ответ передается на RPI
  7 RPI сообщает об успешком сохранении картинки
  8 RPI не ответила в течение 1 секунды
  9 Отправлены пустые итоги распознавания на кассу
*/

byte id_processing;

//bool is_recognize_started     = false;
//bool is_recognize_sended      = false;
//bool is_correct_answer_readed = false;
//bool is_correct_answer_sended = false;
//bool is_waiting               = true;

//bool is_recognize_failed;
unsigned long current_milliseconds;

byte weight_bytes[2];

byte message_from_cass[2];
String message_to_cass;

byte message_from_raspberry[21];
byte message_to_raspberry[3];

int message_from_raspberry_count;

void setup(){
  
  Serial.begin(9600);
  //Serial.println("Loaded");
  
  Wire.begin(SLAVE_ADDRESS);
  Wire.onReceive(receiveData);
  Wire.onRequest(sendData);

  pinMode(pinRaspberry, OUTPUT);
  pinMode(pinArduino, OUTPUT);
  
  digitalWrite(pinRaspberry, LOW);
  digitalWrite(pinArduino, HIGH);

  while (Wire.available()) Wire.read();

  scale.begin(DT, SCK);                                       // инициируем работу с датчиком
  scale.set_scale();                                          // выполняем измерение значения без калибровочного коэффициента
  scale.tare();                                               // сбрасываем значения веса на датчике в 0
  scale.set_scale(calibration_factor);                        // устанавливаем калибровочный коэффициент

  message_from_raspberry_count = sizeof(message_from_raspberry);

  delay(500);

  units = 0;
  units = + scale.get_units(), 1000;
  units = units * 0.035274;
  weight = units;
  adjustWeigth = units;
  //Serial.println(units);
  //message_to_cass[0] = 0;
  //message_to_cass[1] = weight;
  //message_to_cass[2] = int((units - weight) * 100);
  //Serial.write(message_to_cass, sizeof(message_to_cass));
  //adjustWeigth = units;
  //Serial.println("adjustWeigth = " + String(adjustWeigth));
  
}

void loop()
{

  units = + scale.get_units(), 100;
  //Serial.println(units);
  
  if (system_status == 0 && millis() - start_checking_raspberry > 500){
    digitalWrite(pinRaspberry, LOW);
  }
  else if (system_status == 0 && millis() - start_checking_raspberry < 500){
    digitalWrite(pinRaspberry, HIGH);
  }

  
  weight = get_weight();
  
  if (weight < min_weight && system_status == 4){
    message_to_cass = "3";
    Serial.println(message_to_cass); 
    delay(200);
  }
  
  
  if (weight < min_weight) system_status = 0;
  
  if (system_status == 1 && millis() - current_milliseconds > 1000){ // RPI не отвечает в течение секунды на распознавание

    system_status = 8; // RPI своевременно не отдала ответ
    //message_to_cass[0] = 1;
    //message_to_cass[1] = weight_bytes[0];
    //message_to_cass[2] = weight_bytes[1];
    message_to_cass = "1 " + String(weight_bytes[0]) + " " + String(weight_bytes[1]);
    //for (byte i = 3; i < sizeof(message_to_cass); i++) message_to_cass[i] = 0;
    Serial.println(message_to_cass); 
    system_status = 9; 
        
  }
  
  
  if (system_status == 0 && weight >= min_weight){

    system_status = 1;
    current_milliseconds = millis();
    delay(300);
    weight = get_weight();
    get_weight_bytes(weight_bytes, weight);
    
  }
  
  
  if ((system_status == 4 || system_status == 9) && Serial.available() > 0){

    Serial.readBytes(message_from_cass, sizeof(message_from_cass));
    system_status = 5;
    current_milliseconds = millis();
  }
  
  
  if (system_status == 5 && millis() - current_milliseconds > 500){
    system_status = 8;
  }

  
  //sevseg.setNumber(system_status);
  //sevseg.refreshDisplay(); 

  
  delay(100);
  
}

void receiveData(int byteCount){

  byte i = 0;
  while (Wire.available()) {
    message_from_raspberry[i] = byte(Wire.read());
    i += 1;
  }

  start_checking_raspberry = millis();

  if (system_status == 2){

    system_status = 3;
    message_to_cass = "2 " + String(weight_bytes[0]) + " " + String(weight_bytes[1]);
    for (byte k = 1; k < message_from_raspberry_count; k++) {
      message_to_cass += " " + String(message_from_raspberry[k]);
      //Serial.println(message_from_raspberry[k]);
    }
    Serial.println(message_to_cass);
    system_status = 4;
    
  }

  if (system_status == 6){
    system_status = 7;    
  }

  delay(100);
}

void sendData(){
  
  if (system_status == 1){
    
    message_to_raspberry[0] = 1;
    message_to_raspberry[1] = 0;
    message_to_raspberry[2] = 0;
    Wire.write(message_to_raspberry, sizeof(message_to_raspberry));
    system_status = 2;
    
  }

  if (system_status == 5){
    
    message_to_raspberry[0] = 2;
    message_to_raspberry[1] = message_from_cass[0];
    message_to_raspberry[2] = message_from_cass[1];
    Wire.write(message_to_raspberry, sizeof(message_to_raspberry));
    system_status = 6;
    
  }

  if (weight < min_weight && system_status == 4){
    
    message_to_raspberry[0] = 3;
    message_to_raspberry[1] = 0;
    message_to_raspberry[2] = 0;
    
    Wire.write(message_to_raspberry, sizeof(message_to_raspberry));
    system_status = 0;
  }

  delay(100);
}

int get_weight(){
    units = + scale.get_units(), 100;
    weight = (units * 0.035274 - adjustWeigth) / 1;
    //Serial.println(units * 0.035274);
    return weight * 1;
}

void get_weight_bytes(byte *weight_bytes, int wght){
  if (wght < 0){
    weight_bytes[0] = 0;
    weight_bytes[1] = 0;
  }
  else{
    weight_bytes[0] = wght / 128;
    weight_bytes[1] = wght - weight_bytes[0] * 128;
  }
}
