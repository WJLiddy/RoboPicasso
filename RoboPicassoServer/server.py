import socket, sys, time, simplejson, math, os, io
from threading import *
from base64 import b64encode
from google.cloud import vision
from google.cloud.vision import types

class Server:
  # Set up network
  HOST = ''   # Symbolic name meaning all available interfaces
  PORT = 39182 # Arbitrary non-privileged port
  s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  s.settimeout(60.0)
  current_id = 0  # Unique ID for each user

  def __init__(self):
    print('Socket created')

    #Bind socket to local host and port
    try:
        self.s.bind((self.HOST, self.PORT))
    except socket.error as msg:
      print('Bind failed. Error Code : ' + str(msg[0]) + ' Message ' + msg[1])
      sys.exit()

    print('Socket bind complete')
    self.state = 0

    self.s.listen(100)
    print('Socket now listening')
    self.start()

  def start(self):
    clients = []
    while True:
      if self.state <= 0:
        if len(clients) > 0:
          Thread(None, self.game, current_id, (clients,)).start()
        self.state = time.time() + 60 * 2
        clients = []

      while time.time()<self.state:
        # Wait to accept a connection - blocking call
        conn, addr = self.s.accept()
        print('Connected with ' + addr[0] + ':' + str(addr[1]))

        # Start new thread takes 1st argument as a function name to be run, 
        # second is the tuple of arguments to the function.
        # group=None, target=None, name=None, args=(), kwargs={}, *, daemon=None
        cl = Thread(None, self.clientthread, current_id, (conn,)).start()
        clients.append(cl)



    self.s.close()

  # Function for handling connections. This will be used to create threads
  def clientthread(self, conn, server):

    data = ''

    while not self.state == "start":
      # Receiving from client
      data += conn.recv(1024)
      # Data stream ends with newline
      if data.endswith("\n"):
        if data[:-1] == "status":
          left = self.end - time.time()
          msg = "Begin in " + left + " seconds."
          conn.send(msg)
          print("Data Sent")
          time.sleep(0.5)
      
    conn.send("Start!")
    conn.send("Prompt!")

    while True:
      time_out=30.0
      ready=[[s],[],[], time_out]
      # Receiving from client
      if ready:
        data += conn.recv(1024)
      # Data stream ends with newline
      if data.endswith("\n"):
        # Parse the Json Data
        #parsed_data = self.parse_data(data[:-1])
        # Get raw image data
        img = "data:image/png;base64," + b64encode(data)
        self.submit(img)
        # Send off the data
        conn.send(mapimg)
        # Close the image
        #mapimg.close()
        print("Data Sent")
        break

    # Close the connection
    conn.close()

  def parse_data(self, data):
    parsed_data = simplejson.loads(data)
    return parsed_data

  def game(self, cl):


  def rate(self, google_image_annotator_client, content, category):

    # Performace hit? Maybe.
    image = types.Image(content=content)

    # TODO Give the user a lot of leeway - try to detect up to 50 things. Right now gets 10.
    response = google_image_annotator_client.label_detection(image=image)
    labels = response.label_annotations

    guessed_labels = []
    for label in labels:
      if(label.description == category):
        return label.score
    return 0


  def submit(self, img):
    client = vision.ImageAnnotatorClient()
    file = ["barlyke.png","shitty_car.png","good_car.png"]
    imgs = []
    for file in files:
      with io.open("tests/" + file, 'rb') as image_file:
        imgs.append(image_file.read())

    for image in imgs:
      print(rate(client,image,"car"))


se = Server()

                                                                                   
 