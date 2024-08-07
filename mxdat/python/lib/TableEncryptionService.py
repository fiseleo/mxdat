from base64 import b64decode, b64encode
from Crypto.Util.strxor import strxor
from struct import Struct
from typing import TypeVar
from . import XXHashService
from .MersenneTwister import MersenneTwister


def newZipPassword(key: str) -> bytes:
    return b64encode(CreateKey(key, 15))


def CreateKey(name: str, size: int = 8) -> bytes:
    seed = XXHashService.CalculateHash(name)
    return MersenneTwister(seed).NextBytes(size)


def XOR(name: str, data: bytes) -> bytes:
    if len(data) == 0:
        return data
    mask = CreateKey(name, len(data))
    return _XOR(data, mask)


def _XOR(value: bytes, key: bytes) -> bytes:
    # return bytes(x ^ y for x, y in zip(value, key))
    if len(value) == len(key):
        return strxor(value, key)
    elif len(value) < len(key):
        return strxor(value, key[: len(value)])
    else:
        return b"".join(
            strxor(value[i : i + len(key)], key)
            for i in range(0, len(value) - len(key) + 1, len(key))
        ) + strxor(
            value[(len(value) - (len(value) % len(key))) :],
            key[: len(value) % len(key)],
        )


T = TypeVar("T", int, float)


def _XOR_Struct(value: T, key: bytes, struct: Struct) -> T:
    return struct.unpack(_XOR(struct.pack(value), key))[0]


def ConvertSbyte(value: int, key: bytes) -> int:
    """This is just a dummy function."""
    return _XOR_Struct(value, key, Struct("<b")) if value else 0


def ConvertInt(value: int, key: bytes) -> int:
    return _XOR_Struct(value, key, Struct("<i")) if value else 0


def ConvertLong(value: int, key: bytes) -> int:
    return _XOR_Struct(value, key, Struct("<q")) if value else 0


def ConvertUInt(value: int, key: bytes) -> int:
    return _XOR_Struct(value, key, Struct("<I")) if value else 0


def ConvertULong(value: int, key: bytes) -> int:
    return _XOR_Struct(value, key, Struct("<Q")) if value else 0


def ConvertFloat(value: float, key: bytes) -> float:
    return ConvertInt(int(value), key) * 0.00001 if value else 0.0


def ConvertDouble(value: float, key: bytes) -> float:
    return ConvertLong(int(value), key) * 0.00001 if value else 0.0


def EncryptFloat(value: float, key: bytes) -> float:
    return ConvertInt(int(value * 100000), key) if value else 0.0


def EncryptDouble(value: float, key: bytes) -> float:
    return ConvertLong(int(value * 100000), key) if value else 0.0


def ConvertString(value: bytes | str, key: bytes) -> str:
    if not value:
        return ""

    # the animator table contain strings that are not base64 encoded
    try:
        raw = b64decode(value)
        return _XOR(raw, key).decode("utf16")
    except:
        assert isinstance(value, bytes)
        return value.decode("utf8")


def EncryptString(value: str, key: bytes) -> str | bytes:
    if not value or len(value) < 8:
        return value.encode() if value else b""
    raw = value.encode("utf16")
    return b64encode(_XOR(raw, key)).decode()
    # return b64encode(bytes(r ^ k for r, k in zip(raw, cycle(key))))
