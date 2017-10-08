import socket, simplejson

# Gets a JSON from the client.
def get_client_message(conn):
  data = ''
  while(True):
    # Receiving from client
    data += conn.recv(1024).decode(encoding='utf_8')
    # If I got a newline, then -> parse the JSON.
    if ("\n" in data):
      # Do a parse. You can trust me to send good data. I promise.
      parsed_data = simplejson.loads(data[:-1])
      return parsed_data

# Write a JSON to the client.
def write_client_message(conn,dictn):
  conn.send((simplejson.dumps(dictn) + '\n').encode(encoding='utf_8'))