# -*- coding: utf-8 -*-
import socket, sys, time, simplejson, math, os, io, random, rating, net_tools
from threading import *
from base64 import b64encode, b64decode
from google.cloud import vision
from google.cloud.vision import types

class Server:

  def __init__(self):

    # Declare members
    self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    self.sock.settimeout(6000.0)

    self.state = "JOIN" # Others are INROUND, ENDROUND, ENDGAME
    self.round = 0
    self.prompt = ""

    self.WAITTIME = 30
    self.MAX_ROUNDS = 7

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
    game = Thread(None, self.game_thread).start()
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

  def client_thread(self, conn):
    # Ok, the client has connected to us at this point. 

    # If there is a game in progress, we need to put them in a wait loop. 
    if not self.state == "JOIN":
      pls_wait_msg = {"status" : "Game is progress. Please Wait..."}
      write_client_message(pls_wait_msg)
      oldround = self.round
      while not self.state == "JOIN":
        if not oldround == self.round:
          pls_wait_msg = {"status" : "Game is progress. Round " + str(self.round+1) + "/" + str(self.MAX_ROUNDS)}
          write_client_message(pls_wait_msg)
        time.sleep(1)

    # Eventually, the game will re-enter the join phase. Notify that client that it's time to join.
    ok_msg = {"status": "Ready!"}
    write_client_message(conn,ok_msg)

    # At this point, the client will send username, and commits to join. You can save the struct.
    player = {}
    player["uname"] = get_client_message(conn)["uname"]
    player["conn"] = conn
    player["score"] = 0
    player["state"] = "AWAIT_PROMPT"
    self.players.append(player)

    while self.round < self.MAX_ROUNDS:
      # Wait for the game to start, which automatically sends the prompt. 
      while not self.state == "INROUND":
        pass

      prompt_msg = {"prompt" : self.prompt}
      write_client_message(conn, prompt_msg)
      print(self.prompt)

      player["state"] = "PROMPTED"


      # At this point we are in the first round.
      while not self.state == "SCORING":
        time.sleep(.1)

      imgdat = get_client_message(conn)
     
      pngdat = b64decode(imgdat["picture"])
      client = vision.ImageAnnotatorClient()
      score = rate(client,pngdat,self.prompt)
      player["score"] += score
      time.sleep(1)

      # Generate Report player and scores in a tuple
      report = []
      for p in self.players:
        report.append([p["uname"],p["score"]])
      report.sort(key=lambda x: x[1], reverse=True)

      # Get rank
      rank = 0
      for p in report:
        rank += 1
        if(p[0] == player["uname"]):
          break

      # Get score rep
      score_rep = ""
      for p in report:
        score_rep += str(p[0]) + " : " + str(p[1]) + "\n"
         
      score_msg = {"prompt" : self.prompt, "score" : score, "round": self.round+1, "report": score_rep, "ranking": rank}
      write_client_message(conn, score_msg)


    # Await transition to 
    # Close the connection
    conn.close()


  def game_thread(self):
    # Loop will ALWAYS start in Join state
    while (True):
      # Reset!
      self.state = "JOIN" # Others are INROUND, ENDROUND, ENDGAME
      self.round = 0
      self.prompt = ""

      # In join state - If someone joins, wait 5 seconds to start the server. Otherwise idle.
      while(len(self.players) < 1):
        time.sleep(1)

      time.sleep(10)

      # Begin the rounds!
      while self.round < self.MAX_ROUNDS:
        # Let's get started with a fresh prompt
        self.prompt = get_new_prompt()
        self.state = "INROUND"
        time.sleep(self.WAITTIME+3)
        self.state = "SCORING"
        time.sleep(10)
        self.round += 1
      
      time.sleep(1)

      # Ok, transition to round 1, in game 1, and send out our first prompt.
      #for k,v in enumerated(players):
      # prompt = {""}
      # self.write_client_message(players["conn"],

se = Server()
