import gzip
import json

f = open('mx.dat', 'r+b')
mx = f.read()
req_bytes = mx[12:]
req_bytes = bytearray([x ^ 0xD9 for x in req_bytes])
req_bytes = gzip.decompress(req_bytes)
# print(req_bytes)
print(json.loads(req_bytes))



f.close()
