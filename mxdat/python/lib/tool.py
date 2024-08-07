import platform, requests, stat, subprocess
from io import BytesIO
from os import chmod
from zipfile import ZipFile
from .filepath import (
    FP_DUMP_CS,
    FP_FBS,
    FP_FLATDATA,
    FP_JP_APK,
    FP_TOOLS,
)


FP_IL2CPPDUMPER = FP_TOOLS / "Il2CppDumper"
FP_IL2CPPDUMPER.mkdir(parents=True, exist_ok=True)
FP_FLATC = FP_TOOLS / "flatc"
FP_FLATC.mkdir(parents=True, exist_ok=True)


def path_il2cpp_bin():
    os = platform.system()
    if os == "Windows":
        return FP_IL2CPPDUMPER / "Il2CppDumper.exe"
    if os == "Darwin" or os == "Linux":
        return FP_IL2CPPDUMPER / "Il2CppDumper"
    raise RuntimeError(f"Unsupported os: {os}")


def download_il2cpp():
    os = platform.system()
    if os == "Windows":
        url = "https://github.com/Perfare/Il2CppDumper/releases/download/v6.7.40/Il2CppDumper-win-v6.7.40.zip"
    elif os == "Darwin":
        url = "https://github.com/AndnixSH/Il2CppDumper/releases/download/v6.7.40/Il2CppDumper-net7-macos-v6.7.40.zip"
    elif os == "Linux":
        url = "https://github.com/AndnixSH/Il2CppDumper/releases/download/v6.7.40/Il2CppDumper-net7-linux-v6.7.40.zip"
    else:
        raise RuntimeError(f"Unsupported os: {os}")

    res = requests.get(url)
    path = path_il2cpp_bin()
    with ZipFile(BytesIO(res.content)) as zip:
        zip.extract(path.name, path.parent)
        chmod(path, stat.S_IRWXU)


def use_il2cpp():
    path = path_il2cpp_bin()
    if not path.exists():
        download_il2cpp()
    subprocess.run(
        [
            path,
            FP_JP_APK.with_suffix("") / "lib/arm64-v8a/libil2cpp.so",
            FP_JP_APK.with_suffix("")
            / "assets/bin/Data/Managed/Metadata/global-metadata.dat",
            FP_DUMP_CS.parent,
        ],
        check=True,
    )


def path_flatc_bin():
    os = platform.system()
    if os == "Windows":
        return FP_FLATC / "flatc.exe"
    if os == "Darwin" or os == "Linux":
        return FP_FLATC / "flatc"
    raise RuntimeError(f"Unsupported os: {os}")


def download_flatc():
    os = platform.system()
    prefix = "https://github.com/google/flatbuffers/releases/download/v24.3.25/"
    if os == "Windows":
        url = prefix + "Windows.flatc.binary.zip"
    elif os == "Darwin":
        url = prefix + "Mac.flatc.binary.zip"
    elif os == "Linux":
        url = prefix + "Linux.flatc.binary.clang++-15.zip"
    else:
        raise RuntimeError(f"Unsupported os: {os}")

    res = requests.get(url)
    path = path_flatc_bin()
    with ZipFile(BytesIO(res.content)) as zip:
        zip.extract(path.name, path.parent)
        chmod(path, stat.S_IRWXU)


def use_flatc():
    path = path_flatc_bin()
    if not path.exists():
        download_flatc()
    subprocess.run(
        [
            path,
            "--python",
            "--no-warnings",
            "-o",
            FP_FLATDATA.parent,
            FP_FBS,
        ],
        check=True,
    )
