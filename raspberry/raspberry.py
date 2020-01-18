from time import sleep
import smbus2
import numpy as np
import cv2
from PIL import Image
import tensorflow as tf # TF2
from datetime import datetime

from picamera.array import PiRGBArray
from picamera import PiCamera

camera = PiCamera()
camera.resolution = (1280, 720)
        
interpreter = tf.lite.Interpreter(model_path='/home/pi/Documents/tflite_NO_OPTIMIZE.tflite')
interpreter = tf.lite.Interpreter(model_path='/home/pi/Documents/tflite_OPTIMIZE_FOR_LATENCY.tflite')

interpreter.allocate_tensors()

input_details = interpreter.get_input_details()
output_details = interpreter.get_output_details()

  # check the type of the input tensor
floating_model = input_details[0]['dtype'] == np.float32

  # NxHxWxC, H:1, W:2
height = input_details[0]['shape'][1]
width = input_details[0]['shape'][2]

def predict(img):
    
    img = np.array(img.resize((width, height)), dtype=np.float32).reshape(1, width, height, 3) / 255.
    interpreter.set_tensor(input_details[0]['index'], img)
    interpreter.invoke()
    p = interpreter.get_tensor(output_details[0]['index'])
    return np.argsort(- p)[0][:10]

def save_image(mes):
	print(mes)
	class_id = mes[1] * 128 + mes[2]
	#class_id = 4
	filename = datetime.today().strftime('%Y%m%d%H%M%S') + "_" + str(class_id) + "_" + str(np.random.randint(100)) + '.png'
	print('Start saving file: ', filename)
	image.save('/home/pi/Documents/photos/' + filename, "PNG")
	byte_answers = []
	byte_answers.append(2)
	print(byte_answers)
	bus.write_i2c_block_data(address, 5, byte_answers)
	print('End saving file')

bus = smbus2.SMBus(1)
address = 0x08

sleep(0.1)
print('BEGIN')
count_error = 0

while True:
	try:
		message = bus.read_i2c_block_data(address, 5, 3)
		#print(message)
		
		if message[0] == 9:
			
			bus.write_i2c_block_data(address, 9, [9, 9])
		
		if message[0] == 1:
			
			print('Start recognizing')
			camera.capture('tmp.jpg')
			image = Image.open('tmp.jpg')
			#image = np.array(im)
			
			#print(image)
			
			answers = predict(image)
			
			byte_answers = []
			#byte_answers.append(1)
			for i in range(len(answers)):
				a = int(answers[i] / 128)
				byte_answers.append(a)
				byte_answers.append(answers[i] - 128 * a)
			print(byte_answers)
			bus.write_i2c_block_data(address, 1, byte_answers)
			print('End recognition')

		if message[0] == 2:
			
			save_image(message)
			
		if message[0] == 3:
			
			save_image(message)
			
	except Exception as e:
		print('ERROR: ', count_error, e)
		count_error += 1
	sleep(0.1)

