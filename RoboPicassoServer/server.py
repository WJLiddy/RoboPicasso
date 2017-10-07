import socket, sys, time, simplejson, math, os, io
from threading import *
from base64 import b64encode
from google.cloud import vision
from google.cloud.vision import types

class Server:

	def __init__(self):

		# Declare members
		self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.sock.settimeout(60.0)

		self.state = "JOIN" # Others are INROUND, ENDROUND, ENDGAME
		self.round = 0

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
				# Do a parse. You can trust me to send good data. I promise.
				parsed_data = simplejson.loads(data)
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
				pass # do sleep instead.

		# Eventually, the game will re-enter the join phase. Notify that client that it's time to join.
		ok_msg = {"status": "Join Now"}
		self.write_client_message(conn,ok_msg)

		# At this point, the client will send username, and commits to join. You can save the struct.
		player = {}
		player["uname"] = self.get_client_message(conn)["uname"]
		player["conn"] = conn
		player["score"] = 0
		player["state"] = "AWAIT_PROMPT"
		self.players.append(player)

		# Wait for the game to start, which automatically sends the prompt. 
		while not self.state == "INROUND":
			pass

		# At this point we are in the first round.


		# Await transition to 
		# Close the connection
		conn.close()


	def game_thread(self):
		# Loop will ALWAYS start in Join state
		while (True):
			# In join state - If someone joins, wait 5 seconds to start the server. Otherwise idle.
			while(len(self.players) == 0):
				time.sleep(1)
			# Someone joined!
			sleep(5)

			# Ok, transition to round 1, in game 1, and send out our first prompt.
			#for k,v in enumerated(players):
			#	prompt = {""}
			#	self.write_client_message(players["conn"],

se = Server()


