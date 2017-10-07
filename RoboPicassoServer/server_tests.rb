# Some ruby tests cause I'm a lot faster at ruby
require "socket"
c = TCPSocket.open("localhost",39182)
# Should be join now
puts c.gets