# -*- coding: utf-8 -*-
import socket, sys, time, simplejson, math, os, io, random
from threading import *
from base64 import b64encode, b64decode
from google.cloud import vision
from google.cloud.vision import types

class Server:

  def __init__(self):

    # Declare members
    self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    self.sock.settimeout(60.0)

    self.state = "JOIN" # Others are INROUND, ENDROUND, ENDGAME
    self.round = 0
    self.prompt = ""

    self.WAITTIME = 30

    self.players = []
    # a player is a dictionary.
    # "conn" -> connection (socket)
    # "name" -> username
    # "score" -> score
    # "state" -> state.


    self.HOST = ''   # Symbolic name meaning all available interfaces
    self.PORT = 39182 # Arbitrary non-privileged port

    #Bind socket to local host and port
    try:
        # This line causes the "bind fail" to be ignored
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind((self.HOST, self.PORT))
    except socket.error as msg:
      print('Bind failed')
      sys.exit()

    # Socket starts to listen (with 100 queued connections lmao)
    self.sock.listen(100)
    self.start()

  def start(self):
    # Spin up a game thread
    Thread(None, self.game_thread).start()
    print("Server is online.")
    while True:
      # Await a client
      conn, addr = self.sock.accept()
      print('Connected with ' + addr[0] + ':' + str(addr[1]))

      # Start new thread takes 1st argument as a function name to be run, 
      # second is the tuple of arguments to the function.
      # group=None, target=None, name=None, args=(), kwargs={}, *, daemon=None
      Thread(None, self.client_thread, None, (conn,)).start()

      # The player will close the conn.

    self.sock.close()

  # Gets a JSON from the client.
  def get_client_message(self,conn):
    data = ''
    while(True):
      # Receiving from client
      data += conn.recv(1024).decode(encoding='utf_8')
      # If I got a newline, then -> parse the JSON.
      if ("\n" in data):
        print(data)
        # Do a parse. You can trust me to send good data. I promise.
        parsed_data = simplejson.loads(data[:-1])
        return parsed_data

  # Write a JSON to the client.
  def write_client_message(self,conn,dictn):
    conn.send((simplejson.dumps(dictn) + '\n').encode(encoding='utf_8'))


  def client_thread(self, conn):
    # Ok, the client has connected to us at this point. 

    # If there is a game in progress, we need to put them in a wait loop. 
    if not self.state == "JOIN":
      pls_wait_msg = {"status" : "Game is progress. Please Wait..."}
      self.write_client_message(pls_wait_msg)
      while not self.state == "JOIN":
        time.sleep(0.5)

    # Eventually, the game will re-enter the join phase. Notify that client that it's time to join.
    ok_msg = {"status": "Ready!"}
    self.write_client_message(conn,ok_msg)

    # At this point, the client will send username, and commits to join. You can save the struct.
    player = {}
    player["uname"] = self.get_client_message(conn)["uname"]
    player["conn"] = conn
    player["score"] = 0
    player["state"] = "AWAIT_PROMPT"
    self.players.append(player)

    while self.round < 6:
      # Wait for the game to start, which automatically sends the prompt. 
      while not self.state == "INROUND":
        pass

      prompt_msg = {"prompt" : self.prompt}
      self.write_client_message(conn, prompt_msg)
      print(self.prompt)

      player["state"] = "PROMPTED"


      # At this point we are in the first round.
      while not self.state == "SCORING":
        time.sleep(.1)

      imgdat = self.get_client_message(conn)
     
      pngdat = b64decode(imgdat["picture"])
      client = vision.ImageAnnotatorClient()
      score = self.rate(client,pngdat,self.prompt)
      player["score"] += score
      # Add total
      score_msg = {"prompt" : self.prompt, "round": self.round+1, "score": score, "ranking": 1, "total": player["score"]}
      self.write_client_message(conn, score_msg)


    # Await transition to 
    # Close the connection
    conn.close()


  def game_thread(self):
    # Loop will ALWAYS start in Join state
    while (True):
      # In join state - If someone joins, wait 5 seconds to start the server. Otherwise idle.
      while(len(self.players) < 1):
        time.sleep(1)

      time.sleep(10)

      # Begin the rounds!
      while self.round < 6:
        # Let's get started with a fresh prompt
        self.get_new_prompt()
        self.state = "INROUND"
        time.sleep(self.WAITTIME+3)
        self.state = "SCORING"
        time.sleep(10)
        self.round += 1

      break

      # Ok, transition to round 1, in game 1, and send out our first prompt.
      #for k,v in enumerated(players):
      # prompt = {""}
      # self.write_client_message(players["conn"],

  def get_new_prompt(self):

    labels_file = 'labels/labels.csv'

    with open(labels_file) as l:
      lis = l.read()
      labels = lis.split(",")
      rand = random.randint(0, len(labels))

      self.prompt = labels[rand]

  def rate(self, google_image_annotator_client, content, category):

    # Performace hit? Maybe.
    image = types.Image(content=content)

    # TODO Give the user a lot of leeway - try to detect up to 50 things. Right now gets 10.
    response = google_image_annotator_client.label_detection(image=image)
    labels = response.label_annotations

    guessed_labels = []
    for label in labels:
      if(label.description == category):
        return int(label.score*100)
    return 0

se = Server()


