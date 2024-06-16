import gzip
import json

f = open('mx.dat', 'r+b')
mx = f.read()
req_bytes = mx[12:]
req_bytes = bytearray([x ^ 0xD9 for x in req_bytes])
req_bytes = gzip.decompress(req_bytes)
mx_json = json.loads(req_bytes)
print(json.loads(req_bytes))
accountserverid = mx_json["SessionKey"]["AccountServerId"]
mxtoken = mx_json["SessionKey"]["MxToken"]
f.close()

with open('.sessionkey', 'w') as sessionkey_file:
    sessionkey_file.write("AccountServerId=" + str(accountserverid) + "\n")
    sessionkey_file.write("MxToken=" + mxtoken + "\n")
